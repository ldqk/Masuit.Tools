using Masuit.Tools.Systems;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Masuit.Tools;

/// <summary>
/// 字典扩展
/// </summary>
public static class IDictionaryExtensions
{
    /// <summary>
    /// 添加或更新键值对
    /// </summary>
    /// <param name="this"></param>
    /// <param name="that">另一个字典集</param>
    public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> @this, IDictionary<TKey, TValue> that)
    {
        foreach (var item in that)
        {
            @this[item.Key] = item.Value;
        }
    }

    /// <summary>
    /// 添加或更新键值对
    /// </summary>
    /// <param name="this"></param>
    /// <param name="that">另一个字典集</param>
    public static void AddOrUpdate<TKey, TValue>(this NullableDictionary<TKey, TValue> @this, IDictionary<TKey, TValue> that)
    {
        foreach (var item in that)
        {
            @this[item.Key] = item.Value;
        }
    }

    /// <summary>
    /// 添加或更新键值对
    /// </summary>
    /// <param name="this"></param>
    /// <param name="that">另一个字典集</param>
    public static void AddOrUpdate<TKey, TValue>(this NullableConcurrentDictionary<TKey, TValue> @this, IDictionary<TKey, TValue> that)
    {
        foreach (var item in that)
        {
            @this[item.Key] = item.Value;
        }
    }

    /// <summary>
    /// 添加或更新键值对
    /// </summary>
    /// <param name="this"></param>
    /// <param name="key">键</param>
    /// <param name="addValue">添加时的值</param>
    /// <param name="updateValueFactory">更新时的操作</param>
    public static TValue AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
    {
        if (!@this.TryAdd(key, addValue))
        {
            @this[key] = updateValueFactory(key, @this[key]);
        }

        return @this[key];
    }

    /// <summary>
    /// 添加或更新键值对
    /// </summary>
    /// <param name="this"></param>
    /// <param name="key">键</param>
    /// <param name="addValue">添加时的值</param>
    /// <param name="updateValueFactory">更新时的操作</param>
    public static TValue AddOrUpdate<TKey, TValue>(this NullableDictionary<TKey, TValue> @this, TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
    {
        if (!@this.TryAdd(key, addValue))
        {
            @this[key] = updateValueFactory(key, @this[key]);
        }

        return @this[key];
    }

    /// <summary>
    /// 添加或更新键值对
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="this"></param>
    /// <param name="key">键</param>
    /// <param name="addValue">添加时的值</param>
    /// <param name="updateValueFactory">更新时的操作</param>
    public static TValue AddOrUpdate<TKey, TValue>(this NullableConcurrentDictionary<TKey, TValue> @this, TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
    {
        if (!@this.TryAdd(key, addValue))
        {
            @this[key] = updateValueFactory(key, @this[key]);
        }

        return @this[key];
    }

    /// <summary>
    /// 添加或更新键值对
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="this"></param>
    /// <param name="key">键</param>
    /// <param name="addValue">添加时的值</param>
    /// <param name="updateValue">更新时的值</param>
    public static TValue AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, TValue addValue, TValue updateValue)
    {
        if (!@this.TryAdd(key, addValue))
        {
            @this[key] = updateValue;
        }

        return @this[key];
    }

    /// <summary>
    /// 添加或更新键值对
    /// </summary>
    /// <param name="this"></param>
    /// <param name="key">键</param>
    /// <param name="addValue">添加时的值</param>
    /// <param name="updateValue">更新时的值</param>
    public static TValue AddOrUpdate<TKey, TValue>(this NullableDictionary<TKey, TValue> @this, TKey key, TValue addValue, TValue updateValue)
    {
        if (!@this.TryAdd(key, addValue))
        {
            @this[key] = updateValue;
        }

        return @this[key];
    }

    /// <summary>
    /// 添加或更新键值对
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="this"></param>
    /// <param name="key">键</param>
    /// <param name="addValue">添加时的值</param>
    /// <param name="updateValue">更新时的值</param>
    public static TValue AddOrUpdate<TKey, TValue>(this NullableConcurrentDictionary<TKey, TValue> @this, TKey key, TValue addValue, TValue updateValue)
    {
        if (!@this.TryAdd(key, addValue))
        {
            @this[key] = updateValue;
        }

        return @this[key];
    }

    /// <summary>
    /// 添加或更新键值对
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="this"></param>
    /// <param name="that">另一个字典集</param>
    /// <param name="updateValueFactory">更新时的操作</param>
    public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> @this, IDictionary<TKey, TValue> that, Func<TKey, TValue, TValue> updateValueFactory)
    {
        foreach (var item in that)
        {
            AddOrUpdate(@this, item.Key, item.Value, updateValueFactory);
        }
    }

    /// <summary>
    /// 添加或更新键值对
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="this"></param>
    /// <param name="that">另一个字典集</param>
    /// <param name="updateValueFactory">更新时的操作</param>
    public static void AddOrUpdate<TKey, TValue>(this NullableDictionary<TKey, TValue> @this, IDictionary<TKey, TValue> that, Func<TKey, TValue, TValue> updateValueFactory)
    {
        foreach (var item in that)
        {
            AddOrUpdate(@this, item.Key, item.Value, updateValueFactory);
        }
    }

    /// <summary>
    /// 添加或更新键值对
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="this"></param>
    /// <param name="that">另一个字典集</param>
    /// <param name="updateValueFactory">更新时的操作</param>
    public static void AddOrUpdate<TKey, TValue>(this NullableConcurrentDictionary<TKey, TValue> @this, IDictionary<TKey, TValue> that, Func<TKey, TValue, TValue> updateValueFactory)
    {
        foreach (var item in that)
        {
            AddOrUpdate(@this, item.Key, item.Value, updateValueFactory);
        }
    }

    /// <summary>
    /// 添加或更新键值对
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="this"></param>
    /// <param name="key">键</param>
    /// <param name="addValueFactory">添加时的操作</param>
    /// <param name="updateValueFactory">更新时的操作</param>
    public static TValue AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
    {
        if (!@this.TryAdd(key, addValueFactory(key)))
        {
            @this[key] = updateValueFactory(key, @this[key]);
        }

        return @this[key];
    }

    /// <summary>
    /// 添加或更新键值对
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="this"></param>
    /// <param name="key">键</param>
    /// <param name="addValueFactory">添加时的操作</param>
    /// <param name="updateValueFactory">更新时的操作</param>
    public static TValue AddOrUpdate<TKey, TValue>(this NullableDictionary<TKey, TValue> @this, TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
    {
        if (!@this.TryAdd(key, addValueFactory(key)))
        {
            @this[key] = updateValueFactory(key, @this[key]);
        }

        return @this[key];
    }

    /// <summary>
    /// 添加或更新键值对
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="this"></param>
    /// <param name="key">键</param>
    /// <param name="addValueFactory">添加时的操作</param>
    /// <param name="updateValueFactory">更新时的操作</param>
    public static TValue AddOrUpdate<TKey, TValue>(this NullableConcurrentDictionary<TKey, TValue> @this, TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
    {
        if (!@this.TryAdd(key, addValueFactory(key)))
        {
            @this[key] = updateValueFactory(key, @this[key]);
        }

        return @this[key];
    }

    /// <summary>
    /// 添加或更新键值对
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="this"></param>
    /// <param name="key">键</param>
    /// <param name="addValue">添加时的值</param>
    /// <param name="updateValueFactory">更新时的操作</param>
    public static async Task<TValue> AddOrUpdateAsync<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, TValue addValue, Func<TKey, TValue, Task<TValue>> updateValueFactory)
    {
        if (!@this.TryAdd(key, addValue))
        {
            @this[key] = await updateValueFactory(key, @this[key]);
        }

        return @this[key];
    }

    /// <summary>
    /// 添加或更新键值对
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="this"></param>
    /// <param name="key">键</param>
    /// <param name="addValue">添加时的值</param>
    /// <param name="updateValueFactory">更新时的操作</param>
    public static async Task<TValue> AddOrUpdateAsync<TKey, TValue>(this NullableDictionary<TKey, TValue> @this, TKey key, TValue addValue, Func<TKey, TValue, Task<TValue>> updateValueFactory)
    {
        if (!@this.TryAdd(key, addValue))
        {
            @this[key] = await updateValueFactory(key, @this[key]);
        }

        return @this[key];
    }

    /// <summary>
    /// 添加或更新键值对
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="this"></param>
    /// <param name="key">键</param>
    /// <param name="addValue">添加时的值</param>
    /// <param name="updateValueFactory">更新时的操作</param>
    public static async Task<TValue> AddOrUpdateAsync<TKey, TValue>(this NullableConcurrentDictionary<TKey, TValue> @this, TKey key, TValue addValue, Func<TKey, TValue, Task<TValue>> updateValueFactory)
    {
        if (!@this.TryAdd(key, addValue))
        {
            @this[key] = await updateValueFactory(key, @this[key]);
        }

        return @this[key];
    }

    /// <summary>
    /// 添加或更新键值对
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="this"></param>
    /// <param name="that">另一个字典集</param>
    /// <param name="updateValueFactory">更新时的操作</param>
    public static Task AddOrUpdateAsync<TKey, TValue>(this IDictionary<TKey, TValue> @this, IDictionary<TKey, TValue> that, Func<TKey, TValue, Task<TValue>> updateValueFactory, CancellationToken cancellationToken = default)
    {
        return that.ForeachAsync(item => AddOrUpdateAsync(@this, item.Key, item.Value, updateValueFactory), cancellationToken: cancellationToken);
    }

    /// <summary>
    /// 添加或更新键值对
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="this"></param>
    /// <param name="that">另一个字典集</param>
    /// <param name="updateValueFactory">更新时的操作</param>
    public static Task AddOrUpdateAsync<TKey, TValue>(this NullableDictionary<TKey, TValue> @this, IDictionary<TKey, TValue> that, Func<TKey, TValue, Task<TValue>> updateValueFactory, CancellationToken cancellationToken = default)
    {
        return that.ForeachAsync(item => AddOrUpdateAsync(@this, item.Key, item.Value, updateValueFactory), cancellationToken: cancellationToken);
    }

    /// <summary>
    /// 添加或更新键值对
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="this"></param>
    /// <param name="that">另一个字典集</param>
    /// <param name="updateValueFactory">更新时的操作</param>
    public static Task AddOrUpdateAsync<TKey, TValue>(this NullableConcurrentDictionary<TKey, TValue> @this, IDictionary<TKey, TValue> that, Func<TKey, TValue, Task<TValue>> updateValueFactory, CancellationToken cancellationToken = default)
    {
        return that.ForeachAsync(item => AddOrUpdateAsync(@this, item.Key, item.Value, updateValueFactory), cancellationToken: cancellationToken);
    }

    /// <summary>
    /// 添加或更新键值对
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="this"></param>
    /// <param name="key">键</param>
    /// <param name="addValueFactory">添加时的操作</param>
    /// <param name="updateValueFactory">更新时的操作</param>
    public static async Task<TValue> AddOrUpdateAsync<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, Func<TKey, Task<TValue>> addValueFactory, Func<TKey, TValue, Task<TValue>> updateValueFactory)
    {
        if (!@this.TryAdd(key, await addValueFactory(key)))
        {
            @this[key] = await updateValueFactory(key, @this[key]);
        }

        return @this[key];
    }

    /// <summary>
    /// 添加或更新键值对
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="this"></param>
    /// <param name="key">键</param>
    /// <param name="addValueFactory">添加时的操作</param>
    /// <param name="updateValueFactory">更新时的操作</param>
    public static async Task<TValue> AddOrUpdateAsync<TKey, TValue>(this NullableDictionary<TKey, TValue> @this, TKey key, Func<TKey, Task<TValue>> addValueFactory, Func<TKey, TValue, Task<TValue>> updateValueFactory)
    {
        if (!@this.TryAdd(key, await addValueFactory(key)))
        {
            @this[key] = await updateValueFactory(key, @this[key]);
        }

        return @this[key];
    }

    /// <summary>
    /// 添加或更新键值对
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="this"></param>
    /// <param name="key">键</param>
    /// <param name="addValueFactory">添加时的操作</param>
    /// <param name="updateValueFactory">更新时的操作</param>
    public static async Task<TValue> AddOrUpdateAsync<TKey, TValue>(this NullableConcurrentDictionary<TKey, TValue> @this, TKey key, Func<TKey, Task<TValue>> addValueFactory, Func<TKey, TValue, Task<TValue>> updateValueFactory)
    {
        if (!@this.TryAdd(key, await addValueFactory(key)))
        {
            @this[key] = await updateValueFactory(key, @this[key]);
        }

        return @this[key];
    }

    /// <summary>
    /// 添加或更新键值对
    /// </summary>
    /// <param name="this"></param>
    /// <param name="that">另一个字典集</param>
    public static void AddOrUpdateTo<TKey, TValue>(this IDictionary<TKey, TValue> @this, IDictionary<TKey, TValue> that)
    {
        foreach (var item in @this)
        {
            that[item.Key] = item.Value;
        }
    }

    /// <summary>
    /// 添加或更新键值对
    /// </summary>
    /// <param name="this"></param>
    /// <param name="that">另一个字典集</param>
    public static void AddOrUpdateTo<TKey, TValue>(this NullableDictionary<TKey, TValue> @this, IDictionary<TKey, TValue> that)
    {
        foreach (var item in @this)
        {
            that[item.Key] = item.Value;
        }
    }

    /// <summary>
    /// 添加或更新键值对
    /// </summary>
    /// <param name="this"></param>
    /// <param name="that">另一个字典集</param>
    public static void AddOrUpdateTo<TKey, TValue>(this NullableConcurrentDictionary<TKey, TValue> @this, IDictionary<TKey, TValue> that)
    {
        foreach (var item in @this)
        {
            that[item.Key] = item.Value;
        }
    }

    /// <summary>
    /// 添加或更新键值对
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="this"></param>
    /// <param name="that">另一个字典集</param>
    /// <param name="updateValueFactory">更新时的操作</param>
    public static void AddOrUpdateTo<TKey, TValue>(this IDictionary<TKey, TValue> @this, IDictionary<TKey, TValue> that, Func<TKey, TValue, TValue> updateValueFactory)
    {
        foreach (var item in @this)
        {
            AddOrUpdate(that, item.Key, item.Value, updateValueFactory);
        }
    }

    /// <summary>
    /// 添加或更新键值对
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="this"></param>
    /// <param name="that">另一个字典集</param>
    /// <param name="updateValueFactory">更新时的操作</param>
    public static void AddOrUpdateTo<TKey, TValue>(this IDictionary<TKey, TValue> @this, NullableDictionary<TKey, TValue> that, Func<TKey, TValue, TValue> updateValueFactory)
    {
        foreach (var item in @this)
        {
            AddOrUpdate(that, item.Key, item.Value, updateValueFactory);
        }
    }

    /// <summary>
    /// 添加或更新键值对
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="this"></param>
    /// <param name="that">另一个字典集</param>
    /// <param name="updateValueFactory">更新时的操作</param>
    public static void AddOrUpdateTo<TKey, TValue>(this IDictionary<TKey, TValue> @this, NullableConcurrentDictionary<TKey, TValue> that, Func<TKey, TValue, TValue> updateValueFactory)
    {
        foreach (var item in @this)
        {
            AddOrUpdate(that, item.Key, item.Value, updateValueFactory);
        }
    }

    /// <summary>
    /// 添加或更新键值对
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="this"></param>
    /// <param name="that">另一个字典集</param>
    /// <param name="updateValueFactory">更新时的操作</param>
    public static Task AddOrUpdateToAsync<TKey, TValue>(this IDictionary<TKey, TValue> @this, IDictionary<TKey, TValue> that, Func<TKey, TValue, Task<TValue>> updateValueFactory, CancellationToken cancellationToken = default)
    {
        return @this.ForeachAsync(item => AddOrUpdateAsync(that, item.Key, item.Value, updateValueFactory), cancellationToken: cancellationToken);
    }

    /// <summary>
    /// 添加或更新键值对
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="this"></param>
    /// <param name="that">另一个字典集</param>
    /// <param name="updateValueFactory">更新时的操作</param>
    public static Task AddOrUpdateToAsync<TKey, TValue>(this IDictionary<TKey, TValue> @this, NullableDictionary<TKey, TValue> that, Func<TKey, TValue, Task<TValue>> updateValueFactory, CancellationToken cancellationToken = default)
    {
        return @this.ForeachAsync(item => AddOrUpdateAsync(that, item.Key, item.Value, updateValueFactory), cancellationToken: cancellationToken);
    }

    /// <summary>
    /// 添加或更新键值对
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="this"></param>
    /// <param name="that">另一个字典集</param>
    /// <param name="updateValueFactory">更新时的操作</param>
    public static Task AddOrUpdateToAsync<TKey, TValue>(this IDictionary<TKey, TValue> @this, NullableConcurrentDictionary<TKey, TValue> that, Func<TKey, TValue, Task<TValue>> updateValueFactory, CancellationToken cancellationToken = default)
    {
        return @this.ForeachAsync(item => AddOrUpdateAsync(that, item.Key, item.Value, updateValueFactory), cancellationToken: cancellationToken);
    }

    /// <summary>
    /// 获取或添加
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="this"></param>
    /// <param name="key"></param>
    /// <param name="addValueFactory"></param>
    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, Func<TValue> addValueFactory)
    {
        if (!@this.ContainsKey(key))
        {
            @this[key] = addValueFactory();
        }

        return @this[key];
    }

    /// <summary>
    /// 获取或添加
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="this"></param>
    /// <param name="key"></param>
    /// <param name="addValue"></param>
    public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> @this, TKey key, TValue addValue)
    {
        return @this.TryAdd(key, addValue) ? addValue : @this[key];
    }

    /// <summary>
    /// 获取或添加
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="this"></param>
    /// <param name="key"></param>
    /// <param name="addValueFactory"></param>
    public static async Task<TValue> GetOrAddAsync<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, Func<Task<TValue>> addValueFactory)
    {
        if (!@this.ContainsKey(key))
        {
            @this[key] = await addValueFactory();
        }

        return @this[key];
    }

    private static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value) where TKey : notnull
    {
        if (dictionary == null)
            throw new ArgumentNullException(nameof(dictionary));
        if (dictionary.IsReadOnly || dictionary.ContainsKey(key))
            return false;
        dictionary.Add(key, value);
        return true;
    }

    /// <summary>
    /// 遍历IEnumerable
    /// </summary>
    /// <param name="dic"></param>
    /// <param name="action">回调方法</param>
    public static void ForEach<TKey, TValue>(this IDictionary<TKey, TValue> dic, Action<TKey, TValue> action)
    {
        foreach (var item in dic)
        {
            action(item.Key, item.Value);
        }
    }

    /// <summary>
    /// 遍历IDictionary
    /// </summary>
    /// <param name="dic"></param>
    /// <param name="action">回调方法</param>
    public static Task ForEachAsync<TKey, TValue>(this IDictionary<TKey, TValue> dic, Func<TKey, TValue, Task> action, CancellationToken cancellationToken = default)
    {
        return dic.ForeachAsync(x => action(x.Key, x.Value), cancellationToken: cancellationToken);
    }

    /// <summary>
    /// 安全的转换成字典集
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="source"></param>
    /// <param name="keySelector">键选择器</param>
    public static NullableDictionary<TKey, TSource> ToDictionarySafety<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
    {
        var items = source as IList<TSource> ?? source.ToList();
        var dic = new NullableDictionary<TKey, TSource>(items.Count);
        foreach (var item in items)
        {
            dic[keySelector(item)] = item;
        }

        return dic;
    }

    /// <summary>
    /// 安全的转换成字典集
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="source"></param>
    /// <param name="keySelector">键选择器</param>
    /// <param name="defaultValue">键未找到时的默认值</param>
    public static NullableDictionary<TKey, TSource> ToDictionarySafety<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, TSource defaultValue)
    {
        var items = source as IList<TSource> ?? source.ToList();
        var dic = new NullableDictionary<TKey, TSource>(items.Count)
        {
            FallbackValue = defaultValue
        };
        foreach (var item in items)
        {
            dic[keySelector(item)] = item;
        }

        return dic;
    }

    /// <summary>
    /// 安全的转换成字典集
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TElement"></typeparam>
    /// <param name="source"></param>
    /// <param name="keySelector">键选择器</param>
    /// <param name="elementSelector">值选择器</param>
    public static NullableDictionary<TKey, TElement> ToDictionarySafety<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
    {
        var items = source as IList<TSource> ?? source.ToList();
        var dic = new NullableDictionary<TKey, TElement>(items.Count);
        foreach (var item in items)
        {
            dic[keySelector(item)] = elementSelector(item);
        }

        return dic;
    }

    /// <summary>
    /// 安全的转换成字典集
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TElement"></typeparam>
    /// <param name="source"></param>
    /// <param name="keySelector">键选择器</param>
    /// <param name="elementSelector">值选择器</param>
    /// <param name="defaultValue">键未找到时的默认值</param>
    public static NullableDictionary<TKey, TElement> ToDictionarySafety<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, TElement defaultValue)
    {
        var items = source as IList<TSource> ?? source.ToList();
        var dic = new NullableDictionary<TKey, TElement>(items.Count)
        {
            FallbackValue = defaultValue
        };
        foreach (var item in items)
        {
            dic[keySelector(item)] = elementSelector(item);
        }

        return dic;
    }

    /// <summary>
    /// 安全的转换成字典集
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TElement"></typeparam>
    /// <param name="source"></param>
    /// <param name="keySelector">键选择器</param>
    /// <param name="elementSelector">值选择器</param>
    public static async Task<NullableDictionary<TKey, TElement>> ToDictionarySafetyAsync<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, Task<TElement>> elementSelector, CancellationToken cancellationToken = default)
    {
        var items = source as IList<TSource> ?? source.ToList();
        var dic = new NullableDictionary<TKey, TElement>(items.Count);
        await items.ForeachAsync(async item => dic[keySelector(item)] = await elementSelector(item), cancellationToken: cancellationToken);
        return dic;
    }

    /// <summary>
    /// 安全的转换成字典集
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TElement"></typeparam>
    /// <param name="source"></param>
    /// <param name="keySelector">键选择器</param>
    /// <param name="elementSelector">值选择器</param>
    /// <param name="defaultValue">键未找到时的默认值</param>
    public static async Task<NullableDictionary<TKey, TElement>> ToDictionarySafetyAsync<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, Task<TElement>> elementSelector, TElement defaultValue, CancellationToken cancellationToken = default)
    {
        var items = source as IList<TSource> ?? source.ToList();
        var dic = new NullableDictionary<TKey, TElement>(items.Count)
        {
            FallbackValue = defaultValue
        };
        await items.ForeachAsync(async item => dic[keySelector(item)] = await elementSelector(item), cancellationToken: cancellationToken);
        return dic;
    }

    /// <summary>
    /// 安全的转换成字典集
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="source"></param>
    /// <param name="keySelector">键选择器</param>
    public static DisposableDictionary<TKey, TSource> ToDisposableDictionarySafety<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) where TSource : IDisposable
    {
        var items = source as IList<TSource> ?? source.ToList();
        var dic = new DisposableDictionary<TKey, TSource>(items.Count);
        foreach (var item in items)
        {
            dic[keySelector(item)] = item;
        }

        return dic;
    }

    /// <summary>
    /// 安全的转换成字典集
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="source"></param>
    /// <param name="keySelector">键选择器</param>
    /// <param name="defaultValue">键未找到时的默认值</param>
    public static DisposableDictionary<TKey, TSource> ToDisposableDictionarySafety<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, TSource defaultValue) where TSource : IDisposable
    {
        var items = source as IList<TSource> ?? source.ToList();
        var dic = new DisposableDictionary<TKey, TSource>(items.Count)
        {
            FallbackValue = defaultValue
        };
        foreach (var item in items)
        {
            dic[keySelector(item)] = item;
        }

        return dic;
    }

    /// <summary>
    /// 安全的转换成字典集
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TElement"></typeparam>
    /// <param name="source"></param>
    /// <param name="keySelector">键选择器</param>
    /// <param name="elementSelector">值选择器</param>
    public static DisposableDictionary<TKey, TElement> ToDisposableDictionarySafety<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector) where TElement : IDisposable
    {
        var items = source as IList<TSource> ?? source.ToList();
        var dic = new DisposableDictionary<TKey, TElement>(items.Count);
        foreach (var item in items)
        {
            dic[keySelector(item)] = elementSelector(item);
        }

        return dic;
    }

    /// <summary>
    /// 安全的转换成字典集
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TElement"></typeparam>
    /// <param name="source"></param>
    /// <param name="keySelector">键选择器</param>
    /// <param name="elementSelector">值选择器</param>
    /// <param name="defaultValue">键未找到时的默认值</param>
    public static DisposableDictionary<TKey, TElement> ToDisposableDictionarySafety<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, TElement defaultValue) where TElement : IDisposable
    {
        var items = source as IList<TSource> ?? source.ToList();
        var dic = new DisposableDictionary<TKey, TElement>(items.Count)
        {
            FallbackValue = defaultValue
        };
        foreach (var item in items)
        {
            dic[keySelector(item)] = elementSelector(item);
        }

        return dic;
    }

    /// <summary>
    /// 安全的转换成字典集
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TElement"></typeparam>
    /// <param name="source"></param>
    /// <param name="keySelector">键选择器</param>
    /// <param name="elementSelector">值选择器</param>
    public static async Task<DisposableDictionary<TKey, TElement>> ToDisposableDictionarySafetyAsync<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, Task<TElement>> elementSelector, CancellationToken cancellationToken = default) where TElement : IDisposable
    {
        var items = source as IList<TSource> ?? source.ToList();
        var dic = new DisposableDictionary<TKey, TElement>(items.Count);
        await items.ForeachAsync(async item => dic[keySelector(item)] = await elementSelector(item), cancellationToken: cancellationToken);
        return dic;
    }

    /// <summary>
    /// 安全的转换成字典集
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TElement"></typeparam>
    /// <param name="source"></param>
    /// <param name="keySelector">键选择器</param>
    /// <param name="elementSelector">值选择器</param>
    /// <param name="defaultValue">键未找到时的默认值</param>
    public static async Task<DisposableDictionary<TKey, TElement>> ToDisposableDictionarySafetyAsync<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, Task<TElement>> elementSelector, TElement defaultValue, CancellationToken cancellationToken = default) where TElement : IDisposable
    {
        var items = source as IList<TSource> ?? source.ToList();
        var dic = new DisposableDictionary<TKey, TElement>(items.Count)
        {
            FallbackValue = defaultValue
        };
        await items.ForeachAsync(async item => dic[keySelector(item)] = await elementSelector(item), cancellationToken: cancellationToken);
        return dic;
    }

    /// <summary>
    /// 安全的转换成字典集
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="source"></param>
    /// <param name="keySelector">键选择器</param>
    public static NullableConcurrentDictionary<TKey, TSource> ToConcurrentDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
    {
        var dic = new NullableConcurrentDictionary<TKey, TSource>();
        foreach (var item in source)
        {
            dic[keySelector(item)] = item;
        }

        return dic;
    }

    /// <summary>
    /// 安全的转换成字典集
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="source"></param>
    /// <param name="keySelector">键选择器</param>
    /// <param name="defaultValue">键未找到时的默认值</param>
    public static NullableConcurrentDictionary<TKey, TSource> ToConcurrentDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, TSource defaultValue)
    {
        var dic = new NullableConcurrentDictionary<TKey, TSource>()
        {
            FallbackValue = defaultValue
        };
        foreach (var item in source)
        {
            dic[keySelector(item)] = item;
        }

        return dic;
    }

    /// <summary>
    /// 安全的转换成字典集
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TElement"></typeparam>
    /// <param name="source"></param>
    /// <param name="keySelector">键选择器</param>
    /// <param name="elementSelector">值选择器</param>
    public static NullableConcurrentDictionary<TKey, TElement> ToConcurrentDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
    {
        var dic = new NullableConcurrentDictionary<TKey, TElement>();
        foreach (var item in source)
        {
            dic[keySelector(item)] = elementSelector(item);
        }

        return dic;
    }

    /// <summary>
    /// 安全的转换成字典集
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TElement"></typeparam>
    /// <param name="source"></param>
    /// <param name="keySelector">键选择器</param>
    /// <param name="elementSelector">值选择器</param>
    /// <param name="defaultValue"></param>
    public static NullableConcurrentDictionary<TKey, TElement> ToConcurrentDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, TElement defaultValue)
    {
        var dic = new NullableConcurrentDictionary<TKey, TElement>()
        {
            FallbackValue = defaultValue
        };
        foreach (var item in source)
        {
            dic[keySelector(item)] = elementSelector(item);
        }

        return dic;
    }

    /// <summary>
    /// 安全的转换成字典集
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TElement"></typeparam>
    /// <param name="source"></param>
    /// <param name="keySelector">键选择器</param>
    /// <param name="elementSelector">值选择器</param>
    public static async Task<NullableConcurrentDictionary<TKey, TElement>> ToConcurrentDictionaryAsync<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, Task<TElement>> elementSelector, CancellationToken cancellationToken = default)
    {
        var dic = new ConcurrentDictionary<TKey, TElement>();
        await source.ForeachAsync(async item => dic[keySelector(item)] = await elementSelector(item), cancellationToken: cancellationToken);
        return dic;
    }

    /// <summary>
    /// 安全的转换成字典集
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TElement"></typeparam>
    /// <param name="source"></param>
    /// <param name="keySelector">键选择器</param>
    /// <param name="elementSelector">值选择器</param>
    /// <param name="defaultValue">键未找到时的默认值</param>
    public static async Task<NullableConcurrentDictionary<TKey, TElement>> ToConcurrentDictionaryAsync<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, Task<TElement>> elementSelector, TElement defaultValue, CancellationToken cancellationToken = default)
    {
        var dic = new NullableConcurrentDictionary<TKey, TElement>
        {
            FallbackValue = defaultValue
        };
        await source.ForeachAsync(async item => dic[keySelector(item)] = await elementSelector(item), cancellationToken: cancellationToken);
        return dic;
    }

    /// <summary>
    /// 安全的转换成字典集
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="source"></param>
    /// <param name="keySelector">键选择器</param>
    public static DisposableConcurrentDictionary<TKey, TSource> ToDisposableConcurrentDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) where TSource : IDisposable
    {
        var dic = new DisposableConcurrentDictionary<TKey, TSource>();
        foreach (var item in source)
        {
            dic[keySelector(item)] = item;
        }

        return dic;
    }

    /// <summary>
    /// 安全的转换成字典集
    /// </summary>
    /// <param name="source"></param>
    /// <param name="keySelector">键选择器</param>
    /// <param name="defaultValue">键未找到时的默认值</param>
    public static DisposableConcurrentDictionary<TKey, TSource> ToDisposableConcurrentDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, TSource defaultValue) where TSource : IDisposable
    {
        var dic = new DisposableConcurrentDictionary<TKey, TSource>
        {
            FallbackValue = defaultValue
        };
        foreach (var item in source)
        {
            dic[keySelector(item)] = item;
        }

        return dic;
    }

    /// <summary>
    /// 安全的转换成字典集
    /// </summary>
    /// <param name="source"></param>
    /// <param name="keySelector">键选择器</param>
    /// <param name="elementSelector">值选择器</param>
    public static DisposableConcurrentDictionary<TKey, TElement> ToDisposableConcurrentDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector) where TElement : IDisposable
    {
        var dic = new DisposableConcurrentDictionary<TKey, TElement>();
        foreach (var item in source)
        {
            dic[keySelector(item)] = elementSelector(item);
        }

        return dic;
    }

    /// <summary>
    /// 安全的转换成字典集
    /// </summary>
    /// <param name="source"></param>
    /// <param name="keySelector">键选择器</param>
    /// <param name="elementSelector">值选择器</param>
    /// <param name="defaultValue"></param>
    public static DisposableConcurrentDictionary<TKey, TElement> ToDisposableConcurrentDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, TElement defaultValue) where TElement : IDisposable
    {
        var dic = new DisposableConcurrentDictionary<TKey, TElement>()
        {
            FallbackValue = defaultValue
        };
        foreach (var item in source)
        {
            dic[keySelector(item)] = elementSelector(item);
        }

        return dic;
    }

    /// <summary>
    /// 安全的转换成字典集
    /// </summary>
    /// <param name="source"></param>
    /// <param name="keySelector">键选择器</param>
    /// <param name="elementSelector">值选择器</param>
    public static async Task<DisposableConcurrentDictionary<TKey, TElement>> ToDisposableConcurrentDictionaryAsync<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, Task<TElement>> elementSelector, CancellationToken cancellationToken = default) where TElement : IDisposable
    {
        var dic = new DisposableConcurrentDictionary<TKey, TElement>();
        await source.ForeachAsync(async item => dic[keySelector(item)] = await elementSelector(item), cancellationToken: cancellationToken);
        return dic;
    }

    /// <summary>
    /// 安全的转换成字典集
    /// </summary>
    /// <param name="source"></param>
    /// <param name="keySelector">键选择器</param>
    /// <param name="elementSelector">值选择器</param>
    /// <param name="defaultValue">键未找到时的默认值</param>
    public static async Task<DisposableConcurrentDictionary<TKey, TElement>> ToDisposableConcurrentDictionaryAsync<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, Task<TElement>> elementSelector, TElement defaultValue, CancellationToken cancellationToken = default) where TElement : IDisposable
    {
        var dic = new DisposableConcurrentDictionary<TKey, TElement>()
        {
            FallbackValue = defaultValue
        };
        await source.ForeachAsync(async item => dic[keySelector(item)] = await elementSelector(item), cancellationToken: cancellationToken);
        return dic;
    }

    /// <summary>
    /// 转换为Lookup
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="source"></param>
    /// <param name="keySelector">键选择器</param>
    public static LookupX<TKey, TSource> ToLookupX<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
    {
        var items = source as IList<TSource> ?? source.ToList();
        var dic = new Dictionary<TKey, List<TSource>>(items.Count);
        foreach (var item in items)
        {
            var key = keySelector(item);
            if (dic.TryGetValue(key, out var list))
            {
                list.Add(item);
            }
            else
            {
                dic.Add(key, new List<TSource> { item });
            }
        }

        return new LookupX<TKey, TSource>(dic);
    }

    /// <summary>
    /// 转换为Lookup
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TElement"></typeparam>
    /// <param name="source"></param>
    /// <param name="keySelector">键选择器</param>
    /// <param name="elementSelector">值选择器</param>
    public static LookupX<TKey, TElement> ToLookupX<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
    {
        var items = source as IList<TSource> ?? source.ToList();
        var dic = new Dictionary<TKey, List<TElement>>(items.Count);
        foreach (var item in items)
        {
            var key = keySelector(item);
            if (dic.TryGetValue(key, out var list))
            {
                list.Add(elementSelector(item));
            }
            else
            {
                dic.Add(key, new List<TElement> { elementSelector(item) });
            }
        }

        return new LookupX<TKey, TElement>(dic);
    }

    /// <summary>
    /// 转换为Lookup
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TElement"></typeparam>
    /// <param name="source"></param>
    /// <param name="keySelector">键选择器</param>
    /// <param name="elementSelector">值选择器</param>
    public static async Task<LookupX<TKey, TElement>> ToLookupAsync<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, Task<TElement>> elementSelector, CancellationToken cancellationToken = default)
    {
        var items = source as IList<TSource> ?? source.ToList();
        var dic = new ConcurrentDictionary<TKey, List<TElement>>();
        await items.ForeachAsync(async item =>
        {
            var key = keySelector(item);
            if (dic.TryGetValue(key, out var list))
            {
                list.Add(await elementSelector(item));
            }
            else
            {
                dic.TryAdd(key, new List<TElement> { await elementSelector(item) });
            }
        }, cancellationToken: cancellationToken);
        return new LookupX<TKey, TElement>(dic);
    }

    /// <summary>
    /// 转换成并发字典集合
    /// </summary>
    public static NullableConcurrentDictionary<TKey, TValue> AsConcurrentDictionary<TKey, TValue>(this Dictionary<TKey, TValue> dic) => dic;

    /// <summary>
    /// 转换成并发字典集合
    /// </summary>
    /// <param name="dic"></param>
    /// <param name="defaultValue">键未找到时的默认值</param>
    public static NullableConcurrentDictionary<TKey, TValue> AsConcurrentDictionary<TKey, TValue>(this Dictionary<TKey, TValue> dic, TValue defaultValue)
    {
        var nullableDictionary = new NullableConcurrentDictionary<TKey, TValue>()
        {
            FallbackValue = defaultValue
        };
        foreach (var p in dic)
        {
            nullableDictionary[p.Key] = p.Value;
        }

        return nullableDictionary;
    }

    /// <summary>
    /// 转换成普通字典集合
    /// </summary>
    public static NullableDictionary<TKey, TValue> AsDictionary<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dic) => dic;

    /// <summary>
    /// 转换成普通字典集合
    /// </summary>
    /// <param name="dic"></param>
    /// <param name="defaultValue">键未找到时的默认值</param>
    public static NullableDictionary<TKey, TValue> AsDictionary<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dic, TValue defaultValue)
    {
        var nullableDictionary = new NullableDictionary<TKey, TValue>()
        {
            FallbackValue = defaultValue
        };
        foreach (var p in dic)
        {
            nullableDictionary[p.Key] = p.Value;
        }

        return nullableDictionary;
    }
}