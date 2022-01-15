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
using System.Media;
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
        private FingerPrintHelper _fingerPrintHelper;
        private RfidHelper _rfidHelper;
        private MonitorHelper _monitorHelper;
        private MonitorDto monitorDto;
        private DispatcherTimer _borderTimer;

        private readonly ReservationService _reservationService;
        public MainWindow()
        {
            InitializeComponent();
            _reservationService = new ReservationService();
            SetLabelsVisible(false);
            _borderTimer = new DispatcherTimer();
            _borderTimer.Tick += BorderTimerOnTick;
            _borderTimer.Interval = new TimeSpan(0, 0, 5);
        }

        private void BorderTimerOnTick(object sender, EventArgs e)
        {
            brdFingerPrint.Visibility = Visibility.Collapsed;
            brdRfId.Visibility = Visibility.Collapsed;
            lblError.Content = string.Empty;
            brdNoReserve.Visibility = Visibility.Collapsed;
            lblNumber.Content = string.Empty;
            _borderTimer.IsEnabled = false;
        }

        private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            await CheckConfigStatusAndInitUtilitiesAsync();
            //در زمان هایی که برنامه مشغول کار دیگری نیست اجرا میشود
            ComponentDispatcher.ThreadIdle += new EventHandler(CheckTimeForAutoUpdate);
            monitorDto = new MonitorDto();
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
            //ClearLabels();
            //HideBorder(brdFingerPrint);
            //HideBorder(brdRfId);
            App.CurrentMealCode = App.MainInfo?.Meals?.FirstOrDefault(x => x.IsCurrentMeal)?.Cod_Data;
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
                    await InitRfIdListener();

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
                        await File.WriteAllTextAsync(App.MainInfoFilePath, JsonSerializer.Serialize(mainInfo));
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

                    await _reservationService.InsertAsync(reservationDate.Select(x => MapToReservation(x, meal.Cod_Data)).ToList());

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
            var reservationDate = await ApiClient.MainInfo_Send_Offline_Data_Fun(new MainInfo_Send_Offline_Data_Fun_Input_Data(App.AppConfig.Device_Cod, DateTime.Now.ToServerDateFormat(), App.CurrentMealCode));
            await _reservationService.InsertAsync(reservationDate.Select(x => MapToReservation(x, App.CurrentMealCode)).ToList());
            App.LastMealUpdateTime = DateTime.Now;
        }

        private Reservation MapToReservation(MainInfo_Send_Offline_Data_Fun_Output_Data input, string mealCode)
        {
            var res = new Reservation
            {
                Id = Guid.NewGuid(),
                Date = DateTime.Now.ToString(),
                Cod_Meal = mealCode,
                Date_Use = null,
                Status = null,
                Time_Use = null,


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

            foreach (var mainFood in input.Main_Course)
            {
                res.Foods.Add(
                    new Food
                    {
                        IsMain = true,
                        ReservationId = res.Id,

                        Des_Food = mainFood.Des_Food,
                        Num_Amount = mainFood.Num_Amount.ToString(),
                        Typ_Serv_Unit = mainFood.Typ_Serv_Unit
                    });
            }

            foreach (var appFood in input.Appetizer_Dessert)
            {
                res.Foods.Add(
                    new Food
                    {
                        IsMain = false,
                        ReservationId = res.Id,

                        Des_Food = appFood.Des_Food,
                        Num_Amount = appFood.Num_Amount,
                        Typ_Serv_Unit = appFood.Typ_Serv_Unit
                    });
            }

            return res;
        }

        #region ٌWindow Functions
        private async Task ShowNeedConfigAsync()
        {
            _ = new wndNeedConfig().ShowDialog();
            await CheckConfigStatusAndInitUtilitiesAsync();
        }

        private async Task ShowConfirmConfigAsync(ConfigModel configModel)
        {
            _ = new wndConfirmConfig(configModel, _rfidHelper?.IsConnected ?? false, _fingerPrintHelper?.IsConnected ?? false).ShowDialog();
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
            lblTime.Visibility = lblDate.Visibility = lblName.Visibility = lblNumber.Visibility = lblVade.Visibility = lblShift.Visibility = lblShiftCompany.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ClearLabels()
        {
            lblError.Content = lblName.Content = lblNumber.Content = lblVade.Content = lblShift.Content = lblShiftCompany.Content = string.Empty;
        }

        private void UpdateLabels(Reservation reservation)
        {
            lblName.Content = reservation.First_Name_Ide + " " + reservation.Last_Name_Ide;
            lblNumber.Content = reservation.Num_Ide;
            lblVade.Content = reservation.Des_Nam_Meal;
            lblShift.Content = reservation.Employee_Shift_Name;
            lblShiftCompany.Content = reservation.Des_Contract_Order;
        }



        private void ShowBorder(Border brd, bool isSuccess)
        {
            brd.BorderBrush = new SolidColorBrush(isSuccess ? Color.FromRgb(0, 255, 0) : Color.FromRgb(255, 0, 0));
            brd.Visibility = Visibility.Visible;

            PlaySound(isSuccess);

            if (!_borderTimer.IsEnabled)
            {
                _borderTimer.Start();
            }
        }

        private void UpdateDateLabel()
        {
            lblDate.Content = SystemTimeHelper.CurrentPersinaFullDate();
            lblTime.Content = DateTime.Now.ToString("HH:mm", new CultureInfo("fa-IR"));
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
            await Dispatcher.Invoke(async () =>
            {
                if (!IsActive) return;
                try
                {
                    if (obj != uint.MaxValue)
                    {
                        //حذف رقم اول کد خوانده شده
                        var objStr = obj.ToString();
                        var num = objStr.Substring(1, objStr.Length - 1);
                        lblNumber.Content = num.StartsWith("429496729") ? "ناشناس" : num;
                        ShowBorder(brdFingerPrint, true);
                        await CheckReservation(num);
                    }
                    else
                    {
                        ShowBorder(brdFingerPrint, false);
                    }
                }
                catch (Exception e)
                {
                    App.AddLog(e);
                }
            }, DispatcherPriority.Normal);
        }

        #endregion

        #region RfId Listener
        private async Task InitRfIdListener()
        {
            try
            {
                _rfidHelper?.Dispose();

                if (!File.Exists(App.RfIdConfigFilePath))
                {
                    _rfidHelper = new RfidHelper(dataReceivedAction: RfidDataReceivedAction);
                }
                else
                {
                    var rfidConfigContent = await File.ReadAllTextAsync(App.RfIdConfigFilePath);
                    SerialPortConfigModel rfidConfigModel = null;
                    try
                    {
                        rfidConfigModel = JsonSerializer.Deserialize<SerialPortConfigModel>(rfidConfigContent);
                    }
                    catch (Exception e)
                    {
                        App.AddLog(e);
                        _rfidHelper = new RfidHelper(dataReceivedAction: RfidDataReceivedAction);
                    }

                    _rfidHelper = rfidConfigModel == null ?
                        new RfidHelper(dataReceivedAction: RfidDataReceivedAction) :
                        new RfidHelper(rfidConfigModel.DataBits, rfidConfigModel.Parity, rfidConfigModel.StopBits, rfidConfigModel.BaudRate, rfidConfigModel.PortName, RfidDataReceivedAction);
                }
            }
            catch (Exception e)
            {
                App.AddLog(e);
            }

        }

        private async Task<bool> RfidDataReceivedAction(uint personnelNumber, bool isActive, bool isExp)
        {

            await Dispatcher.Invoke(async () =>
            {
                var pNumStr = personnelNumber.ToString();
                lblNumber.Content = pNumStr;
                if (!IsActive) return;
                if (personnelNumber != 0)
                {
                    if (!isActive)
                    {
                        ShowError("کارت غیر فعال است");
                        ShowBorder(brdRfId, false);
                        monitorDto.AddMessageToQueue(pNumStr, "کارت غیر فعال است");
                        await SendMonitorData(monitorDto.ToJson());
                        return;
                    }

                    if (isExp)
                    {
                        ShowError("کارت منقضی شده است");
                        ShowBorder(brdRfId, false);
                        monitorDto.AddMessageToQueue(pNumStr, "کارت غیر فعال است");
                        await SendMonitorData(monitorDto.ToJson());
                        return;
                    }

                    ShowBorder(brdRfId, true);
                    await CheckReservation(pNumStr);
                }
            }, DispatcherPriority.Normal);

            return true;
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
                        _monitorHelper = new MonitorHelper(commandOne: MonitorCommand1, commandTwo: MonitorCommand2);
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
                            _monitorHelper = new MonitorHelper(commandOne: MonitorCommand1, commandTwo: MonitorCommand2);
                        }

                        _monitorHelper = monitorConfigModel == null ?
                            new MonitorHelper(commandOne: MonitorCommand1, commandTwo: MonitorCommand2) :
                            new MonitorHelper(monitorConfigModel.DataBits, monitorConfigModel.Parity, monitorConfigModel.StopBits,
                                monitorConfigModel.BaudRate, monitorConfigModel.PortName, commandOne: MonitorCommand1, commandTwo: MonitorCommand2);
                    }
                }

                _monitorHelper.SendDate(dataStr);
            }
            catch (Exception e)
            {
                App.AddLog(e);
            }
        }
        wndCommandOne wndCommandOne = null;
        private async Task MonitorCommand1()
        {
            await Dispatcher.Invoke(async () =>
            {
                if (wndCommandOne == null)
                {
                    monitorDto.SetCommand("1");
                    await SendMonitorData(monitorDto.ToJson());
                    wndCommandOne = new wndCommandOne();
                    wndCommandOne.Show();
                }
                else
                {
                    monitorDto.SetCommand(string.Empty);
                    await SendMonitorData(monitorDto.ToJson());
                    wndCommandOne.Close();
                    wndCommandOne = null;
                }
            }, DispatcherPriority.Normal);
        }
        wndCommandTwo wndCommandTwo = null;
        private async Task MonitorCommand2()
        {
            await Dispatcher.Invoke(async () =>
           {
               if (wndCommandTwo == null)
               {
                   monitorDto.SetCommand("2");
                   await SendMonitorData(monitorDto.ToJson());
                   wndCommandTwo = new wndCommandTwo();
                   wndCommandTwo.Show();
               }
               else
               {
                   monitorDto.SetCommand(string.Empty);
                   await SendMonitorData(monitorDto.ToJson());
                   wndCommandTwo.Close();
                   wndCommandTwo = null;
               }
           }, DispatcherPriority.Normal);
        }

        private async Task CheckReservation(string personnelNumber)
        {


            if (App.AppConfig.CheckOnline)
            {
                var res = await ApiClient.Restrn_Queue_Have_Reserve_Fun(new RESTRN_QUEUE_HAVE_RESERVE_FUN_Input_Data() { Device_Cod = App.AppConfig.Device_Cod, Num_Prsn = personnelNumber });
                if (res != null)
                {
                    if (res.IsSuccessFull)
                    {
                        return;
                    }
                    else
                    {
                        //TODO How To Show Message?
                        ShowError("رزرو آنلاین یافت نشد");
                        monitorDto.AddMessageToQueue(personnelNumber, "رزرو آنلاین یافت نشد");
                        return;
                    }
                }
            }
            else
            {
                var offlineReserve = await _reservationService.FindReservationAsync(personnelNumber, App.CurrentMealCode, DateTime.Now.Date.ToServerDateFormat());

                if (offlineReserve != null)
                {
                    UpdateLabels(offlineReserve);
                    var remainFood = await _reservationService.GetMealFoodRemain(DateTime.Now.ToServerDateFormat(), App.CurrentMealCode);
                    monitorDto.InsertOrUpdateRemainFood(remainFood.Select(x => new RemainFoodModel(x.Title, x.Remain, x.Total)).ToArray());
                    monitorDto.AddToQueue(offlineReserve);

                }
                else
                {
                    ShowError("رزرو یافت نشد");
                    monitorDto.AddMessageToQueue(personnelNumber,"رزرو یافت نشد");
                    //TODO How To Show Message?
                }
            }
            await SendMonitorData(monitorDto.ToJson());
        }

        private void ShowError(string error)
        {
            Dispatcher.Invoke(() =>
          {
              lblError.Content = error;
              brdNoReserve.Visibility = Visibility.Visible;
          }, DispatcherPriority.Normal);

        }

        private void BtnClose_OnClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private async void btnInfo_Click(object sender, RoutedEventArgs e)
        {
            await ShowConfirmConfigAsync(App.AppConfig);
        }


        private void PlaySound(bool isOk)
        {
            Task.Factory.StartNew(() =>
            {
                SoundPlayer player = new SoundPlayer(isOk ? "Sounds/ok.wav" : "Sounds/NotOk.wav");
                player.Load();
                player.Play();
            });
        }
    }
}