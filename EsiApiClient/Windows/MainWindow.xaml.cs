using System;
using System.IO;
using System.Management;
using System.Text.Json;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EsiApiClient.Models;
using Timer = System.Timers.Timer;

namespace EsiApiClient.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string configPath = Directory.GetCurrentDirectory() + @"\config.json";
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(configPath))
            {
                ShowNeedConfig();
            }
            else
            {
                var configContent = File.ReadAllBytes(configPath);
                var configModel = JsonSerializer.Deserialize<ConfigModel>(configContent);
                if (configModel == null)
                {
                    ShowNeedConfig();
                }
                else if (!configModel.IsConfirmed)
                {
                    ShowConfirmConfig(configModel);
                }
            }


            ManagementEventWatcher watcher = new ManagementEventWatcher();
            WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2");
            watcher.EventArrived += Watcher_EventArrived;
            watcher.Query = query;
            watcher.Start();

            var mainTimer = new Timer(5 * 60 * 1000);//5 Minutes
            mainTimer.Elapsed += MainTimer_Elapsed;
            //mainTimer.Start();
        }

        private async void MainTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //TODO: Update Reservations With Api
            MessageBox.Show(e.SignalTime.ToString());
        }

        private void Watcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            var driveNameProp = e.NewEvent.Properties["DriveName"];
            if (!string.IsNullOrWhiteSpace(driveNameProp?.Value.ToString()))
            {
                var configFilePath = driveNameProp.Value + @"\config.json";
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


        private void ShowNeedConfig()
        {
            new wndNeedConfig().ShowDialog();
        }
        private void ShowConfirmConfig(ConfigModel configModel)
        {
            new wndConfirmConfig(configModel).ShowDialog();
        }

    }
}
