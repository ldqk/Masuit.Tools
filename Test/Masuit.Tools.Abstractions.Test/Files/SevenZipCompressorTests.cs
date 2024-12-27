using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Masuit.Tools.Files;
using Masuit.Tools.Systems;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Files;

public class SevenZipCompressorTests
{
    private readonly SevenZipCompressor _compressor;
    private readonly string _testDir;
    private readonly string _testFile1;
    private readonly string _testFile2;
    private readonly string _zipFile;

    public SevenZipCompressorTests()
    {
        _compressor = new SevenZipCompressor(new HttpClient());
        _testDir = Path.Combine(Path.GetTempPath(), "TestDir");
        _testFile1 = Path.Combine(_testDir, "TestFile1.txt");
        _testFile2 = Path.Combine(_testDir, "TestFile2.txt");
        _zipFile = Path.Combine(Path.GetTempPath(), "TestArchive.zip");

        if (Directory.Exists(_testDir))
        {
            Directory.Delete(_testDir, true);
        }

        Directory.CreateDirectory(_testDir);
        File.WriteAllText(_testFile1, "This is a test file 1.");
        File.WriteAllText(_testFile2, "This is a test file 2.");
    }

    [Fact]
    public void ZipStream_ShouldCreateZipStreamFromFiles()
    {
        var files = new List<string> { _testFile1, _testFile2 };
        using var zipStream = _compressor.ZipStream(files);
        Assert.NotNull(zipStream);
        Assert.True(zipStream.Length > 0);
    }

    [Fact]
    public void ZipStream_ShouldCreateZipStreamFromDirectory()
    {
        using var zipStream = _compressor.ZipStream(_testDir);
        Assert.NotNull(zipStream);
        Assert.True(zipStream.Length > 0);
    }

    [Fact]
    public void ZipStream_ShouldCreateZipStreamFromStreams()
    {
        var streams = new DisposableDictionary<string, Stream>
        {
            { "TestFile1.txt", new MemoryStream(System.Text.Encoding.UTF8.GetBytes("This is a test file 1.")) },
            { "TestFile2.txt", new MemoryStream(System.Text.Encoding.UTF8.GetBytes("This is a test file 2.")) }
        };
        using var zipStream = _compressor.ZipStream(streams);
        Assert.NotNull(zipStream);
        Assert.True(zipStream.Length > 0);
    }

    [Fact]
    public void Zip_ShouldCreateZipFileFromFiles()
    {
        var files = new List<string> { _testFile1, _testFile2 };
        _compressor.Zip(files, _zipFile);
        Assert.True(File.Exists(_zipFile));
        Assert.True(new FileInfo(_zipFile).Length > 0);
    }

    [Fact]
    public void Zip_ShouldCreateZipFileFromDirectory()
    {
        _compressor.Zip(_testDir, _zipFile);
        Assert.True(File.Exists(_zipFile));
        Assert.True(new FileInfo(_zipFile).Length > 0);
    }

    [Fact]
    public void Zip_ShouldCreateZipFileFromStreams()
    {
        var streams = new DisposableDictionary<string, Stream>
        {
            { "TestFile1.txt", new MemoryStream(System.Text.Encoding.UTF8.GetBytes("This is a test file 1.")) },
            { "TestFile2.txt", new MemoryStream(System.Text.Encoding.UTF8.GetBytes("This is a test file 2.")) }
        };
        _compressor.Zip(streams, _zipFile);
        Assert.True(File.Exists(_zipFile));
        Assert.True(new FileInfo(_zipFile).Length > 0);
    }

    [Fact]
    public void Decompress_ShouldExtractFiles()
    {
        _compressor.Zip(_testDir, _zipFile);
        var extractDir = Path.Combine(Path.GetTempPath(), "ExtractDir");
        if (Directory.Exists(extractDir))
        {
            Directory.Delete(extractDir, true);
        }

        _compressor.Decompress(_zipFile, extractDir);
        Assert.True(Directory.Exists(extractDir));
        Assert.True(File.Exists(Path.Combine(extractDir, "TestFile1.txt")));
        Assert.True(File.Exists(Path.Combine(extractDir, "TestFile2.txt")));
    }
}