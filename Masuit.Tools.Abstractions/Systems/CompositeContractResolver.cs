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
    protected override IValueProvider CreateMemberValueProvider(MemberInfo member)
    {
        IValueProvider provider = base.CreateMemberValueProvider(member);
        if (member.MemberType == MemberTypes.Property)
        {
            Type propType = ((PropertyInfo)member).PropertyType;
            if (propType.IsGenericType &&
                propType.GetGenericTypeDefinition() == typeof(List<>))
            {
                return new EmptyListValueProvider(provider, propType);
            }
        }

        return provider;
    }

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

internal class EmptyListValueProvider(IValueProvider innerProvider, Type listType) : IValueProvider
{
    private readonly object _defaultValue = Activator.CreateInstance(listType);

    public void SetValue(object target, object value)
    {
        innerProvider.SetValue(target, value ?? _defaultValue);
    }

    public object GetValue(object target)
    {
        return innerProvider.GetValue(target) ?? _defaultValue;
    }
}