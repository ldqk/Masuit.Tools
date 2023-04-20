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
        public static RsaKey GenerateRsaKeys(RsaKeyType type = RsaKeyType.PKCS8, int length = 1024)
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
        /// <param name="value">需要加密的字符串</param>
        /// <returns>加密后的内容</returns>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
        public static string RSAEncrypt(this string value, string publicKey)
        {
            var rsa = new RSA(publicKey);
            return rsa.Encrypt(value);
        }

        /// <summary>
        /// RSA的加密函数 string
        /// </summary>
        /// <param name="value">需要加密的字符串</param>
        /// <returns>加密后的内容</returns>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
        public static string RSAEncrypt(this string value)
        {
            return RSAEncrypt(value, RsaKey.PublicKey);
        }

        /// <summary>
        /// RSA的加密函数 byte[]
        /// </summary>
        /// <param name="data">需要加密的字节数组</param>
        /// <param name="publicKey">公钥</param>
        /// <returns>加密后的内容</returns>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
        public static string RSAEncrypt(this byte[] data, string publicKey)
        {
            var rsa = new RSA(publicKey);
            return Convert.ToBase64String(rsa.Encrypt(data));
        }

        /// <summary>
        /// RSA的加密函数 byte[]
        /// </summary>
        /// <param name="data">需要加密的字节数组</param>
        /// <returns>加密后的内容</returns>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
        public static string RSAEncrypt(this byte[] data)
        {
            return RSAEncrypt(data, RsaKey.PublicKey);
        }

        #endregion RSA的加密函数

        #region RSA的解密函数

        /// <summary>
        /// RSA的解密函数  string
        /// </summary>
        /// <param name="value">需要解密的字符串</param>
        /// <param name="privateKey">私钥</param>
        /// <returns>解密后的内容</returns>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
        public static string RSADecrypt(this string value, string privateKey)
        {
            var rsa = new RSA(privateKey);
            return rsa.DecryptOrNull(value);
        }

        /// <summary>
        /// RSA的解密函数  string
        /// </summary>
        /// <param name="value">需要解密的字符串</param>
        /// <returns>解密后的内容</returns>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
        public static string RSADecrypt(this string value)
        {
            return RSADecrypt(value, RsaKey.PrivateKey);
        }

        /// <summary>
        /// RSA的解密函数  byte
        /// </summary>
        /// <param name="data">需要解密的字符串</param>
        /// <param name="privateKey">私钥</param>
        /// <returns>解密后的内容</returns>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
        public static string RSADecrypt(this byte[] data, string privateKey)
        {
            var rsa = new RSA(privateKey);
            return new UnicodeEncoding().GetString(rsa.DecryptOrNull(data));
        }

        /// <summary>
        /// RSA的解密函数  byte
        /// </summary>
        /// <param name="data">需要解密的字符串</param>
        /// <returns>解密后的内容</returns>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
        public static string RSADecrypt(this byte[] data)
        {
            return RSADecrypt(data, RsaKey.PrivateKey);
        }

        #endregion RSA的解密函数

        #endregion RSA 加密解密

        #region RSA数字签名

        #region 获取Hash描述表

        /// <summary>
        /// 获取Hash描述表
        /// </summary>
        /// <param name="value">源数据</param>
        /// <returns>Hash描述表</returns>
        public static byte[] GetHashBytes(this string value)
        {
            //从字符串中取得Hash描述
            using var md5 = MD5.Create();
            var buffer = Encoding.UTF8.GetBytes(value);
            return md5.ComputeHash(buffer);
        }

        /// <summary>
        /// 获取Hash描述表
        /// </summary>
        /// <param name="value">源数据</param>
        /// <returns>Hash描述表</returns>
        public static string GetHashString(this string value)
        {
            //从字符串中取得Hash描述
            using var md5 = MD5.Create();
            var buffer = Encoding.UTF8.GetBytes(value);
            var hashData = md5.ComputeHash(buffer);
            return Convert.ToBase64String(hashData);
        }

        /// <summary>
        /// 从文件流获取Hash描述表
        /// </summary>
        /// <param name="file">源文件</param>
        /// <returns>Hash描述表</returns>
        public static byte[] GetHashBytes(this FileStream file)
        {
            //从文件中取得Hash描述
            using var md5 = MD5.Create();
            return md5.ComputeHash(file);
        }

        /// <summary>
        /// 从文件流获取Hash描述表
        /// </summary>
        /// <param name="file">源文件</param>
        /// <returns>Hash描述表</returns>
        public static string GetHashString(this FileStream file)
        {
            //从文件中取得Hash描述
            using var md5 = MD5.Create();
            var hashData = md5.ComputeHash(file);
            return Convert.ToBase64String(hashData);
        }

        #endregion 获取Hash描述表

        #region RSA签名

        /// <summary>
        /// RSA签名
        /// </summary>
        /// <param name="data">签名字节数据</param>
        /// <param name="privateKey">私钥</param>
        /// <param name="halg">hash算法</param>
        /// <returns>处理结果</returns>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
        /// <exception cref="CryptographicUnexpectedOperationException">The key is null.-or- The hash algorithm is null. </exception>
        public static byte[] SignatureBytes(this byte[] data, string privateKey, HashAlgo halg = HashAlgo.MD5)
        {
            var rsa = new RSA(privateKey);
            return rsa.Sign(halg.ToString(), data);
        }

        /// <summary>
        /// RSA签名
        /// </summary>
        /// <param name="data">签名字节数据</param>
        /// <param name="privateKey">私钥</param>
        /// <returns>处理结果</returns>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
        /// <exception cref="CryptographicUnexpectedOperationException">The key is null.-or- The hash algorithm is null. </exception>
        public static string SignatureString(this byte[] data, string privateKey)
        {
            return Convert.ToBase64String(SignatureBytes(data, privateKey));
        }

        /// <summary>
        /// RSA签名
        /// </summary>
        /// <param name="value">签名字符串数据</param>
        /// <param name="privateKey">私钥</param>
        /// <param name="halg">hash算法</param>
        /// <returns>处理结果</returns>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
        /// <exception cref="CryptographicUnexpectedOperationException">The key is null.-or- The hash algorithm is null. </exception>
        public static byte[] SignatureBytes(this string value, string privateKey, HashAlgo halg = HashAlgo.MD5)
        {
            var rsa = new RSA(privateKey);
            return Encoding.UTF32.GetBytes(rsa.Sign(halg.ToString(), value));
        }

        /// <summary>
        /// RSA签名
        /// </summary>
        /// <param name="value">签名字符串数据</param>
        /// <param name="privateKey">私钥</param>
        /// <param name="halg">hash算法</param>
        /// <returns>处理结果</returns>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
        /// <exception cref="CryptographicUnexpectedOperationException">The key is null.-or- The hash algorithm is null. </exception>
        public static string SignatureString(this string value, string privateKey, HashAlgo halg = HashAlgo.MD5)
        {
            var rsa = new RSA(privateKey);
            return rsa.Sign(halg.ToString(), value);
        }

        #endregion RSA签名

        #region RSA 签名验证

        /// <summary>
        /// RSA 签名验证
        /// </summary>
        /// <param name="data">反格式化字节数据</param>
        /// <param name="publicKey">公钥</param>
        /// <param name="sign">哈希字节数据</param>
        /// <param name="halg">hash算法</param>
        /// <returns>处理结果</returns>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
        /// <exception cref="CryptographicUnexpectedOperationException">The key is null.-or- The hash algorithm is null. </exception>
        public static bool SignatureDeformatter(this byte[] data, string publicKey, byte[] sign, HashAlgo halg = HashAlgo.MD5)
        {
            var rsa = new RSA(publicKey);
            return rsa.Verify(halg.ToString(), sign, data);
        }

        /// <summary>
        /// RSA 签名验证
        /// </summary>
        /// <param name="data">反格式化字节数据</param>
        /// <param name="publicKey">公钥</param>
        /// <param name="sign">哈希字符串数据</param>
        /// <param name="halg">hash算法</param>
        /// <returns>处理结果</returns>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
        /// <exception cref="CryptographicUnexpectedOperationException">The key is null.-or- The hash algorithm is null. </exception>
        public static bool SignatureDeformatter(this byte[] data, string publicKey, string sign, HashAlgo halg = HashAlgo.MD5)
        {
            var rsa = new RSA(publicKey);
            return rsa.Verify(halg.ToString(), Convert.FromBase64String(sign), data);
        }

        /// <summary>
        /// RSA 签名验证
        /// </summary>
        /// <param name="value">反格式化字符串数据</param>
        /// <param name="publicKey">公钥</param>
        /// <param name="sign">哈希字节数据</param>
        /// <param name="halg">hash算法</param>
        /// <returns>处理结果</returns>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
        /// <exception cref="CryptographicUnexpectedOperationException">The key is null.-or- The hash algorithm is null. </exception>
        public static bool SignatureDeformatter(this string value, string publicKey, byte[] sign, HashAlgo halg = HashAlgo.MD5)
        {
            var rsa = new RSA(publicKey);
            return rsa.Verify(halg.ToString(), sign, Convert.FromBase64String(value));
        }

        /// <summary>
        /// RSA 签名验证
        /// </summary>
        /// <param name="value">格式字符串数据</param>
        /// <param name="publicKey">公钥</param>
        /// <param name="sign">哈希字符串数据</param>
        /// <param name="halg">hash算法</param>
        /// <returns>处理结果</returns>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
        /// <exception cref="CryptographicUnexpectedOperationException">The key is null.-or- The hash algorithm is null. </exception>
        public static bool SignatureDeformatter(this string value, string publicKey, string sign, HashAlgo halg = HashAlgo.MD5)
        {
            var rsa = new RSA(publicKey);
            return rsa.Verify(halg.ToString(), sign, value);
        }

        #endregion RSA 签名验证

        #endregion RSA数字签名
    }
}
