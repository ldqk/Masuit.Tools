using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Masuit.Tools.Files.FileDetector;
using Masuit.Tools.Media;
using SkiaSharp;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Media;

public class ImageDetectExtTests
{
    public static IEnumerable<object[]> ImageDetectorCases
    {
        get
        {
            yield return [ImageFormat.Bmp, "bmp", new byte[] { 0x42, 0x4D }];
            yield return [ImageFormat.Gif, "gif", new byte[] { 0x47, 0x49, 0x46, 0x38 }];
            yield return [ImageFormat.Jpg, "jpg", new byte[] { 0xFF, 0xD8, 0xFF, 0xE7 }];
            yield return [ImageFormat.Jpeg2000, "jp2", new byte[] { 0x00, 0x00, 0x00, 0x0C, 0x6A, 0x50, 0x20, 0x20 }];
            yield return [ImageFormat.JpegXR, "hdp", new byte[] { 0x49, 0x49, 0xBC, 0x01 }];
            yield return [ImageFormat.Png, "png", new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }];
            yield return [ImageFormat.Tga, "tga", new byte[] { 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x18 }];
            yield return [ImageFormat.Tif, "tif", new byte[] { 0x49, 0x49, 0x2A, 0x00 }];
        }
    }

    private static MemoryStream CreateJpegStream()
    {
        using var bitmap = new SKBitmap(1, 1);
        var ms = new MemoryStream();
        using var data = bitmap.Encode(SKEncodedImageFormat.Jpeg, 90);
        data.SaveTo(ms);
        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }

    [Fact]
    public void IsImage_ShouldReturnTrueForValidImageFile()
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "test.jpg");
        using var bitmap = new SKBitmap(1, 1);
        using (var data = bitmap.Encode(SKEncodedImageFormat.Jpeg, 90))
        using (var fs = File.OpenWrite(filePath)) data.SaveTo(fs);

        var fileInfo = new FileInfo(filePath);
        var result = fileInfo.IsImage();

        Assert.True(result);
        File.Delete(filePath);
    }

    [Fact]
    public void IsImage_ShouldReturnTrueForValidImageStream()
    {
        using var ms = CreateJpegStream();
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

        var result = ms.IsImage();
        Assert.False(result);
    }

    [Fact]
    public void GetImageType_ShouldReturnCorrectImageFormat()
    {
        using var ms = CreateJpegStream();
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

        var result = ms.GetImageType();
        Assert.Null(result);
    }

    [Theory]
    [MemberData(nameof(ImageDetectorCases))]
    public void FileDetector_ShouldMatchImageDetectExt(ImageFormat imageFormat, string extension, byte[] header)
    {
        using var stream = new MemoryStream(header);
        Assert.Equal(imageFormat, stream.GetImageType());

        var detector = FileSignatureDetector.Registered.Single(d => d.Extension == extension);
        Assert.True(detector.Detect(stream));
    }
}
