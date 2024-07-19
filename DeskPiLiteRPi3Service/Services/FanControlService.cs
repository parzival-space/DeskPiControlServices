using System.Device.Gpio;
using DeskPiLiteRPi3Service.Config;
using Microsoft.Extensions.Options;

namespace DeskPiLiteRPi3Service.Services;

public class FanControlService(ILogger<FanControlService> logger, GpioController controller, IOptions<FanControlSettings> fanControlSettings) : BackgroundService
{
    private const string ThermalZoneFile = "/sys/class/thermal/thermal_zone0/temp";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting {serviceName} on GPIO {gpio}.", 
            nameof(FanControlService), fanControlSettings.Value.FanPin);

        DateTime fanOffTime = DateTime.MinValue;
        GpioPin fanPin = controller.OpenPin(fanControlSettings.Value.FanPin, PinMode.Output, PinValue.Low);
        while (!stoppingToken.IsCancellationRequested)
        {
            bool fanActive = fanPin.Read() == PinValue.High;
            
            // read cpu temp
            string cpuTempRaw = await File.ReadAllTextAsync(ThermalZoneFile, stoppingToken);
            if (!float.TryParse(cpuTempRaw, out var cpuTemp)) 
                logger.LogWarning("Failed to parse cpu temperature. Expected float but got: {rawValue}", cpuTempRaw);
            
            // adjust temperature
            cpuTemp /= 1000;
            logger.LogDebug("Current CPU Temp: {cpuTemp}°C / {triggerTemp}°C", 
                cpuTemp, fanControlSettings.Value.TriggerTemp);
            
            // control cpu fan, only log when change is detected
            if (!fanActive && cpuTemp >= fanControlSettings.Value.TriggerTemp)
            {
                logger.LogInformation("CPU temperature above {trigger}°C ({real}°C) threshold! Activating CPU fan.",
                    fanControlSettings.Value.TriggerTemp, cpuTemp);

                fanPin.Write(PinValue.High);
                fanOffTime = DateTime.Now.AddSeconds(fanControlSettings.Value.MinimumTime);
            }
            else if (fanActive && DateTime.Now >= fanOffTime && cpuTemp <= fanControlSettings.Value.TriggerTemp)
            {
                logger.LogInformation("CPU temperature below {trigger}°C ({real}°C) threshold! Deactivating CPU fan.",
                    fanControlSettings.Value.TriggerTemp, cpuTemp);
                fanPin.Write(PinValue.Low);
            }
            
            await Task.Delay(1000, stoppingToken); // wait a second
        }
        
        // cleanup
        controller.ClosePin(fanPin.PinNumber);
    }
}