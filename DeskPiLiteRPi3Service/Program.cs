using System.Device.Gpio;
using DeskPiLiteRPi3Service.Config;
using DeskPiLiteRPi3Service.Services;

namespace DeskPiLiteRPi3Service;

public class Program
{
    public static void Main(string[] args)
    {
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostContext, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json",
                    optional: true, reloadOnChange: true);
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.Configure<FanControlSettings>(hostContext.Configuration.GetSection("FanControl"));
                
                services.AddSingleton<GpioController>();

                services.AddHostedService<FanControlService>(); // Desk Pi Lite RPi 3 Fan Control Service
                //services.AddHostedService<Worker>();
            })
            .UseSystemd()
            .Build()
            .Run();
    }
}