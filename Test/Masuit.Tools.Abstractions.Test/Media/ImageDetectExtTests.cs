using System.IO;
using System.IO.Compression;
using Masuit.Tools.Media;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Media;

public class ImageDetectExtTests
{
    [Fact]
    public void IsImage_ShouldReturnTrueForValidImageFile()
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "test.jpg");
        using var image = new Image<Rgba32>(1, 1);
        image.Save(filePath);

        var fileInfo = new FileInfo(filePath);
        var result = fileInfo.IsImage();

        Assert.True(result);
        File.Delete(filePath);
    }

    [Fact]
    public void IsImage_ShouldReturnTrueForValidImageStream()
    {
        using var ms = new MemoryStream();
        using var image = new Image<Rgba32>(1, 1);
        image.Save(ms, new JpegEncoder());
        ms.Seek(0, SeekOrigin.Begin);

        var result = ms.IsImage();

        Assert.True(result);
    }

    [Fact]
    public void IsImage_ShouldReturnFalseForInvalidImageStream()
    {
        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms);
        writer.Write("This is a test stream.");
        writer.Flush();
        ms.Seek(0, SeekOrigin.Begin);

        Assert.Throws<UnknownImageFormatException>(() => ms.IsImage());
    }

    [Fact]
    public void GetImageType_ShouldReturnCorrectImageFormat()
    {
        using var ms = new MemoryStream();
        using var image = new Image<Rgba32>(1, 1);
        image.Save(ms, new JpegEncoder());
        ms.Seek(0, SeekOrigin.Begin);

        var result = ms.GetImageType();

        Assert.Equal(ImageFormat.Jpg, result);
    }

    [Fact]
    public void GetImageType_ShouldReturnNullForInvalidImageStream()
    {
        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms);
        writer.Write("This is a test stream.");
        writer.Flush();
        ms.Seek(0, SeekOrigin.Begin);

        Assert.Throws<UnknownImageFormatException>(() => ms.GetImageType());
    }
}