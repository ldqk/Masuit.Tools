#if NET5_0_OR_GREATER
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Masuit.Tools.Systems;

public class SerializeSortByNameResolver : DefaultJsonTypeInfoResolver
{
	public override JsonTypeInfo GetTypeInfo(Type t, JsonSerializerOptions o)
	{
		var jti = base.GetTypeInfo(t, o);
        int order = 1;

        foreach (var property in jti.Properties.OrderBy(p => p.Name))
        {
            property.Order = order++;
        }
        
        return jti;
	}
}
#endif
