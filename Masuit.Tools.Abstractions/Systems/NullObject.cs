using System;
using System.ComponentModel;

namespace Masuit.Tools.Systems;

/// <summary>
/// 可空对象
/// </summary>
/// <typeparam name="T"></typeparam>
public readonly struct NullObject<T> : IComparable, IComparable<T>
{
    [DefaultValue(true)]
    private readonly bool _isnull;

    private NullObject(T item, bool isnull) : this()
    {
        _isnull = isnull;
        Item = item;
    }

    public NullObject(T item) : this(item, item == null)
    {
    }

    public static NullObject<T> Null()
    {
        return new NullObject<T>();
    }

    public T Item { get; }

    /// <summary>
    /// 是否是null
    /// </summary>
    /// <returns></returns>
    public bool IsNull()
    {
        return _isnull;
    }

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

    public override bool Equals(object obj)
    {
        if (obj == null)
        {
            return IsNull();
        }

        if (obj is not NullObject<T> nullObject)
        {
            return false;
        }

        if (IsNull())
        {
            return nullObject.IsNull();
        }

        if (nullObject.IsNull())
        {
            return false;
        }

        return Item.Equals(nullObject.Item);
    }

    public override int GetHashCode()
    {
        if (_isnull)
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
