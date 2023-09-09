#if NET5_0_OR_GREATER
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Masuit.Tools.Systems;

public class SerializeIgnoreResolver : DefaultJsonTypeInfoResolver
{
	public override JsonTypeInfo GetTypeInfo(Type t, JsonSerializerOptions o)
	{
		var jti = base.GetTypeInfo(t, o);
		foreach (var prop in jti.Properties)
		{
			if (prop.AttributeProvider.GetCustomAttributes(typeof(DeserializeOnlyJsonPropertyAttribute), true).Union(prop.AttributeProvider.GetCustomAttributes(typeof(SerializeIgnoreAttribute), true)).Any())
			{
				prop.ShouldSerialize = (_, _) => false;
			}
		}

		return jti;
	}
}
#endif
