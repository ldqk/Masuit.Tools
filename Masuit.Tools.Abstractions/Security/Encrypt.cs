using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Masuit.Tools.Security
{
    /// <summary>
    /// 常用的加密解密算法
    /// </summary>
    public static class Encrypt
    {
        #region DES对称加密解密

        /// <summary>
        /// 加密密钥，默认取“masuit”的MD5值
        /// </summary>
        public static string DefaultEncryptKey = "masuit".MDString2();

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
        /// 加密字符串
        /// 加密密钥必须为8位
        /// </summary> 
        /// <param name="strText">被加密的字符串</param> 
        /// <param name="strEncrKey">8位长度密钥</param> 
        /// <returns>加密后的数据</returns> 
        public static string DesEncrypt(this string strText, string strEncrKey)
        {
            if (strEncrKey.Length < 8)
            {
                throw new Exception("密钥长度无效，密钥必须是8位！");
            }

            StringBuilder ret = new StringBuilder();
            using var des = new DESCryptoServiceProvider();
            byte[] inputByteArray = Encoding.Default.GetBytes(strText);
            des.Key = Encoding.ASCII.GetBytes(strEncrKey.Substring(0, 8));
            des.IV = Encoding.ASCII.GetBytes(strEncrKey.Substring(0, 8));
            MemoryStream ms = new MemoryStream();
            using var cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
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
            using DES des = new DESCryptoServiceProvider();
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
            using var des = new DESCryptoServiceProvider();
            var inputByteArray = new byte[pToDecrypt.Length / 2];
            for (int x = 0; x < pToDecrypt.Length / 2; x++)
            {
                int i = Convert.ToInt32(pToDecrypt.Substring(x * 2, 2), 16);
                inputByteArray[x] = (byte)i;
            }

            des.Key = Encoding.ASCII.GetBytes(sKey.Substring(0, 8));
            des.IV = Encoding.ASCII.GetBytes(sKey.Substring(0, 8));
            using var cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
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
            using var rijndaelProvider = new RijndaelManaged
            {
                Key = Encoding.UTF8.GetBytes(encryptKey.Substring(0, 32)),
                IV = Keys
            };
            using ICryptoTransform rijndaelEncrypt = rijndaelProvider.CreateEncryptor();
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
                using var rijndaelProvider = new RijndaelManaged()
                {
                    Key = Encoding.UTF8.GetBytes(decryptKey),
                    IV = Keys
                };
                using ICryptoTransform rijndaelDecrypt = rijndaelProvider.CreateDecryptor();
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
            if (Regex.IsMatch(sourceString, "[\u0800-\u4e00]+") || Regex.IsMatch(sourceString, "[\xAC00-\xD7A3]+"))
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
            using var rijndaelProvider = new RijndaelManaged()
            {
                Key = Encoding.UTF8.GetBytes(decryptKey),
                IV = Keys
            };
            using var encrypto = rijndaelProvider.CreateEncryptor();
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
            using var rijndaelProvider = new RijndaelManaged()
            {
                Key = Encoding.UTF8.GetBytes(decryptKey),
                IV = Keys
            };
            using var decrypto = rijndaelProvider.CreateDecryptor();
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
            using var enfr = AESEncryptStrream(fren, Default_AES_Key);
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
            using FileStream frde = new FileStream(outputPath, FileMode.Create);
            using CryptoStream defr = AESDecryptStream(input, Default_AES_Key);
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

        /// <summary>
        /// SHA256函数
        /// </summary>
        /// <param name="str">原始字符串</param>
        /// <returns>SHA256结果(返回长度为44字节的字符串)</returns>
        public static string SHA256(this string str)
        {
            byte[] sha256Data = Encoding.UTF8.GetBytes(str);
            using var sha256 = new SHA256Managed();
            byte[] result = sha256.ComputeHash(sha256Data);
            return Convert.ToBase64String(result); //返回长度为44字节的字符串
        }

        #region MD5加密算法

        #region 对字符串进行MD5加密

        /// <summary>
        ///     对字符串进行MD5加密
        /// </summary>
        /// <param name="message">需要加密的字符串</param>
        /// <returns>加密后的结果</returns>
        public static string MDString(this string message)
        {
            MD5 md5 = MD5.Create();
            byte[] buffer = Encoding.Default.GetBytes(message);
            byte[] bytes = md5.ComputeHash(buffer);
            return bytes.Aggregate("", (current, b) => current + b.ToString("x2"));
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
            using MD5 md5 = MD5.Create();
            byte[] bytes = Encoding.ASCII.GetBytes(s);
            byte[] bytes1 = md5.ComputeHash(bytes);
            byte[] bytes2 = md5.ComputeHash(bytes1);
            byte[] bytes3 = md5.ComputeHash(bytes2);
            return bytes3.Aggregate("", (current, b) => current + b.ToString("x2"));
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
            using MD5 md5 = MD5.Create();
            byte[] bytes = Encoding.ASCII.GetBytes(s + salt);
            byte[] bytes1 = md5.ComputeHash(bytes);
            byte[] bytes2 = md5.ComputeHash(bytes1);
            byte[] bytes3 = md5.ComputeHash(bytes2);
            return bytes3.Aggregate("", (current, b) => current + b.ToString("x2"));
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
            using var fs = File.Open(fileName, FileMode.Open, FileAccess.Read);
            var array = new byte[fs.Length];
            fs.Read(array, 0, (int)fs.Length);
            using MD5 md5 = MD5.Create();
            byte[] bytes = md5.ComputeHash(array);
            return bytes.Aggregate("", (current, b) => current + b.ToString("x2"));
        }

        /// <summary>
        /// 获取数据流的MD5值
        /// </summary>
        /// <param name="stream"></param>
        /// <returns>MD5字符串</returns>
        public static string MDString(this Stream stream)
        {
            var bytes = stream.ToArray();
            using var md5 = MD5.Create();
            var mdstr = md5.ComputeHash(bytes).Aggregate("", (current, b) => current + b.ToString("x2"));
            stream.Position = 0;
            return mdstr;
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
            using var Encryptor = _rc2Csp.CreateEncryptor(_key, _iv);
            var msEncrypt = new MemoryStream();
            //在此加密转换流中，加密将从csEncrypt，加密后，结果在msEncrypt流中。
            using var csEncrypt = new CryptoStream(msEncrypt, Encryptor, CryptoStreamMode.Write);
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
            if (dataIndex > 10 && dataIndex < 1)
            {
                return "";
            }

            using var tmpFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, 1024, true);
            using var decryptor = _rc2Csp.CreateDecryptor(_key, _iv);
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
            return _textConverter.GetString(decrypted, 0, decrypted.Length);
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
                _iv = null;
                return;
            }

            try
            {
                _iv = _asciiEncoding.GetBytes(s);
            }
            catch (Exception)
            {
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
                _key = null;
                return;
            }

            try
            {
                _key = _asciiEncoding.GetBytes(s);
            }
            catch (Exception)
            {
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
            {
                sTemp = sTemp.Substring(0, keyLength);
            }
            else if (sTemp.Length < keyLength)
            {
                sTemp = sTemp.PadRight(keyLength, ' ');
            }

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
            {
                sTemp = sTemp.Substring(0, ivLength);
            }
            else if (sTemp.Length < ivLength)
            {
                sTemp = sTemp.PadRight(ivLength, ' ');
            }

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
            using var encrypto = _mobjCryptoService.CreateEncryptor();
            using var cs = new CryptoStream(ms, encrypto, CryptoStreamMode.Write);
            cs.Write(bytIn, 0, bytIn.Length);
            cs.FlushFinalBlock();
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
            using var encrypto = _mobjCryptoService.CreateDecryptor();
            using var cs = new CryptoStream(ms, encrypto, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            return sr.ReadToEnd();
        }
    }
}
