using System;
using System.ComponentModel;
using System.Globalization;

namespace Masuit.Tools;

public static class IConvertibleExtensions
{
    public static bool IsNumeric(this Type type)
    {
        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Single:
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// 类型直转
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>

    public static T ConvertTo<T>(this IConvertible value) where T : IConvertible
    {
        if (value != null)
        {
            var type = typeof(T);
            if (value.GetType() == type)
            {
                return (T)value;
            }

            if (type.IsEnum)
            {
                return (T)Enum.Parse(type, value.ToString(CultureInfo.InvariantCulture));
            }

            if (value == DBNull.Value)
            {
                return default;
            }

            if (type.IsNumeric())
            {
                return (T)value.ToType(type, new NumberFormatInfo());
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var underlyingType = Nullable.GetUnderlyingType(type);
                return (T)(underlyingType!.IsEnum ? Enum.Parse(underlyingType, value.ToString(CultureInfo.CurrentCulture)) : Convert.ChangeType(value, underlyingType));
            }

            var converter = TypeDescriptor.GetConverter(value);
            if (converter.CanConvertTo(type))
            {
                return (T)converter.ConvertTo(value, type);
            }

            converter = TypeDescriptor.GetConverter(type);
            if (converter.CanConvertFrom(value.GetType()))
            {
                return (T)converter.ConvertFrom(value);
            }

            return (T)Convert.ChangeType(value, type);
        }

        return (T)value;
    }

    /// <summary>
    /// 类型直转
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <param name="defaultValue">转换失败的默认值</param>
    /// <returns></returns>
    public static T TryConvertTo<T>(this IConvertible value, T defaultValue = default) where T : IConvertible
    {
        try
        {
            return ConvertTo<T>(value);
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// 类型直转
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <param name="result">转换失败的默认值</param>
    /// <returns></returns>
    public static bool TryConvertTo<T>(this IConvertible value, out T result) where T : IConvertible
    {
        try
        {
            result = ConvertTo<T>(value);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    /// <summary>
    /// 类型直转
    /// </summary>
    /// <param name="value"></param>
    /// <param name="type">目标类型</param>
    /// <param name="result">转换失败的默认值</param>
    /// <returns></returns>
    public static bool TryConvertTo(this IConvertible value, Type type, out object result)
    {
        try
        {
            result = ConvertTo(value, type);
            return true;
        }
        catch
        {
            result = null;
            return false;
        }
    }

    /// <summary>
    /// 类型直转
    /// </summary>
    /// <param name="value"></param>
    /// <param name="type">目标类型</param>
    /// <returns></returns>
    public static object ConvertTo(this IConvertible value, Type type)
    {
        if (value == null)
        {
            return null;
        }

        if (value.GetType() == type)
        {
            return value;
        }

        if (value == DBNull.Value)
        {
            return null;
        }

        if (type.IsAssignableFrom(typeof(string)))
        {
            return value.ToString();
        }

        if (type.IsEnum)
        {
            return Enum.Parse(type, value.ToString(CultureInfo.InvariantCulture));
        }

        if (type.IsAssignableFrom(typeof(Guid)))
        {
            return Guid.Parse(value.ToString());
        }

        if (type.IsAssignableFrom(typeof(DateTime)))
        {
            return DateTime.Parse(value.ToString());
        }

        if (type.IsAssignableFrom(typeof(DateTimeOffset)))
        {
            return DateTimeOffset.Parse(value.ToString());
        }

        if (type.IsNumeric())
        {
            return value.ToType(type, new NumberFormatInfo());
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            var underlyingType = Nullable.GetUnderlyingType(type);
            return underlyingType!.IsEnum ? Enum.Parse(underlyingType, value.ToString(CultureInfo.CurrentCulture)) : Convert.ChangeType(value, underlyingType);
        }

        var converter = TypeDescriptor.GetConverter(value);
        if (converter.CanConvertTo(type))
        {
            return converter.ConvertTo(value, type);
        }

        converter = TypeDescriptor.GetConverter(type);
        return converter.CanConvertFrom(value.GetType()) ? converter.ConvertFrom(value) : Convert.ChangeType(value, type);
    }

    /// <summary>
    /// 对象类型转换
    /// </summary>
    /// <param name="this">当前值</param>
    /// <returns>转换后的对象</returns>
    public static T ChangeTypeTo<T>(this object @this)
    {
        return (T)ChangeType(@this, typeof(T));
    }

    /// <summary>
    /// 对象类型转换
    /// </summary>
    /// <param name="this">当前值</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns>转换后的对象</returns>
    public static T ChangeTypeTo<T>(this object @this, T defaultValue)
    {
        return @this == null ? defaultValue : (T)ChangeType(@this, typeof(T));
    }

    /// <summary>
    /// 对象类型转换
    /// </summary>
    /// <param name="this">当前值</param>
    /// <param name="type">指定类型的类型</param>
    /// <returns>转换后的对象</returns>
    public static object ChangeType(this object @this, Type type)
    {
        var currType = Nullable.GetUnderlyingType(@this.GetType()) ?? @this.GetType();
        type = Nullable.GetUnderlyingType(type) ?? type;
        if (@this == DBNull.Value)
        {
            if (!type.IsValueType)
            {
                return null;
            }

            throw new Exception("不能将null值转换为" + type.Name + "类型!");
        }

        if (currType == type)
        {
            return @this;
        }

        if (type.IsAssignableFrom(typeof(string)))
        {
            return @this.ToString();
        }

        if (type.IsEnum)
        {
            return Enum.Parse(type, @this.ToString(), true);
        }

        if (type.IsAssignableFrom(typeof(Guid)))
        {
            return Guid.Parse(@this.ToString());
        }

        if (!type.IsArray || !currType.IsArray)
        {
            return Convert.ChangeType(@this, type);
        }

        var length = ((Array)@this).Length;
        var targetType = Type.GetType(type.FullName.Trim('[', ']'));
        var array = Array.CreateInstance(targetType, length);
        for (int j = 0; j < length; j++)
        {
            var tmp = ((Array)@this).GetValue(j);
            array.SetValue(ChangeType(tmp, targetType), j);
        }

        return array;
    }
}