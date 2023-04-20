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
        public string PublicKey { get; protected internal set; }

        /// <summary>
        /// 私钥
        /// </summary>
        public string PrivateKey { get; protected internal set; }

        public void Deconstruct(out string publicKey, out string privateKey)
        {
            publicKey = PublicKey;
            privateKey = PrivateKey;
        }
    }
}
