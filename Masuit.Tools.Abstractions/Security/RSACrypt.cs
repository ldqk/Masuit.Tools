using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Masuit.Tools.Security
{
    /// <summary>
    /// RSA加密解密及RSA签名和验证
    /// </summary>
    public static class RsaCrypt
    {
        private static RsaKey RsaKey;

        #region RSA 加密解密

        #region RSA 的密钥产生

        /// <summary>
        /// 生成 RSA 公钥和私钥
        /// </summary>
        /// <param name="type">密钥类型</param>
        /// <param name="length">密钥长度</param>
        /// <returns></returns>
        public static RsaKey GenerateRsaKeys(RsaKeyType type = RsaKeyType.XML, int length = 1024)
        {
            var rsa = new RSA(length);
            return type switch
            {
                RsaKeyType.PKCS1 => RsaKey ??= new RsaKey
                {
                    PrivateKey = rsa.ToPEM_PKCS1(),
                    PublicKey = rsa.ToPEM_PKCS1(true)
                },
                RsaKeyType.PKCS8 => RsaKey ??= new RsaKey
                {
                    PrivateKey = rsa.ToPEM_PKCS8(),
                    PublicKey = rsa.ToPEM_PKCS8(true)
                },
                RsaKeyType.XML => RsaKey ??= new RsaKey
                {
                    PrivateKey = rsa.ToXML(),
                    PublicKey = rsa.ToXML(true)
                },
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        #endregion RSA 的密钥产生

        #region RSA的加密函数

        /// <summary>
        /// RSA的加密函数 string
        /// </summary>
        /// <param name="publicKey">公钥</param>
        /// <param name="mStrEncryptString">需要加密的字符串</param>
        /// <returns>加密后的内容</returns>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
        public static string RSAEncrypt(this string mStrEncryptString, string publicKey)
        {
            var rsa = new RSA(publicKey);
            return rsa.Encrypt(mStrEncryptString);
        }

        /// <summary>
        /// RSA的加密函数 string
        /// </summary>
        /// <param name="mStrEncryptString">需要加密的字符串</param>
        /// <returns>加密后的内容</returns>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
        public static string RSAEncrypt(this string mStrEncryptString)
        {
            return RSAEncrypt(mStrEncryptString, RsaKey.PublicKey);
        }

        /// <summary>
        /// RSA的加密函数 byte[]
        /// </summary>
        /// <param name="encryptString">需要加密的字节数组</param>
        /// <param name="publicKey">公钥</param>
        /// <returns>加密后的内容</returns>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
        public static string RSAEncrypt(this byte[] encryptString, string publicKey)
        {
            var rsa = new RSA(publicKey);
            return Convert.ToBase64String(rsa.Encrypt(encryptString));
        }

        /// <summary>
        /// RSA的加密函数 byte[]
        /// </summary>
        /// <param name="encryptString">需要加密的字节数组</param>
        /// <returns>加密后的内容</returns>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
        public static string RSAEncrypt(this byte[] encryptString)
        {
            return RSAEncrypt(encryptString, RsaKey.PublicKey);
        }

        #endregion RSA的加密函数

        #region RSA的解密函数

        /// <summary>
        /// RSA的解密函数  string
        /// </summary>
        /// <param name="mStrDecryptString">需要解密的字符串</param>
        /// <param name="privateKey">私钥</param>
        /// <returns>解密后的内容</returns>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
        public static string RSADecrypt(this string mStrDecryptString, string privateKey)
        {
            var rsa = new RSA(privateKey);
            return rsa.DecryptOrNull(mStrDecryptString);
        }

        /// <summary>
        /// RSA的解密函数  string
        /// </summary>
        /// <param name="mStrDecryptString">需要解密的字符串</param>
        /// <returns>解密后的内容</returns>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
        public static string RSADecrypt(this string mStrDecryptString)
        {
            return RSADecrypt(mStrDecryptString, RsaKey.PrivateKey);
        }

        /// <summary>
        /// RSA的解密函数  byte
        /// </summary>
        /// <param name="decryptString">需要解密的字符串</param>
        /// <param name="privateKey">私钥</param>
        /// <returns>解密后的内容</returns>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
        public static string RSADecrypt(this byte[] decryptString, string privateKey)
        {
            var rsa = new RSA(privateKey);
            return new UnicodeEncoding().GetString(rsa.DecryptOrNull(decryptString));
        }

        /// <summary>
        /// RSA的解密函数  byte
        /// </summary>
        /// <param name="decryptString">需要解密的字符串</param>
        /// <returns>解密后的内容</returns>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
        public static string RSADecrypt(this byte[] decryptString)
        {
            return RSADecrypt(decryptString, RsaKey.PrivateKey);
        }

        #endregion RSA的解密函数

        #endregion RSA 加密解密

        #region RSA数字签名

        #region 获取Hash描述表

        /// <summary>
        /// 获取Hash描述表
        /// </summary>
        /// <param name="mStrSource">源数据</param>
        /// <returns>Hash描述表</returns>
        public static byte[] GetHashBytes(this string mStrSource)
        {
            //从字符串中取得Hash描述
            using var md5 = MD5.Create();
            var buffer = Encoding.UTF8.GetBytes(mStrSource);
            return md5.ComputeHash(buffer);
        }

        /// <summary>
        /// 获取Hash描述表
        /// </summary>
        /// <param name="mStrSource">源数据</param>
        /// <returns>Hash描述表</returns>
        public static string GetHashString(this string mStrSource)
        {
            //从字符串中取得Hash描述
            using var md5 = MD5.Create();
            var buffer = Encoding.UTF8.GetBytes(mStrSource);
            var hashData = md5.ComputeHash(buffer);
            return Convert.ToBase64String(hashData);
        }

        /// <summary>
        /// 从文件流获取Hash描述表
        /// </summary>
        /// <param name="objFile">源文件</param>
        /// <returns>Hash描述表</returns>
        public static byte[] GetHashBytes(this FileStream objFile)
        {
            //从文件中取得Hash描述
            using var md5 = MD5.Create();
            return md5.ComputeHash(objFile);
        }

        /// <summary>
        /// 从文件流获取Hash描述表
        /// </summary>
        /// <param name="objFile">源文件</param>
        /// <returns>Hash描述表</returns>
        public static string GetHashString(this FileStream objFile)
        {
            //从文件中取得Hash描述
            using var md5 = MD5.Create();
            var hashData = md5.ComputeHash(objFile);
            return Convert.ToBase64String(hashData);
        }

        #endregion 获取Hash描述表

        #region RSA签名

        /// <summary>
        /// RSA签名
        /// </summary>
        /// <param name="hashbyteSignature">签名字节数据</param>
        /// <param name="privateKey">私钥</param>
        /// <returns>处理结果</returns>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
        /// <exception cref="CryptographicUnexpectedOperationException">The key is null.-or- The hash algorithm is null. </exception>
        public static byte[] SignatureBytes(this byte[] hashbyteSignature, string privateKey)
        {
            var rsa = new RSA(privateKey);
            return rsa.Sign("MD5", hashbyteSignature);
        }

        /// <summary>
        /// RSA签名
        /// </summary>
        /// <param name="hashbyteSignature">签名字节数据</param>
        /// <param name="privateKey">私钥</param>
        /// <returns>处理结果</returns>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
        /// <exception cref="CryptographicUnexpectedOperationException">The key is null.-or- The hash algorithm is null. </exception>
        public static string SignatureString(this byte[] hashbyteSignature, string privateKey)
        {
            return Convert.ToBase64String(SignatureBytes(hashbyteSignature, privateKey));
        }

        /// <summary>
        /// RSA签名
        /// </summary>
        /// <param name="mStrHashbyteSignature">签名字符串数据</param>
        /// <param name="pStrKeyPrivate">私钥</param>
        /// <returns>处理结果</returns>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
        /// <exception cref="CryptographicUnexpectedOperationException">The key is null.-or- The hash algorithm is null. </exception>
        public static byte[] SignatureBytes(this string mStrHashbyteSignature, string pStrKeyPrivate)
        {
            var rsa = new RSA(pStrKeyPrivate);
            return Encoding.UTF32.GetBytes(rsa.Sign("MD5", mStrHashbyteSignature));
        }

        /// <summary>
        /// RSA签名
        /// </summary>
        /// <param name="mStrHashbyteSignature">签名字符串数据</param>
        /// <param name="pStrKeyPrivate">私钥</param>
        /// <returns>处理结果</returns>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
        /// <exception cref="CryptographicUnexpectedOperationException">The key is null.-or- The hash algorithm is null. </exception>
        public static string SignatureString(this string mStrHashbyteSignature, string pStrKeyPrivate)
        {
            var rsa = new RSA(pStrKeyPrivate);
            return rsa.Sign("MD5", mStrHashbyteSignature);
        }

        #endregion RSA签名

        #region RSA 签名验证

        /// <summary>
        /// RSA 签名验证
        /// </summary>
        /// <param name="deformatterData">反格式化字节数据</param>
        /// <param name="publicKey">公钥</param>
        /// <param name="hashbyteDeformatter">哈希字节数据</param>
        /// <returns>处理结果</returns>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
        /// <exception cref="CryptographicUnexpectedOperationException">The key is null.-or- The hash algorithm is null. </exception>
        public static bool SignatureDeformatter(this byte[] deformatterData, string publicKey, byte[] hashbyteDeformatter)
        {
            var rsa = new RSA(publicKey);
            return rsa.Verify("MD5", deformatterData, hashbyteDeformatter);
        }

        /// <summary>
        /// RSA 签名验证
        /// </summary>
        /// <param name="deformatterData">反格式化字节数据</param>
        /// <param name="publicKey">公钥</param>
        /// <param name="pStrHashbyteDeformatter">哈希字符串数据</param>
        /// <returns>处理结果</returns>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
        /// <exception cref="CryptographicUnexpectedOperationException">The key is null.-or- The hash algorithm is null. </exception>
        public static bool SignatureDeformatter(this byte[] deformatterData, string publicKey, string pStrHashbyteDeformatter)
        {
            var rsa = new RSA(publicKey);
            return rsa.Verify("MD5", deformatterData, Convert.FromBase64String(pStrHashbyteDeformatter));
        }

        /// <summary>
        /// RSA 签名验证
        /// </summary>
        /// <param name="pStrDeformatterData">反格式化字符串数据</param>
        /// <param name="publicKey">公钥</param>
        /// <param name="hashbyteDeformatter">哈希字节数据</param>
        /// <returns>处理结果</returns>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
        /// <exception cref="CryptographicUnexpectedOperationException">The key is null.-or- The hash algorithm is null. </exception>
        public static bool SignatureDeformatter(this string pStrDeformatterData, string publicKey, byte[] hashbyteDeformatter)
        {
            var rsa = new RSA(publicKey);
            return rsa.Verify("MD5", Convert.FromBase64String(pStrDeformatterData), hashbyteDeformatter);
        }

        /// <summary>
        /// RSA 签名验证
        /// </summary>
        /// <param name="pStrDeformatterData">格式字符串数据</param>
        /// <param name="publicKey">公钥</param>
        /// <param name="pStrHashbyteDeformatter">哈希字符串数据</param>
        /// <returns>处理结果</returns>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
        /// <exception cref="CryptographicUnexpectedOperationException">The key is null.-or- The hash algorithm is null. </exception>
        public static bool SignatureDeformatter(this string pStrDeformatterData, string publicKey, string pStrHashbyteDeformatter)
        {
            var rsa = new RSA(publicKey);
            return rsa.Verify("MD5", Convert.FromBase64String(pStrDeformatterData), Convert.FromBase64String(pStrHashbyteDeformatter));
        }

        #endregion RSA 签名验证

        #endregion RSA数字签名
    }
}