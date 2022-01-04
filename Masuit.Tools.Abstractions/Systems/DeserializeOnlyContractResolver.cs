using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Reflection;
using System;

namespace Masuit.Tools.Systems;

/// <summary>
/// 只允许反序列化
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class DeserializeOnlyJsonPropertyAttribute : Attribute
{
}

/// <summary>
/// 只允许反序列化的契约解释器
/// </summary>
public class DeserializeOnlyContractResolver : DefaultContractResolver
{
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var property = base.CreateProperty(member, memberSerialization);
        if (property is { Writable: true })
        {
            var attributes = property.AttributeProvider.GetAttributes(typeof(DeserializeOnlyJsonPropertyAttribute), true);
            if (attributes is { Count: > 0 })
            {
                property.ShouldSerialize = _ => false;
            }
        }

        return property;
    }
}
