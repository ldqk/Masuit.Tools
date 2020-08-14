using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xunit;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Masuit.Tools.Test
{
    [TestClass]
    public class ExtensionMethodsTest
    {
        [TestMethod]
        public void MatchUrl_True()
        {
            bool expect = "https://git.lug.us-tc.edu.cn/masuit/soft".MatchUrl();
            Assert.AreEqual(true, expect);
        }

        [TestMethod]
        public void MatchEmail()
        {
            var (expect, match) = "admin@sina.com.cn".MatchEmail();
            Assert.AreEqual(true, expect);
        }
        [TestMethod]
        public void MatchIdentifyCard_False()
        {
            bool expect = "513901199509120610".MatchIdentifyCard();
            Assert.AreEqual(false, expect);
        }

        [Theory]
        [InlineData("16666666666")]
        [InlineData("19999999999")]
        public void Can_MatchPhoneNumber_(string phone)
        {
            Xunit.Assert.True(phone.MatchPhoneNumber());
        }
    }
}