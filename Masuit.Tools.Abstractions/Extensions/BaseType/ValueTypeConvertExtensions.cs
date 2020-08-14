using System;
using System.Collections.Generic;
using System.Text;

namespace Masuit.Tools
{
#pragma warning disable IDE0060 // 删除未使用的参数

    public static class ValueTypeConvertExtensions
    {  /// <summary>
       /// 字符串转int
       /// </summary>
       /// <param name="s">源字符串</param>
       /// <param name="defaultValue">失败时返回的值</param>
       /// <returns>int类型的数字</returns>
        public static int ToInt32(this string s, int defaultValue = default)

        {
            int.TryParse(s, out defaultValue);
            return defaultValue;
        }

        /// <summary>
        /// 字符串转long
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <param name="defaultValue">失败时返回的值</param>
        /// <returns>int类型的数字</returns>
        public static long ToInt64(this string s, long defaultValue = default)
        {
            return s.ToLong(defaultValue);
        }

        /// <summary>
        /// 字符串转long类型
        /// </summary>
        /// <param name="str"></param>
        /// <param name="defaultResult">转换失败的默认值</param>
        /// <returns></returns>
        public static long ToLong(this string s, long defaultValue = default)
        {
            long.TryParse(s, out defaultValue);
            return defaultValue;
        }

        /// <summary>
        /// 字符串转double
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <param name="defaultValue">失败时返回的值</param>
        /// <returns>double类型的数据</returns>
        public static double ToDouble(this string s, double defaultValue = default)
        {
            double.TryParse(s, out defaultValue);
            return defaultValue;
        }

        /// <summary>
        /// 字符串转decimal
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <param name="defaultValue">失败时返回的值</param>
        /// <returns>int类型的数字</returns>
        public static decimal ToDecimal(this string s, decimal defaultValue = default)
        {
            decimal.TryParse(s, out defaultValue);
            return defaultValue;
        }

        /// <summary>
        /// 字符串转decimal
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <returns>int类型的数字</returns>
        public static decimal ToDecimal(this double s)
        {
            return new decimal(s);
        }

        /// <summary>
        /// 字符串转double
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <returns>double类型的数据</returns>
        public static double ToDouble(this decimal s)
        {
            return (double)s;
        }

        /// <summary>
        /// 将double转换成int
        /// </summary>
        /// <param name="num">double类型</param>
        /// <returns>int类型</returns>
        public static int ToInt32(this double num)
        {
            return (int)Math.Floor(num);
        }

        /// <summary>
        /// 将double转换成int
        /// </summary>
        /// <param name="num">double类型</param>
        /// <returns>int类型</returns>
        public static int ToInt32(this decimal num)
        {
            return (int)Math.Floor(num);
        }

        /// <summary>
        /// 将int转换成double
        /// </summary>
        /// <param name="num">int类型</param>
        /// <returns>int类型</returns>
        public static double ToDouble(this int num)
        {
            return num * 1.0;
        }

        /// <summary>
        /// 将int转换成decimal
        /// </summary>
        /// <param name="num">int类型</param>
        /// <returns>int类型</returns>
        public static decimal ToDecimal(this int num)
        {
            return (decimal)(num * 1.0);
        }
    }

#pragma warning restore IDE0060 // 删除未使用的参数
}