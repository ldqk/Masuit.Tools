using System;

namespace Masuit.Tools
{
    public static class DoubleExtensions
    {
        /// <summary>
        /// 将小数截断为8位
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static double Digits8(this double num)
        {
            return (long)(num * 1E+8) * 1e-8;
        }

        /// <summary>
        /// 转decimal
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static decimal ToDecimal(this double num)
        {
            return num.ConvertTo<decimal>();
        }

        /// <summary>
        /// 转decimal
        /// </summary>
        /// <param name="num"></param>
        /// <param name="precision">小数位数</param>
        /// <returns></returns>
        public static decimal ToDecimal(this double num, int precision)
        {
            return Math.Round(num.ConvertTo<decimal>(), precision);
        }

        /// <summary>
        /// 转decimal
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static decimal ToDecimal(this float num)
        {
            return num.ConvertTo<decimal>();
        }

        /// <summary>
        /// 转decimal
        /// </summary>
        /// <param name="num"></param>
        /// <param name="precision">小数位数</param>
        /// <returns></returns>
        public static decimal ToDecimal(this float num, int precision)
        {
            return Math.Round(num.ConvertTo<decimal>(), precision);
        }
    }
}