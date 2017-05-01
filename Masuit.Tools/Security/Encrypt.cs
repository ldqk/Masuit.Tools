using System;
using System.Collections;
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
        #region DES加密算法

        /// <summary>
        ///     DES加密算法
        ///     密钥为8位或16位
        /// </summary>
        /// <param name="pToEncrypt">需要加密的字符串</param>
        /// <param name="sKey">密钥</param>
        /// <returns>解密后的数据</returns>
        public static string DesEncrypt(this string pToEncrypt, string sKey)
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
                ret.AppendFormat("{0:X2}", b);
            return ret.ToString();
        }

        #endregion

        #region DES解密算法

        /// <summary>
        ///     DES解密算法
        ///     密钥为8位或16位
        /// </summary>
        /// <param name="pToDecrypt">需要解密的字符串</param>
        /// <param name="sKey">密钥</param>
        /// <returns>解密后的数据</returns>
        public static string DesDecrypt(this string pToDecrypt, string sKey)
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
            var ret = new StringBuilder();
            return Encoding.Default.GetString(ms.ToArray());
        }

        #endregion

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
                ret.AppendFormat("{0:X2}", b);
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

            var ret = new StringBuilder();

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
            var zeros = 0;
            var ones = 1;
            var size = 0;
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
            return new[] { A, B, C, D };
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
        private static ASCIIEncoding asciiEncoding;
        private static byte[] iv;
        private static byte[] key;
        private static RC2CryptoServiceProvider rc2CSP;
        private static UnicodeEncoding textConverter;

        static RC2()
        {
            InitializeComponent();
        }

        private static void InitializeComponent()
        {
            key = new byte[] { 106, 51, 25, 141, 157, 142, 23, 111, 234, 159, 187, 154, 215, 34, 37, 204 };
            iv = new byte[] { 135, 186, 133, 136, 184, 149, 153, 144 };
            asciiEncoding = new ASCIIEncoding();
            textConverter = new UnicodeEncoding();
            rc2CSP = new RC2CryptoServiceProvider();
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
                var writeFileStream = new FileStream(filePath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None, 512, false);

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
                return r;
            byte[] encrypted;
            //打开要写入的文件，主要是为了保持原文件的内容不丢失
            var tmpFileStream = new FileStream(filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.None, 1024, true);

            var index = new byte[10261];
            //将读取的内容写到byte数组
            tmpFileStream.Read(index, 0, 10261);
            tmpFileStream.Close();
            //定义基本的加密转换运算
            var Encryptor = rc2CSP.CreateEncryptor(key, iv);
            var msEncrypt = new MemoryStream();
            //在此加密转换流中，加密将从csEncrypt，加密后，结果在msEncrypt流中。
            var csEncrypt = new CryptoStream(msEncrypt,
                Encryptor, CryptoStreamMode.Write);
            //将要加密的文本转换成UTF-16 编码，保存在tmp数组。
            var tmp = textConverter.GetBytes(toEncryptText);
            //将tmp输入csEncrypt,将通过Encryptor来加密。
            csEncrypt.Write(tmp, 0, tmp.Length);
            //输出到msEnctypt
            csEncrypt.FlushFinalBlock();
            //将流转成byte[]
            encrypted = msEncrypt.ToArray();
            if (encrypted.Length > 1024)
                return false;
            //得到加密后数据的大小，将结果存在指定的位置。
            index[dataIndex * 2 - 1] = Convert.ToByte(Convert.ToString(encrypted.Length / 128));
            index[dataIndex * 2] = Convert.ToByte(Convert.ToString(encrypted.Length % 128));
            //将加密后的结果写入index（覆盖）
            for (var i = 0; i < encrypted.Length; i++)
                index[1024 * (dataIndex - 1) + 21 + i] = encrypted[i];
            //建立文件流
            tmpFileStream = new FileStream(filePath,
                FileMode.Truncate,
                FileAccess.Write,
                FileShare.None, 1024, true);
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
                return r;
            byte[] decrypted;
            var tmpFileStream = new FileStream(filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.None, 1024, true);

            var Decryptor = rc2CSP.CreateDecryptor(key, iv);
            var msDecrypt = new MemoryStream();
            var csDecrypt = new CryptoStream(msDecrypt,
                Decryptor, CryptoStreamMode.Write);
            var index = new byte[10261];

            tmpFileStream.Read(index, 0, 10261);
            var startIndex = 1024 * (dataIndex - 1) + 21;
            var count = index[dataIndex * 2 - 1] * 128 + index[dataIndex * 2];
            var tmp = new byte[count];

            Array.Copy(index, 1024 * (dataIndex - 1) + 21, tmp, 0, count);
            csDecrypt.Write(tmp, 0, count);
            csDecrypt.FlushFinalBlock();
            decrypted = msDecrypt.ToArray();
            r = textConverter.GetString(decrypted, 0, decrypted.Length);
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
        public static bool EncryptToFile(this string toEncryptText, string filePath)
        {
            var r = false;
            byte[] encrypted;
            var tmpFileStream = new FileStream(filePath,
                FileMode.OpenOrCreate,
                FileAccess.Write,
                FileShare.None, 1024, true);

            var Encryptor = rc2CSP.CreateEncryptor(key, iv);
            var msEncrypt = new MemoryStream();
            var csEncrypt = new CryptoStream(msEncrypt,
                Encryptor, CryptoStreamMode.Write);

            var tmp = textConverter.GetBytes(toEncryptText);
            csEncrypt.Write(tmp, 0, tmp.Length);
            csEncrypt.FlushFinalBlock();
            encrypted = msEncrypt.ToArray();
            tmpFileStream.Write(encrypted, 0, encrypted.Length);
            tmpFileStream.Flush();
            r = true;
            tmpFileStream.Close();
            return r;
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
            var r = "";
            byte[] decrypted;
            var tmpFileStream = new FileStream(filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.None, 1024, true);
            var Decryptor = rc2CSP.CreateDecryptor(key, iv);
            var msDecrypt = new MemoryStream();
            var csDecrypt = new CryptoStream(msDecrypt,
                Decryptor, CryptoStreamMode.Write);

            var tmp = new byte[tmpFileStream.Length];
            tmpFileStream.Read(tmp, 0, tmp.Length);
            csDecrypt.Write(tmp, 0, tmp.Length);
            csDecrypt.FlushFinalBlock();
            decrypted = msDecrypt.ToArray();
            r = textConverter.GetString(decrypted, 0, decrypted.Length);
            tmpFileStream.Close();
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
        public static bool EncryptToFile(this string toEncryptText, string filePath, int dataIndex, byte[] IV, byte[] Key)
        {
            var r = false;
            if ((dataIndex > 10) && (dataIndex < 1))
                return r;
            byte[] encrypted;
            //打开要写入的文件，主要是为了保持原文件的内容不丢失
            var tmpFileStream = new FileStream(filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.None, 1024, true);

            var index = new byte[10261];
            //将读取的内容写到byte数组
            tmpFileStream.Read(index, 0, 10261);
            tmpFileStream.Close();
            //定义基本的加密转换运算
            var Encryptor = rc2CSP.CreateEncryptor(Key, IV);
            var msEncrypt = new MemoryStream();
            //在此加密转换流中，加密将从csEncrypt，加密后，结果在msEncrypt流中。
            var csEncrypt = new CryptoStream(msEncrypt,
                Encryptor, CryptoStreamMode.Write);
            //将要加密的文本转换成UTF-16 编码，保存在tmp数组。
            var tmp = textConverter.GetBytes(toEncryptText);
            //将tmp输入csEncrypt,将通过Encryptor来加密。
            csEncrypt.Write(tmp, 0, tmp.Length);
            //输出到msEnctypt
            csEncrypt.FlushFinalBlock();
            //将流转成byte[]
            encrypted = msEncrypt.ToArray();
            if (encrypted.Length > 1024)
                return false;
            //得到加密后数据的大小，将结果存在指定的位置。
            index[dataIndex * 2 - 1] = Convert.ToByte(Convert.ToString(encrypted.Length / 128));
            index[dataIndex * 2] = Convert.ToByte(Convert.ToString(encrypted.Length % 128));
            //将加密后的结果写入index（覆盖）
            for (var i = 0; i < encrypted.Length; i++)
                index[1024 * (dataIndex - 1) + 21 + i] = encrypted[i];
            //建立文件流
            tmpFileStream = new FileStream(filePath,
                FileMode.Truncate,
                FileAccess.Write,
                FileShare.None, 1024, true);
            //写文件
            tmpFileStream.Write(index, 0, 10261);
            tmpFileStream.Flush();
            r = true;
            tmpFileStream.Close();
            return r;
        }

        #endregion

        #region 从一个文件中解密出一段文本

        /// <summary>
        ///     从一个文件中解密出一段文本，其中，这个文件是由InitBinFile建立的，并且由 EncryptToFile加密的
        /// </summary>
        /// <param name="filePath">要解密的文件</param>
        /// <param name="dataIndex">要从哪一个块中解密</param>
        /// <param name="IV">初始化向量</param>
        /// <param name="Key">解密密匙</param>
        /// <returns>解密后的文本</returns>
        public static string DecryptFromFile(this string filePath, int dataIndex, byte[] IV, byte[] Key)
        {
            var r = "";
            if ((dataIndex > 10) && (dataIndex < 1))
                return r;
            byte[] decrypted;
            var tmpFileStream = new FileStream(filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.None, 1024, true);

            var Decryptor = rc2CSP.CreateDecryptor(Key, IV);
            var msDecrypt = new MemoryStream();
            var csDecrypt = new CryptoStream(msDecrypt,
                Decryptor, CryptoStreamMode.Write);
            var index = new byte[10261];

            tmpFileStream.Read(index, 0, 10261);
            var startIndex = 1024 * (dataIndex - 1) + 21;
            var count = index[dataIndex * 2 - 1] * 128 + index[dataIndex * 2];
            var tmp = new byte[count];

            Array.Copy(index, 1024 * (dataIndex - 1) + 21, tmp, 0, count);
            csDecrypt.Write(tmp, 0, count);
            csDecrypt.FlushFinalBlock();
            decrypted = msDecrypt.ToArray();
            r = textConverter.GetString(decrypted, 0, decrypted.Length);
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
        /// <param name="IV">初始化向量</param>
        /// <param name="Key">加密密匙</param>
        /// <returns>是否加密成功</returns>
        public static bool EncryptToFile(this string toEncryptText, string filePath, byte[] IV, byte[] Key)
        {
            var r = false;
            byte[] encrypted;
            var tmpFileStream = new FileStream(filePath,
                FileMode.OpenOrCreate,
                FileAccess.Write,
                FileShare.None, 1024, true);

            var Encryptor = rc2CSP.CreateEncryptor(Key, IV);
            var msEncrypt = new MemoryStream();
            var csEncrypt = new CryptoStream(msEncrypt,
                Encryptor, CryptoStreamMode.Write);

            var tmp = textConverter.GetBytes(toEncryptText);
            csEncrypt.Write(tmp, 0, tmp.Length);
            csEncrypt.FlushFinalBlock();
            encrypted = msEncrypt.ToArray();
            tmpFileStream.Write(encrypted, 0, encrypted.Length);
            tmpFileStream.Flush();
            r = true;
            tmpFileStream.Close();
            return r;
        }

        #endregion

        #region 将一个被加密的文件解密

        /// <summary>
        ///     将一个被加密的文件解密
        /// </summary>
        /// <param name="filePath">要解密的文件</param>
        /// <param name="IV">初始化向量</param>
        /// <param name="Key">解密密匙</param>
        /// <returns>解密后的文本</returns>
        public static string DecryptFromFile(this string filePath, byte[] IV, byte[] Key)
        {
            var r = "";
            byte[] decrypted;
            var tmpFileStream = new FileStream(filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.None, 1024, true);
            var Decryptor = rc2CSP.CreateDecryptor(Key, IV);
            var msDecrypt = new MemoryStream();
            var csDecrypt = new CryptoStream(msDecrypt,
                Decryptor, CryptoStreamMode.Write);

            var tmp = new byte[tmpFileStream.Length];
            tmpFileStream.Read(tmp, 0, tmp.Length);
            csDecrypt.Write(tmp, 0, tmp.Length);
            csDecrypt.FlushFinalBlock();
            decrypted = msDecrypt.ToArray();
            r = textConverter.GetString(decrypted, 0, decrypted.Length);
            tmpFileStream.Close();
            return r;
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
                iv = null;
                return;
            }
            try
            {
                iv = asciiEncoding.GetBytes(s);
            }
            catch (Exception)
            {
                // MessageBox.Show("输入的字符串必须为长度为8的且属于ASCII字符集的字符串");
                iv = null;
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
                key = null;
                return;
            }
            try
            {
                key = asciiEncoding.GetBytes(s);
            }
            catch (Exception)
            {
                //MessageBox.Show("输入的字符串必须为长度为16的且属于ASCII字符集的字符串");
                key = null;
            }
        }

        #endregion
    }

    /// <summary>
    ///     对称加密解密算法类
    /// </summary>
    public static class Rijndael
    {
        private static string Key;
        private static SymmetricAlgorithm mobjCryptoService;

        /// <summary>
        ///     对称加密类的构造函数
        /// </summary>
        public static void SymmetricMethod()
        {
            mobjCryptoService = new RijndaelManaged();
            Key = "Guz(%&hj7x89H$yuBI0456FtmaT5&fvHUFCy76*h%(HilJ$lhj!y6&(*jkP87jH7";
        }

        /// <summary>
        ///     获得密钥
        /// </summary>
        /// <returns>密钥</returns>
        private static byte[] GetLegalKey()
        {
            var sTemp = Key;
            mobjCryptoService.GenerateKey();
            var bytTemp = mobjCryptoService.Key;
            var KeyLength = bytTemp.Length;
            if (sTemp.Length > KeyLength)
                sTemp = sTemp.Substring(0, KeyLength);
            else if (sTemp.Length < KeyLength)
                sTemp = sTemp.PadRight(KeyLength, ' ');
            return Encoding.ASCII.GetBytes(sTemp);
        }

        /// <summary>
        ///     获得初始向量IV
        /// </summary>
        /// <returns>初试向量IV</returns>
        private static byte[] GetLegalIV()
        {
            var sTemp = "E4ghj*Ghg7!rNIfb&95GUY86GfghUber57HBh(u%g6HJ($jhWk7&!hg4ui%$hjk";
            mobjCryptoService.GenerateIV();
            var bytTemp = mobjCryptoService.IV;
            var IVLength = bytTemp.Length;
            if (sTemp.Length > IVLength)
                sTemp = sTemp.Substring(0, IVLength);
            else if (sTemp.Length < IVLength)
                sTemp = sTemp.PadRight(IVLength, ' ');
            return Encoding.ASCII.GetBytes(sTemp);
        }

        /// <summary>
        ///     加密方法
        /// </summary>
        /// <param name="Source">待加密的串</param>
        /// <returns>经过加密的串</returns>
        public static string Encrypto(this string Source)
        {
            var bytIn = Encoding.UTF8.GetBytes(Source);
            var ms = new MemoryStream();
            mobjCryptoService.Key = GetLegalKey();
            mobjCryptoService.IV = GetLegalIV();
            var encrypto = mobjCryptoService.CreateEncryptor();
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
        /// <param name="Source">待解密的串</param>
        /// <returns>经过解密的串</returns>
        public static string Decrypto(this string Source)
        {
            var bytIn = Convert.FromBase64String(Source);
            var ms = new MemoryStream(bytIn, 0, bytIn.Length);
            mobjCryptoService.Key = GetLegalKey();
            mobjCryptoService.IV = GetLegalIV();
            var encrypto = mobjCryptoService.CreateDecryptor();
            var cs = new CryptoStream(ms, encrypto, CryptoStreamMode.Read);
            var sr = new StreamReader(cs);
            return sr.ReadToEnd();
        }
    }
}