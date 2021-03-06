using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ApiWrapper;
using ApiWrapper.Dto;
using DataLayer.Dtos;
using DataLayer.Services;
using DNTPersianUtils.Core;
using IPAClient.Models;
using IPAClient.Tools;

namespace IPAClient.Windows
{
    /// <summary>
    /// Interaction logic for wndReport.xaml
    /// </summary>
    public partial class wndReport : Window
    {
        private readonly ReservationService _reservationService;
        public SerialBusHelper _serialBusHelper;
        public wndReport(SerialBusHelper serialBusHelper)
        {
            InitializeComponent();
            _serialBusHelper = serialBusHelper;
            _reservationService = new ReservationService();
        }

        private async void WndReport_OnLoaded(object sender, RoutedEventArgs e)
        {
            await FillGrid();
            lblDbPath.Content = "Database File Address: " + _reservationService.GetDbPath();
        }


        private async void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            SetMessage("در حال تهیه گزارش. لطفا منتظر بمانید");
            await FillGrid();
            ClearMessage();
        }

        private async void btnSend_Click(object sender, RoutedEventArgs e)
        {
            await SendDeliveredReservationsToServer(_reservationService, setMessageAction: SetMessage, sendMessageToSerialPortAction: SendMessageToSerialPort);
            await FillGrid();
        }

        private async void btnReceive_Click(object sender, RoutedEventArgs e)
        {
            await GetAllMealsReservationsFromServer();
            await FillGrid();
        }

        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            await DeleteAllSent();
            await FillGrid();
        }

        private async void BtnDeleteAll_OnClick(object sender, RoutedEventArgs e)
        {
            await DeleteAll();
            await FillGrid();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }


        public static async Task SendDeliveredReservationsToServer(ReservationService reservationService, int deliveredTilNow = 0, Action<string> setMessageAction = null, Action<string> sendMessageToSerialPortAction = null)
        {
            try
            {
                setMessageAction?.Invoke("در حال ارسال رزرو های تحویل داده شده به سرور. لطفا منتظر بمانید");
                var reservesToSend = await reservationService.GetDeliveredReservesToSendAsync();
                if (reservesToSend is not { Count: > 0 })
                {
                    if (deliveredTilNow > 0)
                    {
                        var message = $"{deliveredTilNow} رزرو در {DateTime.Now.ToLongPersianDateTimeString()} به سرور ارسال شد";
                        setMessageAction?.Invoke(message);
                        sendMessageToSerialPortAction?.Invoke(message);
                    }
                    else
                    {
                        var message = "هیچ رزروی برای تحویل وجود ندارد";
                        setMessageAction?.Invoke(message);
                        sendMessageToSerialPortAction?.Invoke(message);
                    }
                    return;
                }

                var syncResult = await ApiClient.MainInfo_Synchronize_Data_Fun(new MainInfo_Synchronize_Data_Fun_Input(reservesToSend
                    .Select(x => new MainInfo_Synchronize_Data_Fun_Input_Data(App.AppConfig.Device_Cod, x.Reciver_Coupon_Id, x.Status, x.Date_Use, x.Time_Use)).ToList()));

                if (!syncResult.isSuccessful)
                {
                    var message = "خطا در ارسال رزرو های تحویل داده شده به سرور" + Environment.NewLine + syncResult.message;
                    setMessageAction?.Invoke(message);
                    sendMessageToSerialPortAction?.Invoke(message);
                    App.AddLog(new Exception(syncResult.message));
                    return;
                }

                await reservationService.SetSentToWebServiceDateTimeAsync(reservesToSend.Select(x => x.Id));
                deliveredTilNow += reservesToSend.Count;
                await SendDeliveredReservationsToServer(reservationService, deliveredTilNow, setMessageAction, sendMessageToSerialPortAction);

            }
            catch (Exception e)
            {
                var message = "خطا در ارسال رزرو های تحویل داده شده به سرور" + Environment.NewLine + e.Message;
                setMessageAction?.Invoke(message);
                sendMessageToSerialPortAction?.Invoke(message);
                App.AddLog(e);
            }
        }

        private async Task GetAllMealsReservationsFromServer()
        {
            SetMessage("در حال دریافت رزرو های آفلاین از سرور. لطفا منتظر بمانید");
            try
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
                            .Select(x => MainWindow.MapToReservation(x, meal.Cod_Data)).ToList());

                        var minMealTime = reservationDate.Min(x => x.Num_Tim_Str_Meal_Rsmls).ToTimeSpan();
                        var maxMealTime = reservationDate.Max(x => x.Num_Tim_End_Meal_Rsmls).ToTimeSpan();

                        meal.StartTime = minMealTime;
                        meal.EndTime = maxMealTime;
                    }
                    SetMessage("رزرو های آفلاین از سرور دریافت شد");
                }
                else
                {
                    SetMessage("اطلاعات پایه و وعده های غذایی موجود نیست");
                }
            }
            catch (Exception e)
            {
                SetMessage("خطا در دریافت رزرو های آفلاین از سرور" + Environment.NewLine + e.Message);
                App.AddLog(e);
            }

        }

        private async Task DeleteAllSent()
        {
            SetMessage("در حال پاک سازی رزرو های ارسال شده به سرور از پایگاه داده. لطفا منتظر بمانید");
            try
            {
                await _reservationService.DeleteAllSendAsync();
                SetMessage("رزرو های ارسال شده به سرور از پایگاه داده پاک سازی شد");
            }
            catch (Exception e)
            {
                SetMessage("خطا در پاک سازی رزرو های ارسال شده به سرور از پایگاه داده" + Environment.NewLine + e.Message);
                App.AddLog(e);
            }

        }

        private async Task DeleteAll()
        {
            SetMessage("در حال پاک سازی رزرو ها از پایگاه داده. لطفا منتظر بمانید");
            try
            {
                await _reservationService.DeleteAllAsync();
                SetMessage("رزرو ها از پایگاه داده پاک سازی شد");
            }
            catch (Exception e)
            {
                SetMessage("خطا در پاک سازی رزرو ها از پایگاه داده" + Environment.NewLine + e.Message);
                App.AddLog(e);
            }

        }
        private async ValueTask FillGrid()
        {
            grdReport.ItemsSource = await _reservationService.GetReportAsync();

        }


        private void SetMessage(string message)
        {
            lblMessage.Content = message;
        }

        private void ClearMessage()
        {
            lblMessage.Content = string.Empty;
        }


        private void BtnReset_OnClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("آیا از راه اندازی مجدد سیستم اطمینان دارید؟", "Restart System", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No, MessageBoxOptions.RightAlign | MessageBoxOptions.ServiceNotification) == MessageBoxResult.Yes)
            {
                SendCommandToSerialPort("10");
                if (MessageBox.Show("دستور راه اندازی مجدد به زیر سیستم ها ارسال شد" + Environment.NewLine +
                                    "پس از اطمینان  از اعمال دستور این پیام را تایید نمایید", "Restart System", MessageBoxButton.OKCancel, MessageBoxImage.Asterisk, MessageBoxResult.Cancel, MessageBoxOptions.RightAlign | MessageBoxOptions.ServiceNotification) == MessageBoxResult.OK)
                {
                    ShutdownHelper.Restart();
                }
            }
        }

        private void BtnShutDown_OnClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("آیا از خاموش شدن سیستم اطمینان دارید؟", "ShutDown System", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No, MessageBoxOptions.RightAlign | MessageBoxOptions.ServiceNotification) == MessageBoxResult.Yes)
            {
                SendCommandToSerialPort("20");
                if (MessageBox.Show("دستور خاموش شدن به زیر سیستم ها ارسال شد" + Environment.NewLine +
                                    "پس از اطمینان  از اعمال دستور این پیام را تایید نمایید", "ShutDown System", MessageBoxButton.OKCancel, MessageBoxImage.Asterisk, MessageBoxResult.Cancel, MessageBoxOptions.ServiceNotification) == MessageBoxResult.OK)
                {
                    ShutdownHelper.Shut();
                }
            }
        }

        private void SendCommandToSerialPort(string command)
        {
            if (!App.AppConfig.HasExtraMonitors || string.IsNullOrWhiteSpace(command) || _serialBusHelper == null) return;
            try
            {
                var dto = new MonitorDto();
                dto.SetCommand(command);
                _serialBusHelper.SendDate(dto.ToJson());
            }
            catch (Exception e)
            {
                App.AddLog(e);
            }
        }

        private void SendMessageToSerialPort(string message)
        {
            if (!App.AppConfig.HasExtraMonitors || string.IsNullOrWhiteSpace(message) || _serialBusHelper == null) return;
            try
            {
                var dto = new MonitorDto();
                dto.AddMessageToQueue("پیام سیستم", message, "ارسال گزارش تحویل غذا");
                _serialBusHelper.SendDate(dto.ToJson());
            }
            catch (Exception e)
            {
                App.AddLog(e);
            }
        }

    }
}
