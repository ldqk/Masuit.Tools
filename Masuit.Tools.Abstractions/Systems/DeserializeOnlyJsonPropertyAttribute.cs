using System;

namespace Masuit.Tools.Systems;

/// <summary>
/// 只允许反序列化
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class DeserializeOnlyJsonPropertyAttribute : Attribute
{
}