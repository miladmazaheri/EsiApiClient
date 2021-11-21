﻿using System;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Management;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ApiWrapper;
using ApiWrapper.Dto;
using IPAClient.Models;
using IPAClient.Tools;
using Timer = System.Timers.Timer;

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
        private readonly Timer _recheckTimer;
        private SerialPort _serialPort;
        private FingerPrintHelper _fingerPrintHelper;
        public MainWindow()
        {
            InitializeComponent();
            _recheckTimer = new Timer(30000);
            _recheckTimer.Elapsed += async (sender, e) => await GetConfigFromServerAsync();
        }

        private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            await CheckConfigStatusAndInitUtilitiesAsync();
        }

        private async Task CheckConfigStatusAndInitUtilitiesAsync()
        {
            if (!File.Exists(App.ConfigFilePath))
            {
                await ShowNeedConfigAsync();
            }
            else
            {
                var configContent = await File.ReadAllBytesAsync(App.ConfigFilePath);
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
                    ApiClient.SetBaseUrl(string.IsNullOrWhiteSpace(configModel.WebServiceUrl) ? "http://eis.msc.ir/" : configModel.WebServiceUrl);
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
            //دریافت اطلاعات اولیه و تاریخ و ساعت سرور 
            SetBackGroundImage("9");
            try
            {
                var mainInfo = await ApiClient.MainInfo_Send_Lookup_Data_Fun();
                SetBackGroundImage("10");
                SystemTimeHelper.SetSystemTime(mainInfo.ServerDateTime);
                //TODO Save In Database
                Thread.Sleep(2000);
            }
            catch (Exception ex)
            {
                App.AddLog(ex);
                SetBackGroundImage("11");
                _recheckTimer.Start();
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
                //TODO decide about cod meal
                var reservationDate = await ApiClient.MainInfo_Send_Offline_Data_Fun(new MainInfo_Send_Offline_Data_Fun_Input_Data(App.AppConfig.Device_Cod, DateTime.Now.ToServerDateFormat(), "22"));
                SetBackGroundImage("16");
                //TODO Save In Database
                Thread.Sleep(2000);
            }
            catch (Exception ex)
            {
                App.AddLog(ex);
                SetBackGroundImage("17");
                _recheckTimer.Start();
                return;
            }


            //تصویر پس زمینه آماده به کار
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
                        new Uri(@$"pack://application:,,,/IPAClient;component/Images/{imageName}.png")));
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
            _fingerPrintHelper?.Dispose();
            _fingerPrintHelper = new FingerPrintHelper(dataReceivedAction: FingerPrintDataReceived);
        }

        private void FingerPrintDataReceived(uint obj)
        {
            //TODO 
        }

        #endregion

        #region RfId Listener
        private void InitRfIdListener()
        {

        }
        #endregion

        #region Serial Port


        private void SendSerialData(string dataStr)
        {
            if (string.IsNullOrWhiteSpace(dataStr)) return;
            try
            {
                _serialPort ??= new SerialPort("COM6", 1200, Parity.None, 8, StopBits.One);
                var messageBytes = System.Text.Encoding.UTF8.GetBytes(dataStr);
                _serialPort.Write(messageBytes, 0, messageBytes.Length);
            }
            catch (Exception e)
            {
                App.AddLog(e);
            }
        }
        #endregion

    }
}
