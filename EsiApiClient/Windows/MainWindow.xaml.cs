using System;
using System.CodeDom;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Media;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ApiWrapper;
using ApiWrapper.Dto;
using DataLayer.Entities;
using DataLayer.Services;
using IPAClient.Models;
using IPAClient.Tools;

namespace IPAClient.Windows
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ReservationService _reservationService;
        private DispatcherTimer _borderTimer;
        private FingerPrintHelper _fingerPrintHelper;
        private DispatcherTimer _labelTimer;
        private DispatcherTimer _mainTimer;
        private MonitorDto _monitorDto;
        private MonitorHelper _monitorHelper;
        private RfidHelper _rfIdHelper;
        private wndCommandOne _wndCommandOne;
        private wndCommandTwo _wndCommandTwo;

        public MainWindow()
        {
            InitializeComponent();
            _reservationService = new ReservationService();
            InitBorderTimer();
            InitLabelTimer();
            ClearLabels();
        }

        private void BorderTimerOnTick(object sender, EventArgs e)
        {
            brdFingerPrint.Visibility = Visibility.Collapsed;
            brdRfId.Visibility = Visibility.Collapsed;
            brdKeyPad.Visibility = Visibility.Collapsed;
            lblError.Content = string.Empty;
            brdNoReserve.Visibility = Visibility.Collapsed;
            lblNumber.Content = string.Empty;
            _borderTimer.Stop();
        }

        private void BtnClose_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private async void btnInfo_Click(object sender, RoutedEventArgs e)
        {
            await ShowConfirmConfigAsync(App.AppConfig);
        }

        private async void BtnKeyPad_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            ClearLabels();
            if (App.IsActive || !App.AppConfig.CheckMealTime)
            {
                var wndKeyPad = new wndKeyPad();
                wndKeyPad.ShowDialog();
                if (!string.IsNullOrWhiteSpace(wndKeyPad.PersonnelNumber))
                {
                    ShowBorder(brdKeyPad, true);
                    await CheckReservation(wndKeyPad.PersonnelNumber);
                }
            }
            else
            {
                var message = "خارج از وعده";
                ShowBorder(brdNoReserve, false);
                ShowError(message);
                _monitorDto.AddMessageToQueue("", message);
            }
        }

        //private async void CheckTimeForAutoUpdate(object sender, EventArgs e)
        //{
        //    // در هر روز همه ی اطلاعات بروز میشود
        //    if (App.LastFullUpdateTime == null || App.LastFullUpdateTime.Value.IsNextDay())
        //    {
        //        await GetConfigFromServerAsync();
        //    }
        //    //هر 30 دقیقه اطلاعات رزرو وعده فعلی بروز میشود
        //    else if (App.LastMealUpdateTime == null || App.LastMealUpdateTime.Value.IsMinutePassed(30))
        //    {
        //        await UpdateCurrentMealReservationFromServer();
        //    }

        //    App.CurrentMealCode = App.MainInfo?.Meals?.FirstOrDefault(x => x.IsCurrentMeal)?.Cod_Data;
        //}

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
                    ApiClient.SetAuthToken(string.IsNullOrWhiteSpace(configModel.WebServiceAuthToken)
                        ? "Basic TkFNRk9PRHVzZXIxOjUxZjIzMDQxYWNkOGRmNzlkMWIxOGY2ZjE2ZWE4YzM2"
                        : configModel.WebServiceAuthToken);
                    ApiClient.SetBaseUrl(string.IsNullOrWhiteSpace(configModel.WebServiceUrl)
                        ? "http://eis.msc.ir/"
                        : configModel.WebServiceUrl);
                    if (!App.AppConfig.IsDemo)
                        await GetConfigFromServerAsync();
                    //Commented Because Of Uknown Error That Says We Should Install .Net 4 !!!
                    //InitReConfigListener();
                    else
                        SetBackGroundImage("21");

                    ClearLabels();
                    await InitFingerPrintListener();
                    await InitRfIdListener();
                    InitAndStartMainTimer();
                }
            }
        }

        private async Task CheckReservation(string personnelNumber)
        {
            if (App.AppConfig.CheckOnline)
            {
                var res = await ApiClient.Restrn_Queue_Have_Reserve_Fun(new RESTRN_QUEUE_HAVE_RESERVE_FUN_Input_Data { Device_Cod = App.AppConfig.Device_Cod, Num_Prsn = personnelNumber });
                if (res != null)
                {
                    if (res.IsSuccessFull)
                    {
                        var onlineReserve = res.Data?.FirstOrDefault();
                        if (onlineReserve != null)
                        {
                            var offlineReserve = await _reservationService.FindReservationByCouponIdAsync(onlineReserve.Reciver_Coupon_Id, DateTime.Now.Date.ToServerDateFormat());
                            if (offlineReserve != null)
                            {
                                await OfflineReserveSuccessOperation(offlineReserve);
                                return;
                            }
                            else
                            {
                                //TODO InsertOnlineReserveAsync And Update Labels By RESTRN_QUEUE_HAVE_RESERVE_FUN_Output_Data
                                //await _reservationService.InsertOnlineReserveAsync()
                                //PlaySound(true);
                                //return;
                            }
                        }

                    };

                    var message = "رزرو آنلاین یافت نشد";
                    ShowError(message);
                    PlaySound(false);
                    _monitorDto.AddMessageToQueue(personnelNumber, message);
                    return;
                }
            }
            else
            {
                var offlineReserve = await _reservationService.FindReservationAsync(personnelNumber, App.CurrentMealCode, DateTime.Now.Date.ToServerDateFormat());

                if (offlineReserve != null)
                {
                    await OfflineReserveSuccessOperation(offlineReserve);
                }
                else
                {
                    
                    var message = "رزرو یافت نشد";
                    ShowError(message);
                    PlaySound(false);
                    _monitorDto.AddMessageToQueue(personnelNumber, message);
                }
            }

            await SendMonitorData(_monitorDto.ToJson());
        }

        private async Task OfflineReserveSuccessOperation(Reservation reservation)
        {
            if (string.IsNullOrWhiteSpace(reservation.Status))
            {
                UpdateLabels(reservation);
                PlaySound(true);
                _monitorDto.AddToQueue(reservation);
                await SetRemainFoods();
            }
            else
            {
                var message = "تحویل داده شده است " + reservation.Time_Use.ToFriendlyTimeFormat();
                ShowError(message);
                PlaySound(false);
                _monitorDto.AddMessageToQueue(reservation.Num_Ide, message);
            }
        }

        private void ClearLabels()
        {
            Dispatcher.Invoke(() =>
            {
                lblError.Content = lblName.Content =
                    lblNumber.Content = lblVade.Content =
                        lblShift.Content = lblShiftCompany.Content =
                              lblFoodNum.Content =
                                lblAppFoodName0.Content = lblAppFoodNum0.Content =
                                    lblAppFoodName1.Content = lblAppFoodNum1.Content =
                                        lblAppFoodName2.Content = lblAppFoodNum2.Content =
                                        lblAppFoodName3.Content = lblAppFoodNum3.Content =
                                        lblAppFoodName4.Content = lblAppFoodNum4.Content =
                                            string.Empty;
                lblFoodName.Text = string.Empty;
                brdFood.Visibility = Visibility.Collapsed;
                if (_labelTimer.IsEnabled) _labelTimer.Stop();
            });
        }

        private async void FingerPrintDataReceived(uint obj)
        {
            ClearLabels();
            await Dispatcher.Invoke(async () =>
            {
                if (!IsActive) return;
                try
                {
                    if (App.IsActive || !App.AppConfig.CheckMealTime)
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
                    else
                    {
                        var message = "خارج از وعده";
                        ShowError(message);
                        ShowBorder(brdNoReserve, false);
                        _monitorDto.AddMessageToQueue("", message);
                    }
                    //await CheckReservation("919633");
                }
                catch (Exception e)
                {
                    App.AddLog(e);
                }
            }, DispatcherPriority.Normal);
        }

        private async Task GetAllMealsReservationsFromServer()
        {
            if (App.MainInfo?.Meals != null && App.MainInfo.Meals.Any())
            {
                foreach (var meal in App.MainInfo.Meals)
                {
                    var reservationDate =
                        await ApiClient.MainInfo_Send_Offline_Data_Fun(
                            new MainInfo_Send_Offline_Data_Fun_Input_Data(App.AppConfig.Device_Cod,
                                DateTime.Now.ToServerDateFormat(), meal.Cod_Data));

                    await _reservationService.InsertAsync(reservationDate
                        .Select(x => MapToReservation(x, meal.Cod_Data)).ToList());

                    var minMealTime = reservationDate.Min(x => x.Num_Tim_Str_Meal_Rsmls).ToTimeSpan();
                    var maxMealTime = reservationDate.Max(x => x.Num_Tim_End_Meal_Rsmls).ToTimeSpan();

                    meal.StartTime = minMealTime;
                    meal.EndTime = maxMealTime;
                }

                await SetCurrentMeal();
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

                //تنظیم ساعت سیستم بر اساس ساعت سرور
                var serverDateTime = await ApiClient.GetServerDateTime();
                if (serverDateTime.HasValue)
                {
                    SystemTimeHelper.SetSystemTime(serverDateTime.Value);
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
                    await _reservationService.DeleteAllSendByDayAsync(2);
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

        private void InitAndStartMainTimer()
        {
            _mainTimer = new DispatcherTimer();
            _mainTimer.Tick += MainTimerOnTick;
            _mainTimer.Interval = new TimeSpan(0, 1, 0);
            _mainTimer.Start();
        }

        private void InitBorderTimer()
        {
            _borderTimer = new DispatcherTimer();
            _borderTimer.Tick += BorderTimerOnTick;
            _borderTimer.Interval = new TimeSpan(0, 0, 5);
        }

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

                    _fingerPrintHelper = fingerConfigModel == null
                        ? new FingerPrintHelper(dataReceivedAction: FingerPrintDataReceived)
                        : new FingerPrintHelper(fingerConfigModel.DataBits, fingerConfigModel.Parity,
                            fingerConfigModel.StopBits, fingerConfigModel.BaudRate, fingerConfigModel.PortName,
                            FingerPrintDataReceived);
                }
            }
            catch (Exception e)
            {
                App.AddLog(e);
            }
        }

        private void InitLabelTimer()
        {
            _labelTimer = new DispatcherTimer();
            _labelTimer.Tick += LabelTimerOnTick;
            _labelTimer.Interval = new TimeSpan(0, 0, 20);
        }

        private void InitReConfigListener()
        {
            var watcher = new ManagementEventWatcher();
            var query = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2");
            watcher.EventArrived += Watcher_EventArrived;
            watcher.Query = query;
            watcher.Start();
        }

        private async Task InitRfIdListener()
        {
            try
            {
                _rfIdHelper?.Dispose();

                if (!File.Exists(App.RfIdConfigFilePath))
                {
                    _rfIdHelper = new RfidHelper(dataReceivedAction: RfIdDataReceivedAction);
                }
                else
                {
                    var rfIdConfigContent = await File.ReadAllTextAsync(App.RfIdConfigFilePath);
                    SerialPortConfigModel rfIdConfigModel = null;
                    try
                    {
                        rfIdConfigModel = JsonSerializer.Deserialize<SerialPortConfigModel>(rfIdConfigContent);
                    }
                    catch (Exception e)
                    {
                        App.AddLog(e);
                        _rfIdHelper = new RfidHelper(dataReceivedAction: RfIdDataReceivedAction);
                    }

                    _rfIdHelper = rfIdConfigModel == null
                        ? new RfidHelper(dataReceivedAction: RfIdDataReceivedAction)
                        : new RfidHelper(rfIdConfigModel.DataBits, rfIdConfigModel.Parity, rfIdConfigModel.StopBits,
                            rfIdConfigModel.BaudRate, rfIdConfigModel.PortName, RfIdDataReceivedAction);
                }
            }
            catch (Exception e)
            {
                App.AddLog(e);
            }
        }

        private void LabelTimerOnTick(object sender, EventArgs e)
        {
            ClearLabels();
        }

        private async void MainTimerOnTick(object sender, EventArgs e)
        {
            await Dispatcher.Invoke(async () =>
            {
                if (!IsActive) return;
                // در هر روز همه ی اطلاعات بروز میشود
                if (App.LastFullUpdateTime == null || App.LastFullUpdateTime.Value.IsNextDay())
                    await GetConfigFromServerAsync();
                //هر App.AppConfig.GetFromServerIntervalMinutes دقیقه اطلاعات رزرو وعده فعلی بروز میشود
                else if (App.LastMealUpdateTime == null ||
                         App.LastMealUpdateTime.Value.IsMinutePassed(App.AppConfig.GetFromServerIntervalMinutes))
                    await UpdateCurrentMealReservationFromServer();

                await SetCurrentMeal();
                UpdateDateLabel();

                await SendMonitorData(_monitorDto.ToJson());
            });
        }

        private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            _monitorDto = new MonitorDto();
            await CheckConfigStatusAndInitUtilitiesAsync();
            //در زمان هایی که برنامه مشغول کار دیگری نیست اجرا میشود
            //ComponentDispatcher.ThreadIdle += new EventHandler(CheckTimeForAutoUpdate);
            //InitilizeRecheckTimer();
        }

        public static Reservation MapToReservation(MainInfo_Send_Offline_Data_Fun_Output_Data input, string mealCode)
        {
            var res = new Reservation
            {
                Id = Guid.NewGuid(),
                Date = DateTime.Now.ToString(CultureInfo.InvariantCulture),
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
                Reciver_Coupon_Id = input.Reciver_Coupon_Id
            };

            foreach (var mainFood in input.Main_Course)
                res.Foods.Add(
                    new Food
                    {
                        IsMain = true,
                        ReservationId = res.Id,

                        Des_Food = mainFood.Des_Food,
                        Num_Amount = mainFood.Num_Amount.ToString(),
                        Typ_Serv_Unit = mainFood.Typ_Serv_Unit
                    });

            foreach (var appFood in input.Appetizer_Dessert)
                res.Foods.Add(
                    new Food
                    {
                        IsMain = false,
                        ReservationId = res.Id,

                        Des_Food = appFood.Des_Food,
                        Num_Amount = appFood.Num_Amount,
                        Typ_Serv_Unit = appFood.Typ_Serv_Unit
                    });

            return res;
        }

        private async Task MonitorCommand1()
        {
            await Dispatcher.Invoke(async () =>
            {
                if (_wndCommandOne == null)
                {
                    _monitorDto.SetCommand("1");
                    await SendMonitorData(_monitorDto.ToJson());
                    _wndCommandOne = new wndCommandOne();
                    _wndCommandOne.Show();
                }
                else
                {
                    _monitorDto.SetCommand(string.Empty);
                    await SendMonitorData(_monitorDto.ToJson());
                    _wndCommandOne.Close();
                    _wndCommandOne = null;
                }
            }, DispatcherPriority.Normal);
        }

        private async Task MonitorCommand2()
        {
            await Dispatcher.Invoke(async () =>
            {
                if (_wndCommandTwo == null)
                {
                    _monitorDto.SetCommand("2");
                    await SendMonitorData(_monitorDto.ToJson());
                    _wndCommandTwo = new wndCommandTwo();
                    _wndCommandTwo.Show();
                }
                else
                {
                    _monitorDto.SetCommand(string.Empty);
                    await SendMonitorData(_monitorDto.ToJson());
                    _wndCommandTwo.Close();
                    _wndCommandTwo = null;
                }
            }, DispatcherPriority.Normal);
        }

        private void PlaySound(bool isOk)
        {
            Task.Factory.StartNew(() =>
            {
                var player = new SoundPlayer(isOk ? "Sounds/ok.wav" : "Sounds/NotOk.wav");
                player.Load();
                player.Play();
            });
        }

        private async void RfIdDataReceivedAction(uint personnelNumber, bool isActive, bool isExp, string expDate)
        {
            ClearLabels();
            await Dispatcher.Invoke(async () =>
            {
                _rfIdHelper.SetIsBusy(true);
                try
                {
                    if (App.IsActive || !App.AppConfig.CheckMealTime)
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
                                PlaySound(false);
                                _monitorDto.AddMessageToQueue(pNumStr, "کارت غیر فعال است");
                                await SendMonitorData(_monitorDto.ToJson());
                                return;
                            }

                            if (isExp)
                            {
                                
                                var message = "کارت منقضی شده است " + expDate;
                                ShowError(message);
                                ShowBorder(brdRfId, false);
                                PlaySound(false);
                                _monitorDto.AddMessageToQueue(pNumStr, message);
                                await SendMonitorData(_monitorDto.ToJson());
                                return;
                            }

                            ShowBorder(brdRfId, true);
                            await CheckReservation(pNumStr);
                        }
                    }
                    else
                    {
                        var message = "خارج از وعده";
                        ShowError(message);
                        ShowBorder(brdNoReserve, false);
                        _monitorDto.AddMessageToQueue("", message);
                    }
                }
                catch (Exception ex)
                {
                    App.AddLog(ex);
                }
                finally
                {
                    _rfIdHelper.SetIsBusy(false);
                }
            }, DispatcherPriority.Normal);
        }

        private async Task SendMonitorData(string dataStr)
        {
            if (!App.AppConfig.HasExtraMonitors || string.IsNullOrWhiteSpace(dataStr)) return;
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
                            monitorConfigModel =
                                JsonSerializer.Deserialize<SerialPortConfigModel>(monitorConfigContent);
                        }
                        catch (Exception e)
                        {
                            App.AddLog(e);
                            _monitorHelper =
                                new MonitorHelper(commandOne: MonitorCommand1, commandTwo: MonitorCommand2);
                        }

                        _monitorHelper = monitorConfigModel == null
                            ? new MonitorHelper(commandOne: MonitorCommand1, commandTwo: MonitorCommand2)
                            : new MonitorHelper(monitorConfigModel.DataBits, monitorConfigModel.Parity,
                                monitorConfigModel.StopBits,
                                monitorConfigModel.BaudRate, monitorConfigModel.PortName, MonitorCommand1,
                                MonitorCommand2);
                    }
                }

                _monitorHelper.SendDate(dataStr);
            }
            catch (Exception e)
            {
                App.AddLog(e);
            }
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

        private async Task SetCurrentMeal()
        {
            var activeMeal = App.MainInfo?.Meals?.FirstOrDefault(x => x.IsCurrentMeal);
            var activeMealCode = activeMeal?.Cod_Data ?? ((App.AppConfig?.CheckMealTime ?? false) ? null : "22");
            if (App.CurrentMealCode != activeMealCode)
            {
                App.CurrentMealCode = activeMealCode;
                if (string.IsNullOrWhiteSpace(activeMealCode))
                {
                    App.IsActive = false;
                    _monitorDto?.Clear();
                }
                else
                {
                    App.IsActive = true;
                    await SetRemainFoods();
                }
            }

            if (!App.IsActive)
            {
#pragma warning disable CS4014
                //DO NOT NEED await 
                //بعد از اتمام زمان هر وعده اطلاعات تحویل غذا را به وب سرویس ارسال میکند
                Task.Factory.StartNew(SendDeliveredReservationsToServer);
#pragma warning restore CS4014
            }


            if (_monitorDto != null)
            {
                if (activeMeal is { StartTime: { }, EndTime: { } })
                    _monitorDto.CurrentMealRemainTime = activeMeal.StartTime.Value - activeMeal.EndTime.Value;
                else
                    _monitorDto.CurrentMealRemainTime = new TimeSpan(0, 0, 0);
            }
        }

        private async Task SendDeliveredReservationsToServer()
        {
            if (!await _reservationService.HasAnyNotSentToServer()) return;

            var reservesToSend = await _reservationService.GetDeliveredReservesToSendAsync();
            if (reservesToSend is not { Count: > 0 }) return;

            var syncResult = await ApiClient.MainInfo_Synchronize_Data_Fun(new MainInfo_Synchronize_Data_Fun_Input(reservesToSend
                .Select(x => new MainInfo_Synchronize_Data_Fun_Input_Data(App.AppConfig.Device_Cod, x.Reciver_Coupon_Id, x.Status, x.Date_Use, x.Time_Use)).ToList()));

            if (!syncResult.isSuccessful)
            {
                App.AddLog(new Exception(syncResult.message));
            }
            else
            {
                await _reservationService.SetSentToWebServiceDateTimeAsync(reservesToSend.Select(x => x.Id));
            }

            await SendDeliveredReservationsToServer();
        }

        private async Task SetRemainFoods()
        {
            if (_monitorDto is { })
            {
                var remainFood =
              await _reservationService.GetMealFoodRemain(DateTime.Now.ToServerDateFormat(), App.CurrentMealCode);
                _monitorDto.InsertOrUpdateRemainFood(remainFood.Select(x => new RemainFoodModel(x.Title, x.Remain, x.Total))
                    .ToArray());
            }
        }

        private void ShowBorder(Border brd, bool isSuccess)
        {
            Dispatcher.Invoke(() =>
            {
                brd.BorderBrush = new SolidColorBrush(isSuccess ? Color.FromRgb(0, 255, 0) : Color.FromRgb(255, 0, 0));
                brd.Visibility = Visibility.Visible;
                _borderTimer.Start();
            }, DispatcherPriority.Normal);
        }

        private async Task ShowConfirmConfigAsync(ConfigModel configModel)
        {
            _ = new wndConfirmConfig(configModel, _rfIdHelper?.IsConnected ?? false,
                _fingerPrintHelper?.IsConnected ?? false).ShowDialog();
            await CheckConfigStatusAndInitUtilitiesAsync();
        }

        private void ShowError(string error)
        {
            Dispatcher.Invoke(() =>
            {
                lblError.Content = error;
                brdNoReserve.Visibility = Visibility.Visible;
            }, DispatcherPriority.Normal);
        }

        private async Task ShowNeedConfigAsync()
        {
            _ = new wndNeedConfig().ShowDialog();
            await CheckConfigStatusAndInitUtilitiesAsync();
        }

        private async Task UpdateCurrentMealReservationFromServer()
        {
            var reservationDate = await ApiClient.MainInfo_Send_Offline_Data_Fun(
                new MainInfo_Send_Offline_Data_Fun_Input_Data(App.AppConfig.Device_Cod,
                    DateTime.Now.ToServerDateFormat(), App.CurrentMealCode));
            await _reservationService.InsertAsync(reservationDate.Select(x => MapToReservation(x, App.CurrentMealCode))
                .ToList());
            App.LastMealUpdateTime = DateTime.Now;
        }

        private void UpdateDateLabel()
        {
            lblDate.Content = SystemTimeHelper.CurrentPersinaFullDate();
            lblTime.Content = DateTime.Now.ToString("HH:mm", new CultureInfo("fa-IR"));
        }

        private void UpdateLabels(Reservation reservation)
        {
            Dispatcher.Invoke(() =>
            {
                lblName.Content = reservation.First_Name_Ide + " " + reservation.Last_Name_Ide;
                lblNumber.Content = reservation.Num_Ide;
                lblVade.Content = reservation.Des_Nam_Meal;
                lblShift.Content = reservation.Employee_Shift_Name;
                lblShiftCompany.Content = reservation.Des_Contract_Order;
                var mainFood = reservation.Foods.FirstOrDefault(x => x.IsMain);
                if (mainFood != null)
                {
                    lblFoodName.Text = mainFood.Des_Food;
                    lblFoodNum.Content = mainFood.Num_Amount;
                }
                else
                {
                    lblFoodName.Text = "نامشخص";
                    lblFoodNum.Content = string.Empty;
                }

                var appFoods = reservation.Foods.Where(x => !x.IsMain).ToList();
                for (var i = 0; i < 5; i++)
                {
                    var appFood = appFoods.Skip(i).Take(1).FirstOrDefault();
                    if (appFood == null) break;
                    switch (i)
                    {
                        case 0:
                            lblAppFoodName0.Content = appFood.Des_Food;
                            lblAppFoodNum0.Content = appFood.Num_Amount;
                            break;
                        case 1:
                            lblAppFoodName1.Content = appFood.Des_Food;
                            lblAppFoodNum1.Content = appFood.Num_Amount;
                            break;
                        case 2:
                            lblAppFoodName2.Content = appFood.Des_Food;
                            lblAppFoodNum2.Content = appFood.Num_Amount;
                            break;
                        case 3:
                            lblAppFoodName3.Content = appFood.Des_Food;
                            lblAppFoodNum3.Content = appFood.Num_Amount;
                            break;
                        case 4:
                            lblAppFoodName4.Content = appFood.Des_Food;
                            lblAppFoodNum4.Content = appFood.Num_Amount;
                            break;
                    }
                }

                brdFood.Visibility = Visibility.Visible;
                if (!_labelTimer.IsEnabled) _labelTimer.Start();
            });
        }

        private async void Watcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            var driveNameProp = e.NewEvent.Properties["DriveName"];
            if (string.IsNullOrWhiteSpace(driveNameProp.Value.ToString())) return;
            var configFilePath = driveNameProp.Value + @"\reconfig.json";
            if (!File.Exists(configFilePath)) return;
            try
            {
                var configContent = await File.ReadAllTextAsync(configFilePath);
                if (string.IsNullOrWhiteSpace(configContent)) return;
                var configModel = JsonSerializer.Deserialize<ConfigModel>(configContent);
                if (configModel != null) await ShowConfirmConfigAsync(configModel);
            }
            catch (Exception exception)
            {
                MessageBox.Show("Can Not Use Config File\n" + exception.Message);
            }
        }

        private void BtnReport_OnClick(object sender, RoutedEventArgs e)
        {
            new wndReport().ShowDialog();
        }
    }
}