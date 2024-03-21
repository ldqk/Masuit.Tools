using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

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

    public NullableConcurrentDictionary(TValue fallbackValue) : base()
    {
        FallbackValue = fallbackValue;
    }

    public NullableConcurrentDictionary(int concurrencyLevel, int capacity) : base(concurrencyLevel, capacity)
    {
    }

    public NullableConcurrentDictionary(IEqualityComparer<NullObject<TKey>> comparer) : base(comparer)
    {
    }

    internal TValue FallbackValue { get; set; }

    public new TValue this[NullObject<TKey> key]
    {
        get => TryGetValue(key, out var value) ? value : FallbackValue;
        set => base[key] = value;
    }

    public TValue this[Func<KeyValuePair<TKey, TValue>, bool> condition]
    {
        get
        {
            foreach (var pair in this.Where(pair => condition(new KeyValuePair<TKey, TValue>(pair.Key.Item, pair.Value))))
            {
                return pair.Value;
            }

            return FallbackValue;
        }
        set
        {
            foreach (var pair in this.Where(pair => condition(new KeyValuePair<TKey, TValue>(pair.Key.Item, pair.Value))))
            {
                this[pair.Key] = value;
            }
        }
    }

    public virtual TValue this[Func<TKey, TValue, bool> condition]
    {
        get
        {
            foreach (var pair in this.Where(pair => condition(pair.Key.Item, pair.Value)))
            {
                return pair.Value;
            }

            return FallbackValue;
        }
        set
        {
            foreach (var pair in this.Where(pair => condition(pair.Key.Item, pair.Value)))
            {
                this[pair.Key] = value;
            }
        }
    }

    public virtual TValue this[Func<TKey, bool> condition]
    {
        get
        {
            foreach (var pair in this.Where(pair => condition(pair.Key.Item)))
            {
                return pair.Value;
            }

            return FallbackValue;
        }
        set
        {
            foreach (var pair in this.Where(pair => condition(pair.Key.Item)))
            {
                this[pair.Key] = value;
            }
        }
    }

    public virtual TValue this[Func<TValue, bool> condition]
    {
        get
        {
            foreach (var pair in this.Where(pair => condition(pair.Value)))
            {
                return pair.Value;
            }

            return FallbackValue;
        }
        set
        {
            foreach (var pair in this.Where(pair => condition(pair.Value)))
            {
                this[pair.Key] = value;
            }
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="key"></param>
    public virtual TValue this[TKey key]
    {
        get => TryGetValue(new NullObject<TKey>(key), out var value) ? value : FallbackValue;
        set => base[new NullObject<TKey>(key)] = value;
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