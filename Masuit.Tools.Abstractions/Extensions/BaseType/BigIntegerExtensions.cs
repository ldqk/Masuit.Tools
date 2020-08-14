using System.Numerics;
using Masuit.Tools.Strings;

namespace Masuit.Tools
{
    public static class BigIntegerExtensions
    {
        /// <summary>
        /// 十进制转任意进制
        /// </summary>
        /// <param name="num"></param>
        /// <param name="bin">进制</param>
        /// <returns></returns>
        public static string ToBinary(this BigInteger num, int bin)
        {
            var nf = new NumberFormater(bin);
            return nf.ToString(num);
        }
    }
}