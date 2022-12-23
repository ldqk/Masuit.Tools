using Masuit.Tools.Strings;
using System.Numerics;

namespace Masuit.Tools
{
    public static class BigIntegerExtensions
    {
        /// <summary>
        /// 十进制转任意进制
        /// </summary>
        /// <param name="num"></param>
        /// <param name="base">进制</param>
        /// <returns></returns>
        public static string ToBase(this BigInteger num, byte @base)
        {
            var nf = new NumberFormater(@base);
            return nf.ToString(num);
        }
    }
}