using System;
using System.Security.Cryptography;
using System.Text;

namespace Masuit.Tools.Security
{
    /// <summary>
    /// 得到随机安全码（哈希加密）。
    /// </summary>
    public static class HashEncode
    {
        /// <summary>
        /// 得到随机哈希加密字符串
        /// </summary>
        /// <returns>随机哈希加密字符串</returns>
        public static string GetSecurity(this Random r) => HashEncoding(r.StrictNext().ToString());

        /// <summary>
        /// 哈希加密一个字符串
        /// </summary>
        /// <param name="security">需要加密的字符串</param>
        /// <returns>加密后的数据</returns>
        public static string HashEncoding(this string security)
        {
            var code = new UnicodeEncoding();
            byte[] message = code.GetBytes(security);
            using var arithmetic = SHA512.Create();
            var value = arithmetic.ComputeHash(message);
            var sb = new StringBuilder();
            foreach (byte o in value)
            {
                sb.Append((int)o + "O");
            }

            return sb.ToString();
        }
    }
}