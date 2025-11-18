using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Masuit.Tools.Files;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Files;

public class FileExtTests
{
    public string GetTestFile(string n)
    {
        string testFilePath = Path.Combine(Path.GetTempPath(), $"testfile{n}.txt");
        if (File.Exists(testFilePath))
        {
            File.Delete(testFilePath);
        }

        File.WriteAllText(testFilePath, "This is a test file.");
        return testFilePath;
    }

    [Fact]
    public void CopyToFile_ShouldCopyFile()
    {
        var _testFilePath = GetTestFile("1");
        string _copyFilePath = Path.Combine(Path.GetTempPath(), "copyfile.txt");
        using var fs = new FileStream(_testFilePath, FileMode.Open, FileAccess.Read);
        fs.CopyToFile(_copyFilePath);
        Assert.True(File.Exists(_copyFilePath));
        Assert.Equal(File.ReadAllText(_testFilePath), File.ReadAllText(_copyFilePath));
    }

    [Fact]
    public void SaveFile_ShouldSaveFile()
    {
        string _copyFilePath = Path.Combine(Path.GetTempPath(), "copyfile3.txt");
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes("This is a test file."));
        ms.SaveFile(_copyFilePath);
        Assert.True(File.Exists(_copyFilePath));
        Assert.Equal("This is a test file.", File.ReadAllText(_copyFilePath));
    }

    [Fact]
    public async Task SaveFileAsync_ShouldSaveFile()
    {
        string _copyFilePath = Path.Combine(Path.GetTempPath(), "copyfile4.txt");
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes("This is a test file."));
        await ms.SaveFileAsync(_copyFilePath);
        Assert.True(File.Exists(_copyFilePath));
        Assert.Equal("This is a test file.", File.ReadAllText(_copyFilePath));
    }

    [Fact]
    public void GetFileMD5_ShouldReturnMD5Hash()
    {
        var _testFilePath = GetTestFile("3");
        using var fs = new FileStream(_testFilePath, FileMode.Open, FileAccess.Read);
        var md5 = fs.GetFileMD5();
        using var md5Crypto = MD5.Create();
        var expectedMd5 = BitConverter.ToString(md5Crypto.ComputeHash(Encoding.UTF8.GetBytes("This is a test file."))).Replace("-", "").ToLower();
        Assert.Equal(expectedMd5, md5);
    }

    [Fact]
    public void GetFileSha1_ShouldReturnSha1Hash()
    {
        var _testFilePath = GetTestFile("4");
        using var fs = new FileStream(_testFilePath, FileMode.Open, FileAccess.Read);
        var sha1 = fs.GetFileSha1();
        using var sha1Crypto = SHA1.Create();
        var expectedSha1 = BitConverter.ToString(sha1Crypto.ComputeHash(Encoding.UTF8.GetBytes("This is a test file."))).Replace("-", "").ToLower();
        Assert.Equal(expectedSha1, sha1);
    }

    [Fact]
    public void GetFileSha256_ShouldReturnSha256Hash()
    {
        var _testFilePath = GetTestFile("5");
        using var fs = new FileStream(_testFilePath, FileMode.Open, FileAccess.Read);
        var sha256 = fs.GetFileSha256();
        using var sha256Crypto = SHA256.Create();
        var expectedSha256 = BitConverter.ToString(sha256Crypto.ComputeHash(Encoding.UTF8.GetBytes("This is a test file."))).Replace("-", "").ToLower();
        Assert.Equal(expectedSha256, sha256);
    }

    [Fact]
    public void GetFileSha512_ShouldReturnSha512Hash()
    {
        var _testFilePath = GetTestFile("6");
        using var fs = new FileStream(_testFilePath, FileMode.Open, FileAccess.Read);
        var sha512 = fs.GetFileSha512();
        using var sha512Crypto = SHA512.Create();
        var expectedSha512 = BitConverter.ToString(sha512Crypto.ComputeHash(Encoding.UTF8.GetBytes("This is a test file."))).Replace("-", "").ToLower();
        Assert.Equal(expectedSha512, sha512);
    }
}