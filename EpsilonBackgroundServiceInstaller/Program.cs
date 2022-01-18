using System;
using System.Diagnostics;

namespace EpsilonBackgroundServiceInstaller
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Process.Start("EpsilonBackgroundService.exe","Setup");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("EpsilonBackgroundService.exe Not Found!");
                Console.ReadKey();
            }
        }
    }
}
