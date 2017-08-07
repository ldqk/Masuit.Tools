using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Text;

//加密字符串,注意strEncrKey的长度为8位(如果要增加或者减少key长度,调整IV的长度就是了) 
//public string DesEncrypt(string strText, string strEncrKey) 

//解密字符串,注意strEncrKey的长度为8位(如果要增加或者减少key长度,调整IV的长度就是了) 
//public string DesDecrypt(string strText,string sDecrKey) 

//加密数据文件,注意strEncrKey的长度为8位(如果要增加或者减少key长度,调整IV的长度就是了) 
//public void DesEncrypt(string m_InFilePath,string m_OutFilePath,string strEncrKey) 

//解密数据文件,注意strEncrKey的长度为8位(如果要增加或者减少key长度,调整IV的长度就是了) 
//public void DesDecrypt(string m_InFilePath,string m_OutFilePath,string sDecrKey) 

//MD5加密 
//public string MD5Encrypt(string strText) 

namespace Masuit.Tools.Security
{
    /// <summary>
    /// DES对称加解密、AES RijndaelManaged加解密、Base64加密解密、MD5加密等操作辅助类
    /// </summary>
    public static class EncodeHelper
    {
        #region DES对称加密解密
        /// <summary>
        /// 加密密钥，需要在config配置文件中AppSettings节点中配置desSecret值，若未配置，默认取“masuit”的MD5值
        /// </summary>
        public static string DEFAULT_ENCRYPT_KEY = ConfigurationManager.AppSettings["desSecret"] ?? "masuit".MDString2();

        /// <summary>
        /// 使用默认加密
        /// </summary>
        /// <param name="strText">被加密的字符串</param>
        /// <returns>加密后的数据</returns>
        public static string DesEncrypt(this string strText)
        {
            try
            {
                return DesEncrypt(strText, DEFAULT_ENCRYPT_KEY);
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
                return DesDecrypt(strText, DEFAULT_ENCRYPT_KEY);
            }
            catch
            {
                return "";
            }
        }

        /// <summary> 
        /// 解密字符串
        /// 加密密钥必须是8位
        /// </summary> 
        /// <param name="strText">被解密的字符串</param> 
        /// <param name="strEncrKey">密钥</param> 
        /// <returns>解密后的数据</returns> 
        public static string DesEncrypt(this string strText, string strEncrKey)
        {
            byte[] iv = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };

            var byKey = Encoding.UTF8.GetBytes(strEncrKey.Substring(0, 8));
            var des = new DESCryptoServiceProvider();
            byte[] inputByteArray = Encoding.UTF8.GetBytes(strText);
            var ms = new MemoryStream();
            var cs = new CryptoStream(ms, des.CreateEncryptor(byKey, iv), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            return Convert.ToBase64String(ms.ToArray());
        }

        /// <summary> 
        /// Decrypt string 
        /// Attention:key must be 8 bits 
        /// </summary> 
        /// <param name="strText">Decrypt string</param> 
        /// <param name="sDecrKey">key</param> 
        /// <returns>output string</returns> 
        public static string DesDecrypt(this string strText, string sDecrKey)
        {
            byte[] iv = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
            var byKey = Encoding.UTF8.GetBytes(sDecrKey.Substring(0, 8));
            var des = new DESCryptoServiceProvider();
            var inputByteArray = Convert.FromBase64String(strText);
            var ms = new MemoryStream();
            var cs = new CryptoStream(ms, des.CreateDecryptor(byKey, iv), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            Encoding encoding = new UTF8Encoding();
            return encoding.GetString(ms.ToArray());
        }

        /// <summary>
        /// DES加密文件
        /// </summary>
        /// <param name="fin">文件输入流</param>
        /// <param name="outFilePath">文件输出路径</param>
        /// <param name="strEncrKey">加密密钥</param>
        public static void DesEncrypt(this FileStream fin, string outFilePath, string strEncrKey)
        {
            byte[] iv = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
            var byKey = Encoding.UTF8.GetBytes(strEncrKey.Substring(0, 8));
            using (fin)
            {
                using (FileStream fout = new FileStream(outFilePath, FileMode.OpenOrCreate, FileAccess.Write))
                {
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
            byte[] iv = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
            var byKey = Encoding.UTF8.GetBytes(sDecrKey.Substring(0, 8));
            using (fin)
            {
                using (FileStream fout = new FileStream(outFilePath, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    fout.SetLength(0);
                    byte[] bin = new byte[100];
                    long rdlen = 0;
                    long totlen = fin.Length;
                    DES des = new DESCryptoServiceProvider();
                    CryptoStream encStream = new CryptoStream(fout, des.CreateDecryptor(byKey, iv), CryptoStreamMode.Write);
                    while (rdlen < totlen)
                    {
                        var len = fin.Read(bin, 0, 100);
                        encStream.Write(bin, 0, len);
                        rdlen += len;
                    }
                }
            }
        }
        #endregion

        #region 对称加密算法AES RijndaelManaged加密解密
        private static readonly string Default_AES_Key = "@#kim123";
        private static byte[] Keys = { 0x41, 0x72, 0x65, 0x79, 0x6F, 0x75, 0x6D, 0x79,
                                             0x53,0x6E, 0x6F, 0x77, 0x6D, 0x61, 0x6E, 0x3F };

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
            var rijndaelProvider = new RijndaelManaged { Key = Encoding.UTF8.GetBytes(encryptKey.Substring(0, 32)), IV = Keys };
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
                {   //当不在有效范围内时,只取到字符串的结尾
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
            ICryptoTransform decrypto = rijndaelProvider.CreateDecryptor();
            return new CryptoStream(fs, decrypto, CryptoStreamMode.Read);
        }

        /// <summary>
        /// 对指定文件AES加密
        /// </summary>
        /// <param name="input">源文件流</param>
        /// <param name="outputPath">输出文件路径</param>
        public static void AESEncryptFile(this FileStream input, string outputPath)
        {
            using (input)
            {
                using (FileStream fren = new FileStream(outputPath, FileMode.Create))
                {
                    CryptoStream enfr = AESEncryptStrream(fren, Default_AES_Key);
                    byte[] bytearrayinput = new byte[input.Length];
                    input.Read(bytearrayinput, 0, bytearrayinput.Length);
                    enfr.Write(bytearrayinput, 0, bytearrayinput.Length);
                }
            }
        }

        /// <summary>
        /// 对指定的文件AES解密
        /// </summary>
        /// <param name="input">源文件流</param>
        /// <param name="outputPath">输出文件路径</param>
        public static void AESDecryptFile(this FileStream input, string outputPath)
        {
            using (input)
            {
                FileStream frde = new FileStream(outputPath, FileMode.Create);
                CryptoStream defr = AESDecryptStream(input, Default_AES_Key);
                byte[] bytearrayoutput = new byte[1024];
                while (true)
                {
                    var count = defr.Read(bytearrayoutput, 0, bytearrayoutput.Length);
                    frde.Write(bytearrayoutput, 0, count);
                    if (count < bytearrayoutput.Length) break;
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
            return Convert.ToBase64String(result);  //返回长度为44字节的字符串
        }
    }
}