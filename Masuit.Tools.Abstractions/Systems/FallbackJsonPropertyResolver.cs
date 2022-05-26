using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Masuit.Tools.Systems;

/// <summary>
/// 多别名属性的解释器
/// </summary>
public class FallbackJsonPropertyResolver : CamelCasePropertyNamesContractResolver
{
    /// <summary>
    /// 序列化时忽略的字段集
    /// </summary>
    protected readonly Dictionary<Type, HashSet<string>> SerializeIgnores = new();

    /// <summary>
    /// 反序列化时忽略的字段集
    /// </summary>
    protected readonly Dictionary<Type, HashSet<string>> DeserializeIgnores = new();

    /// <summary>
    /// 序列化时忽略
    /// </summary>
    /// <param name="type">类型</param>
    /// <param name="propertyName">属性名</param>
    /// <returns></returns>
    public FallbackJsonPropertyResolver SerializeIgnore(Type type, params string[] propertyName)
    {
        if (!SerializeIgnores.ContainsKey(type)) SerializeIgnores[type] = new HashSet<string>();
        foreach (var prop in propertyName)
        {
            SerializeIgnores[type].Add(prop);
        }

        return this;
    }

    /// <summary>
    /// 反序列化时忽略
    /// </summary>
    /// <param name="type">类型</param>
    /// <param name="propertyName">属性名</param>
    /// <returns></returns>
    public FallbackJsonPropertyResolver DeserializeIgnore(Type type, params string[] propertyName)
    {
        if (!DeserializeIgnores.ContainsKey(type)) DeserializeIgnores[type] = new HashSet<string>();
        foreach (var prop in propertyName)
        {
            DeserializeIgnores[type].Add(prop);
        }

        return this;
    }

    public bool IsSerializeIgnored(Type type, string propertyName)
    {
        if (!SerializeIgnores.ContainsKey(type)) return false;
        if (SerializeIgnores[type].Count == 0) return true;
        return SerializeIgnores[type].Contains(propertyName, StringComparer.CurrentCultureIgnoreCase);
    }

    public bool IsDeserializeIgnored(Type type, string propertyName)
    {
        if (!DeserializeIgnores.ContainsKey(type)) return false;
        if (DeserializeIgnores[type].Count == 0) return true;
        return DeserializeIgnores[type].Contains(propertyName, StringComparer.CurrentCultureIgnoreCase);
    }

    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        var typeMembers = GetSerializableMembers(type).DistinctBy(m => m.Name);
        var properties = new List<JsonProperty>();

        foreach (var member in typeMembers)
        {
            var property = CreateProperty(member, memberSerialization);
            if (IsSerializeIgnored(property.DeclaringType, property.PropertyName))
            {
                property.ShouldSerialize = _ => false;
            }

            if (IsDeserializeIgnored(property.DeclaringType, property.PropertyName))
            {
                property.ShouldDeserialize = _ => false;
            }

            properties.RemoveAll(p => p.PropertyName == property.PropertyName);
            properties.Add(property);
            var fallbackAttribute = member.GetCustomAttribute<FallbackJsonProperty>();
            if (fallbackAttribute == null)
            {
                continue;
            }

            property.PropertyName = fallbackAttribute.PreferredName;
            foreach (var alternateName in fallbackAttribute.FallbackReadNames)
            {
                properties.RemoveAll(p => p.PropertyName == alternateName);
                var fallbackProperty = CreateProperty(member, memberSerialization);
                fallbackProperty.PropertyName = alternateName;
                fallbackProperty.ShouldSerialize = _ => false;
                fallbackProperty.ShouldDeserialize = _ => true;
                properties.Add(fallbackProperty);
            }
        }

        return properties;
    }
}
