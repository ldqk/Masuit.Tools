using Masuit.Tools.Strings;
using Xunit;

namespace Masuit.Tools.UnitTest
{
    public class NumberFormaterTest
    {
        [Theory]
        [InlineData(2, 16, "10000")]
        [InlineData(8, 8, "10")]
        [InlineData(16, 16, "10")]
        [InlineData(36, 36, "10")]
        [InlineData(62, 62, "10")]
        public void Can_ConvertOct2AnySystem(int bin, long input, string expectOutput)
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
        public void Can_ConvertAnySystem2Oct(int bin, string input, long expected)
        {
            var nf = new NumberFormater(bin);
            string output = nf.ToString(expected);
            Assert.Equal(input, output);
        }
    }
}