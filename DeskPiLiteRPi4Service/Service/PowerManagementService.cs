using System.Diagnostics;
using System.IO.Ports;
using System.Text;
using DeskPiLiteRPi4Service.Config;
using Microsoft.Extensions.Options;

namespace DeskPiLiteRPi4Service.Service;

public class PowerManagementService(ILogger<PwmFanControlService> logger, IOptions<PowerManagementSettings> settings) : BackgroundService
{
    private const string PowerOffRequest = "poweroff";
    private const string PowerOffResponse = "power_off";
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        SerialPort serialPort = new(settings.Value.SerialPort, settings.Value.BaudRate);
        serialPort.Open();
        
        // store if a power off command was received
        bool powerOffReceived = false;
        
        while (!stoppingToken.IsCancellationRequested && !powerOffReceived)
        {
            try
            {
                byte[] buffer = new byte[16];
                _ = await serialPort.BaseStream.ReadAsync(buffer, 0, 16, stoppingToken);

                // parse buffer
                String message = Encoding.UTF8.GetString(buffer);

                if (message.Contains(PowerOffRequest))
                {
                    logger.LogInformation("Received power off command.");
                    powerOffReceived = true; // stop listening for power off commands

                    // notify daughter board that power off command was received
                    byte[] utf8Response = Encoding.UTF8.GetBytes(PowerOffResponse);
                    serialPort.Write(utf8Response, 0, utf8Response.Length);
                    serialPort.Write(utf8Response, 0, utf8Response.Length);

                    // send sync and shutdown to system
                    await RunProcessAsync(settings.Value.SyncCommand, stoppingToken, true);
                    await RunProcessAsync(settings.Value.ShutdownCommand, stoppingToken, true);
                }

            }
            catch (TaskCanceledException)
            {
                // do nothing
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to read from serial port.");
            }
        }
        
        serialPort.Close();
        serialPort.Dispose();
    }
    
    private async Task RunProcessAsync(string command, CancellationToken stoppingToken, bool logOutput = false)
    {
        Process process = new()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{command}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        
        process.Start();
        await process.WaitForExitAsync(stoppingToken);
        
        if (logOutput)
        {
            string output = await process.StandardOutput.ReadToEndAsync(stoppingToken);
            string error = await process.StandardError.ReadToEndAsync(stoppingToken);
            
            if (!string.IsNullOrWhiteSpace(output))
                logger.LogInformation(output);
            if (!string.IsNullOrWhiteSpace(error))
                logger.LogError(error);
        }
    }
}