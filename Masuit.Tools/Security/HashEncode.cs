using System;
using System.Security.Cryptography;
using System.Text;
using Masuit.Tools.Win32;

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
        /// <param name="Security">需要加密的字符串</param>
        /// <returns>加密后的数据</returns>
        public static string HashEncoding(this string Security)
        {
            byte[] Value;
            UnicodeEncoding Code = new UnicodeEncoding();
            byte[] Message = Code.GetBytes(Security);
            SHA512Managed Arithmetic = new SHA512Managed();
            Value = Arithmetic.ComputeHash(Message);
            Security = "";
            foreach (byte o in Value)
            {
                Security += (int)o + "O";
            }
            return Security;
        }
    }
}
