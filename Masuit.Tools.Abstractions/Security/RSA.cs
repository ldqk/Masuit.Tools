using Masuit.Tools.Systems;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Masuit.Tools.Security
{
    /// <summary>
    /// RSA操作类
    /// </summary>
    internal class RSA
    {
        /// <summary>
        /// 导出XML格式密钥对，如果convertToPublic含私钥的RSA将只返回公钥，仅含公钥的RSA不受影响
        /// </summary>
        public string ToXML(bool convertToPublic = false)
        {
            return RSAObject.ToXmlString(!RSAObject.PublicOnly && !convertToPublic);
        }

        /// <summary>
        /// 导出PEM PKCS#1格式密钥对，如果convertToPublic含私钥的RSA将只返回公钥，仅含公钥的RSA不受影响
        /// </summary>
        public string ToPEM_PKCS1(bool convertToPublic = false)
        {
            return new RsaPem(RSAObject).ToPEM(convertToPublic, false);
        }

        /// <summary>
        /// 导出PEM PKCS#8格式密钥对，如果convertToPublic含私钥的RSA将只返回公钥，仅含公钥的RSA不受影响
        /// </summary>
        public string ToPEM_PKCS8(bool convertToPublic = false)
        {
            return new RsaPem(RSAObject).ToPEM(convertToPublic, true);
        }

        /// <summary>
        /// 将密钥对导出成PEM对象，如果convertToPublic含私钥的RSA将只返回公钥，仅含公钥的RSA不受影响
        /// </summary>
        public RsaPem ToPEM(bool convertToPublic = false)
        {
            return new RsaPem(RSAObject, convertToPublic);
        }

        /// <summary>
        /// 加密字符串（utf-8），出错抛异常
        /// </summary>
        public string Encrypt(string str)
        {
            return Convert.ToBase64String(Encrypt(Encoding.UTF8.GetBytes(str)));
        }

        /// <summary>
        /// 加密数据，出错抛异常
        /// </summary>
        public byte[] Encrypt(byte[] data)
        {
            int blockLen = RSAObject.KeySize / 8 - 11;
            if (data.Length <= blockLen)
            {
                return RSAObject.Encrypt(data, false);
            }

            using var dataStream = new PooledMemoryStream(data);
            using var enStream = new PooledMemoryStream();
            var buffer = new byte[blockLen];
            int len = dataStream.Read(buffer, 0, blockLen);

            while (len > 0)
            {
                var block = new byte[len];
                Array.Copy(buffer, 0, block, 0, len);
                var enBlock = RSAObject.Encrypt(block, false);
                enStream.Write(enBlock, 0, enBlock.Length);
                len = dataStream.Read(buffer, 0, blockLen);
            }

            return enStream.ToArray();
        }

        /// <summary>
        /// 解密字符串（utf-8），解密异常返回null
        /// </summary>
        public string DecryptOrNull(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }

            try
            {
                var bytes = Convert.FromBase64String(str);
                var val = DecryptOrNull(bytes);
                return val == null ? null : Encoding.UTF8.GetString(val);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 解密数据，解密异常返回null
        /// </summary>
        public byte[] DecryptOrNull(byte[] data)
        {
            try
            {
                int blockLen = RSAObject.KeySize / 8;
                if (data.Length <= blockLen)
                {
                    return RSAObject.Decrypt(data, false);
                }

                using var dataStream = new PooledMemoryStream(data);
                using var deStream = new PooledMemoryStream();
                byte[] buffer = new byte[blockLen];
                int len = dataStream.Read(buffer, 0, blockLen);

                while (len > 0)
                {
                    var block = new byte[len];
                    Array.Copy(buffer, 0, block, 0, len);
                    var deBlock = RSAObject.Decrypt(block, false);
                    deStream.Write(deBlock, 0, deBlock.Length);
                    len = dataStream.Read(buffer, 0, blockLen);
                }

                return deStream.ToArray();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 对str进行签名，并指定hash算法（如：SHA256）
        /// </summary>
        public string Sign(string hash, string str)
        {
            return Convert.ToBase64String(Sign(hash, Encoding.UTF8.GetBytes(str)));
        }

        /// <summary>
        /// 对data进行签名，并指定hash算法（如：SHA256）
        /// </summary>
        public byte[] Sign(string hash, byte[] data)
        {
            return RSAObject.SignData(data, hash);
        }

        /// <summary>
        /// 验证字符串str的签名是否是sgin，并指定hash算法（如：SHA256）
        /// </summary>
        public bool Verify(string hash, string sgin, string str)
        {
            byte[] bytes = null;
            try
            {
                bytes = Convert.FromBase64String(sgin);
            }
            catch
            {
            }

            return bytes != null && Verify(hash, bytes, Encoding.UTF8.GetBytes(str));
        }

        /// <summary>
        /// 验证data的签名是否是sgin，并指定hash算法（如：SHA256）
        /// </summary>
        public bool Verify(string hash, byte[] sgin, byte[] data)
        {
            try
            {
                return RSAObject.VerifyData(data, hash, sgin);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 最底层的RSACryptoServiceProvider对象
        /// </summary>
        public RSACryptoServiceProvider RSAObject { get; }

        /// <summary>
        /// 密钥位数
        /// </summary>
        public int KeySize => RSAObject.KeySize;

        /// <summary>
        /// 是否包含私钥
        /// </summary>
        public bool HasPrivate => !RSAObject.PublicOnly;

        /// <summary>
        /// 用指定密钥大小创建一个新的RSA，出错抛异常
        /// </summary>
        public RSA(int keySize)
        {
            RSAObject = new RSACryptoServiceProvider(keySize);
        }

        /// <summary>
        /// 通过公钥或私钥创建RSA，出错抛异常
        /// </summary>
        public RSA(string key)
        {
            if (!key.StartsWith("<"))
            {
                RSAObject = RsaPem.FromPEM(key).GetRSA();
            }
            else
            {
                RSAObject = new RSACryptoServiceProvider();
                RSAObject.FromXmlString(key);
            }
        }

        /// <summary>
        /// 通过一个pem对象创建RSA，pem为公钥或私钥，出错抛异常
        /// </summary>
        public RSA(RsaPem pem)
        {
            RSAObject = pem.GetRSA();
        }
    }
}
