using System;

namespace Masuit.Tools.Systems;

/// <summary>
/// 多别名属性
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class FallbackJsonProperty : Attribute
{
    public string PreferredName { get; }

    public string[] FallbackReadNames { get; }

    public FallbackJsonProperty(string preferredName, params string[] fallbackReadNames)
    {
        PreferredName = preferredName;
        FallbackReadNames = fallbackReadNames;
    }
}