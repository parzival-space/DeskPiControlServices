namespace DeskPiLiteRPi3Service.Config;

public class FanControlSettings
{
    /// <summary>
    /// The GPIO pin number (14 by default) to which the fan is connected.
    /// </summary>
    public int FanPin { get; set; } = 14;

    /// <summary>
    /// The temperature (in degrees Celsius) at which the fan should be triggered to turn on (60°C by default).
    /// </summary>
    public int TriggerTemp { get; set; } = 60;

    /// <summary>
    /// The minimum time (in seconds) that the fan should stay on once it is triggered (60 by default).
    /// </summary>
    public int MinimumTime { get; set; } = 60;
}