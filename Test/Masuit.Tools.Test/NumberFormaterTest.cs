using Masuit.Tools.Strings;
using Xunit;

namespace Masuit.Tools.Test
{
    public class NumberFormaterTest
    {
        [Theory]
        [InlineData(2, 16, "10000")]
        [InlineData(8, 8, "10")]
        [InlineData(16, 16, "10")]
        [InlineData(36, 36, "10")]
        [InlineData(62, 62, "10")]
        public void Can_ConvertOct2AnySystem(byte bin, long input, string expectOutput)
        {
            var nf = new NumberFormater(bin);
            string output = nf.ToString(input);
            Assert.Equal(expectOutput, output);
        }

        [Theory]
        [InlineData(2, "10000", 16)]
        [InlineData(8, "10", 8)]
        [InlineData(16, "10", 16)]
        [InlineData(36, "10", 36)]
        [InlineData(62, "10", 62)]
        public void Can_ConvertAnySystem2Oct(byte bin, string input, long expected)
        {
            var nf = new NumberFormater(bin);
            string output = nf.ToString(expected);
            Assert.Equal(input, output);
        }

        [Fact]
        public void Can_ConvertAnyNum2Chinese()
        {
            // arrange
            double num = 1234567809.321;

            // act
            var chineseNumber = NumberFormater.ToChineseNumber(num);

            // assert
            Assert.Equal("十二亿三千四百五十六万七千八百零九点三二一", chineseNumber);
        }

        [Fact]
        public void Can_ConvertAnyNum2ChineseMoney()
        {
            // arrange
            double num = 123456789.321;

            // act
            var chineseNumber = NumberFormater.ToChineseMoney(num);

            // assert
            Assert.Equal("壹億贰仟叁佰肆拾伍萬陆仟柒佰捌拾玖元叁角贰分", chineseNumber);
        }
    }
}