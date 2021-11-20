using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using EsiApiClient.Api;
using EsiApiClient.Api.Dto;
using EsiApiClient.Models;

namespace EsiApiClient.Windows
{
    /// <summary>
    /// Interaction logic for wndCompleteConfig.xaml
    /// </summary>
    public partial class wndCompleteConfig : Window
    {
        private readonly ConfigModel _configModel;
        public wndCompleteConfig(ConfigModel configModel)
        {
            InitializeComponent();
            _configModel = configModel;
            lblError.Visibility = Visibility.Collapsed;

        }

        private async void SendRequest()
        {
            var res = await ApiClient.Maininfo_Register_Device_Fun(new MAININFO_REGISTER_DEVICE_FUN_Input(
                new MAININFO_REGISTER_DEVICE_FUN_Input_Data
                {
                    Device_Category = _configModel.Device_Category,
                    Device_Cod = _configModel.Device_Cod,
                    Device_Name = _configModel.Device_Name,
                    Device_Type = _configModel.Device_Type,
                    IP = _configModel.IP,
                    Num_Queue = _configModel.Num_Queue,
                    Restaurant_Cod = _configModel.Restaurant_Cod,
                }));
            var isSuccess = res?.MAININFO_REGISTER_DEVICE_FUN.Any(x => x.Message_Code == "WS300") ?? false;
            SetBackGroundImage(isSuccess);
            if (isSuccess)
            {
                _configModel.IsConfirmed = true;
                await File.WriteAllTextAsync(App.ConfigFilePath, JsonSerializer.Serialize(_configModel));
            }
            _ = new Timer(Callback, isSuccess, 3000, int.MaxValue);
        }

        private void Callback(object state)
        {
            Dispatcher.Invoke(() =>
            {
                DialogResult = (bool)state;
            });
        }

        private void SetBackGroundImage(bool isSuccessful)
        {
            if (isSuccessful)
            {
                Dispatcher.Invoke(() =>
                {
                    grdMain.Background = new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/EsiApiClient;component/Images/5.png")));
                    App.AppConfig = _configModel;
                });
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    lblError.Visibility = Visibility.Visible;
                });
            }
        }

        private void WndCompleteConfig_OnLoaded(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(SendRequest);
        }
    }
}
