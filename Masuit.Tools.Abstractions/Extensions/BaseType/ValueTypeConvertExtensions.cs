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
            return Math.Round(s.TryConvertTo(defaultValue), round, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// 转double
        /// </summary>
        /// <param name="num"></param>
        /// <returns>double类型的数据</returns>
        public static double ToDouble(this decimal num)
        {
            return (double)num;
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

        /// <summary>
        /// 保留小数
        /// </summary>
        /// <param name="num"></param>
        /// <param name="decimals"></param>
        /// <param name="mode">四舍五入策略</param>
        /// <returns></returns>
        public static decimal Round(this decimal num, int decimals, MidpointRounding mode = MidpointRounding.AwayFromZero)
        {
            num = Math.Round(num, decimals, mode);
            return num;
        }

        /// <summary>
        /// 保留小数
        /// </summary>
        /// <param name="num"></param>
        /// <param name="decimals"></param>
        /// <param name="mode">四舍五入策略</param>
        /// <returns></returns>
        public static double Round(this double num, int decimals, MidpointRounding mode = MidpointRounding.AwayFromZero)
        {
            num = Math.Round(num, decimals, mode);
            return num;
        }

        /// <summary>
        /// 保留小数
        /// </summary>
        /// <param name="num"></param>
        /// <param name="decimals"></param>
        /// <param name="mode">四舍五入策略</param>
        /// <returns></returns>
        public static decimal? Round(this decimal? num, int decimals, MidpointRounding mode = MidpointRounding.AwayFromZero)
        {
            if (num.HasValue)
            {
                num = Math.Round(num.Value, decimals, mode);
            }
            return num;
        }

        /// <summary>
        /// 保留小数
        /// </summary>
        /// <param name="num"></param>
        /// <param name="decimals"></param>
        /// <param name="mode">四舍五入策略</param>
        /// <returns></returns>
        public static double? Round(this double? num, int decimals, MidpointRounding mode = MidpointRounding.AwayFromZero)
        {
            if (num.HasValue)
            {
                num = Math.Round(num.Value, decimals, mode);
            }
            return num;
        }
    }
}
