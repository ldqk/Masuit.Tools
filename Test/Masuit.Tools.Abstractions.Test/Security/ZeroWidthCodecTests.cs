using Masuit.Tools.Security;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Security
{
    public class ZeroWidthCodecTests
    {
        [Fact]
        public void EncodeToZeroWidthText_ShouldEncodeString()
        {
            string input = "Test";
            string encoded = input.EncodeToZeroWidthText();

            Assert.False(string.IsNullOrEmpty(encoded));
            Assert.NotEqual(input, encoded);
        }

        [Fact]
        public void DecodeZeroWidthString_ShouldDecodeString()
        {
            string hiddenString = "HiddenMessage";
            string publicString = "HelloWorld".InjectZeroWidthString(hiddenString);
            string decoded = publicString.DecodeZeroWidthString();

            Assert.Equal(hiddenString, decoded);
        }

        [Fact]
        public void Encode_ShouldEncodeString()
        {
            string input = "Test";
            string encoded = ZeroWidthCodec.Encode(input);

            Assert.False(string.IsNullOrEmpty(encoded));
            Assert.NotEqual(input, encoded);
        }

        [Fact]
        public void Decrypt_ShouldDecodeString()
        {
            string hiddenString = "HiddenMessage";
            string publicString = ZeroWidthCodec.Encrypt("HelloWorld", hiddenString);
            string decoded = ZeroWidthCodec.Decrypt(publicString);

            Assert.Equal(hiddenString, decoded);
        }
    }
}