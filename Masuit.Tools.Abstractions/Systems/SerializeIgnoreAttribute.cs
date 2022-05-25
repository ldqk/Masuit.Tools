using System;

namespace Masuit.Tools.Systems;

/// <summary>
/// 序列化时忽略
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class SerializeIgnoreAttribute : Attribute
{
}

/// <summary>
/// 序列化时忽略
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class DeserializeIgnoreAttribute : Attribute
{
}
