using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using Masuit.Tools.Files;
using Masuit.Tools.Systems;
using Xunit;
using ZipArchive = System.IO.Compression.ZipArchive;
using ZipArchiveMode = System.IO.Compression.ZipArchiveMode;

namespace Masuit.Tools.Abstractions.Test.Files;

public class SevenZipCompressorTests : System.IDisposable
{
    private readonly SevenZipCompressor _compressor;
    private readonly HttpClient _client;
    private readonly string _testDir;
    private readonly string _testFile1;
    private readonly string _testFile2;
    private readonly string _zipFile;

    public SevenZipCompressorTests()
    {
        _client = new HttpClient();
        _compressor = new SevenZipCompressor(_client);
        _testDir = Path.Combine(Path.GetTempPath(), "SevenZipTests-" + System.Guid.NewGuid());
        _testFile1 = Path.Combine(_testDir, "TestFile1.txt");
        _testFile2 = Path.Combine(_testDir, "TestFile2.txt");
        _zipFile = Path.Combine(_testDir, "TestArchive.zip");

        Directory.CreateDirectory(_testDir);
        File.WriteAllText(_testFile1, "This is a test file 1.");
        File.WriteAllText(_testFile2, "This is a test file 2.");
    }

    public void Dispose()
    {
        _client.Dispose();
        if (Directory.Exists(_testDir)) Directory.Delete(_testDir, true);
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
        var extractDir = Path.Combine(_testDir, "extracted");

        _compressor.Decompress(_zipFile, extractDir);
        Assert.True(Directory.Exists(extractDir));
        Assert.True(File.Exists(Path.Combine(extractDir, "TestFile1.txt")));
        Assert.True(File.Exists(Path.Combine(extractDir, "TestFile2.txt")));
    }

    [Fact]
    public void Zip_ProducesStandardDeflateWithRelativeUnicodePaths()
    {
        var nested = Path.Combine(_testDir, "nested");
        Directory.CreateDirectory(nested);
        File.WriteAllText(Path.Combine(nested, "中文.txt"), "hello");
        _compressor.Zip(new[] { _testFile1, Path.Combine(nested, "中文.txt") }, _zipFile, "root");

        using var archive = System.IO.Compression.ZipFile.OpenRead(_zipFile);
        var entry = archive.GetEntry("root/nested/中文.txt");
        Assert.NotNull(entry);
        using var reader = new StreamReader(entry.Open());
        Assert.Equal("hello", reader.ReadToEnd());
        Assert.NotNull(archive.GetEntry("root/TestFile1.txt"));
    }

    [Fact]
    public void ZipStream_FromStreams_IsReadableAndKeepsInputOpen()
    {
        var input = new MemoryStream(Encoding.UTF8.GetBytes("content"));
        var streams = new DisposableDictionary<string, Stream> { { "nested\\file.txt", input } };
        using var result = _compressor.ZipStream(streams);
        result.Position = 0;
        using var archive = new ZipArchive(result, ZipArchiveMode.Read, true);
        using var reader = new StreamReader(archive.GetEntry("nested/file.txt").Open());
        Assert.Equal("content", reader.ReadToEnd());
        Assert.True(input.CanRead);
        streams.Dispose();
    }

    [Fact]
    public void Zip_FromStreams_IsReadableByDotNet()
    {
        var streams = new DisposableDictionary<string, Stream>
        {
            { "file.txt", new MemoryStream(Encoding.UTF8.GetBytes("content")) }
        };
        _compressor.Zip(streams, _zipFile, true);
        using var archive = System.IO.Compression.ZipFile.OpenRead(_zipFile);
        using var reader = new StreamReader(archive.GetEntry("file.txt").Open());
        Assert.Equal("content", reader.ReadToEnd());
    }

    [Fact]
    public void DirectoryArchives_PreserveNestedPathsWhenNoFilesAreAtRoot()
    {
        var source = Path.Combine(_testDir, "source");
        Directory.CreateDirectory(Path.Combine(source, "nested"));
        File.WriteAllText(Path.Combine(source, "nested", "file.txt"), "nested");
        using var zip = _compressor.ZipStream(source);
        zip.Position = 0;
        using var archive = new ZipArchive(zip, ZipArchiveMode.Read, true);
        Assert.NotNull(archive.GetEntry("nested/file.txt"));

        var path = Path.Combine(_testDir, "nested.7z");
        _compressor.SevenZip(source, path);
        var extracted = Path.Combine(_testDir, "extracted");
        _compressor.Decompress(path, extracted);
        Assert.Equal("nested", File.ReadAllText(Path.Combine(extracted, "nested", "file.txt")));
    }

    [Fact]
    public void SevenZip_FileAndStreamOverloads_RoundTrip()
    {
        ISevenZipCompressor compressor = _compressor;
        var nested = Path.Combine(_testDir, "nested");
        Directory.CreateDirectory(nested);
        File.WriteAllText(Path.Combine(nested, "中文.txt"), "seven zip");
        var sevenZipFile = Path.Combine(_testDir, "output.7z");
        var extracted = Path.Combine(Path.GetTempPath(), "SevenZipExtract-" + System.Guid.NewGuid());
        try
        {
            compressor.SevenZip(new[] { _testFile1, Path.Combine(nested, "中文.txt") }, sevenZipFile, "root");
            Assert.Equal(new byte[] { 0x37, 0x7a, 0xbc, 0xaf, 0x27, 0x1c }, File.ReadAllBytes(sevenZipFile)[..6]);
            compressor.Decompress(sevenZipFile, extracted);
            Assert.Equal("seven zip", File.ReadAllText(Path.Combine(extracted, "root", "nested", "中文.txt")));

            using var result = compressor.SevenZipStream(new[] { _testFile1 }, "root");
            File.WriteAllBytes(sevenZipFile, result.ToArray());
            compressor.Decompress(sevenZipFile, extracted);
            Assert.Equal("This is a test file 1.", File.ReadAllText(Path.Combine(extracted, "root", "TestFile1.txt")));
        }
        finally
        {
            if (Directory.Exists(extracted)) Directory.Delete(extracted, true);
        }
    }

    [Fact]
    public void SevenZip_DirectoryAndInputStreams_RoundTrip()
    {
        var sevenZipFile = Path.Combine(Path.GetTempPath(), "SevenZip-" + System.Guid.NewGuid() + ".7z");
        var extracted = Path.Combine(Path.GetTempPath(), "SevenZipExtract-" + System.Guid.NewGuid());
        var input = new MemoryStream(Encoding.UTF8.GetBytes("stream data"));
        var streams = new DisposableDictionary<string, Stream> { { "dir\\stream.txt", input } };
        try
        {
            _compressor.SevenZip(_testDir, sevenZipFile);
            _compressor.Decompress(sevenZipFile, extracted);
            Assert.Equal("This is a test file 1.", File.ReadAllText(Path.Combine(extracted, "TestFile1.txt")));

            using (var result = _compressor.SevenZipStream(_testDir))
            {
                Assert.True(result.Length > 6);
            }

            _compressor.SevenZip(streams, sevenZipFile);
            Assert.True(input.CanRead);
            _compressor.Decompress(sevenZipFile, extracted);
            Assert.Equal("stream data", File.ReadAllText(Path.Combine(extracted, "dir", "stream.txt")));

            using var output = _compressor.SevenZipStream(streams, true);
            File.WriteAllBytes(sevenZipFile, output.ToArray());
            _compressor.Decompress(sevenZipFile, extracted);
            Assert.Equal("stream data", File.ReadAllText(Path.Combine(extracted, "dir", "stream.txt")));
            Assert.False(input.CanRead);
        }
        finally
        {
            streams.Dispose();
            if (File.Exists(sevenZipFile)) File.Delete(sevenZipFile);
            if (Directory.Exists(extracted)) Directory.Delete(extracted, true);
        }
    }

    [Fact]
    public void Archives_IncludeRemoteFilesAndReportHttpErrors()
    {
        using var client = new HttpClient(new TestHttpHandler());
        var compressor = new SevenZipCompressor(client);
        using var zip = compressor.ZipStream(new[] { "https://example.test/remote.txt" });
        zip.Position = 0;
        using var archive = new ZipArchive(zip, ZipArchiveMode.Read, true);
        using var reader = new StreamReader(archive.GetEntry("remote.txt").Open());
        Assert.Equal("remote data", reader.ReadToEnd());
        using var sevenZip = compressor.SevenZipStream(new[] { "https://example.test/remote.txt" });
        var path = Path.Combine(_testDir, "remote.7z");
        File.WriteAllBytes(path, sevenZip.ToArray());
        var extracted = Path.Combine(_testDir, "remote");
        compressor.Decompress(path, extracted);
        Assert.Equal("remote data", File.ReadAllText(Path.Combine(extracted, "remote.txt")));
        Assert.Throws<HttpRequestException>(() => compressor.SevenZipStream(new[] { "https://example.test/missing" }));
    }

    [Fact]
    public void Decompress_ExtractsArchiveCreatedBySevenZip()
    {
        const string archiveData = "N3q8ryccAATq4xCLGgAAAAAAAABiAAAAAAAAAPVNxdgBABVUaGlzIGlzIGEgdGVzdCBmaWxlIDEuAAEEBgABCRoABwsBAAEhIQEADBYACAoBO9/xtAAABQEZDAAAAAAAAAAAAAAAABEdAFQAZQBzAHQARgBpAGwAZQAxAC4AdAB4AHQAAAAUCgEAPocxCNx73QEVBgEAIAAAAAAA";
        var archivePath = Path.Combine(_testDir, "external.7z");
        var extracted = Path.Combine(_testDir, "extracted");
        File.WriteAllBytes(archivePath, System.Convert.FromBase64String(archiveData));
        _compressor.Decompress(archivePath, extracted);
        Assert.Equal("This is a test file 1.", File.ReadAllText(Path.Combine(extracted, "TestFile1.txt")));
    }

    private sealed class TestHttpHandler : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await Task.Delay(10, cancellationToken);
            return request.RequestUri.AbsolutePath == "/missing"
                ? new HttpResponseMessage(HttpStatusCode.NotFound)
                : new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("remote data") };
        }
    }
}