using System;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Management;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ApiWrapper;
using ApiWrapper.Dto;
using DNTPersianUtils.Core;
using IPAClient.Models;
using IPAClient.Tools;
using DataLayer.Services;
using DataLayer.Entities;
using System.Linq;
using System.Windows.Input;
using System.Windows.Interop;

namespace IPAClient.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// از این تایمر برای تلاش مجدد برای دریافت اطلاعات اولیه از سرور در صورت بروز خطا استفاده می شود
        /// </summary>
        //private DispatcherTimer _recheckTimer;
        private FingerPrintHelper _fingerPrintHelper;
        private MonitorHelper _monitorHelper;
        private readonly ReservationService _reservationService;
        private MonitorDto monitorDto;
        public MainWindow()
        {
            InitializeComponent();
            _reservationService = new ReservationService();
            SetLabelsVisible(false);

        }

        private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            await CheckConfigStatusAndInitUtilitiesAsync();
            //در زمان هایی که برنامه مشغول کار دیگری نیست اجرا میشود
            ComponentDispatcher.ThreadIdle += new EventHandler(CheckTimeForAutoUpdate);
            monitorDto = new MonitorDto();
            monitorDto.QueueChange += async (sender, json) => { await SendMonitorData(json); };
            //InitilizeRecheckTimer();
        }

        private async void CheckTimeForAutoUpdate(object sender, EventArgs e)
        {
            // در هر روز همه ی اطلاعات بروز میشود
            if (App.LastFullUpdateTime == null || App.LastFullUpdateTime.Value.IsNextDay())
            {
                await GetConfigFromServerAsync();
            }
            //هر 30 دقیقه اطلاعات رزرو وعده فعلی بروز میشود
            else if (App.LastMealUpdateTime == null || App.LastMealUpdateTime.Value.IsMinutePassed(30))
            {
                await UpdateCurrentMealReservationFromServer();
            }

            App.CurrentMealCode = App.MainInfo.Meals.FirstOrDefault(x => x.IsCurrentMeal)?.Cod_Data;
        }
        
        private async Task CheckConfigStatusAndInitUtilitiesAsync()
        {
            UpdateDateLabel();
            if (!File.Exists(App.ConfigFilePath))
            {
                await ShowNeedConfigAsync();
            }
            else
            {
                var configContent = await File.ReadAllTextAsync(App.ConfigFilePath);
                ConfigModel configModel;
                try
                {
                    configModel = JsonSerializer.Deserialize<ConfigModel>(configContent);
                }
                catch (Exception)
                {
                    configModel = null;
                }

                if (configModel == null)
                {
                    await ShowNeedConfigAsync();
                }
                else if (!configModel.IsConfirmed)
                {
                    await ShowConfirmConfigAsync(configModel);
                }
                else
                {
                    App.AppConfig = configModel;
                    ApiClient.SetAuthToken(string.IsNullOrWhiteSpace(configModel.WebServiceAuthToken) ? "Basic TkFNRk9PRHVzZXIxOjUxZjIzMDQxYWNkOGRmNzlkMWIxOGY2ZjE2ZWE4YzM2" : configModel.WebServiceAuthToken);
                    ApiClient.SetBaseUrl(string.IsNullOrWhiteSpace(configModel.WebServiceUrl) ? "http://eis.msc.ir/" : configModel.WebServiceUrl);
                    if (!App.AppConfig.IsDemo)
                    {
                        await GetConfigFromServerAsync();
                        InitReConfigListener();
                    }
                    else
                    {
                        SetBackGroundImage("21");
                    }
                    ClearLabels();
                    SetLabelsVisible(true);
                    await InitFingerPrintListener();
                    InitRfIdListener();

                }
            }
        }

        private async Task GetConfigFromServerAsync()
        {
            //_recheckTimer.Stop();
            if (!App.AppConfig.IsDemo)
            {
                //دریافت اطلاعات اولیه و تاریخ و ساعت سرور 
                SetBackGroundImage("9");
                try
                {
                    var mainInfo = await ApiClient.MainInfo_Send_Lookup_Data_Fun();
                    if (mainInfo == null)
                    {
                        if (File.Exists(App.MainInfoFilePath))
                        {
                            var mainInfoContent = await File.ReadAllTextAsync(App.MainInfoFilePath);
                            try
                            {
                                mainInfo = JsonSerializer.Deserialize<MainInfo_Send_Lookup_Data_Fun>(mainInfoContent);
                            }
                            catch (Exception e)
                            {
                                App.AddLog(e);
                                throw;
                            }
                            App.MainInfo = mainInfo;
                        }
                        else
                        {
                            throw new Exception("Main Info File Not Found");
                        }
                    }
                    else
                    {
                        SetBackGroundImage("10");
                        await File.WriteAllTextAsync(App.ConfigFilePath, JsonSerializer.Serialize(mainInfo));
                        App.MainInfo = mainInfo;
                        SystemTimeHelper.SetSystemTime(mainInfo.ServerDateTime);
                        UpdateDateLabel();
                    }
                }
                catch (Exception ex)
                {
                    App.AddLog(ex);
                    SetBackGroundImage("11");
                    //_recheckTimer.Start();
                    return;
                }
                //دریافت اطلاعات هویتی پرسنل
                SetBackGroundImage("12");
                //TODO Update Personnel Info
                Thread.Sleep(2000);
                SetBackGroundImage("13");

                //دریافت رزرواسیون آفلاین
                SetBackGroundImage("15");
                try
                {
                    await GetAllMealsReservationsFromServer();
                    SetBackGroundImage("16");
                }
                catch (Exception ex)
                {
                    App.AddLog(ex);
                    SetBackGroundImage("17");
                    //_recheckTimer.Start();
                    return;
                }
            }
            App.LastFullUpdateTime = DateTime.Now;
            App.LastMealUpdateTime = DateTime.Now;
            //تصویر پس زمینه آماده به کار
            SetBackGroundImage("21");
        }

        private async Task GetAllMealsReservationsFromServer()
        {
            if (App.MainInfo?.Meals != null && App.MainInfo.Meals.Any())
            {
                foreach (var meal in App.MainInfo.Meals)
                {
                    var reservationDate =
                        await ApiClient.MainInfo_Send_Offline_Data_Fun(new MainInfo_Send_Offline_Data_Fun_Input_Data(App.AppConfig.Device_Cod, DateTime.Now.ToServerDateFormat(), meal.Cod_Data));

                    await _reservationService.InsertAsync(reservationDate.Select(x=> MapToReservation(x,meal.Cod_Data)).ToList());

                    var minMealTime = reservationDate.Min(x => x.Num_Tim_Str_Meal_Rsmls).ToTimeSpan();
                    var maxMealTime = reservationDate.Max(x => x.Num_Tim_End_Meal_Rsmls).ToTimeSpan();

                    meal.StartTime = minMealTime;
                    meal.EndTime = maxMealTime;
                }
                App.CurrentMealCode = App.MainInfo.Meals.FirstOrDefault(x => x.IsCurrentMeal)?.Cod_Data;
            }
        }

        private async Task UpdateCurrentMealReservationFromServer()
        {
            var reservationDate = await ApiClient.MainInfo_Send_Offline_Data_Fun(new MainInfo_Send_Offline_Data_Fun_Input_Data(App.AppConfig.Device_Cod, DateTime.Now.ToServerDateFormat(),App.CurrentMealCode));
            await _reservationService.InsertAsync(reservationDate.Select(x=>MapToReservation(x, App.CurrentMealCode)).ToList());
            App.LastMealUpdateTime = DateTime.Now;
        }

        private Reservation MapToReservation(MainInfo_Send_Offline_Data_Fun_Output_Data input,string mealCode)
        {
            return new Reservation
            {
                Id = Guid.NewGuid(),
                Date = DateTime.Now.ToString(),
                Cod_Meal = mealCode,
                Date_Use = null,
                Status = null,
                Time_Use = null,

                Main_Course = input.Main_Course.Select(x => new Food { Des_Food = x.Des_Food, Num_Amount = x.Num_Amount, Typ_Serv_Unit = x.Typ_Serv_Unit }).ToList(),
                Appetizer_Dessert = input.Appetizer_Dessert.Select(x => new Food { Des_Food = x.Des_Food, Num_Amount = x.Num_Amount, Typ_Serv_Unit = x.Typ_Serv_Unit }).ToList(),
                Cod_Contract_Order = input.Cod_Contract_Order,
                Cod_Coupon = input.Cod_Coupon,
                Cod_Resturant = input.Cod_Resturant,
                Cod_Serial = input.Cod_Serial,
                Dat_Day_Mepdy = input.Dat_Day_Mepdy,
                Des_Contract_Order = input.Des_Contract_Order,
                Des_Food_Order_Mepdy = input.Des_Food_Order_Mepdy,
                Des_Nam_Meal = input.Des_Nam_Meal,
                Des_Nam_Resturant_Rstm = input.Des_Nam_Resturant_Rstm,
                Employee_Shift_Name = input.Employee_Shift_Name,
                First_Name_Ide = input.First_Name_Ide,
                Last_Name_Ide = input.Last_Name_Ide,
                Lkp_Cod_Order_Mepdy_Means = input.Lkp_Cod_Order_Mepdy_Means,
                Meal_Plan_Day_Id = input.Meal_Plan_Day_Id,
                Num_Ide = input.Num_Ide,
                Num_Tim_End_Meal_Rsmls = input.Num_Tim_End_Meal_Rsmls,
                Num_Tim_Str_Meal_Rsmls = input.Num_Tim_Str_Meal_Rsmls,
                Num_Tot_Coupon_Rccpn = input.Num_Tot_Coupon_Rccpn,
                Receiver_Meal_Plan_Day_Id = input.Receiver_Meal_Plan_Day_Id,
                Reciver_Coupon_Id = input.Reciver_Coupon_Id,

            };
        }

        #region ٌWindow Functions
        private async Task ShowNeedConfigAsync()
        {
            _ = new wndNeedConfig().ShowDialog();
            await CheckConfigStatusAndInitUtilitiesAsync();
        }

        private async Task ShowConfirmConfigAsync(ConfigModel configModel)
        {
            _ = new wndConfirmConfig(configModel).ShowDialog();
            await CheckConfigStatusAndInitUtilitiesAsync();
        }

        private void SetBackGroundImage(string imageName)
        {
            Dispatcher.BeginInvoke(() =>
            {
                dgMain.Background =
                    new ImageBrush(new BitmapImage(
                        new Uri(@$"pack://application:,,,/IPAClient;component/Images/{imageName}.png")));
            });
        }

        private void SetLabelsVisible(bool isVisible)
        {
            lblDate.Visibility = lblName.Visibility = lblNumber.Visibility = lblVade.Visibility = lblShift.Visibility = lblShiftCompany.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ClearLabels()
        {
            lblName.Content = lblNumber.Content = lblVade.Content = lblShift.Content = lblShiftCompany.Content = string.Empty;
        }

        private void HideBorder(Border brd)
        {
            brd.Visibility = Visibility.Collapsed;
        }

        private void ShowBorder(Border brd, bool isSuccess)
        {
            Dispatcher.Invoke(() =>
            {
                brd.BorderBrush = new SolidColorBrush(isSuccess ? Color.FromRgb(0, 255, 0) : Color.FromRgb(255, 0, 0));
                brd.Visibility = Visibility.Visible;
                if (isSuccess)
                {
                    System.Media.SystemSounds.Hand.Play();
                }
                else
                {
                    System.Media.SystemSounds.Asterisk.Play();
                }
            });
        }

        private void UpdateDateLabel()
        {
            lblDate.Content = SystemTimeHelper.CurrentPersinaFullDateTime();
        }

        private async void BtnKeyPad_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var wndKeyPad = new wndKeyPad();
            wndKeyPad.ShowDialog();
            if (!string.IsNullOrWhiteSpace(wndKeyPad.PersonnelNumber))
            {
                await CheckReservation(wndKeyPad.PersonnelNumber);
            }
        }
        #endregion

        #region ReConfig Listener
        private void InitReConfigListener()
        {
            ManagementEventWatcher watcher = new ManagementEventWatcher();
            WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2");
            watcher.EventArrived += Watcher_EventArrived;
            watcher.Query = query;
            watcher.Start();
        }

        private async void Watcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            var driveNameProp = e.NewEvent.Properties["DriveName"];
            if (!string.IsNullOrWhiteSpace(driveNameProp?.Value.ToString()))
            {
                var configFilePath = driveNameProp.Value + @"\reconfig.json";
                if (File.Exists(configFilePath))
                {
                    try
                    {
                        var configContent = await File.ReadAllTextAsync(configFilePath);
                        if (!string.IsNullOrWhiteSpace(configContent))
                        {
                            var configModel = JsonSerializer.Deserialize<ConfigModel>(configContent);
                            if (configModel != null)
                            {
                                await ShowConfirmConfigAsync(configModel);
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show("Can Not Use Config File\n" + exception.Message);
                    }
                }
            }

        }
        #endregion

        #region Finger Print Listener
        private async Task InitFingerPrintListener()
        {
            try
            {
                _fingerPrintHelper?.Dispose();

                if (!File.Exists(App.FingerPrintConfigFilePath))
                {
                    _fingerPrintHelper = new FingerPrintHelper(dataReceivedAction: FingerPrintDataReceived);
                }
                else
                {
                    var fingerConfigContent = await File.ReadAllTextAsync(App.FingerPrintConfigFilePath);
                    SerialPortConfigModel fingerConfigModel = null;
                    try
                    {
                        fingerConfigModel = JsonSerializer.Deserialize<SerialPortConfigModel>(fingerConfigContent);
                    }
                    catch (Exception e)
                    {
                        App.AddLog(e);
                        _fingerPrintHelper = new FingerPrintHelper(dataReceivedAction: FingerPrintDataReceived);
                    }

                    _fingerPrintHelper = fingerConfigModel == null ?
                        new FingerPrintHelper(dataReceivedAction: FingerPrintDataReceived) :
                        new FingerPrintHelper(fingerConfigModel.DataBits, fingerConfigModel.Parity, fingerConfigModel.StopBits, fingerConfigModel.BaudRate, fingerConfigModel.PortName, FingerPrintDataReceived);
                }
            }
            catch (Exception e)
            {
                App.AddLog(e);
            }

        }

        private async void FingerPrintDataReceived(uint obj)
        {
            if (obj != uint.MaxValue)
            {
                ShowBorder(brdRfId, true);
                await CheckReservation(obj.ToString());
            }
            else
            {
                ShowBorder(brdRfId, false);
            }
        }

        #endregion

        #region RfId Listener
        private void InitRfIdListener()
        {

        }
        #endregion

        private async Task SendMonitorData(string dataStr)
        {
            if (string.IsNullOrWhiteSpace(dataStr)) return;
            try
            {
                if (_monitorHelper == null)
                {
                    if (!File.Exists(App.MonitorConfigFilePath))
                    {
                        _monitorHelper = new MonitorHelper();
                    }
                    else
                    {
                        var monitorConfigContent = await File.ReadAllTextAsync(App.MonitorConfigFilePath);
                        SerialPortConfigModel monitorConfigModel = null;
                        try
                        {
                            monitorConfigModel = JsonSerializer.Deserialize<SerialPortConfigModel>(monitorConfigContent);
                        }
                        catch (Exception e)
                        {
                            App.AddLog(e);
                            _monitorHelper = new MonitorHelper();
                        }

                        _monitorHelper = monitorConfigModel == null ?
                            new MonitorHelper() :
                            new MonitorHelper(monitorConfigModel.DataBits, monitorConfigModel.Parity, monitorConfigModel.StopBits,
                                monitorConfigModel.BaudRate, monitorConfigModel.PortName);
                    }
                }

                _monitorHelper.SendDate(dataStr);
            }
            catch (Exception e)
            {
                App.AddLog(e);
            }
        }

        private async Task CheckReservation(string personnelNumber)
        {
            if (App.AppConfig.CheckOnline)
            {
                var res = await ApiClient.Restrn_Queue_Have_Reserve_Fun(new RESTRN_QUEUE_HAVE_RESERVE_FUN_Input_Data() { Device_Cod = App.AppConfig.Device_Cod,Num_Prsn = personnelNumber});
                if (res != null)
                {
                    if (res.IsSuccessFull)
                    {
                        //TODO Different Type Of Data
                        return;
                    }
                    else
                    {
                        //TODO How To Show Message?
                        return;
                    }
                }
            }
            var offlineReserve = await _reservationService.FindReservationAsync(personnelNumber,App.CurrentMealCode,DateTime.Now.Date.ToServerDateFormat());

            if(offlineReserve != null)
            {
                //TODO Add To Q And Display
            }
            else
            {
                //TODO How To Show Message?
            }
        }

        
    }
}