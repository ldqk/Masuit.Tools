using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Masuit.Tools.Systems;

/// <summary>
/// 支持只允许反序列化属性和多别名属性的解释器
/// </summary>
public class CompositeContractResolver : FallbackJsonPropertyResolver
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
