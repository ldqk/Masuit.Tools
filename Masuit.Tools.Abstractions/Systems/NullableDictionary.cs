using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Masuit.Tools.Systems;

/// <summary>
/// 支持null-key和value的字典类型
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
public class NullableDictionary<TKey, TValue> : Dictionary<NullObject<TKey>, TValue>
{
    public NullableDictionary() : base()
    {
    }

    public NullableDictionary(TValue fallbackValue) : base()
    {
        FallbackValue = fallbackValue;
    }

    public NullableDictionary(int capacity) : base(capacity)
    {
    }

    public NullableDictionary(IEqualityComparer<NullObject<TKey>> comparer) : base(comparer)
    {
    }

    public NullableDictionary(int capacity, IEqualityComparer<NullObject<TKey>> comparer) : base(capacity, comparer)
    {
    }

    public NullableDictionary(IDictionary<NullObject<TKey>, TValue> dictionary) : base(dictionary)
    {
    }

    public NullableDictionary(IDictionary<NullObject<TKey>, TValue> dictionary, IEqualityComparer<NullObject<TKey>> comparer) : base(dictionary, comparer)
    {
    }

    internal TValue FallbackValue { get; set; }

    /// <summary>
    ///
    /// </summary>
    /// <param name="key"></param>
    public new TValue this[NullObject<TKey> key]
    {
        get => TryGetValue(key, out var value) ? value : FallbackValue;
        set => base[key] = value;
    }

    ///// <summary>
    /////
    ///// </summary>
    ///// <param name="key"></param>
    //public virtual TValue this[TKey key]
    //{
    //    get => TryGetValue(key, out var value) ? value : FallbackValue;
    //    set => base[key] = value;
    //}

    /// <summary>
    /// 隐式转换
    /// </summary>
    /// <param name="dic"></param>
    public static implicit operator NullableDictionary<TKey, TValue>(Dictionary<TKey, TValue> dic)
    {
        var nullableDictionary = new NullableDictionary<TKey, TValue>();
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
    public static implicit operator NullableDictionary<TKey, TValue>(ConcurrentDictionary<TKey, TValue> dic)
    {
        var nullableDictionary = new NullableDictionary<TKey, TValue>();
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
    public static implicit operator Dictionary<TKey, TValue>(NullableDictionary<TKey, TValue> dic)
    {
        var newdic = new Dictionary<TKey, TValue>();
        foreach (var p in dic)
        {
            newdic[p.Key] = p.Value;
        }
        return newdic;
    }
}
