using System.IO;
using System.Text;
using Masuit.Tools.Security;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Security
{
    public class EncryptTests
    {
        private const string TestString = "Hello, World!";
        private const string DesKey = "12345678"; // 8位密钥
        private static readonly byte[] DesKeyBytes = Encoding.ASCII.GetBytes(DesKey);
        private static readonly byte[] DesIVBytes = Encoding.ASCII.GetBytes(DesKey);

        [Fact]
        public void DesEncrypt_StringKey_ShouldEncryptAndDecrypt()
        {
            // Arrange
            string original = TestString;

            // Act
            string encrypted = original.DesEncrypt(DesKey);
            string decrypted = encrypted.DesDecrypt(DesKey);

            // Assert
            Assert.Equal(original, decrypted);
        }

        [Fact]
        public void DesEncrypt_ByteKey_ShouldEncryptAndDecrypt()
        {
            // Arrange
            string original = TestString;

            // Act
            string encrypted = original.DesEncrypt(DesKeyBytes, DesIVBytes);
            string decrypted = encrypted.DesDecrypt(DesKeyBytes, DesIVBytes);

            // Assert
            Assert.Equal(original, decrypted);
        }
    }
}