using System;
using System.IO;
using System.Management;
using System.Text.Json;
using System.Windows;
using IPAClient.Models;

namespace IPAClient.Windows
{
    /// <summary>
    /// Interaction logic for wndNeedConfig.xaml
    /// </summary>
    public partial class wndNeedConfig : Window
    {
        public wndNeedConfig()
        {
            InitializeComponent();
        }

        private void WndNeedConfig_OnLoaded(object sender, RoutedEventArgs e)
        {
            InitReConfigListener();
        }

        private void ShowConfirmConfig(ConfigModel configModel)
        {
            var confirmResult = new wndConfirmConfig(configModel).ShowDialog();
            if (confirmResult != true) return;

            DialogResult = true;
            Close();
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
            if (string.IsNullOrWhiteSpace(driveNameProp?.Value.ToString())) return;

            var configFilePath = driveNameProp.Value + @"\config.json";
            if (File.Exists(configFilePath))
            {
                ReadConfigFile(configFilePath);
            }
            else
            {
                configFilePath = driveNameProp.Value + @"\reconfig.json";
                if (File.Exists(configFilePath))
                {
                    ReadConfigFile(configFilePath);
                }
            }

        }

        private void ReadConfigFile(string configFilePath)
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

        #endregion


    }
}
