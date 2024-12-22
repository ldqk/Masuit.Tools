namespace Masuit.Tools.Abstractions.Hardware;

public enum SensorTypes
{
    Unknown,
    CoolingFan,
    FanDuty,
    Temperature,
    Voltage,
    Current,
    Power,
    System,
}

public static class SensorTypeStrings
{
    public const string FanDuty = "Fan Duty";
    public const string FanSpeed = "Fan Speed";
    public const string Temperature = "Temperature";
    public const string Voltage = "Voltage";
    public const string Current = "Current";
    public const string Power = "Power";
    public const string System = "System";
    public const string Unknown = "Unkown";

    internal static SensorTypes GetTypeFromStringCode(string typeString)
    {
        switch (typeString)
        {
            case "fan":
                return SensorTypes.CoolingFan;

            case "temp":
                return SensorTypes.Temperature;

            case "volt":
                return SensorTypes.Voltage;

            case "curr":
                return SensorTypes.Current;

            case "pwr":
                return SensorTypes.Power;

            case "sys":
                return SensorTypes.System;

            case "duty":
                return SensorTypes.FanDuty;

            default:
                return SensorTypes.Unknown;
        }
    }
}