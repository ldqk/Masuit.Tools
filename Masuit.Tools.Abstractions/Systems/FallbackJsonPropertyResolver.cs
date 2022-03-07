using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Masuit.Tools.Systems;

/// <summary>
/// 多别名属性的解释器
/// </summary>
public class FallbackJsonPropertyResolver : CamelCasePropertyNamesContractResolver
{
    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        var typeMembers = GetSerializableMembers(type);
        var properties = new List<JsonProperty>();

        foreach (var member in typeMembers)
        {
            var property = CreateProperty(member, memberSerialization);
            properties.Add(property);

            var fallbackAttribute = member.GetCustomAttribute<FallbackJsonProperty>();

            if (fallbackAttribute == null)
            {
                continue;
            }

            property.PropertyName = fallbackAttribute.PreferredName;

            foreach (var alternateName in fallbackAttribute.FallbackReadNames)
            {
                var fallbackProperty = CreateProperty(member, memberSerialization);
                fallbackProperty.PropertyName = alternateName;
                fallbackProperty.ShouldSerialize = (x) => false;
                properties.Add(fallbackProperty);
            }
        }

        return properties;
    }
}