using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using EpsilonService;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace EpsilonRunnerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private static readonly string CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private readonly string _applicationName = CurrentDirectory + "HandBrake";//"IPAClient.exe";
        private Timer timer;
        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Epsilon Runner Service Started", DateTimeOffset.Now);
            timer = new Timer { Interval = 1000 * 10 }; // تنظیم فاصله زمانی بمدت ‍‍‍‍‍‍‍15 تانیه
            timer.Elapsed += TimerOnElapsed;
            timer.Enabled = true;
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            RunServer();
        }

        private void RunServer()
        {
            try
            {
                if (!Process.GetProcesses().Any(p => p.ProcessName.Contains("IPAClient")))
                {
                    ApplicationLoader.PROCESS_INFORMATION procInfo;
                    bool result = ApplicationLoader.StartProcessAndBypassUAC(_applicationName, CurrentDirectory, out procInfo);
                    _logger.LogInformation(result ? "Epsilon Client Started" : "Epsilon Client Not Started", DateTimeOffset.Now);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error On Starting IPAClient", DateTimeOffset.Now);
            }

        }
    }
}
