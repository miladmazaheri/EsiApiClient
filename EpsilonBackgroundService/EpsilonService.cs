using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace EpsilonService
{
    partial class EpsilonService : ServiceBase
    {
        private static readonly string CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
        readonly string _applicationName = CurrentDirectory + "IPAClient.exe";
        private string msg = "";
        public EpsilonService()
        {
            InitializeComponent();
        }


        private Timer timer;
        /// <summary>
        /// تابعی حاوی کدهایی که هنگام اجرای سرویس باید استفاده شوند
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            msg = "سرویس اپسیلون شروع شد....";
            WriteLog(msg + DateTime.Now);
            runServer();
            //timer = new Timer {Interval = 1000 * 60 * 5}; // تنظیم فاصله زمانی بمدت ‍‍‍‍‍‍‍5 دقیقه
            timer = new Timer { Interval = 1000 * 60 * 5 }; // تنظیم فاصله زمانی بمدت ‍‍‍‍‍‍‍15 تانیه
            timer.Elapsed += timer_tick;
            timer.Enabled = true;
        }

        /// <summary>
        /// تابعی برای نگه داشتن وضعیت اجرای سرویس در یک فایل تکست
        /// </summary>
        /// <param name="message"></param>
        private void WriteLog(string message)
        {
            //TODO : Control Logging By Some Setting
            //try
            //{
            //    string logServicePath = CurrentDirectory + "EpsilonLog.log"; // وضعیت در یک فایل تکست به اسم "اپسیلون سرویس لوگ" ذخیره می شود
            //    if (!File.Exists(logServicePath))
            //    {
            //        File.Create(logServicePath);

            //    }
            //    using (StreamWriter writer = new StreamWriter(logServicePath, true))
            //    {
            //        writer.WriteLine(message);
            //    }
            //}
            //catch { }
        }

        private void timer_tick(object sender, ElapsedEventArgs e)
        {
            runServer();
        }
        /// <summary>
        /// تابعی که وضعیت اجرای سرویس را مشخص میکند
        /// </summary>
        private void runServer()
        {
            try
            {
                if (!Process.GetProcesses().Any(p => p.ProcessName.Contains("IPAClient")))
                {
                    // اجرای سرویس
                    msg = "سرور اپسیلون شروع شده است....";
                    WriteLog(msg + DateTime.Now);
                    ApplicationLoader.PROCESS_INFORMATION procInfo;
                    bool result = ApplicationLoader.StartProcessAndBypassUAC(_applicationName, CurrentDirectory, out procInfo);
                    if (result)
                        msg = "سرور اپسیلون اجرا شد....";
                    else
                        msg = "سرور اپسیلون اجرا نشد!....";
                    WriteLog(msg + DateTime.Now);
                }
                else
                {
                    msg = "سرور اپسیلون در حال اجرا است....";
                    WriteLog(msg + DateTime.Now);
                }
            }
            catch (Exception e)
            {
                WriteLog(e.Message + DateTime.Now);
                throw;
            }

        }
        /// <summary>
        /// تابعی حاوی کدهایی که هنگام توقف سرویس باید استفاده شوند
        /// </summary>
        protected override void OnStop()
        {
            msg = "سرویس اپسیلون متوقف شده است....";
            WriteLog(msg + DateTime.Now);
        }
    }
}
