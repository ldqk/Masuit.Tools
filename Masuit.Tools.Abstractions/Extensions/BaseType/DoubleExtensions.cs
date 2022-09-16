using System;

namespace Masuit.Tools
{
    public static class DoubleExtensions
    {
        /// <summary>
        /// 转decimal
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static decimal ToDecimal(this double num)
        {
            return (decimal)num;
        }

        /// <summary>
        /// 转decimal
        /// </summary>
        /// <param name="num"></param>
        /// <param name="precision">小数位数</param>
        /// <returns></returns>
        public static decimal ToDecimal(this double num, int precision)
        {
            return Math.Round((decimal)num, precision);
        }

        /// <summary>
        /// 转decimal
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static decimal ToDecimal(this float num)
        {
            return (decimal)num;
        }

        /// <summary>
        /// 转decimal
        /// </summary>
        /// <param name="num"></param>
        /// <param name="precision">小数位数</param>
        /// <returns></returns>
        public static decimal ToDecimal(this float num, int precision)
        {
            return Math.Round((decimal)num, precision);
        }
    }
}