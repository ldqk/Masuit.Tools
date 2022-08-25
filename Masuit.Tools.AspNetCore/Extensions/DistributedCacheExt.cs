using System;
using System.Text;
using Masuit.Tools.Systems;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Masuit.Tools.AspNetCore.Extensions;

/// <summary>
/// 
/// </summary>
public static class DistributedCacheExt
{
    public static T GetOrAdd<T>(this IDistributedCache cache, string key, Func<T> valueFactory)
    {
        var bytes = cache.Get(key);
        if (bytes is null)
        {
            var value = new NullObject<T>(valueFactory());
            bytes = Encoding.UTF8.GetBytes(value.ToJsonString());
            cache.Set(key, bytes);
            return value;
        }

        return JsonConvert.DeserializeObject<NullObject<T>>(Encoding.UTF8.GetString(bytes));
    }

    public static T Get<T>(this IDistributedCache cache, string key)
    {
        var bytes = cache.Get(key);
        if (bytes is null)
        {
            return default;
        }

        return JsonConvert.DeserializeObject<NullObject<T>>(Encoding.UTF8.GetString(bytes));
    }

    public static void Set<T>(this IDistributedCache cache, string key, T value)
    {
        cache.Set(key, Encoding.UTF8.GetBytes(new NullObject<T>(value).ToJsonString()), new DistributedCacheEntryOptions());
    }

    public static void Set<T>(this IDistributedCache cache, string key, T value, DistributedCacheEntryOptions options)
    {
        cache.Set(key, Encoding.UTF8.GetBytes(new NullObject<T>(value).ToJsonString()), options);
    }

    public static T GetOrAdd<T>(this IDistributedCache cache, string key, T value)
    {
        var bytes = cache.Get(key);
        if (bytes is null)
        {
            bytes = Encoding.UTF8.GetBytes(new NullObject<T>(value).ToJsonString());
            cache.Set(key, bytes);
            return value;
        }

        return JsonConvert.DeserializeObject<NullObject<T>>(Encoding.UTF8.GetString(bytes));
    }

    public static void AddOrUpdate<T>(this IDistributedCache cache, string key, T addValue, Func<T, T> updateFunc)
    {
        var bytes = cache.Get(key);
        if (bytes is null)
        {
            bytes = Encoding.UTF8.GetBytes(new NullObject<T>(addValue).ToJsonString());
            cache.Set(key, bytes);
            return;
        }

        var value = new NullObject<T>(updateFunc(JsonConvert.DeserializeObject<NullObject<T>>(Encoding.UTF8.GetString(bytes))));
        cache.Set(key, Encoding.UTF8.GetBytes(value.ToJsonString()));
    }
}