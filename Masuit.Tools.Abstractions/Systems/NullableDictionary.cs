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
public class NullableDictionary<TKey, TValue> : Dictionary<NullObject<TKey>, TValue>
{
    public NullableDictionary()
    {
    }

    public NullableDictionary(TValue fallbackValue)
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

    public virtual TValue this[Func<KeyValuePair<TKey, TValue>, bool> condition]
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

    public bool ContainsKey(TKey key)
    {
        return base.ContainsKey(new NullObject<TKey>(key));
    }

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