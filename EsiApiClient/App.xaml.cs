using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using IPAClient.Models;
using Microsoft.Win32;

namespace IPAClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static ConfigModel AppConfig { get; set; }
        public static string ConfigFilePath = Directory.GetCurrentDirectory() + @"\config.json";

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            SetStartup();
        }
        private void SetStartup()
        {
            try
            {
                var appName = "IPA_Client";
                var exePath = Process.GetCurrentProcess().MainModule?.FileName;

                if (exePath != null)
                {
                    ////Current User
                    //RegistryKey rku = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                    //rku.SetValue(appName, exePath);

                    ////Local Machine
                    RegistryKey rkm = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                    rkm.SetValue(appName, exePath);
                }
            }
            catch (Exception)
            {
                //TODO Log
            }
        }
    }
}
