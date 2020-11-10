using System;
using System.Globalization;

namespace Masuit.Tools
{
    public static class IConvertibleExtensions
    {
        /// <summary>
        /// 类型直转
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="convertibleValue"></param>
        /// <returns></returns>

        public static T ConvertTo<T>(this IConvertible convertibleValue) where T : IConvertible
        {
            return (T)ConvertTo(convertibleValue, typeof(T));
        }

        /// <summary>
        /// 类型直转
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="convertibleValue"></param>
        /// <param name="defaultValue">转换失败的默认值</param>
        /// <returns></returns>
        public static T TryConvertTo<T>(this IConvertible convertibleValue, T defaultValue = default) where T : IConvertible
        {
            try
            {
                return (T)ConvertTo(convertibleValue, typeof(T));
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
        /// <param name="convertibleValue"></param>
        /// <param name="value">转换失败的默认值</param>
        /// <returns></returns>
        public static bool TryConvertTo<T>(this IConvertible convertibleValue, out T value) where T : IConvertible
        {
            try
            {
                value = (T)ConvertTo(convertibleValue, typeof(T));
                return true;
            }
            catch
            {
                value = default;
                return false;
            }
        }

        /// <summary>
        /// 类型直转
        /// </summary>
        /// <param name="convertibleValue"></param>
        /// <param name="type">目标类型</param>
        /// <param name="value">转换失败的默认值</param>
        /// <returns></returns>
        public static bool TryConvertTo(this IConvertible convertibleValue, Type type, out object value)
        {
            try
            {
                value = ConvertTo(convertibleValue, type);
                return true;
            }
            catch
            {
                value = default;
                return false;
            }
        }

        /// <summary>
        /// 类型直转
        /// </summary>
        /// <param name="convertibleValue"></param>
        /// <param name="type">目标类型</param>
        /// <returns></returns>
        public static object ConvertTo(this IConvertible convertibleValue, Type type)
        {
            if (null == convertibleValue)
            {
                return default;
            }

            if (type.IsEnum)
            {
                return Enum.Parse(type, convertibleValue.ToString(CultureInfo.InvariantCulture));
            }

            if (!type.IsGenericType)
            {
                return Convert.ChangeType(convertibleValue, type);
            }

            if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var underlyingType = Nullable.GetUnderlyingType(type);
                return underlyingType!.IsEnum ? Enum.Parse(underlyingType, convertibleValue.ToString(CultureInfo.CurrentCulture)) : Convert.ChangeType(convertibleValue, underlyingType);
            }

            throw new InvalidCastException($"不能将类型 \"{convertibleValue.GetType().FullName}\" 转换为 \"{type.FullName}\"");
        }
    }
}