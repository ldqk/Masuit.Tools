namespace Masuit.Tools.Abstractions.Hardware;

public class SensorValue
{
    public string Identifier { get; internal set; }
    public string Name { get; internal set; }
    public string Value { get; internal set; }
    public SensorTypes Type { get; internal set; }
}