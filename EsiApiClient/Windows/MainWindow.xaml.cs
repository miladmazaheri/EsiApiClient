using System;
using System.Globalization;
using System.IO;
using System.Management;
using System.Text.Json;
using System.Timers;
using System.Windows;
using EsiApiClient.Models;
using Timer = System.Timers.Timer;

namespace EsiApiClient.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string _configPath = Directory.GetCurrentDirectory() + @"\config.json";
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            CheckConfigStatusAndInitUtilities();
        }

        private void CheckConfigStatusAndInitUtilities()
        {
            if (!File.Exists(_configPath))
            {
                ShowNeedConfig();
            }
            else
            {
                var configContent = File.ReadAllBytes(_configPath);
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
                    ShowNeedConfig();
                }
                else if (!configModel.IsConfirmed)
                {
                    ShowConfirmConfig(configModel);
                }
                else
                {
                    InitReConfigListener();
                    InitUpdateFromApiTimer();
                    InitFingerPrintListener();
                    InitRfIdListener();
                }
            }
        }
        private void ShowNeedConfig()
        {
            _ = new wndNeedConfig().ShowDialog();
            CheckConfigStatusAndInitUtilities();
        }
        private void ShowConfirmConfig(ConfigModel configModel)
        {
            _ = new wndConfirmConfig(configModel).ShowDialog();
            CheckConfigStatusAndInitUtilities();
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

        private void Watcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            var driveNameProp = e.NewEvent.Properties["DriveName"];
            if (!string.IsNullOrWhiteSpace(driveNameProp?.Value.ToString()))
            {
                var configFilePath = driveNameProp.Value + @"\reconfig.json";
                if (File.Exists(configFilePath))
                {
                    try
                    {
                        var configContent = File.ReadAllText(configFilePath);
                        if (!string.IsNullOrWhiteSpace(configContent))
                        {
                            var configModel = JsonSerializer.Deserialize<ConfigModel>(configContent);
                            if (configModel != null)
                            {
                                ShowConfirmConfig(configModel);
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
