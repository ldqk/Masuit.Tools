using System;
using System.Numerics;

namespace Masuit.Tools
{
    public static partial class StringExtensions
    {
        /// <summary>
        /// 任意进制转十进制
        /// </summary>
        /// <param name="str"></param>
        /// <param name="bin">进制</param>
        /// <returns></returns>

        public static long FromBinary(this string str, int bin)
        {
            return str.ToBinary(bin);
        }

        /// <summary>
        /// 任意进制转大数十进制
        /// </summary>
        /// <param name="str"></param>
        /// <param name="bin">进制</param>
        /// <returns></returns>
        [Obsolete("ToBinaryBig")]
        public static BigInteger FromBinaryBig(this string str, int bin)
        {
            return str.ToBinaryBig(bin);
        }
    }
}