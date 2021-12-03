using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using ApiWrapper;
using ApiWrapper.Dto;
using DataLayer;
using DataLayer.Entities;
using IPAClient.Models;
using IPAClient.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using NLog.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace IPAClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string CurrentMealCode { get; set; }
        public static ConfigModel AppConfig { get; set; }
        public static MainInfo_Send_Lookup_Data_Fun MainInfo { get; set; }
        public static string ConfigFilePath => Directory.GetCurrentDirectory() + @"\config.json";
        public static string FingerPrintConfigFilePath => Directory.GetCurrentDirectory() + @"\fingerPrintConfig.json";
        public static string RfIdConfigFilePath => Directory.GetCurrentDirectory() + @"\rfidConfig.json";
        public static string MonitorConfigFilePath => Directory.GetCurrentDirectory() + @"\monitorConfig.json";
        public static string MainInfoFilePath => Directory.GetCurrentDirectory() + @"\mainInfo.json";
        public static DateTime? LastFullUpdateTime { get; set; }
        public static DateTime? LastMealUpdateTime { get; set; }

        private static ILogger Logger;
        public App()
        {
            #region For Initilize Logger
            var host = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    //Lets Create single tone instance of Master windows
                    services.AddSingleton<MainWindow>();

                })
                .ConfigureLogging(logBuilder =>
                {
                    logBuilder.SetMinimumLevel(LogLevel.Error);
                    logBuilder.AddNLog("nlog.config");

                }).Build();

            using var serviceScope = host.Services.CreateScope();
            {
                var services = serviceScope.ServiceProvider;
                try
                {
                    Logger = services.GetRequiredService<ILogger<MainWindow>>();
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            #endregion
        }
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            //var model = new MonitorDto()
            //{
            //    CurrentMealRemainTime = new TimeSpan(2, 40, 35),
            //};
            //model.AddToQueue(new Reservation() { Main_Course = new List<Food>() { new Food() }, Appetizer_Dessert = new List<Food>() { new Food() } });
            //model.AddToQueue(new Reservation());
            //model.AddToQueue(new Reservation());
            //model.AddToQueue(new Reservation());
            //model.AddToQueue(new Reservation());
            //model.InsertOrUpdateRemainFood(new RemainFoodModel(), new RemainFoodModel(), new RemainFoodModel());

            //var str = JsonSerializer.Serialize(model);

            ManageUnhandledExceptions();
            SetStartup();
            await CreateDateBase();
            ConfigureApiClient();
        }

        private async Task CreateDateBase()
        {
            var context = new EsiDbContext();
            await context.Database.MigrateAsync();
        }

        private void ConfigureApiClient()
        {
            ApiClient.SetLogger(Logger);
        }
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            AddLog(e.Exception);
            e.Handled = true;
        }
        private void ManageUnhandledExceptions()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                AddLog((Exception)args.ExceptionObject);
            };
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
            catch (Exception ex)
            {
                AddLog(ex);
            }


        }

        public static void AddLog(Exception ex)
        {
            Logger.LogError(ex, "");
        }
    }
}
