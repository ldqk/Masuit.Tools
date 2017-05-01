using System;
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
        /// 默认的加密密钥
        /// </summary>
        public const string DEFAULT_ENCRYPT_KEY = "12345678";

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
            byte[] byKey = null;
            byte[] IV = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };

            byKey = Encoding.UTF8.GetBytes(strEncrKey.Substring(0, 8));
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] inputByteArray = Encoding.UTF8.GetBytes(strText);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(byKey, IV), CryptoStreamMode.Write);
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
            byte[] byKey = null;
            byte[] IV = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
            byte[] inputByteArray = new Byte[strText.Length];

            byKey = Encoding.UTF8.GetBytes(sDecrKey.Substring(0, 8));
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            inputByteArray = Convert.FromBase64String(strText);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(byKey, IV), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            Encoding encoding = new UTF8Encoding();
            return encoding.GetString(ms.ToArray());
        }

        /// <summary> 
        /// Encrypt files 
        /// Attention:key must be 8 bits 
        /// </summary> 
        /// <param name="m_InFilePath">Encrypt file path</param> 
        /// <param name="m_OutFilePath">output file</param> 
        /// <param name="strEncrKey">key</param> 
        public static void DesEncrypt(this string m_InFilePath, string m_OutFilePath, string strEncrKey)
        {
            byte[] byKey = null;
            byte[] IV = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };

            byKey = Encoding.UTF8.GetBytes(strEncrKey.Substring(0, 8));
            FileStream fin = new FileStream(m_InFilePath, FileMode.Open, FileAccess.Read);
            FileStream fout = new FileStream(m_OutFilePath, FileMode.OpenOrCreate, FileAccess.Write);
            fout.SetLength(0);
            //Create variables to help with read and write. 
            byte[] bin = new byte[100]; //This is intermediate storage for the encryption. 
            long rdlen = 0; //This is the total number of bytes written. 
            long totlen = fin.Length; //This is the total length of the input file. 
            int len; //This is the number of bytes to be written at a time. 

            DES des = new DESCryptoServiceProvider();
            CryptoStream encStream = new CryptoStream(fout, des.CreateEncryptor(byKey, IV), CryptoStreamMode.Write);

            //Read from the input file, then encrypt and write to the output file. 
            while (rdlen < totlen)
            {
                len = fin.Read(bin, 0, 100);
                encStream.Write(bin, 0, len);
                rdlen += len;
            }
            encStream.Close();
            fout.Close();
            fin.Close();
        }

        /// <summary> 
        /// Decrypt files 
        /// Attention:key must be 8 bits 
        /// </summary> 
        /// <param name="m_InFilePath">Decrypt filepath</param> 
        /// <param name="m_OutFilePath">output filepath</param> 
        /// <param name="sDecrKey">key</param> 
        public static void DesDecrypt(this string m_InFilePath, string m_OutFilePath, string sDecrKey)
        {
            byte[] byKey = null;
            byte[] IV = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };

            byKey = Encoding.UTF8.GetBytes(sDecrKey.Substring(0, 8));
            FileStream fin = new FileStream(m_InFilePath, FileMode.Open, FileAccess.Read);
            FileStream fout = new FileStream(m_OutFilePath, FileMode.OpenOrCreate, FileAccess.Write);
            fout.SetLength(0);
            //Create variables to help with read and write. 
            byte[] bin = new byte[100]; //This is intermediate storage for the encryption. 
            long rdlen = 0; //This is the total number of bytes written. 
            long totlen = fin.Length; //This is the total length of the input file. 
            int len; //This is the number of bytes to be written at a time. 

            DES des = new DESCryptoServiceProvider();
            CryptoStream encStream = new CryptoStream(fout, des.CreateDecryptor(byKey, IV), CryptoStreamMode.Write);

            //Read from the input file, then encrypt and write to the output file. 
            while (rdlen < totlen)
            {
                len = fin.Read(bin, 0, 100);
                encStream.Write(bin, 0, len);
                rdlen += len;
            }
            encStream.Close();
            fout.Close();
            fin.Close();
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
        public static string AES_Encrypt(this string encryptString)
        {
            return AES_Encrypt(encryptString, Default_AES_Key);
        }

        /// <summary>
        /// 对称加密算法AES RijndaelManaged加密(RijndaelManaged（AES）算法是块式加密算法)
        /// </summary>
        /// <param name="encryptString">待加密字符串</param>
        /// <param name="encryptKey">加密密钥，须半角字符</param>
        /// <returns>加密结果字符串</returns>
        public static string AES_Encrypt(this string encryptString, string encryptKey)
        {
            encryptKey = GetSubString(encryptKey, 32, "");
            encryptKey = encryptKey.PadRight(32, ' ');

            RijndaelManaged rijndaelProvider = new RijndaelManaged();
            rijndaelProvider.Key = Encoding.UTF8.GetBytes(encryptKey.Substring(0, 32));
            rijndaelProvider.IV = Keys;
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
        public static string AES_Decrypt(this string decryptString)
        {
            return AES_Decrypt(decryptString, Default_AES_Key);
        }

        /// <summary>
        /// 对称加密算法AES RijndaelManaged解密字符串
        /// </summary>
        /// <param name="decryptString">待解密的字符串</param>
        /// <param name="decryptKey">解密密钥,和加密密钥相同</param>
        /// <returns>解密成功返回解密后的字符串,失败返回空</returns>
        public static string AES_Decrypt(this string decryptString, string decryptKey)
        {
            try
            {
                decryptKey = GetSubString(decryptKey, 32, "");
                decryptKey = decryptKey.PadRight(32, ' ');

                RijndaelManaged rijndaelProvider = new RijndaelManaged();
                rijndaelProvider.Key = Encoding.UTF8.GetBytes(decryptKey);
                rijndaelProvider.IV = Keys;
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
            string myResult = sourceString;

            //当是日文或韩文时(注:中文的范围:\u4e00 - \u9fa5, 日文在\u0800 - \u4e00, 韩文为\xAC00-\xD7A3)
            if (System.Text.RegularExpressions.Regex.IsMatch(sourceString, "[\u0800-\u4e00]+") || System.Text.RegularExpressions.Regex.IsMatch(sourceString, "[\xAC00-\xD7A3]+"))
            {
                //当截取的起始位置超出字段串长度时
                if (startIndex >= sourceString.Length)
                {
                    return string.Empty;
                }
                else
                {
                    return sourceString.Substring(startIndex,
                                                   ((length + startIndex) > sourceString.Length) ? (sourceString.Length - startIndex) : length);
                }
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

                int[] anResultFlag = new int[length];
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
                myResult = Encoding.Default.GetString(bsResult);
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
        public static CryptoStream AES_EncryptStrream(this FileStream fs, string decryptKey)
        {
            decryptKey = GetSubString(decryptKey, 32, "");
            decryptKey = decryptKey.PadRight(32, ' ');

            RijndaelManaged rijndaelProvider = new RijndaelManaged();
            rijndaelProvider.Key = Encoding.UTF8.GetBytes(decryptKey);
            rijndaelProvider.IV = Keys;

            ICryptoTransform encrypto = rijndaelProvider.CreateEncryptor();
            CryptoStream cytptostreamEncr = new CryptoStream(fs, encrypto, CryptoStreamMode.Write);
            return cytptostreamEncr;
        }

        /// <summary>
        /// 解密文件流
        /// </summary>
        /// <param name="fs">需要解密的文件流</param>
        /// <param name="decryptKey">解密密钥</param>
        /// <returns>加密流</returns>
        public static CryptoStream AES_DecryptStream(this FileStream fs, string decryptKey)
        {
            decryptKey = GetSubString(decryptKey, 32, "");
            decryptKey = decryptKey.PadRight(32, ' ');

            RijndaelManaged rijndaelProvider = new RijndaelManaged();
            rijndaelProvider.Key = Encoding.UTF8.GetBytes(decryptKey);
            rijndaelProvider.IV = Keys;
            ICryptoTransform Decrypto = rijndaelProvider.CreateDecryptor();
            CryptoStream cytptostreamDecr = new CryptoStream(fs, Decrypto, CryptoStreamMode.Read);
            return cytptostreamDecr;
        }

        /// <summary>
        /// 对指定文件加密
        /// </summary>
        /// <param name="InputFile">源文件</param>
        /// <param name="OutputFile">输出文件</param>
        /// <returns>处理结果</returns>
        public static bool AES_EncryptFile(this string InputFile, string OutputFile)
        {
            try
            {
                string decryptKey = "www.masuit.com";
                FileStream fr = new FileStream(InputFile, FileMode.Open);
                FileStream fren = new FileStream(OutputFile, FileMode.Create);
                CryptoStream Enfr = AES_EncryptStrream(fren, decryptKey);
                byte[] bytearrayinput = new byte[fr.Length];
                fr.Read(bytearrayinput, 0, bytearrayinput.Length);
                Enfr.Write(bytearrayinput, 0, bytearrayinput.Length);
                Enfr.Close();
                fr.Close();
                fren.Close();
            }
            catch
            {
                //文件异常
                return false;
            }
            return true;
        }

        /// <summary>
        /// 对指定的文件解压缩
        /// </summary>
        /// <param name="InputFile">输入文件</param>
        /// <param name="OutputFile">输出文件</param>
        /// <returns>处理结果</returns>
        public static bool AES_DecryptFile(this string InputFile, string OutputFile)
        {
            try
            {
                string decryptKey = "www.masuit.com";
                FileStream fr = new FileStream(InputFile, FileMode.Open);
                FileStream frde = new FileStream(OutputFile, FileMode.Create);
                CryptoStream Defr = AES_DecryptStream(fr, decryptKey);
                byte[] bytearrayoutput = new byte[1024];
                int m_count = 0;
                while (true)
                {
                    m_count = Defr.Read(bytearrayoutput, 0, bytearrayoutput.Length);
                    frde.Write(bytearrayoutput, 0, m_count);
                    if (m_count < bytearrayoutput.Length)
                        break;
                }
                Defr.Close();
                fr.Close();
                frde.Close();
            }
            catch
            {
                //文件异常
                return false;
            }
            return true;
        }

        #endregion

        #region Base64加密解密
        /// <summary>
        /// Base64是一種使用64基的位置計數法。它使用2的最大次方來代表僅可列印的ASCII 字元。
        /// 這使它可用來作為電子郵件的傳輸編碼。在Base64中的變數使用字元A-Z、a-z和0-9 ，
        /// 這樣共有62個字元，用來作為開始的64個數字，最後兩個用來作為數字的符號在不同的
        /// 系統中而不同。
        /// Base64加密
        /// </summary>
        /// <param name="str">需要加密的字符串</param>
        /// <returns>加密后的数据</returns>
        public static string Base64Encrypt(this string str)
        {
            byte[] encbuff = System.Text.Encoding.UTF8.GetBytes(str);
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
            return System.Text.Encoding.UTF8.GetString(decbuff);
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

        /// <summary>
        /// MD5 三次加密算法.计算过程: (QQ使用)
        /// 1. 验证码转为大写
        /// 2. 将密码使用这个方法进行三次加密后,与验证码进行叠加
        /// 3. 然后将叠加后的内容再次MD5一下,得到最终验证码的值
        /// </summary>
        /// <param name="s">需要加密的字符串</param>
        /// <returns>MD5字符串</returns>
        public static string EncyptMD5_3_16(this string s)
        {
            MD5 md5 = MD5CryptoServiceProvider.Create();
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(s);
            byte[] bytes1 = md5.ComputeHash(bytes);
            byte[] bytes2 = md5.ComputeHash(bytes1);
            byte[] bytes3 = md5.ComputeHash(bytes2);

            StringBuilder sb = new StringBuilder();
            foreach (var item in bytes3)
            {
                sb.Append(item.ToString("x").PadLeft(2, '0'));
            }
            return sb.ToString().ToUpper();
        }
        #endregion

        /// <summary>
        /// SHA256函数
        /// </summary>
        /// <param name="str">原始字符串</param>
        /// <returns>SHA256结果(返回长度为44字节的字符串)</returns>
        public static string SHA256(this string str)
        {
            byte[] SHA256Data = Encoding.UTF8.GetBytes(str);
            SHA256Managed Sha256 = new SHA256Managed();
            byte[] Result = Sha256.ComputeHash(SHA256Data);
            return Convert.ToBase64String(Result);  //返回长度为44字节的字符串
        }
    }
}