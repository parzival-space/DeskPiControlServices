namespace DeskPiLiteRPi4Service.Config;

public class PwmFanControlSettings
{
    /// <summary>
    /// The GPIO pin number (14 by default) to which the fan is connected.
    /// </summary>
    public int FanPin { get; set; } = 14;

    /// <summary>
    /// The minimum time (in seconds) that the fan should stay on once it is triggered (60 by default).
    /// </summary>
    public int MinimumTime { get; set; } = 60;
    
    /// <summary>
    /// A fan speed map that defines the fan speed based on the temperature.
    /// </summary>
    public Dictionary<int, float> FanProfile { get; set; } = new()
    {
        // temp, speed
        { 0, 0.0f },
        { 40, 0.7f },
        { 45, 0.85f },
        { 50, 1.0f }
    };
}