using System;
using System.ServiceProcess;

namespace EpsilonService
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                Console.WriteLine("Press i for install or u for unistall service: ");
                var key = Console.ReadKey();
                if (key.KeyChar == 'u')
                {
                    try
                    {
                        SelfInstaller.UninstallMe();
                        Console.WriteLine("\n\npress any key to exit...");
                        Console.ReadKey();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        Console.WriteLine("\n\npress any key to exit...");
                        Console.ReadKey();
                    }
                    return;
                }
            }
            bool _IsInstalled = false;
            bool serviceStarting = false;
            string SERVICE_NAME = "EpsilonBackgroundService";

            ServiceController[] services = ServiceController.GetServices();

            foreach (ServiceController service in services)
            {
                if (service.ServiceName.Equals(SERVICE_NAME))
                {
                    _IsInstalled = true;
                    if (service.Status == ServiceControllerStatus.StartPending)
                    {
                        // If the status is StartPending then the service was started via the SCM             
                        serviceStarting = true;
                    }
                    break;
                }
            }
            if (!serviceStarting)
            {
                if (_IsInstalled == true)
                {
                    if (args.Length != 0)
                    {
                        if (args[0].ToLower() == "u")
                        {
                            SelfInstaller.UninstallMe();
                        }
                    }
                }
                else
                {

                    SelfInstaller.InstallMe();
                    ServiceController controller = new ServiceController(SERVICE_NAME);
                    if (controller.Status == ServiceControllerStatus.Running)
                        controller.Stop();

                    if (controller.Status == ServiceControllerStatus.Stopped)
                        controller.Start();
                }
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    new EpsilonService()
                };
                ServiceBase.Run(services: ServicesToRun);
            }
        }

    }
}
