using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Linq;
using System.Reflection;

namespace Masuit.Tools.Systems;

/// <summary>
/// 支持只允许反序列化属性和多别名属性的解释器
/// </summary>
public class CompositeContractResolver : FallbackJsonPropertyResolver
{
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var property = base.CreateProperty(member, memberSerialization);
        if (property.AttributeProvider.GetAttributes(typeof(DeserializeOnlyJsonPropertyAttribute), true).Union(property.AttributeProvider.GetAttributes(typeof(SerializeIgnoreAttribute), true)).Any())
        {
            property.ShouldSerialize = _ => false;
        }

        if (property.AttributeProvider.GetAttributes(typeof(SerializeOnlyJsonPropertyAttribute), true).Union(property.AttributeProvider.GetAttributes(typeof(DeserializeIgnoreAttribute), true)).Any())
        {
            property.ShouldDeserialize = _ => false;
        }

        return property;
    }
}
