using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Extensions.BaseType
{
    public class StringExtensionsTest
    {
        [Theory, InlineData("123.234.145.156"), InlineData("223.234.145.156"), InlineData("2001:0db8:85a3::1319:8a2e:0370:7344"), InlineData("::1")]
        public void MatchIPAddress_Valid_ReturnTrue(string s)
        {
            var isip = s.MatchInetAddress();
            Assert.True(isip);
        }

        [Theory, InlineData("123.234.345.456"), InlineData("masuit.com"), InlineData("2001:0db8:85a3::1319:8a2e:0370:7344:7344")]
        public void MatchIPAddress_Invalid_ReturnFalse(string s)
        {
            var isip = s.MatchInetAddress();
            Assert.False(isip);
        }

        [Theory]
        [InlineData("//www.baidu.com")]
        [InlineData("http://www.baidu.com")]
        [InlineData("https://www.baidu.com")]
        [InlineData("ftp://admin:123456@baidu.com/abc/def")]
        [InlineData("https://baidu.com:8080")]
        [InlineData("https://baidu.com:8080/abc/def/hhh.html?s=www")]
        [InlineData("https://baidu.com:8080/abc/def/hi_jk-mn%ADF%AA/hhh.html?s=www&x=yyy#top")]
        [InlineData("https://baidu.com:8080/abc/def/hi_jk-mn%ADF%AA?s=www&x=yyy#top/aaa/bbb/ccc")]
        [InlineData("http://music.163.com/def/hhh.html?s=www&x=yyy#/my/m/music/empty")]
        [InlineData("http://music.163.com/#/search/m/?%23%2Fmy%2Fm%2Fmusic%2Fempty=&s=fade&type=1!k")]
        public void MatchUrl_Valid_ReturnTrue(string s)
        {
            var isip = s.MatchUrl();
            Assert.True(isip);
        }

        [Theory]
        [InlineData("127.0.0.1", true)]
        [InlineData("10.23.254.223", true)]
        [InlineData("172.16.23.22", true)]
        [InlineData("172.17.23.22", false)]
        [InlineData("169.254.23.21", true)]
        [InlineData("192.168.1.1", true)]
        public void IsPrivateIP_ReturnTrue(string ip, bool actual)
        {
            bool expected = ip.IsPrivateIP();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("https://baidu.com", true)]
        [InlineData("http://localhost", false)]
        public void IsExtenalAddress_RetuenTrue(string url, bool actual)
        {
            bool expected = url.IsExternalAddress();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Join_ShouldJoinStrings()
        {
            var strings = new List<string> { "a", "b", "c" };
            var result = strings.Join(", ");
            Assert.Equal("a, b, c", result);
        }

        [Fact]
        public void Join_ShouldJoinStringsAndRemoveEmptyEntries()
        {
            var strings = new List<string> { "a", "", "c" };
            var result = strings.Join(", ", true);
            Assert.Equal("a, c", result);
        }

        [Fact]
        public void ToDateTime_ShouldConvertStringToDateTime()
        {
            var dateString = "2023-10-10";
            var result = dateString.ToDateTime();
            Assert.Equal(new DateTime(2023, 10, 10), result);
        }

        [Fact]
        public void ToGuid_ShouldConvertStringToGuid()
        {
            var guidString = "d2719b1e-1d2b-4b5a-9b1e-1d2b4b5a9b1e";
            var result = guidString.ToGuid();
            Assert.Equal(Guid.Parse(guidString), result);
        }

        [Fact]
        public void Replace_ShouldReplaceUsingRegex()
        {
            var input = "Hello World";
            var regex = new Regex("World");
            var result = input.Replace(regex, "Universe");
            Assert.Equal("Hello Universe", result);
        }

        [Fact]
        public void CreateShortToken_ShouldGenerateToken()
        {
            var result = "".CreateShortToken();
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void FromBase_ShouldConvertFromBase()
        {
            var base36String = "1l";
            var result = base36String.FromBase(36);
            Assert.Equal(57, result);
        }

        [Fact]
        public void FromBaseBig_ShouldConvertFromBaseBig()
        {
            var base36String = "1l";
            var result = base36String.FromBaseBig(36);
            Assert.Equal(new BigInteger(57), result);
        }

        [Fact]
        public void Contains_ShouldCheckIfStringContainsAnyKeywords()
        {
            var input = "Hello World";
            var keywords = new List<string> { "world", "universe" };
            var result = input.Contains(keywords);
            Assert.True(result);
        }

        [Fact]
        public void ContainsSafety_ShouldCheckIfStringContainsAnyKeywordsSafely()
        {
            var input = "Hello World";
            var keywords = new List<string> { "world", "universe" };
            var result = input.ContainsSafety(keywords);
            Assert.True(result);
        }

        [Fact]
        public void EndsWith_ShouldCheckIfStringEndsWithAnyKeywords()
        {
            var input = "Hello World";
            var keywords = new List<string> { "world", "universe" };
            var result = input.EndsWith(keywords);
            Assert.True(result);
        }

        [Fact]
        public void StartsWith_ShouldCheckIfStringStartsWithAnyKeywords()
        {
            var input = "Hello World";
            var keywords = new List<string> { "hello", "hi" };
            var result = input.StartsWith(keywords);
            Assert.True(result);
        }

        [Fact]
        public void RegexMatch_ShouldCheckIfStringMatchesRegex()
        {
            var input = "Hello World";
            var regex = "World";
            var result = input.RegexMatch(regex);
            Assert.True(result);
        }

        [Fact]
        public void RegexMatch_ShouldCheckIfStringMatchesRegexObject()
        {
            var input = "Hello World";
            var regex = new Regex("World");
            var result = input.RegexMatch(regex);
            Assert.True(result);
        }

        [Fact]
        public void IsNullOrEmpty_ShouldCheckIfStringIsNullOrEmpty()
        {
            var input = "";
            var result = input.IsNullOrEmpty();
            Assert.True(result);
        }

        [Fact]
        public void NotNullOrEmpty_ShouldCheckIfStringIsNotNullOrEmpty()
        {
            var input = "Hello";
            var result = input.NotNullOrEmpty();
            Assert.True(result);
        }

        [Fact]
        public void AsNotNull_ShouldReturnNonNullString()
        {
            string input = null;
            var result = input.AsNotNull();
            Assert.Equal("", result);
        }

        [Fact]
        public void IfNullOrEmpty_ShouldReturnReplacementIfNullOrEmpty()
        {
            string input = null;
            var result = input.IfNullOrEmpty("replacement");
            Assert.Equal("replacement", result);
        }

        [Fact]
        public void IfNullOrEmpty_ShouldReturnReplacementFromFactoryIfNullOrEmpty()
        {
            string input = null;
            var result = input.IfNullOrEmpty(() => "replacement");
            Assert.Equal("replacement", result);
        }

        [Fact]
        public void Mask_ShouldMaskString()
        {
            var input = "1234567890";
            var result = input.Mask();
            Assert.Equal("123****890", result);
        }

        [Fact]
        public async Task MatchEmailAsync_ShouldMatchEmail()
        {
            var input = "test@example.com";
            var (isMatch, match) = await input.MatchEmailAsync();
            Assert.True(isMatch);
            Assert.NotNull(match);
        }

        [Fact]
        public void MaskEmail_ShouldMaskEmail()
        {
            var input = "test@example.com";
            var result = input.MaskEmail();
            Assert.Equal("t****@example.com", result);
        }

        [Fact]
        public void MatchUrl_ShouldMatchUrl()
        {
            var input = "http://example.com";
            var result = input.MatchUrl();
            Assert.True(result);
        }

        [Fact]
        public void MatchIdentifyCard_ShouldMatchIdentifyCard()
        {
            var input = "11010519491231002X";
            var result = input.MatchIdentifyCard();
            Assert.True(result);
        }

        [Fact]
        public void MatchInetAddress_ShouldMatchInetAddress()
        {
            var input = "192.168.1.1";
            var result = input.MatchInetAddress();
            Assert.True(result);
        }

        [Fact]
        public void IPToID_ShouldConvertIPToID()
        {
            var input = "192.168.1.1";
            var result = input.IPToID();
            Assert.Equal(3232235777, result);
        }

        [Fact]
        public void IsPrivateIP_ShouldCheckIfIPIsPrivate()
        {
            var input = "192.168.1.1";
            var result = input.IsPrivateIP();
            Assert.True(result);
        }

        [Fact]
        public void IpAddressInRange_ShouldCheckIfIPIsInRange()
        {
            var input = "192.168.1.1";
            var result = input.IpAddressInRange("192.168.1.0", "192.168.1.255");
            Assert.True(result);
        }

        [Fact]
        public void MatchPhoneNumber_ShouldMatchPhoneNumber()
        {
            var input = "13800138000";
            var result = input.MatchPhoneNumber();
            Assert.True(result);
        }

        [Fact]
        public void MatchLandline_ShouldMatchLandline()
        {
            var input = "010-12345678";
            var result = input.MatchLandline();
            Assert.True(result);
        }

        [Fact]
        public void MatchUSCC_ShouldMatchUSCC()
        {
            var input = "91350211347112345X";
            var result = input.MatchUSCC();
            Assert.True(result);
        }

        [Fact]
        public void IsExternalAddress_ShouldCheckIfUrlIsExternal()
        {
            var input = "http://example.com";
            var result = input.IsExternalAddress();
            Assert.True(result);
        }

        [Fact]
        public void ToByteArray_ShouldConvertStringToByteArray()
        {
            var input = "Hello";
            var result = input.ToByteArray();
            Assert.Equal(Encoding.UTF8.GetBytes("Hello"), result);
        }

        [Fact]
        public void Crc32_ShouldComputeCrc32()
        {
            var input = "Hello";
            var result = input.Crc32();
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void Crc64_ShouldComputeCrc64()
        {
            var input = "Hello";
            var result = input.Crc64();
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void MatchCNPatentNumber_ShouldMatchCNPatentNumber()
        {
            var input = "200410018477.9";
            var result = input.MatchCNPatentNumber();
            Assert.True(result);
        }

        [Fact]
        public void Take_ShouldTakeFirstNCharacters()
        {
            var input = "Hello World";
            var result = input.Take(5);
            Assert.Equal("Hello", result);
        }

        [Fact]
        public void HammingDistance_ShouldComputeHammingDistance()
        {
            var input1 = "Hello";
            var input2 = "Helli";
            var result = input1.HammingDistance(input2);
            Assert.Equal(1, result);
        }

        [Fact]
        public void MatchEmoji_ShouldMatchEmoji()
        {
            var input = "Hello 😊";
            var result = input.MatchEmoji();
            Assert.True(result);
        }

        [Fact]
        public void CharacterCount_ShouldCountCharacters()
        {
            var input = "Hello 😊";
            var result = input.CharacterCount();
            Assert.Equal(7, result);
        }

        [Fact]
        public void BytesCount_ShouldCountBytes()
        {
            var input = "Hello";
            var result = input.BytesCount();
            Assert.Equal(5, result);
        }

        [Fact]
        public void ToDBC_ShouldConvertToDBC()
        {
            var input = "ｈｅｌｌｏ";
            var result = input.ToDBC();
            Assert.Equal("hello", result);
        }

        [Fact]
        public void ToSBC_ShouldConvertToSBC()
        {
            var input = "hello";
            var result = input.ToSBC();
            Assert.Equal("ｈｅｌｌｏ", result);
        }
    }
}