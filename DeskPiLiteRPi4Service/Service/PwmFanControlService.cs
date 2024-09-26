using System.Device.Gpio;
using System.Device.Pwm;
using System.Device.Pwm.Drivers;
using DeskPiLiteRPi4Service.Config;
using Microsoft.Extensions.Options;

namespace DeskPiLiteRPi4Service.Service;

public class PwmFanControlService(ILogger<PwmFanControlService> logger, GpioController controller, IOptions<PwmFanControlSettings> pwmFanControlSettings) : BackgroundService
{
    private const string ThermalZoneFile = "/sys/class/thermal/thermal_zone0/temp";
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting {serviceName} on GPIO {gpio}.", 
            nameof(PwmFanControlService), pwmFanControlSettings.Value.FanPin);
        
        DateTime fanOffTime = DateTime.MinValue;
        SoftwarePwmChannel pwmChannel = new SoftwarePwmChannel(
            pwmFanControlSettings.Value.FanPin, 
            100, 0.5f, controller: controller,
            shouldDispose: false);
        
        pwmChannel.Start();
        while (!stoppingToken.IsCancellationRequested)
        {
            // read cpu temp
            string cpuTempRaw = await File.ReadAllTextAsync(ThermalZoneFile, stoppingToken);
            if (!float.TryParse(cpuTempRaw, out var cpuTemp)) 
                logger.LogWarning("Failed to parse cpu temperature. Expected float but got: {rawValue}", cpuTempRaw);
            
            // adjust temperature
            cpuTemp /= 1000;
            
            // check if the fan profile has a value for the current temperature
            float fanSpeedPercentage = pwmFanControlSettings.Value.FanProfile
                .LastOrDefault(x => cpuTemp >= x.Key).Value;
            logger.LogDebug("Current CPU Temp: {cpuTemp}°C, Selected Speed: {fanSpeed}%", cpuTemp, fanSpeedPercentage * 100);
            
            // update duty cycle if necessary
            if (Math.Abs(pwmChannel.DutyCycle - fanSpeedPercentage) > 0.001)
            {
                // if fanspeed increases, update now, if not skip until fanOffTime
                if (fanSpeedPercentage > pwmChannel.DutyCycle)
                {
                    pwmChannel.DutyCycle = fanSpeedPercentage;
                    logger.LogInformation("Fan speed updated to {fanSpeed}%", fanSpeedPercentage * 100);
                    fanOffTime = DateTime.Now.AddSeconds(pwmFanControlSettings.Value.MinimumTime);
                }
                else if (DateTime.Now >= fanOffTime)
                {
                    pwmChannel.DutyCycle = fanSpeedPercentage;
                    logger.LogInformation("Fan speed updated to {fanSpeed}%", fanSpeedPercentage * 100);
                    fanOffTime = DateTime.Now.AddSeconds(pwmFanControlSettings.Value.MinimumTime);
                }
            }
            
            await Task.Delay(1000, stoppingToken); // wait a second
        }
        
        // cleanup
        pwmChannel.Stop();
        pwmChannel.Dispose();
    }
}