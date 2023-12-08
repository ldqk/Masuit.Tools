using System;

namespace Masuit.Tools.Systems;

/// <summary>
/// 多别名属性
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class FallbackJsonProperty(string preferredName, params string[] fallbackReadNames) : Attribute
{
    public string PreferredName { get; } = preferredName;

    public string[] FallbackReadNames { get; } = fallbackReadNames;
}
