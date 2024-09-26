namespace DeskPiLiteRPi4Service.Config;

public class PowerManagementSettings
{
    /// <summary>
    /// The serial port to which the DeskPi Lite daughter board is connected (/dev/ttyUSB0 by default).
    /// </summary>
    public string SerialPort { get; set; } = "/dev/ttyUSB0";
    
    /// <summary>
    /// The baud rate at which the DeskPi Lite daughter board is connected (9600 by default).
    /// </summary>
    public int BaudRate { get; set; } = 9600;
    
    /// <summary>
    /// The command that your system uses to sync the file system ("sync" by default).
    /// This gets called before the power off command.
    /// </summary>
    public string SyncCommand { get; set; } = "sync";
    
    /// <summary>
    /// The command to run when a power off command is received ("shutdown -h now" by default).
    /// This gets called after the sync command.
    /// </summary>
    public string ShutdownCommand { get; set; } = "shutdown -h now";
}