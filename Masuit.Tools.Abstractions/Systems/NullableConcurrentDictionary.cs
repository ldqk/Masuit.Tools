using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Masuit.Tools.Systems;

/// <summary>
/// 支持null-key和value的字典类型
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
public class NullableConcurrentDictionary<TKey, TValue> : ConcurrentDictionary<NullObject<TKey>, TValue>
{
    public NullableConcurrentDictionary() : base()
    {
    }

    public NullableConcurrentDictionary(int concurrencyLevel, int capacity) : base(concurrencyLevel, capacity)
    {
    }

    public NullableConcurrentDictionary(IEqualityComparer<NullObject<TKey>> comparer) : base(comparer)
    {
    }

    public new TValue this[NullObject<TKey> key]
    {
        get => TryGetValue(key, out var value) ? value : default;
        set => base[key] = value;
    }

    /// <summary>
    /// 隐式转换
    /// </summary>
    /// <param name="dic"></param>
    public static implicit operator NullableConcurrentDictionary<TKey, TValue>(Dictionary<TKey, TValue> dic)
    {
        var nullableDictionary = new NullableConcurrentDictionary<TKey, TValue>();
        foreach (var p in dic)
        {
            nullableDictionary[p.Key] = p.Value;
        }
        return nullableDictionary;
    }

    /// <summary>
    /// 隐式转换
    /// </summary>
    /// <param name="dic"></param>
    public static implicit operator NullableConcurrentDictionary<TKey, TValue>(ConcurrentDictionary<TKey, TValue> dic)
    {
        var nullableDictionary = new NullableConcurrentDictionary<TKey, TValue>();
        foreach (var p in dic)
        {
            nullableDictionary[p.Key] = p.Value;
        }
        return nullableDictionary;
    }

    /// <summary>
    /// 隐式转换
    /// </summary>
    /// <param name="dic"></param>
    public static implicit operator ConcurrentDictionary<TKey, TValue>(NullableConcurrentDictionary<TKey, TValue> dic)
    {
        var newdic = new ConcurrentDictionary<TKey, TValue>();
        foreach (var p in dic)
        {
            newdic[p.Key] = p.Value;
        }
        return newdic;
    }
}
