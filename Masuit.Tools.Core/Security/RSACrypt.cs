using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Masuit.Tools.Security
{
    /// <summary>
    /// RSA密钥对
    /// </summary>
    public class RsaKey
    {
        /// <summary>
        /// 公钥
        /// </summary>
        public string PublicKey;

        /// <summary>
        /// 私钥
        /// </summary>
        public string PrivateKey;
    }

    /// <summary> 
    /// RSA加密解密及RSA签名和验证
    /// </summary> 
    public static class RsaCrypt
    {
        private static RsaKey RsaKey = GenerateRsaKeys();
        #region RSA 加密解密 

        #region RSA 的密钥产生 

        /// <summary>
        /// 生成 RSA 公钥和私钥
        /// </summary>
        public static RsaKey GenerateRsaKeys()
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                return RsaKey ?? (RsaKey = new RsaKey
                {
                    PrivateKey = rsa.ToXmlString(true),
                    PublicKey = rsa.ToXmlString(false)
                });
            }
        }

        #endregion

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
            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(publicKey);
            var plainTextBArray = new UnicodeEncoding().GetBytes(mStrEncryptString);
            var cypherTextBArray = rsa.Encrypt(plainTextBArray, false);
            return Convert.ToBase64String(cypherTextBArray);
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
            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(publicKey);
            var cypherTextBArray = rsa.Encrypt(encryptString, false);
            return Convert.ToBase64String(cypherTextBArray);
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

        #endregion

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
            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(privateKey);
            var plainTextBArray = Convert.FromBase64String(mStrDecryptString);
            var dypherTextBArray = rsa.Decrypt(plainTextBArray, false);
            return new UnicodeEncoding().GetString(dypherTextBArray);
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
            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(privateKey);
            var dypherTextBArray = rsa.Decrypt(decryptString, false);
            return new UnicodeEncoding().GetString(dypherTextBArray);
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

        #endregion

        #endregion

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
            HashAlgorithm md5 = HashAlgorithm.Create("MD5");
            var buffer = Encoding.UTF8.GetBytes(mStrSource);
            return md5?.ComputeHash(buffer);
        }

        /// <summary>
        /// 获取Hash描述表
        /// </summary>
        /// <param name="mStrSource">源数据</param>
        /// <returns>Hash描述表</returns>
        public static string GetHashString(this string mStrSource)
        {
            //从字符串中取得Hash描述 
            HashAlgorithm md5 = HashAlgorithm.Create("MD5");
            var buffer = Encoding.UTF8.GetBytes(mStrSource);
            var hashData = md5?.ComputeHash(buffer);
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
            using (objFile)
            {
                HashAlgorithm md5 = HashAlgorithm.Create("MD5");
                return md5?.ComputeHash(objFile);
            }
        }

        /// <summary>
        /// 从文件流获取Hash描述表
        /// </summary>
        /// <param name="objFile">源文件</param>
        /// <returns>Hash描述表</returns>
        public static string GetHashString(this FileStream objFile)
        {
            //从文件中取得Hash描述 
            using (objFile)
            {
                HashAlgorithm md5 = HashAlgorithm.Create("MD5");
                var hashData = md5?.ComputeHash(objFile);
                return Convert.ToBase64String(hashData);
            }
        }

        #endregion

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
            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(privateKey);
            var rsaFormatter = new RSAPKCS1SignatureFormatter(rsa);
            //设置签名的算法为MD5 
            rsaFormatter.SetHashAlgorithm("MD5");
            //执行签名 
            return rsaFormatter.CreateSignature(hashbyteSignature);
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
            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(privateKey);
            var rsaFormatter = new RSAPKCS1SignatureFormatter(rsa);
            //设置签名的算法为MD5 
            rsaFormatter.SetHashAlgorithm("MD5");
            //执行签名 
            var encryptedSignatureData = rsaFormatter.CreateSignature(hashbyteSignature);
            return Convert.ToBase64String(encryptedSignatureData);
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
            byte[] hashbyteSignature = Convert.FromBase64String(mStrHashbyteSignature);
            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(pStrKeyPrivate);
            var rsaFormatter = new RSAPKCS1SignatureFormatter(rsa);
            //设置签名的算法为MD5 
            rsaFormatter.SetHashAlgorithm("MD5");
            //执行签名 
            return rsaFormatter.CreateSignature(hashbyteSignature);
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
            var hashbyteSignature = Convert.FromBase64String(mStrHashbyteSignature);
            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(pStrKeyPrivate);
            var rsaFormatter = new RSAPKCS1SignatureFormatter(rsa);
            //设置签名的算法为MD5 
            rsaFormatter.SetHashAlgorithm("MD5");
            //执行签名 
            var encryptedSignatureData = rsaFormatter.CreateSignature(hashbyteSignature);
            return Convert.ToBase64String(encryptedSignatureData);
        }

        #endregion

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
            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(publicKey);
            var rsaDeformatter = new RSAPKCS1SignatureDeformatter(rsa);
            //指定解密的时候HASH算法为MD5 
            rsaDeformatter.SetHashAlgorithm("MD5");
            if (rsaDeformatter.VerifySignature(hashbyteDeformatter, deformatterData)) return true;
            return false;
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
            byte[] hashbyteDeformatter = Convert.FromBase64String(pStrHashbyteDeformatter);
            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(publicKey);
            var rsaDeformatter = new RSAPKCS1SignatureDeformatter(rsa);
            //指定解密的时候HASH算法为MD5 
            rsaDeformatter.SetHashAlgorithm("MD5");
            if (rsaDeformatter.VerifySignature(hashbyteDeformatter, deformatterData)) return true;
            return false;
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
            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(publicKey);
            var rsaDeformatter = new RSAPKCS1SignatureDeformatter(rsa);
            //指定解密的时候HASH算法为MD5 
            rsaDeformatter.SetHashAlgorithm("MD5");
            var deformatterData = Convert.FromBase64String(pStrDeformatterData);
            if (rsaDeformatter.VerifySignature(hashbyteDeformatter, deformatterData)) return true;
            return false;
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
            byte[] hashbyteDeformatter = Convert.FromBase64String(pStrHashbyteDeformatter);
            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(publicKey);
            var rsaDeformatter = new RSAPKCS1SignatureDeformatter(rsa);
            //指定解密的时候HASH算法为MD5 
            rsaDeformatter.SetHashAlgorithm("MD5");
            var deformatterData = Convert.FromBase64String(pStrDeformatterData);
            if (rsaDeformatter.VerifySignature(hashbyteDeformatter, deformatterData)) return true;
            return false;
        }

        #endregion

        #endregion
    }
}