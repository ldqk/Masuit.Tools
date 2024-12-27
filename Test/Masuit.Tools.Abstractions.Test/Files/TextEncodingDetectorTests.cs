using System;
using System.IO;
using System.Text;
using Masuit.Tools.Files;
using Masuit.Tools.Systems;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Files;

public class TextEncodingDetectorTests
{
    [Theory]
    [InlineData("Hello, World!", "ASCII")]
    [InlineData("Hello, World!", "UTF-8")]
    [InlineData("Hello, World!", "Unicode")]
    [InlineData("Hello, World!", "UTF-32")]
    public void GetEncoding_ShouldDetectEncodingFromFile(string content, string encodingName)
    {
        string _testFilePath = Path.Combine(Path.GetTempPath(), "testfile1.txt");
        var encoding = Encoding.GetEncoding(encodingName);
        File.WriteAllText(_testFilePath, content, encoding);
        var detectedEncoding = TextEncodingDetector.GetEncoding(_testFilePath);
        Assert.Equal(encoding, detectedEncoding);
    }

    [Theory]
    [InlineData("Hello, World!", "ASCII")]
    [InlineData("Hello, World!", "UTF-8")]
    [InlineData("Hello, World!", "Unicode")]
    [InlineData("Hello, World!", "UTF-32")]
    public void GetEncoding_ShouldDetectEncodingFromFileInfo(string content, string encodingName)
    {
        string _testFilePath = Path.Combine(Path.GetTempPath(), "testfile2.txt");
        var encoding = Encoding.GetEncoding(encodingName);
        File.WriteAllText(_testFilePath, content, encoding);
        var fileInfo = new FileInfo(_testFilePath);
        var detectedEncoding = fileInfo.GetEncoding();
        Assert.Equal(encoding, detectedEncoding);
    }

    [Theory]
    [InlineData("Hello, World!", "ASCII")]
    [InlineData("Hello, World!", "UTF-8")]
    [InlineData("Hello, World!", "Unicode")]
    [InlineData("Hello, World!", "UTF-32")]
    public void GetEncoding_ShouldDetectEncodingFromStream(string content, string encodingName)
    {
        string _testFilePath = Path.Combine(Path.GetTempPath(), "testfile3.txt");
        var encoding = Encoding.GetEncoding(encodingName);
        File.WriteAllText(_testFilePath, content, encoding);
        using var stream = new FileStream(_testFilePath, FileMode.Open, FileAccess.Read);
        var detectedEncoding = stream.GetEncoding();
        Assert.Equal(encoding, detectedEncoding);
    }

    [Theory]
    [InlineData(new byte[] { 255, 254, 0, 0 }, "UTF-32")]
    [InlineData(new byte[] { 0, 0, 254, 255 }, "utf-32BE")]
    [InlineData(new byte[] { 239, 187, 191 }, "UTF-8")]
    [InlineData(new byte[] { 72, 101, 108, 108 }, "ASCII")]
    public void GetEncoding_ShouldDetectEncodingFromBytes(byte[] bytes, string expectedEncodingName)
    {
        var expectedEncoding = Encoding.GetEncoding(expectedEncodingName);
        var detectedEncoding = new PooledMemoryStream(bytes).GetEncoding();
        Assert.Equal(expectedEncoding, detectedEncoding);
    }
}