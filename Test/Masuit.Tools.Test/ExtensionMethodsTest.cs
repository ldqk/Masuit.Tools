using Xunit;

namespace Masuit.Tools.Test
{
    public class ExtensionMethodsTest
    {
        [Fact]
        public void MatchUrl_True()
        {
            bool expect = "https://git.lug.us-tc.edu.cn/masuit/soft".MatchUrl();
            Assert.Equal(true, expect);
        }

        [Fact]
        public void MatchEmail()
        {
            var (expect, match) = "admin@sina.com.cn".MatchEmail();
            Assert.Equal(true, expect);
        }

        [Fact]
        public void MatchIdentifyCard_False()
        {
            bool expect = "513901199509120610".MatchIdentifyCard();
            Assert.Equal(false, expect);
        }

        [Theory]
        [InlineData("16666666666")]
        [InlineData("19999999999")]
        public void Can_MatchPhoneNumber_(string phone)
        {
            Xunit.Assert.True(phone.MatchPhoneNumber());
        }

        [Theory]
        [InlineData("166666666666")]
        [InlineData("199999999996")]
        public void CanNot_MatchPhoneNumber_(string phone)
        {
            Xunit.Assert.False(phone.MatchPhoneNumber());
        }

        [Theory]
        [InlineData("010-12345678")]
        [InlineData("0731-87654321")]
        [InlineData("0351-7654321")]
        [InlineData("01012345678")]
        [InlineData("073187654321")]
        [InlineData("03517654321")]
        public void Can_MatchLandline_(string phone)
        {
            Xunit.Assert.True(phone.MatchLandline());
        }
    }
}