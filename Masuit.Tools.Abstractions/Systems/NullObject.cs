using System;

namespace Masuit.Tools.Systems;

/// <summary>
/// 可空对象
/// </summary>
/// <typeparam name="T"></typeparam>
public readonly record struct NullObject<T> : IComparable, IComparable<T>
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
        if (Item.IsDefaultValue())
        {
            return 0;
        }

        var result = Item.GetHashCode();

        if (result >= 0)
        {
            result++;
        }

        return result;
    }
}
