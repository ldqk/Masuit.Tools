using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Masuit.Tools.Security;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Security
{
    public class EncryptTests
    {
        private const string TestString = "Hello, World!";
        private const string DesKey = "12345678"; // 8位密钥
        private const string AesKey = "0123456789abcdef0123456789abcdef";
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

        [Fact]
        public void AesEncrypt_StringKey_ShouldEncryptAndDecrypt()
        {
            string original = "Hello, 世界!";

            string encrypted = original.AESEncrypt(AesKey);
            string decrypted = encrypted.AESDecrypt(AesKey);

            Assert.Equal(original, decrypted);
        }

        [Fact]
        public void AesEncrypt_ShouldUseRandomIvAndAuthenticateCiphertext()
        {
            string first = TestString.AESEncrypt(AesKey);
            string second = TestString.AESEncrypt(AesKey);
            byte[] tampered = Convert.FromBase64String(first);
            tampered[tampered.Length - 1] ^= 0x01;

            Assert.NotEqual(first, second);
            Assert.Throws<CryptographicException>(() => Convert.ToBase64String(tampered).AESDecrypt(AesKey));
        }

        [Fact]
        public void AesEncrypt_ByteKey_ShouldEncryptAndDecrypt()
        {
            string original = "Hello, 世界!";
            byte[] key = Encoding.UTF8.GetBytes(AesKey);

            string encrypted = original.AESEncrypt(key);
            string decrypted = encrypted.AESDecrypt(key);

            Assert.Equal(original, decrypted);
        }

        [Fact]
        public void AesEncrypt_AesOptions_ShouldUseAuthenticatedEnvelope()
        {
            using var options = Aes.Create();
            options.Key = Encoding.UTF8.GetBytes(AesKey);

            string encrypted = TestString.AESEncrypt(options);
            string decrypted = encrypted.AESDecrypt(options);

            Assert.Equal(TestString, decrypted);
        }

        [Fact]
        public void AesDecrypt_InvalidCiphertext_ShouldThrow()
        {
            Assert.Throws<FormatException>(() => "not base64".AESDecrypt(AesKey));
        }

        [Fact]
        public void AesGcmEncrypt_StringKey_ShouldEncryptAndDecrypt()
        {
            const string original = "Hello, AES-GCM 世界!";
            byte[] associatedData = "header"u8.ToArray();

            string encrypted = original.AesGcmEncrypt(AesKey, associatedData);
            string decrypted = encrypted.AesGcmDecrypt(AesKey, associatedData);

            Assert.NotEqual(original, encrypted);
            Assert.Equal(original, decrypted);
        }

        [Fact]
        public void AesGcmDecrypt_ModifiedCiphertext_ShouldThrow()
        {
            byte[] encrypted = Convert.FromBase64String("Hello, AES-GCM!".AesGcmEncrypt(AesKey));
            encrypted[12] ^= 0x01;

            Assert.ThrowsAny<CryptographicException>(() => Convert.ToBase64String(encrypted).AesGcmDecrypt(AesKey));
        }

        [Fact]
        public void AesGcmDecrypt_DifferentAssociatedData_ShouldThrow()
        {
            string ciphertext = "authenticated data".AesGcmEncrypt(AesKey, "header"u8.ToArray());

            Assert.ThrowsAny<CryptographicException>(() => ciphertext.AesGcmDecrypt(AesKey, "other header"u8.ToArray()));
        }

        [Fact]
        public void AesFileEncryption_ShouldRoundTrip()
        {
            string inputPath = Path.GetTempFileName();
            string encryptedPath = Path.GetTempFileName();
            string decryptedPath = Path.GetTempFileName();

            try
            {
                File.WriteAllText(inputPath, "AES file content");
                using (var input = File.OpenRead(inputPath))
                {
                    input.AESEncryptFile(encryptedPath, AesKey);
                }

                using (var input = File.OpenRead(encryptedPath))
                {
                    input.AESDecryptFile(decryptedPath, AesKey);
                }

                Assert.Equal("AES file content", File.ReadAllText(decryptedPath));
            }
            finally
            {
                File.Delete(inputPath);
                File.Delete(encryptedPath);
                File.Delete(decryptedPath);
            }
        }

        [Fact]
        public void DesFileEncryption_ShouldRoundTrip()
        {
            string inputPath = Path.GetTempFileName();
            string encryptedPath = Path.GetTempFileName();
            string decryptedPath = Path.GetTempFileName();

            try
            {
                File.WriteAllText(inputPath, "DES file content");
                using (var input = File.OpenRead(inputPath))
                {
                    input.DesEncrypt(encryptedPath, DesKeyBytes, DesIVBytes);
                }

                using (var input = File.OpenRead(encryptedPath))
                {
                    input.DesDecrypt(decryptedPath, DesKeyBytes, DesIVBytes);
                }

                Assert.Equal("DES file content", File.ReadAllText(decryptedPath));
            }
            finally
            {
                File.Delete(inputPath);
                File.Delete(encryptedPath);
                File.Delete(decryptedPath);
            }
        }

        [Theory]
        [InlineData(false, 0)]
        [InlineData(false, 100000)]
        [InlineData(true, 0)]
        [InlineData(true, 100000)]
        public void Rc2FileEncryption_ShouldRoundTrip(bool useCustomKey, int length)
        {
            string path = Path.GetTempFileName();
            string original = new string('中', length);
            byte[] key = Encoding.ASCII.GetBytes("0123456789abcdef");
            byte[] iv = Encoding.ASCII.GetBytes("12345678");

            try
            {
                if (useCustomKey)
                {
                    original.EncryptToFile(path, iv, key);
                    Assert.Equal(original, path.DecryptFromFile(iv, key));
                }
                else
                {
                    original.EncryptToFile(path);
                    Assert.Equal(original, path.DecryptFromFile());
                }
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Rc2FileDecryption_TruncatedCiphertext_ShouldThrow(bool useCustomKey)
        {
            string path = Path.GetTempFileName();
            byte[] key = Encoding.ASCII.GetBytes("0123456789abcdef");
            byte[] iv = Encoding.ASCII.GetBytes("12345678");

            try
            {
                if (useCustomKey)
                {
                    TestString.EncryptToFile(path, iv, key);
                }
                else
                {
                    TestString.EncryptToFile(path);
                }

                using (var file = new FileStream(path, FileMode.Open, FileAccess.Write))
                {
                    file.SetLength(file.Length - 1);
                }

                if (useCustomKey)
                {
                    Assert.ThrowsAny<CryptographicException>(() => path.DecryptFromFile(iv, key));
                }
                else
                {
                    Assert.ThrowsAny<CryptographicException>(() => path.DecryptFromFile());
                }
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Rc2IndexedFileDecryption_TruncatedFile_ShouldThrow(bool useCustomKey)
        {
            string path = Path.GetTempFileName();
            byte[] key = Encoding.ASCII.GetBytes("0123456789abcdef");
            byte[] iv = Encoding.ASCII.GetBytes("12345678");

            try
            {
                File.WriteAllBytes(path, new byte[5]);

                if (useCustomKey)
                {
                    Assert.Throws<EndOfStreamException>(() => path.DecryptFromFile(1, iv, key));
                }
                else
                {
                    Assert.Throws<EndOfStreamException>(() => path.DecryptFromFile(1));
                }
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void AesDecryptFile_TamperedCiphertext_ShouldNotCreateOutput()
        {
            string inputPath = Path.GetTempFileName();
            string encryptedPath = Path.GetTempFileName();
            string decryptedPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".txt");

            try
            {
                File.WriteAllText(inputPath, "Authenticated AES file content");
                using (var input = File.OpenRead(inputPath))
                {
                    input.AESEncryptFile(encryptedPath, AesKey);
                }

                using (var encrypted = new FileStream(encryptedPath, FileMode.Open, FileAccess.ReadWrite))
                {
                    encrypted.Position = encrypted.Length - 1;
                    int lastByte = encrypted.ReadByte();
                    encrypted.Position = encrypted.Length - 1;
                    encrypted.WriteByte((byte)(lastByte ^ 0x01));
                }

                using var tamperedInput = File.OpenRead(encryptedPath);
                Assert.Throws<CryptographicException>(() => tamperedInput.AESDecryptFile(decryptedPath, AesKey));
                Assert.False(File.Exists(decryptedPath));
            }
            finally
            {
                File.Delete(inputPath);
                File.Delete(encryptedPath);
                if (File.Exists(decryptedPath))
                {
                    File.Delete(decryptedPath);
                }
            }
        }
    }
}