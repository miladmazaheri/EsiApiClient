using System;
using System.Globalization;
using System.IO;
using System.Management;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using DNTPersianUtils.Core;
using EsiApiClient.Api;
using EsiApiClient.Api.Dto;
using EsiApiClient.Models;
using EsiApiClient.Tools;
using Timer = System.Timers.Timer;

namespace EsiApiClient.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string _configPath = Directory.GetCurrentDirectory() + @"\config.json";
        private readonly Timer _recheckTimer;

        public MainWindow()
        {
            InitializeComponent();
            _recheckTimer = new Timer(30000);
            _recheckTimer.Elapsed += async (sender, e) => await GetConfigFromServerAsync();
        }

        private void RecheckTimerCallBack(object state)
        {
            throw new NotImplementedException();
        }

        private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            await CheckConfigStatusAndInitUtilitiesAsync();
        }

        private async Task CheckConfigStatusAndInitUtilitiesAsync()
        {
            if (!File.Exists(_configPath))
            {
                await ShowNeedConfigAsync();
            }
            else
            {
                var configContent = await File.ReadAllBytesAsync(_configPath);
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
                    await GetConfigFromServerAsync();
                    InitReConfigListener();
                    InitUpdateFromApiTimer();
                    InitFingerPrintListener();
                    InitRfIdListener();
                }
            }
        }

        private async Task GetConfigFromServerAsync()
        {
            _recheckTimer.Stop();
            SetBackGroundImage("9");
            try
            {
                var mainInfo = await ApiClient.MainInfo_Send_Lookup_Data_Fun();
                SetBackGroundImage("10");
                SystemTimeHelper.SetSystemTime(mainInfo.ServerDateTime);
                //TODO Save In Database
                Thread.Sleep(2000);
            }
            catch (Exception)
            {
                //TODO Log
                SetBackGroundImage("11");
                _recheckTimer.Start();
                return;
            }

            SetBackGroundImage("12");
            //TODO Update Personnel Info
            Thread.Sleep(2000);
            SetBackGroundImage("13");

            SetBackGroundImage("15");
            try
            {
                //TODO decide about cod meal
                var reservationDate = await ApiClient.MainInfo_Send_Offline_Data_Fun(new MainInfo_Send_Offline_Data_Fun_Input_Data(App.AppConfig.Device_Cod, DateTime.Now.ToServerDateFormat(), "22"));
                SetBackGroundImage("16");
                //TODO Save In Database
                Thread.Sleep(2000);
            }
            catch (Exception)
            {
                //TODO Log
                SetBackGroundImage("17");
                _recheckTimer.Start();
                return;
            }

            SetBackGroundImage("21");
        }

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
                        new Uri(@$"pack://application:,,,/EsiApiClient;component/Images/{imageName}.png")));
            });
        }

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

        #region Update Form Api Timer
        private void InitUpdateFromApiTimer()
        {
            var mainTimer = new Timer(5 * 60 * 1000);//5 Minutes
            mainTimer.Elapsed += MainTimer_Elapsed;
            mainTimer.Start();
        }

        private void MainTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //TODO: Update Reservations With Api
            _ = MessageBox.Show(e.SignalTime.ToString(CultureInfo.InvariantCulture));
        }
        #endregion

        #region Finger Print Listener
        private void InitFingerPrintListener()
        {

        }
        #endregion

        #region RfId Listener
        private void InitRfIdListener()
        {

        }
        #endregion



    }
}
