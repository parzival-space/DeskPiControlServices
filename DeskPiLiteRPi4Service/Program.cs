using System.Device.Gpio;
using DeskPiLiteRPi4Service.Service;

namespace DeskPiLiteRPi4Service;

public class Program
{
    public static void Main(string[] args)
    {
        if (!OperatingSystem.IsLinux())
        {
            Console.WriteLine("This service is designed to run on Linux only.");
            return;
        }
        
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostContext, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json",
                    optional: true, reloadOnChange: true);
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.Configure<PwmFanControlService>(hostContext.Configuration.GetSection("FanControl"));
                services.Configure<PowerManagementService>(hostContext.Configuration.GetSection("PowerManagement"));
                
                services.AddSingleton<GpioController>();

                services.AddHostedService<PwmFanControlService>(); // Desk Pi Lite RPi 4 Fan Control Service
                services.AddHostedService<PowerManagementService>(); // Desk Pi Lite RPi 4 Power Management Service
            })
            .UseSystemd()
            .Build()
            .Run();
    }
}