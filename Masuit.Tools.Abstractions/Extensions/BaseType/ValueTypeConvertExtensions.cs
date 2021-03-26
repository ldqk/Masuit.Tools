using System;

namespace Masuit.Tools
{
    public static class ValueTypeConvertExtensions
    {
        /// <summary>
        /// 字符串转int
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <param name="defaultValue">转换失败的默认值</param>
        /// <returns>int类型的数字</returns>
        public static int ToInt32(this string s, int defaultValue = 0)
        {
            return s.TryConvertTo(defaultValue);
        }

        /// <summary>
        /// 字符串转long
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <param name="defaultValue">转换失败的默认值</param>
        /// <returns>int类型的数字</returns>
        public static long ToInt64(this string s, long defaultValue = 0)
        {
            return s.TryConvertTo(defaultValue);
        }

        /// <summary>
        /// 字符串转double
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <param name="defaultValue">转换失败的默认值</param>
        /// <returns>double类型的数据</returns>
        public static double ToDouble(this string s, double defaultValue = 0)
        {
            return s.TryConvertTo(defaultValue);
        }

        /// <summary>
        /// 字符串转decimal
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <param name="defaultValue">转换失败的默认值</param>
        /// <returns>int类型的数字</returns>
        public static decimal ToDecimal(this string s, decimal defaultValue = 0)
        {
            return s.TryConvertTo(defaultValue);
        }

        /// <summary>
        /// 字符串转decimal
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <param name="round">小数位数</param>
        /// <param name="defaultValue">转换失败的默认值</param>
        /// <returns>int类型的数字</returns>
        public static decimal ToDecimal(this string s, int round, decimal defaultValue = 0)
        {
            return Math.Round(s.TryConvertTo(defaultValue), round);
        }

        /// <summary>
        /// 转double
        /// </summary>
        /// <param name="s"></param>
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
            return new decimal(num);
        }
    }
}