using System;
using System.Collections.Generic;
using System.Text;
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
    }
}