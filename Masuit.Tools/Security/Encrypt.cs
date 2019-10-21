using System;
using System.Collections;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Masuit.Tools.Security
{
    /// <summary>
    /// 常用的加密解密算法
    /// </summary>
    public static class Encrypt
    {
        #region DES对称加密解密

        /// <summary>
        /// 加密密钥，需要在config配置文件中AppSettings节点中配置desSecret值，若未配置，默认取“masuit”的MD5值
        /// </summary>
        public static string DefaultEncryptKey = ConfigurationManager.AppSettings["desSecret"] ?? "masuit".MDString2();

        /// <summary>
        /// 使用默认加密
        /// </summary>
        /// <param name="strText">被加密的字符串</param>
        /// <returns>加密后的数据</returns>
        public static string DesEncrypt(this string strText)
        {
            try
            {
                return DesEncrypt(strText, DefaultEncryptKey);
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// 使用默认解密
        /// </summary>
        /// <param name="strText">需要解密的 字符串</param>
        /// <returns>解密后的数据</returns>
        public static string DesDecrypt(this string strText)
        {
            try
            {
                return DesDecrypt(strText, DefaultEncryptKey);
            }
            catch
            {
                return "";
            }
        }

        /// <summary> 
        /// 解密字符串
        /// 加密密钥必须为8位
        /// </summary> 
        /// <param name="strText">被解密的字符串</param> 
        /// <param name="strEncrKey">密钥</param> 
        /// <returns>解密后的数据</returns> 
        public static string DesEncrypt(this string strText, string strEncrKey)
        {
            if (strEncrKey.Length < 8)
            {
                throw new Exception("密钥长度无效，密钥必须是8位！");
            }

            StringBuilder ret = new StringBuilder();
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] inputByteArray = Encoding.Default.GetBytes(strText);
            des.Key = Encoding.ASCII.GetBytes(strEncrKey.Substring(0, 8));
            des.IV = Encoding.ASCII.GetBytes(strEncrKey.Substring(0, 8));
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            foreach (byte b in ms.ToArray())
            {
                ret.AppendFormat($"{b:X2}");
            }

            return ret.ToString();
        }

        /// <summary>
        /// DES加密文件
        /// </summary>
        /// <param name="fin">文件输入流</param>
        /// <param name="outFilePath">文件输出路径</param>
        /// <param name="strEncrKey">加密密钥</param>
        public static void DesEncrypt(this FileStream fin, string outFilePath, string strEncrKey)
        {
            byte[] iv =
            {
                0x12,
                0x34,
                0x56,
                0x78,
                0x90,
                0xAB,
                0xCD,
                0xEF
            };
            var byKey = Encoding.UTF8.GetBytes(strEncrKey.Substring(0, 8));
            using var fout = new FileStream(outFilePath, FileMode.OpenOrCreate, FileAccess.Write);
            fout.SetLength(0);
            byte[] bin = new byte[100];
            long rdlen = 0;
            long totlen = fin.Length;
            DES des = new DESCryptoServiceProvider();
            var encStream = new CryptoStream(fout, des.CreateEncryptor(byKey, iv), CryptoStreamMode.Write);
            while (rdlen < totlen)
            {
                var len = fin.Read(bin, 0, 100);
                encStream.Write(bin, 0, len);
                rdlen += len;
            }
        }

        /// <summary>
        /// DES解密文件
        /// </summary>
        /// <param name="fin">输入文件流</param>
        /// <param name="outFilePath">文件输出路径</param>
        /// <param name="sDecrKey">解密密钥</param>
        public static void DesDecrypt(this FileStream fin, string outFilePath, string sDecrKey)
        {
            byte[] iv =
            {
                0x12,
                0x34,
                0x56,
                0x78,
                0x90,
                0xAB,
                0xCD,
                0xEF
            };
            var byKey = Encoding.UTF8.GetBytes(sDecrKey.Substring(0, 8));
            using var fout = new FileStream(outFilePath, FileMode.OpenOrCreate, FileAccess.Write);
            fout.SetLength(0);
            byte[] bin = new byte[100];
            long rdlen = 0;
            long totlen = fin.Length;
            DES des = new DESCryptoServiceProvider();
            var encStream = new CryptoStream(fout, des.CreateDecryptor(byKey, iv), CryptoStreamMode.Write);
            while (rdlen < totlen)
            {
                var len = fin.Read(bin, 0, 100);
                encStream.Write(bin, 0, len);
                rdlen += len;
            }
        }

        /// <summary>
        ///     DES解密算法
        ///     密钥为8位
        /// </summary>
        /// <param name="pToDecrypt">需要解密的字符串</param>
        /// <param name="sKey">密钥</param>
        /// <returns>解密后的数据</returns>
        public static string DesDecrypt(this string pToDecrypt, string sKey)
        {
            if (sKey.Length < 8)
            {
                throw new Exception("密钥长度无效，密钥必须是8位！");
            }

            var ms = new MemoryStream();

            var des = new DESCryptoServiceProvider();
            var inputByteArray = new byte[pToDecrypt.Length / 2];
            for (int x = 0; x < pToDecrypt.Length / 2; x++)
            {
                int i = (Convert.ToInt32(pToDecrypt.Substring(x * 2, 2), 16));
                inputByteArray[x] = (byte)i;
            }

            des.Key = Encoding.ASCII.GetBytes(sKey.Substring(0, 8));
            des.IV = Encoding.ASCII.GetBytes(sKey.Substring(0, 8));
            var cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            return Encoding.Default.GetString(ms.ToArray());
        }

        #endregion

        #region 对称加密算法AES RijndaelManaged加密解密

        private static readonly string Default_AES_Key = "@#kim123";

        private static byte[] Keys =
        {
            0x41,
            0x72,
            0x65,
            0x79,
            0x6F,
            0x75,
            0x6D,
            0x79,
            0x53,
            0x6E,
            0x6F,
            0x77,
            0x6D,
            0x61,
            0x6E,
            0x3F
        };

        /// <summary>
        /// 对称加密算法AES RijndaelManaged加密(RijndaelManaged（AES）算法是块式加密算法)
        /// </summary>
        /// <param name="encryptString">待加密字符串</param>
        /// <returns>加密结果字符串</returns>
        public static string AESEncrypt(this string encryptString)
        {
            return AESEncrypt(encryptString, Default_AES_Key);
        }

        /// <summary>
        /// 对称加密算法AES RijndaelManaged加密(RijndaelManaged（AES）算法是块式加密算法)
        /// </summary>
        /// <param name="encryptString">待加密字符串</param>
        /// <param name="encryptKey">加密密钥，须半角字符</param>
        /// <returns>加密结果字符串</returns>
        public static string AESEncrypt(this string encryptString, string encryptKey)
        {
            encryptKey = GetSubString(encryptKey, 32, "");
            encryptKey = encryptKey.PadRight(32, ' ');
            var rijndaelProvider = new RijndaelManaged
            {
                Key = Encoding.UTF8.GetBytes(encryptKey.Substring(0, 32)),
                IV = Keys
            };
            ICryptoTransform rijndaelEncrypt = rijndaelProvider.CreateEncryptor();
            byte[] inputData = Encoding.UTF8.GetBytes(encryptString);
            byte[] encryptedData = rijndaelEncrypt.TransformFinalBlock(inputData, 0, inputData.Length);
            return Convert.ToBase64String(encryptedData);
        }

        /// <summary>
        /// 对称加密算法AES RijndaelManaged解密字符串
        /// </summary>
        /// <param name="decryptString">待解密的字符串</param>
        /// <returns>解密成功返回解密后的字符串,失败返源串</returns>
        public static string AESDecrypt(this string decryptString)
        {
            return AESDecrypt(decryptString, Default_AES_Key);
        }

        /// <summary>
        /// 对称加密算法AES RijndaelManaged解密字符串
        /// </summary>
        /// <param name="decryptString">待解密的字符串</param>
        /// <param name="decryptKey">解密密钥,和加密密钥相同</param>
        /// <returns>解密成功返回解密后的字符串,失败返回空</returns>
        public static string AESDecrypt(this string decryptString, string decryptKey)
        {
            try
            {
                decryptKey = GetSubString(decryptKey, 32, "");
                decryptKey = decryptKey.PadRight(32, ' ');
                var rijndaelProvider = new RijndaelManaged()
                {
                    Key = Encoding.UTF8.GetBytes(decryptKey),
                    IV = Keys
                };
                ICryptoTransform rijndaelDecrypt = rijndaelProvider.CreateDecryptor();
                byte[] inputData = Convert.FromBase64String(decryptString);
                byte[] decryptedData = rijndaelDecrypt.TransformFinalBlock(inputData, 0, inputData.Length);
                return Encoding.UTF8.GetString(decryptedData);
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 按字节长度(按字节,一个汉字为2个字节)取得某字符串的一部分
        /// </summary>
        /// <param name="sourceString">源字符串</param>
        /// <param name="length">所取字符串字节长度</param>
        /// <param name="tailString">附加字符串(当字符串不够长时，尾部所添加的字符串，一般为"...")</param>
        /// <returns>某字符串的一部分</returns>
        private static string GetSubString(this string sourceString, int length, string tailString)
        {
            return GetSubString(sourceString, 0, length, tailString);
        }

        /// <summary>
        /// 按字节长度(按字节,一个汉字为2个字节)取得某字符串的一部分
        /// </summary>
        /// <param name="sourceString">源字符串</param>
        /// <param name="startIndex">索引位置，以0开始</param>
        /// <param name="length">所取字符串字节长度</param>
        /// <param name="tailString">附加字符串(当字符串不够长时，尾部所添加的字符串，一般为"...")</param>
        /// <returns>某字符串的一部分</returns>
        private static string GetSubString(this string sourceString, int startIndex, int length, string tailString)
        {
            //当是日文或韩文时(注:中文的范围:\u4e00 - \u9fa5, 日文在\u0800 - \u4e00, 韩文为\xAC00-\xD7A3)
            if (System.Text.RegularExpressions.Regex.IsMatch(sourceString, "[\u0800-\u4e00]+") || System.Text.RegularExpressions.Regex.IsMatch(sourceString, "[\xAC00-\xD7A3]+"))
            {
                //当截取的起始位置超出字段串长度时
                if (startIndex >= sourceString.Length)
                {
                    return string.Empty;
                }

                return sourceString.Substring(startIndex, length + startIndex > sourceString.Length ? (sourceString.Length - startIndex) : length);
            }

            //中文字符，如"中国人民abcd123"
            if (length <= 0)
            {
                return string.Empty;
            }

            byte[] bytesSource = Encoding.Default.GetBytes(sourceString);

            //当字符串长度大于起始位置
            if (bytesSource.Length > startIndex)
            {
                int endIndex = bytesSource.Length;

                //当要截取的长度在字符串的有效长度范围内
                if (bytesSource.Length > (startIndex + length))
                {
                    endIndex = length + startIndex;
                }
                else
                {
                    //当不在有效范围内时,只取到字符串的结尾
                    length = bytesSource.Length - startIndex;
                    tailString = "";
                }

                var anResultFlag = new int[length];
                int nFlag = 0;
                //字节大于127为双字节字符
                for (int i = startIndex; i < endIndex; i++)
                {
                    if (bytesSource[i] > 127)
                    {
                        nFlag++;
                        if (nFlag == 3)
                        {
                            nFlag = 1;
                        }
                    }
                    else
                    {
                        nFlag = 0;
                    }

                    anResultFlag[i] = nFlag;
                }

                //最后一个字节为双字节字符的一半
                if ((bytesSource[endIndex - 1] > 127) && (anResultFlag[length - 1] == 1))
                {
                    length++;
                }

                byte[] bsResult = new byte[length];
                Array.Copy(bytesSource, startIndex, bsResult, 0, length);
                var myResult = Encoding.Default.GetString(bsResult);
                myResult += tailString;
                return myResult;
            }

            return string.Empty;
        }

        /// <summary>
        /// 加密文件流
        /// </summary>
        /// <param name="fs">需要加密的文件流</param>
        /// <param name="decryptKey">加密密钥</param>
        /// <returns>加密流</returns>
        public static CryptoStream AESEncryptStrream(this FileStream fs, string decryptKey)
        {
            decryptKey = GetSubString(decryptKey, 32, "");
            decryptKey = decryptKey.PadRight(32, ' ');
            var rijndaelProvider = new RijndaelManaged()
            {
                Key = Encoding.UTF8.GetBytes(decryptKey),
                IV = Keys
            };
            ICryptoTransform encrypto = rijndaelProvider.CreateEncryptor();
            return new CryptoStream(fs, encrypto, CryptoStreamMode.Write);
        }

        /// <summary>
        /// 解密文件流
        /// </summary>
        /// <param name="fs">需要解密的文件流</param>
        /// <param name="decryptKey">解密密钥</param>
        /// <returns>加密流</returns>
        public static CryptoStream AESDecryptStream(this FileStream fs, string decryptKey)
        {
            decryptKey = GetSubString(decryptKey, 32, "");
            decryptKey = decryptKey.PadRight(32, ' ');
            var rijndaelProvider = new RijndaelManaged()
            {
                Key = Encoding.UTF8.GetBytes(decryptKey),
                IV = Keys
            };
            var decrypto = rijndaelProvider.CreateDecryptor();
            return new CryptoStream(fs, decrypto, CryptoStreamMode.Read);
        }

        /// <summary>
        /// 对指定文件AES加密
        /// </summary>
        /// <param name="input">源文件流</param>
        /// <param name="outputPath">输出文件路径</param>
        public static void AESEncryptFile(this FileStream input, string outputPath)
        {
            using var fren = new FileStream(outputPath, FileMode.Create);
            var enfr = AESEncryptStrream(fren, Default_AES_Key);
            byte[] bytearrayinput = new byte[input.Length];
            input.Read(bytearrayinput, 0, bytearrayinput.Length);
            enfr.Write(bytearrayinput, 0, bytearrayinput.Length);
        }

        /// <summary>
        /// 对指定的文件AES解密
        /// </summary>
        /// <param name="input">源文件流</param>
        /// <param name="outputPath">输出文件路径</param>
        public static void AESDecryptFile(this FileStream input, string outputPath)
        {
            FileStream frde = new FileStream(outputPath, FileMode.Create);
            CryptoStream defr = AESDecryptStream(input, Default_AES_Key);
            byte[] bytearrayoutput = new byte[1024];
            while (true)
            {
                var count = defr.Read(bytearrayoutput, 0, bytearrayoutput.Length);
                frde.Write(bytearrayoutput, 0, count);
                if (count < bytearrayoutput.Length)
                {
                    break;
                }
            }
        }

        #endregion

        #region Base64加密解密

        /// <summary>
        /// Base64加密
        /// </summary>
        /// <param name="str">需要加密的字符串</param>
        /// <returns>加密后的数据</returns>
        public static string Base64Encrypt(this string str)
        {
            byte[] encbuff = Encoding.UTF8.GetBytes(str);
            return Convert.ToBase64String(encbuff);
        }

        /// <summary>
        /// Base64解密
        /// </summary>
        /// <param name="str">需要解密的字符串</param>
        /// <returns>解密后的数据</returns>
        public static string Base64Decrypt(this string str)
        {
            byte[] decbuff = Convert.FromBase64String(str);
            return Encoding.UTF8.GetString(decbuff);
        }

        #endregion

        #region MD5加密

        /// <summary> 
        /// MD5加密
        /// </summary> 
        /// <param name="strText">原数据</param> 
        /// <returns>MD5字符串</returns> 
        public static string MD5Encrypt(this string strText)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] result = md5.ComputeHash(Encoding.Default.GetBytes(strText));
            return Encoding.Default.GetString(result);
        }

        #endregion

        /// <summary>
        /// SHA256函数
        /// </summary>
        /// <param name="str">原始字符串</param>
        /// <returns>SHA256结果(返回长度为44字节的字符串)</returns>
        public static string SHA256(this string str)
        {
            byte[] sha256Data = Encoding.UTF8.GetBytes(str);
            var sha256 = new SHA256Managed();
            byte[] result = sha256.ComputeHash(sha256Data);
            return Convert.ToBase64String(result); //返回长度为44字节的字符串
        }

        #region 创建Key

        /// <summary>
        ///     创建Key
        /// </summary>
        /// <returns>密钥</returns>
        public static string GenerateKey()
        {
            var desCrypto = (DESCryptoServiceProvider)DES.Create();
            return Encoding.ASCII.GetString(desCrypto.Key);
        }

        #endregion

        #region MD5加密

        /// <summary>
        ///     MD5加密
        /// </summary>
        /// <param name="pToEncrypt">加密字符串</param>
        /// <param name="sKey">密钥Key</param>
        /// <returns>加密后的字符串</returns>
        public static string MD5Encrypt(this string pToEncrypt, string sKey)
        {
            var des = new DESCryptoServiceProvider();
            var inputByteArray = Encoding.Default.GetBytes(pToEncrypt);
            des.Key = Encoding.ASCII.GetBytes(sKey);
            des.IV = Encoding.ASCII.GetBytes(sKey);
            var ms = new MemoryStream();
            var cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            var ret = new StringBuilder();
            foreach (var b in ms.ToArray())
            {
                ret.AppendFormat("{0:X2}", b);
            }

            ret.ToString();
            return ret.ToString();
        }

        #endregion

        #region MD5解密

        /// <summary>
        ///     MD5解密
        /// </summary>
        /// <param name="pToDecrypt">解密字符串</param>
        /// <param name="sKey">密钥Key</param>
        /// <returns>解密后的数据</returns>
        public static string MD5Decrypt(this string pToDecrypt, string sKey)
        {
            var des = new DESCryptoServiceProvider();

            var inputByteArray = new byte[pToDecrypt.Length / 2];
            for (var x = 0; x < pToDecrypt.Length / 2; x++)
            {
                var i = Convert.ToInt32(pToDecrypt.Substring(x * 2, 2), 16);
                inputByteArray[x] = (byte)i;
            }

            des.Key = Encoding.ASCII.GetBytes(sKey);
            des.IV = Encoding.ASCII.GetBytes(sKey);
            var ms = new MemoryStream();
            var cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            return Encoding.Default.GetString(ms.ToArray());
        }

        #endregion

        #region MD5加密算法

        //number of bits to rotate in tranforming
        private const int S11 = 7;

        private const int S12 = 12;
        private const int S13 = 17;
        private const int S14 = 22;
        private const int S21 = 5;
        private const int S22 = 9;
        private const int S23 = 14;
        private const int S24 = 20;
        private const int S31 = 4;
        private const int S32 = 11;
        private const int S33 = 16;
        private const int S34 = 23;
        private const int S41 = 6;
        private const int S42 = 10;
        private const int S43 = 15;
        private const int S44 = 21;

        //static state variables
        private static uint A;

        private static uint B;
        private static uint C;
        private static uint D;

        private static uint F(uint x, uint y, uint z)
        {
            return (x & y) | (~x & z);
        }

        private static uint G(uint x, uint y, uint z)
        {
            return (x & z) | (y & ~z);
        }

        private static uint H(uint x, uint y, uint z)
        {
            return x ^ y ^ z;
        }

        private static uint I(uint x, uint y, uint z)
        {
            return y ^ (x | ~z);
        }

        private static void FF(ref uint a, uint b, uint c, uint d, uint mj, int s, uint ti)
        {
            a = a + F(b, c, d) + mj + ti;
            a = (a << s) | (a >> (32 - s));
            a += b;
        }

        private static void GG(ref uint a, uint b, uint c, uint d, uint mj, int s, uint ti)
        {
            a = a + G(b, c, d) + mj + ti;
            a = (a << s) | (a >> (32 - s));
            a += b;
        }

        private static void HH(ref uint a, uint b, uint c, uint d, uint mj, int s, uint ti)
        {
            a = a + H(b, c, d) + mj + ti;
            a = (a << s) | (a >> (32 - s));
            a += b;
        }

        private static void II(ref uint a, uint b, uint c, uint d, uint mj, int s, uint ti)
        {
            a = a + I(b, c, d) + mj + ti;
            a = (a << s) | (a >> (32 - s));
            a += b;
        }

        private static void MD5_Init()
        {
            A = 0x67452301; //in memory, this is 0x01234567
            B = 0xefcdab89; //in memory, this is 0x89abcdef
            C = 0x98badcfe; //in memory, this is 0xfedcba98
            D = 0x10325476; //in memory, this is 0x76543210
        }

        private static uint[] MD5_Append(byte[] input)
        {
            int zeros;
            var ones = 1;
            int size;
            var n = input.Length;
            var m = n % 64;
            if (m < 56)
            {
                zeros = 55 - m;
                size = n - m + 64;
            }
            else if (m == 56)
            {
                zeros = 0;
                ones = 0;
                size = n + 8;
            }
            else
            {
                zeros = 63 - m + 56;
                size = n + 64 - m + 64;
            }

            var bs = new ArrayList(input);
            if (ones == 1)
                bs.Add((byte)0x80); // 0x80 = $10000000
            for (var i = 0; i < zeros; i++)
                bs.Add((byte)0);

            var N = (ulong)n * 8;
            var h1 = (byte)(N & 0xFF);
            var h2 = (byte)((N >> 8) & 0xFF);
            var h3 = (byte)((N >> 16) & 0xFF);
            var h4 = (byte)((N >> 24) & 0xFF);
            var h5 = (byte)((N >> 32) & 0xFF);
            var h6 = (byte)((N >> 40) & 0xFF);
            var h7 = (byte)((N >> 48) & 0xFF);
            var h8 = (byte)(N >> 56);
            bs.Add(h1);
            bs.Add(h2);
            bs.Add(h3);
            bs.Add(h4);
            bs.Add(h5);
            bs.Add(h6);
            bs.Add(h7);
            bs.Add(h8);
            var ts = (byte[])bs.ToArray(typeof(byte));

            /* Decodes input (byte[]) into output (UInt32[]). Assumes len is
            * a multiple of 4.
           */
            var output = new uint[size / 4];
            for (long i = 0, j = 0; i < size; j++, i += 4)
                output[j] = (uint)(ts[i] | (ts[i + 1] << 8) | (ts[i + 2] << 16) | (ts[i + 3] << 24));
            return output;
        }

        private static uint[] MD5_Trasform(uint[] x)
        {
            uint a, b, c, d;

            for (var k = 0; k < x.Length; k += 16)
            {
                a = A;
                b = B;
                c = C;
                d = D;

                /* Round 1 */
                FF(ref a, b, c, d, x[k + 0], S11, 0xd76aa478); /* 1 */
                FF(ref d, a, b, c, x[k + 1], S12, 0xe8c7b756); /* 2 */
                FF(ref c, d, a, b, x[k + 2], S13, 0x242070db); /* 3 */
                FF(ref b, c, d, a, x[k + 3], S14, 0xc1bdceee); /* 4 */
                FF(ref a, b, c, d, x[k + 4], S11, 0xf57c0faf); /* 5 */
                FF(ref d, a, b, c, x[k + 5], S12, 0x4787c62a); /* 6 */
                FF(ref c, d, a, b, x[k + 6], S13, 0xa8304613); /* 7 */
                FF(ref b, c, d, a, x[k + 7], S14, 0xfd469501); /* 8 */
                FF(ref a, b, c, d, x[k + 8], S11, 0x698098d8); /* 9 */
                FF(ref d, a, b, c, x[k + 9], S12, 0x8b44f7af); /* 10 */
                FF(ref c, d, a, b, x[k + 10], S13, 0xffff5bb1); /* 11 */
                FF(ref b, c, d, a, x[k + 11], S14, 0x895cd7be); /* 12 */
                FF(ref a, b, c, d, x[k + 12], S11, 0x6b901122); /* 13 */
                FF(ref d, a, b, c, x[k + 13], S12, 0xfd987193); /* 14 */
                FF(ref c, d, a, b, x[k + 14], S13, 0xa679438e); /* 15 */
                FF(ref b, c, d, a, x[k + 15], S14, 0x49b40821); /* 16 */

                /* Round 2 */
                GG(ref a, b, c, d, x[k + 1], S21, 0xf61e2562); /* 17 */
                GG(ref d, a, b, c, x[k + 6], S22, 0xc040b340); /* 18 */
                GG(ref c, d, a, b, x[k + 11], S23, 0x265e5a51); /* 19 */
                GG(ref b, c, d, a, x[k + 0], S24, 0xe9b6c7aa); /* 20 */
                GG(ref a, b, c, d, x[k + 5], S21, 0xd62f105d); /* 21 */
                GG(ref d, a, b, c, x[k + 10], S22, 0x2441453); /* 22 */
                GG(ref c, d, a, b, x[k + 15], S23, 0xd8a1e681); /* 23 */
                GG(ref b, c, d, a, x[k + 4], S24, 0xe7d3fbc8); /* 24 */
                GG(ref a, b, c, d, x[k + 9], S21, 0x21e1cde6); /* 25 */
                GG(ref d, a, b, c, x[k + 14], S22, 0xc33707d6); /* 26 */
                GG(ref c, d, a, b, x[k + 3], S23, 0xf4d50d87); /* 27 */
                GG(ref b, c, d, a, x[k + 8], S24, 0x455a14ed); /* 28 */
                GG(ref a, b, c, d, x[k + 13], S21, 0xa9e3e905); /* 29 */
                GG(ref d, a, b, c, x[k + 2], S22, 0xfcefa3f8); /* 30 */
                GG(ref c, d, a, b, x[k + 7], S23, 0x676f02d9); /* 31 */
                GG(ref b, c, d, a, x[k + 12], S24, 0x8d2a4c8a); /* 32 */

                /* Round 3 */
                HH(ref a, b, c, d, x[k + 5], S31, 0xfffa3942); /* 33 */
                HH(ref d, a, b, c, x[k + 8], S32, 0x8771f681); /* 34 */
                HH(ref c, d, a, b, x[k + 11], S33, 0x6d9d6122); /* 35 */
                HH(ref b, c, d, a, x[k + 14], S34, 0xfde5380c); /* 36 */
                HH(ref a, b, c, d, x[k + 1], S31, 0xa4beea44); /* 37 */
                HH(ref d, a, b, c, x[k + 4], S32, 0x4bdecfa9); /* 38 */
                HH(ref c, d, a, b, x[k + 7], S33, 0xf6bb4b60); /* 39 */
                HH(ref b, c, d, a, x[k + 10], S34, 0xbebfbc70); /* 40 */
                HH(ref a, b, c, d, x[k + 13], S31, 0x289b7ec6); /* 41 */
                HH(ref d, a, b, c, x[k + 0], S32, 0xeaa127fa); /* 42 */
                HH(ref c, d, a, b, x[k + 3], S33, 0xd4ef3085); /* 43 */
                HH(ref b, c, d, a, x[k + 6], S34, 0x4881d05); /* 44 */
                HH(ref a, b, c, d, x[k + 9], S31, 0xd9d4d039); /* 45 */
                HH(ref d, a, b, c, x[k + 12], S32, 0xe6db99e5); /* 46 */
                HH(ref c, d, a, b, x[k + 15], S33, 0x1fa27cf8); /* 47 */
                HH(ref b, c, d, a, x[k + 2], S34, 0xc4ac5665); /* 48 */

                /* Round 4 */
                II(ref a, b, c, d, x[k + 0], S41, 0xf4292244); /* 49 */
                II(ref d, a, b, c, x[k + 7], S42, 0x432aff97); /* 50 */
                II(ref c, d, a, b, x[k + 14], S43, 0xab9423a7); /* 51 */
                II(ref b, c, d, a, x[k + 5], S44, 0xfc93a039); /* 52 */
                II(ref a, b, c, d, x[k + 12], S41, 0x655b59c3); /* 53 */
                II(ref d, a, b, c, x[k + 3], S42, 0x8f0ccc92); /* 54 */
                II(ref c, d, a, b, x[k + 10], S43, 0xffeff47d); /* 55 */
                II(ref b, c, d, a, x[k + 1], S44, 0x85845dd1); /* 56 */
                II(ref a, b, c, d, x[k + 8], S41, 0x6fa87e4f); /* 57 */
                II(ref d, a, b, c, x[k + 15], S42, 0xfe2ce6e0); /* 58 */
                II(ref c, d, a, b, x[k + 6], S43, 0xa3014314); /* 59 */
                II(ref b, c, d, a, x[k + 13], S44, 0x4e0811a1); /* 60 */
                II(ref a, b, c, d, x[k + 4], S41, 0xf7537e82); /* 61 */
                II(ref d, a, b, c, x[k + 11], S42, 0xbd3af235); /* 62 */
                II(ref c, d, a, b, x[k + 2], S43, 0x2ad7d2bb); /* 63 */
                II(ref b, c, d, a, x[k + 9], S44, 0xeb86d391); /* 64 */

                A += a;
                B += b;
                C += c;
                D += d;
            }

            return new[]
            {
                A,
                B,
                C,
                D
            };
        }

        #region MD5对数组数据加密

        /// <summary>
        ///     MD5对数组数据加密
        /// </summary>
        /// <param name="input">包含需要加密的数据的数组</param>
        /// <returns>加密后的字节流</returns>
        public static byte[] MD5Array(this byte[] input)
        {
            MD5_Init();
            var block = MD5_Append(input);
            var bits = MD5_Trasform(block);

            var output = new byte[bits.Length * 4];
            for (int i = 0, j = 0; i < bits.Length; i++, j += 4)
            {
                output[j] = (byte)(bits[i] & 0xff);
                output[j + 1] = (byte)((bits[i] >> 8) & 0xff);
                output[j + 2] = (byte)((bits[i] >> 16) & 0xff);
                output[j + 3] = (byte)((bits[i] >> 24) & 0xff);
            }

            return output;
        }

        #endregion

        #region 获取数组的Hex值

        /// <summary>
        ///     获取数组的Hex值
        /// </summary>
        /// <param name="array">需要求Hex值的数组</param>
        /// <param name="uppercase">是否转大写</param>
        /// <returns>字节数组的16进制表示</returns>
        public static string ArrayToHexString(this byte[] array, bool uppercase)
        {
            var hexString = "";
            var format = "x2";
            if (uppercase)
                format = "X2";
            foreach (var b in array)
                hexString += b.ToString(format);
            return hexString;
        }

        #endregion

        #region 对字符串进行MD5加密

        /// <summary>
        ///     对字符串进行MD5加密
        /// </summary>
        /// <param name="message">需要加密的字符串</param>
        /// <returns>加密后的结果</returns>
        public static string MDString(this string message)
        {
            var c = message.ToCharArray();
            var b = new byte[c.Length];
            for (var i = 0; i < c.Length; i++)
                b[i] = (byte)c[i];
            var digest = MD5Array(b);
            return ArrayToHexString(digest, false);
        }

        /// <summary>
        ///     对字符串进行MD5二次加密
        /// </summary>
        /// <param name="message">需要加密的字符串</param>
        /// <returns>加密后的结果</returns>
        public static string MDString2(this string message) => MDString(MDString(message));

        /// <summary>
        /// MD5 三次加密算法
        /// </summary>
        /// <param name="s">需要加密的字符串</param>
        /// <returns>MD5字符串</returns>
        public static string MDString3(this string s)
        {
            MD5 md5 = MD5.Create();
            byte[] bytes = Encoding.ASCII.GetBytes(s);
            byte[] bytes1 = md5.ComputeHash(bytes);
            byte[] bytes2 = md5.ComputeHash(bytes1);
            byte[] bytes3 = md5.ComputeHash(bytes2);

            StringBuilder sb = new StringBuilder();
            foreach (var item in bytes3)
            {
                sb.Append(item.ToString("x").PadLeft(2, '0'));
            }

            return sb.ToString();
        }

        /// <summary>
        ///     对字符串进行MD5加盐加密
        /// </summary>
        /// <param name="message">需要加密的字符串</param>
        /// <param name="salt">盐</param>
        /// <returns>加密后的结果</returns>
        public static string MDString(this string message, string salt) => MDString(message + salt);

        /// <summary>
        ///     对字符串进行MD5二次加盐加密
        /// </summary>
        /// <param name="message">需要加密的字符串</param>
        /// <param name="salt">盐</param>
        /// <returns>加密后的结果</returns>
        public static string MDString2(this string message, string salt) => MDString(MDString(message + salt), salt);

        /// <summary>
        /// MD5 三次加密算法
        /// </summary>
        /// <param name="s">需要加密的字符串</param>
        /// <param name="salt">盐</param>
        /// <returns>MD5字符串</returns>
        public static string MDString3(this string s, string salt)
        {
            MD5 md5 = MD5.Create();
            byte[] bytes = Encoding.ASCII.GetBytes(s + salt);
            byte[] bytes1 = md5.ComputeHash(bytes);
            byte[] bytes2 = md5.ComputeHash(bytes1);
            byte[] bytes3 = md5.ComputeHash(bytes2);

            StringBuilder sb = new StringBuilder();
            foreach (var item in bytes3)
            {
                sb.Append(item.ToString("x").PadLeft(2, '0'));
            }

            return sb.ToString();
        }

        #endregion

        #region 获取文件的MD5值

        /// <summary>
        /// 获取文件的MD5值
        /// </summary>
        /// <param name="fileName">需要求MD5值的文件的文件名及路径</param>
        /// <returns>MD5字符串</returns>
        public static string MDFile(this string fileName)
        {
            var fs = File.Open(fileName, FileMode.Open, FileAccess.Read);
            var array = new byte[fs.Length];
            fs.Read(array, 0, (int)fs.Length);
            var digest = MD5Array(array);
            fs.Close();
            return ArrayToHexString(digest, false);
        }

        #endregion

        #region 测试MD5加密算法的函数

        /// <summary>
        ///     测试MD5加密算法的函数
        /// </summary>
        /// <param name="message">需要加密的字符串</param>
        /// <returns>加密后的 数据</returns>
        private static string MD5Test(this string message)
        {
            return "rnMD5 (" + "message" + ") = " + MDString(message);
        }

        #endregion

        #region MD5加密算法测试用数据

        /// <summary>
        ///     MD5加密算法测试用数据
        /// </summary>
        /// <returns> </returns>
        private static string TestSuite()
        {
            var s = "";
            s += MD5Test("");
            s += MD5Test("a");
            s += MD5Test("abc");
            s += MD5Test("message digest");
            s += MD5Test("abcdefghijklmnopqrstuvwxyz");
            s += MD5Test("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789");
            s += MD5Test("12345678901234567890123456789012345678901234567890123456789012345678901234567890");
            return s;
        }

        #endregion

        #endregion MD5加密算法
    }

    /// <summary>
    ///     RC2加密解密算法
    /// </summary>
    public static class RC2
    {
        private static ASCIIEncoding _asciiEncoding;
        private static byte[] _iv;
        private static byte[] _key;
        private static RC2CryptoServiceProvider _rc2Csp;
        private static UnicodeEncoding _textConverter;

        static RC2()
        {
            InitializeComponent();
        }

        private static void InitializeComponent()
        {
            _key = new byte[]
            {
                106,
                51,
                25,
                141,
                157,
                142,
                23,
                111,
                234,
                159,
                187,
                154,
                215,
                34,
                37,
                204
            };
            _iv = new byte[]
            {
                135,
                186,
                133,
                136,
                184,
                149,
                153,
                144
            };
            _asciiEncoding = new ASCIIEncoding();
            _textConverter = new UnicodeEncoding();
            _rc2Csp = new RC2CryptoServiceProvider();
        }

        #region 新建一个大小为10261B的文件，以便将加密数据写入固定大小的文件。

        /// <summary>
        ///     新建一个大小为10261B的文件，以便将加密数据写入固定大小的文件。
        /// </summary>
        /// <param name="filePath">文件保存的地址，包含文件名</param>
        public static string InitBinFile(this string filePath)
        {
            var tmp = new byte[10261];
            try //创建文件流，将其内容全部写入0
            {
                var writeFileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 512, false);

                for (var i = 0; i < 10261; i++)
                    tmp[i] = 0;
                writeFileStream.Write(tmp, 0, 10261);
                writeFileStream.Flush();
                writeFileStream.Close();
            }
            catch (IOException)
            {
                // MessageBox.Show("文件操作错误！", "错误！", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "Error,file operation error!";
            }

            return "OK";
        }

        #endregion

        #region 将文本数据加密后写入一个文件

        /// <summary>
        ///     将文本数据加密后写入一个文件，其中，这个文件是用InitBinFile建立的，这个文件将被分成十块，
        ///     用来分别保存10组不同的数据，第一个byte位保留，第2位到第21位分别用来存放每块数据的长度，但
        ///     一个byte的取值为0-127，所以，用两个byte来存放一个长度。
        /// </summary>
        /// <param name="toEncryptText">要加密的文本数据</param>
        /// <param name="filePath">要写入的文件</param>
        /// <param name="dataIndex">写入第几块，取值为1--10</param>
        /// <returns>是否操作成功</returns>
        public static bool EncryptToFile(this string toEncryptText, string filePath, int dataIndex)
        {
            var r = false;
            if ((dataIndex > 10) && (dataIndex < 1))
            {
                return r;
            }

            //打开要写入的文件，主要是为了保持原文件的内容不丢失
            var tmpFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, 1024, true);

            var index = new byte[10261];
            //将读取的内容写到byte数组
            tmpFileStream.Read(index, 0, 10261);
            tmpFileStream.Close();
            //定义基本的加密转换运算
            var Encryptor = _rc2Csp.CreateEncryptor(_key, _iv);
            var msEncrypt = new MemoryStream();
            //在此加密转换流中，加密将从csEncrypt，加密后，结果在msEncrypt流中。
            var csEncrypt = new CryptoStream(msEncrypt, Encryptor, CryptoStreamMode.Write);
            //将要加密的文本转换成UTF-16 编码，保存在tmp数组。
            var tmp = _textConverter.GetBytes(toEncryptText);
            //将tmp输入csEncrypt,将通过Encryptor来加密。
            csEncrypt.Write(tmp, 0, tmp.Length);
            //输出到msEnctypt
            csEncrypt.FlushFinalBlock();
            //将流转成byte[]
            var encrypted = msEncrypt.ToArray();
            if (encrypted.Length > 1024)
                return false;
            //得到加密后数据的大小，将结果存在指定的位置。
            index[dataIndex * 2 - 1] = Convert.ToByte(Convert.ToString(encrypted.Length / 128));
            index[dataIndex * 2] = Convert.ToByte(Convert.ToString(encrypted.Length % 128));
            //将加密后的结果写入index（覆盖）
            for (var i = 0; i < encrypted.Length; i++)
                index[1024 * (dataIndex - 1) + 21 + i] = encrypted[i];
            //建立文件流
            tmpFileStream = new FileStream(filePath, FileMode.Truncate, FileAccess.Write, FileShare.None, 1024, true);
            //写文件
            tmpFileStream.Write(index, 0, 10261);
            tmpFileStream.Flush();
            r = true;
            tmpFileStream.Close();
            return r;
        }

        #endregion

        #region 从一个文件中解密出一段文本，其中，这个文件是由InitBinFile建立的，并且由 EncryptToFile加密的

        /// <summary>
        ///     从一个文件中解密出一段文本，其中，这个文件是由InitBinFile建立的，并且由 EncryptToFile加密的
        /// </summary>
        /// <param name="filePath">要解密的文件</param>
        /// <param name="dataIndex">要从哪一个块中解密</param>
        /// <returns>解密后的文本</returns>
        public static string DecryptFromFile(this string filePath, int dataIndex)
        {
            var r = "";
            if ((dataIndex > 10) && (dataIndex < 1))
            {
                return r;
            }

            var tmpFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, 1024, true);

            var decryptor = _rc2Csp.CreateDecryptor(_key, _iv);
            var msDecrypt = new MemoryStream();
            var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Write);
            var index = new byte[10261];

            tmpFileStream.Read(index, 0, 10261);
            //var startIndex = 1024 * (dataIndex - 1) + 21;
            var count = index[dataIndex * 2 - 1] * 128 + index[dataIndex * 2];
            var tmp = new byte[count];

            Array.Copy(index, 1024 * (dataIndex - 1) + 21, tmp, 0, count);
            csDecrypt.Write(tmp, 0, count);
            csDecrypt.FlushFinalBlock();
            var decrypted = msDecrypt.ToArray();
            r = _textConverter.GetString(decrypted, 0, decrypted.Length);
            tmpFileStream.Close();
            return r;
        }

        #endregion

        #region 将一段文本加密后保存到一个文件

        /// <summary>
        ///     将一段文本加密后保存到一个文件
        /// </summary>
        /// <param name="toEncryptText">要加密的文本数据</param>
        /// <param name="filePath">要保存的文件</param>
        /// <returns>是否加密成功</returns>
        public static void EncryptToFile(this string toEncryptText, string filePath)
        {
            using var tmpFileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, 1024, true);
            using var encryptor = _rc2Csp.CreateEncryptor(_key, _iv);
            var msEncrypt = new MemoryStream();
            using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            var tmp = _textConverter.GetBytes(toEncryptText);
            csEncrypt.Write(tmp, 0, tmp.Length);
            csEncrypt.FlushFinalBlock();
            var encrypted = msEncrypt.ToArray();
            tmpFileStream.Write(encrypted, 0, encrypted.Length);
        }

        #endregion

        #region 将一个被加密的文件解密

        /// <summary>
        ///     将一个被加密的文件解密
        /// </summary>
        /// <param name="filePath">要解密的文件</param>
        /// <returns>解密后的文本</returns>
        public static string DecryptFromFile(this string filePath)
        {
            using var tmpFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, 1024, true);
            using var decryptor = _rc2Csp.CreateDecryptor(_key, _iv);
            var msDecrypt = new MemoryStream();
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Write);
            var tmp = new byte[tmpFileStream.Length];
            tmpFileStream.Read(tmp, 0, tmp.Length);
            csDecrypt.Write(tmp, 0, tmp.Length);
            csDecrypt.FlushFinalBlock();
            var decrypted = msDecrypt.ToArray();
            var r = _textConverter.GetString(decrypted, 0, decrypted.Length);
            return r;
        }

        #endregion

        #region 将文本数据加密后写入一个文件

        /// <summary>
        ///     将文本数据加密后写入一个文件，其中，这个文件是用InitBinFile建立的，这个文件将被分成十块，
        ///     用来分别保存10组不同的数据，第一个byte位保留，第2位到第21位分别用来存放每块数据的长度，但
        ///     一个byte的取值为0-127，所以，用两个byte来存放一个长度。
        /// </summary>
        /// <param name="toEncryptText">要加密的文本数据</param>
        /// <param name="filePath">要写入的文件</param>
        /// <param name="dataIndex">写入第几块，取值为1--10</param>
        /// <param name="IV">初始化向量</param>
        /// <param name="Key">加密密匙</param>
        /// <returns>是否操作成功</returns>
        public static void EncryptToFile(this string toEncryptText, string filePath, int dataIndex, byte[] IV, byte[] Key)
        {
            if ((dataIndex > 10) && (dataIndex < 1))
            {
                return;
            }
            //打开要写入的文件，主要是为了保持原文件的内容不丢失
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, 1024, true);
            var index = new byte[10261];
            //将读取的内容写到byte数组
            fs.Read(index, 0, 10261);

            //定义基本的加密转换运算
            using var encryptor = _rc2Csp.CreateEncryptor(Key, IV);
            var msEncrypt = new MemoryStream();
            //在此加密转换流中，加密将从csEncrypt，加密后，结果在msEncrypt流中。
            using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            var tmp = _textConverter.GetBytes(toEncryptText);
            //将tmp输入csEncrypt,将通过Encryptor来加密。
            csEncrypt.Write(tmp, 0, tmp.Length);
            //输出到msEnctypt
            csEncrypt.FlushFinalBlock();

            //将流转成byte[]
            var encrypted = msEncrypt.ToArray();
            if (encrypted.Length > 1024)
            {
                return;
            }

            //得到加密后数据的大小，将结果存在指定的位置。
            index[dataIndex * 2 - 1] = Convert.ToByte(Convert.ToString(encrypted.Length / 128));
            index[dataIndex * 2] = Convert.ToByte(Convert.ToString(encrypted.Length % 128));
            //将加密后的结果写入index（覆盖）
            for (int i = 0; i < encrypted.Length; i++)
            {
                index[1024 * (dataIndex - 1) + 21 + i] = encrypted[i];
            }

            //建立文件流
            using var newStream = new FileStream(filePath, FileMode.Truncate, FileAccess.Write, FileShare.None, 1024, true);
            newStream.Write(index, 0, 10261);
            newStream.Flush();
            newStream.Close();
        }

        #endregion

        #region 从一个文件中解密出一段文本

        /// <summary>
        ///     从一个文件中解密出一段文本，其中，这个文件是由InitBinFile建立的，并且由 EncryptToFile加密的
        /// </summary>
        /// <param name="filePath">要解密的文件</param>
        /// <param name="dataIndex">要从哪一个块中解密</param>
        /// <param name="iv">初始化向量</param>
        /// <param name="key">解密密匙</param>
        /// <returns>解密后的文本</returns>
        public static string DecryptFromFile(this string filePath, int dataIndex, byte[] iv, byte[] key)
        {
            if ((dataIndex > 10) && (dataIndex < 1))
            {
                return "";
            }

            using var tmpFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, 1024, true);
            using var decryptor = _rc2Csp.CreateDecryptor(key, iv);
            var msDecrypt = new MemoryStream();
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Write);
            var index = new byte[10261];

            tmpFileStream.Read(index, 0, 10261);
            var count = index[dataIndex * 2 - 1] * 128 + index[dataIndex * 2];
            var tmp = new byte[count];

            Array.Copy(index, 1024 * (dataIndex - 1) + 21, tmp, 0, count);
            csDecrypt.Write(tmp, 0, count);
            csDecrypt.FlushFinalBlock();
            var decrypted = msDecrypt.ToArray();
            return _textConverter.GetString(decrypted, 0, decrypted.Length);
        }

        #endregion

        #region 将一段文本加密后保存到一个文件

        /// <summary>
        ///     将一段文本加密后保存到一个文件
        /// </summary>
        /// <param name="toEncryptText">要加密的文本数据</param>
        /// <param name="filePath">要保存的文件</param>
        /// <param name="iv">初始化向量</param>
        /// <param name="key">加密密匙</param>
        /// <returns>是否加密成功</returns>
        public static void EncryptToFile(this string toEncryptText, string filePath, byte[] iv, byte[] key)
        {
            using var tmpFileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, 1024, true);
            using var encryptor = _rc2Csp.CreateEncryptor(key, iv);
            var msEncrypt = new MemoryStream();
            using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            var tmp = _textConverter.GetBytes(toEncryptText);
            csEncrypt.Write(tmp, 0, tmp.Length);
            csEncrypt.FlushFinalBlock();
            var encrypted = msEncrypt.ToArray();
            tmpFileStream.Write(encrypted, 0, encrypted.Length);
            tmpFileStream.Flush();
        }

        #endregion

        #region 将一个被加密的文件解密

        /// <summary>
        ///     将一个被加密的文件解密
        /// </summary>
        /// <param name="filePath">要解密的文件</param>
        /// <param name="iv">初始化向量</param>
        /// <param name="key">解密密匙</param>
        /// <returns>解密后的文本</returns>
        public static string DecryptFromFile(this string filePath, byte[] iv, byte[] key)
        {
            using var tmpFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, 1024, true);
            using var decryptor = _rc2Csp.CreateDecryptor(key, iv);
            var msDecrypt = new MemoryStream();
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Write);
            var tmp = new byte[tmpFileStream.Length];
            tmpFileStream.Read(tmp, 0, tmp.Length);
            csDecrypt.Write(tmp, 0, tmp.Length);
            csDecrypt.FlushFinalBlock();
            var decrypted = msDecrypt.ToArray();
            return _textConverter.GetString(decrypted, 0, decrypted.Length);
        }

        #endregion

        #region 设置加密或解密的初始化向量

        /// <summary>
        ///     设置加密或解密的初始化向量
        /// </summary>
        /// <param name="s">长度等于8的ASCII字符集的字符串</param>
        public static void SetIV(this string s)
        {
            if (s.Length != 8)
            {
                // MessageBox.Show("输入的字符串必须为长度为8的且属于ASCII字符集的字符串");
                _iv = null;
                return;
            }

            try
            {
                _iv = _asciiEncoding.GetBytes(s);
            }
            catch (Exception)
            {
                // MessageBox.Show("输入的字符串必须为长度为8的且属于ASCII字符集的字符串");
                _iv = null;
            }
        }

        #endregion

        #region 设置加密或解密的密匙

        /// <summary>
        ///     设置加密或解密的密匙
        /// </summary>
        /// <param name="s">长度等于16的ASCII字符集的字符串</param>
        public static void SetKey(this string s)
        {
            if (s.Length != 16)
            {
                // MessageBox.Show("输入的字符串必须为长度为16的且属于ASCII字符集的字符串");
                _key = null;
                return;
            }

            try
            {
                _key = _asciiEncoding.GetBytes(s);
            }
            catch (Exception)
            {
                //MessageBox.Show("输入的字符串必须为长度为16的且属于ASCII字符集的字符串");
                _key = null;
            }
        }

        #endregion
    }

    /// <summary>
    ///     对称加密解密算法类
    /// </summary>
    public static class Rijndael
    {
        private static string _key;
        private static SymmetricAlgorithm _mobjCryptoService;

        /// <summary>
        ///     对称加密类的构造函数
        /// </summary>
        public static void SymmetricMethod()
        {
            _mobjCryptoService = new RijndaelManaged();
            _key = "Guz(%&hj7x89H$yuBI0456FtmaT5&fvHUFCy76*h%(HilJ$lhj!y6&(*jkP87jH7";
        }

        /// <summary>
        ///     获得密钥
        /// </summary>
        /// <returns>密钥</returns>
        private static byte[] GetLegalKey()
        {
            var sTemp = _key;
            _mobjCryptoService.GenerateKey();
            var bytTemp = _mobjCryptoService.Key;
            var keyLength = bytTemp.Length;
            if (sTemp.Length > keyLength)
                sTemp = sTemp.Substring(0, keyLength);
            else if (sTemp.Length < keyLength)
                sTemp = sTemp.PadRight(keyLength, ' ');
            return Encoding.ASCII.GetBytes(sTemp);
        }

        /// <summary>
        ///     获得初始向量IV
        /// </summary>
        /// <returns>初试向量IV</returns>
        private static byte[] GetLegalIV()
        {
            var sTemp = "E4ghj*Ghg7!rNIfb&95GUY86GfghUber57HBh(u%g6HJ($jhWk7&!hg4ui%$hjk";
            _mobjCryptoService.GenerateIV();
            var bytTemp = _mobjCryptoService.IV;
            var ivLength = bytTemp.Length;
            if (sTemp.Length > ivLength)
                sTemp = sTemp.Substring(0, ivLength);
            else if (sTemp.Length < ivLength)
                sTemp = sTemp.PadRight(ivLength, ' ');
            return Encoding.ASCII.GetBytes(sTemp);
        }

        /// <summary>
        ///     加密方法
        /// </summary>
        /// <param name="source">待加密的串</param>
        /// <returns>经过加密的串</returns>
        public static string Encrypto(this string source)
        {
            var bytIn = Encoding.UTF8.GetBytes(source);
            var ms = new MemoryStream();
            _mobjCryptoService.Key = GetLegalKey();
            _mobjCryptoService.IV = GetLegalIV();
            var encrypto = _mobjCryptoService.CreateEncryptor();
            var cs = new CryptoStream(ms, encrypto, CryptoStreamMode.Write);
            cs.Write(bytIn, 0, bytIn.Length);
            cs.FlushFinalBlock();
            ms.Close();
            var bytOut = ms.ToArray();
            return Convert.ToBase64String(bytOut);
        }

        /// <summary>
        ///     解密方法
        /// </summary>
        /// <param name="source">待解密的串</param>
        /// <returns>经过解密的串</returns>
        public static string Decrypto(this string source)
        {
            var bytIn = Convert.FromBase64String(source);
            var ms = new MemoryStream(bytIn, 0, bytIn.Length);
            _mobjCryptoService.Key = GetLegalKey();
            _mobjCryptoService.IV = GetLegalIV();
            var encrypto = _mobjCryptoService.CreateDecryptor();
            var cs = new CryptoStream(ms, encrypto, CryptoStreamMode.Read);
            var sr = new StreamReader(cs);
            return sr.ReadToEnd();
        }
    }
}