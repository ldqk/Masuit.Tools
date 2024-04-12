namespace Masuit.Tools.Files;

public class IniItem
{
    public string Name { get; set; }

    public string Value { get; set; }

    public override string ToString() => $"{Name ?? string.Empty}={Value ?? string.Empty}";
}