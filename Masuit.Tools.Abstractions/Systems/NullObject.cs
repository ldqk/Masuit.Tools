using System;
using System.Collections.Generic;

namespace Masuit.Tools.Systems;

/// <summary>
/// 可空对象
/// </summary>
/// <typeparam name="T"></typeparam>
public readonly record struct NullObject<T> : IComparable, IComparable<T>, IEquatable<NullObject<T>>
{
    public NullObject()
    {
    }

    public NullObject(T item)
    {
        Item = item;
    }

    public static NullObject<T> Null => new();

    public T Item { get; }

    /// <summary>
    /// 是否是null
    /// </summary>
    /// <returns></returns>
    public bool IsNull => Item.IsDefaultValue();

    /// <summary>
    /// 隐式转换
    /// </summary>
    /// <param name="nullObject"></param>
    public static implicit operator T(NullObject<T> nullObject)
    {
        return nullObject.Item;
    }

    /// <summary>
    /// 隐式转换
    /// </summary>
    /// <param name="item"></param>
    public static implicit operator NullObject<T>(T item)
    {
        return new NullObject<T>(item);
    }

    public override string ToString()
    {
        return (Item != null) ? Item.ToString() : "NULL";
    }

    public int CompareTo(object value)
    {
        if (value is NullObject<T> nullObject)
        {
            if (nullObject.Item is IComparable c)
            {
                return ((IComparable)Item).CompareTo(c);
            }

            return Item.ToString().CompareTo(nullObject.Item.ToString());
        }

        return 1;
    }

    public int CompareTo(T other)
    {
        if (other is IComparable c)
        {
            return ((IComparable)Item).CompareTo(c);
        }

        return Item.ToString().CompareTo(other.ToString());
    }

    public override int GetHashCode()
    {
        return EqualityComparer<T>.Default.GetHashCode(Item);
    }

    /// <summary>指示当前对象是否等于同一类型的另一个对象。</summary>
    /// <param name="other">一个与此对象进行比较的对象。</param>
    /// <returns>如果当前对象等于 <paramref name="other" /> 参数，则为 true；否则为 false。</returns>
    public bool Equals(NullObject<T> other)
    {
        return EqualityComparer<T>.Default.Equals(Item, other.Item);
    }
}