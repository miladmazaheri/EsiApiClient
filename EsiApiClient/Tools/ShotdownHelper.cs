using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPAClient.Tools
{
    public class ShutdownHelper
    {
        /// <summary>
        /// Windows restart
        /// </summary>
        public static void Restart()
        {
            StartShutDown("-f -r -t 5");
        }

        /// <summary>
        ///  Shutting Down Windows 
        /// </summary>
        public static void Shut()
        {
            StartShutDown("-f -s -t 5");
        }

        private static void StartShutDown(string param)
        {
            ProcessStartInfo proc = new ProcessStartInfo();
            proc.FileName = "cmd";
            proc.WindowStyle = ProcessWindowStyle.Hidden;
            proc.Arguments = "/C shutdown " + param;
            Process.Start(proc);
        }
    }
}
