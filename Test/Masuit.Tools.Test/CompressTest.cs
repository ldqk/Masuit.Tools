using Masuit.Tools.Files;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Xunit;

namespace Masuit.Tools.Test
{
    public class CompressTest
    {
        [Fact]
        public void Can_CompressFiles_ReturnStream()
        {
            var ms = SevenZipCompressor.ZipStream(new List<string>()
            {
                //AppContext.BaseDirectory,
                "http://ww3.sinaimg.cn/large/87c01ec7gy1fsq6rywto2j20je0d3td0.jpg",
                "http://192.168.16.150:3000string",
            });
            Assert.True(ms.Length > 0);
        }

        [Fact]
        public void Can_CompressToFile()
        {
            string zip = Path.Combine(Environment.GetEnvironmentVariable("temp"), Stopwatch.GetTimestamp() + ".rar");
            SevenZipCompressor.Zip(new List<string>()
            {
                AppContext.BaseDirectory,
                "http://ww3.sinaimg.cn/large/87c01ec7gy1fsq6rywto2j20je0d3td0.jpg",
                "http://192.168.16.150:3000string",
            }, zip);
            using (FileStream stream = File.OpenRead(zip))
            {
                Assert.True(stream.Length > 0);
            }
        }
    }
}
