using System.Collections.Generic;
using System.IO;
using Masuit.Tools.Files.FileDetector;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Files;

public class FileSignatureDetectorTests
{
    [Fact]
    public void AddDetector_ShouldAddDetector()
    {
        var detector = new TestDetector();
        FileSignatureDetector.AddDetector(detector);
        Assert.Contains(detector, FileSignatureDetector.Registered);
    }

    [Fact]
    public void AddDetector_Generic_ShouldAddDetector()
    {
        FileSignatureDetector.AddDetector<TestDetector>();
        Assert.Contains(FileSignatureDetector.Registered, d => d is TestDetector);
    }

    [Fact]
    public void DetectFiletype_ShouldDetectFiletypeFromFilepath()
    {
        var detector = new TestDetector();
        FileSignatureDetector.AddDetector(detector);

        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "Test content");

        var result = FileSignatureDetector.DetectFiletype(tempFile);
        Assert.IsType<TestDetector>(result);

        File.Delete(tempFile);
    }

    [Fact]
    public void DetectFiletype_ShouldDetectFiletypeFromFileInfo()
    {
        var detector = new TestDetector();
        FileSignatureDetector.AddDetector(detector);

        var tempFile = new FileInfo(Path.GetTempFileName());
        File.WriteAllText(tempFile.FullName, "Test content");

        var result = FileSignatureDetector.DetectFiletype(tempFile);
        Assert.IsType<TestDetector>(result);

        tempFile.Delete();
    }

    [Fact]
    public void DetectFiletype_ShouldDetectFiletypeFromStream()
    {
        var detector = new TestDetector();
        FileSignatureDetector.AddDetector(detector);

        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream);
        writer.Write("Test content");
        writer.Flush();
        stream.Position = 0;

        var result = FileSignatureDetector.DetectFiletype(stream);
        Assert.IsType<TestDetector>(result);
    }

    private class TestDetector : IDetector
    {
        public string Precondition => "Test";
        public string Extension => ".test";
        public string MimeType => "application/test";
        public List<FormatCategory> FormatCategories => new List<FormatCategory> { FormatCategory.Document };

        public bool Detect(Stream stream)
        {
            var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();
            return content.Contains("Test content");
        }
    }
}