using System;
using System.IO;
using Masuit.Tools.Media;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Media;

public class ImageWatermarkerTests
{
    [Fact]
    public void AddWatermark_ShouldAddImageWatermark()
    {
        var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "test4.jpg");
        using var image = new Image<Rgba32>(100, 100);
        image.Save(imagePath);

        var watermarkPath = Path.Combine(Directory.GetCurrentDirectory(), "watermark.png");
        using var watermark = new Image<Rgba32>(20, 20);
        watermark.Save(watermarkPath);

        using var imageStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
        using var watermarkStream = new FileStream(watermarkPath, FileMode.Open, FileAccess.Read);
        var watermarker = new ImageWatermarker(imageStream);
        var resultStream = watermarker.AddWatermark(watermarkStream);

        Assert.NotNull(resultStream);
        Assert.True(resultStream.Length > 0);
        try
        {
            File.Delete(imagePath);
            File.Delete(watermarkPath);
        }
        catch (Exception e)
        {
        }
    }

    [Fact]
    public void AddWatermark_ShouldAddImageWatermarkWithSkipSmallImages()
    {
        var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "test5.jpg");
        using var image = new Image<Rgba32>(50, 50);
        image.Save(imagePath);

        var watermarkPath = Path.Combine(Directory.GetCurrentDirectory(), "watermark.png");
        using var watermark = new Image<Rgba32>(20, 20);
        watermark.Save(watermarkPath);

        using var imageStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
        using var watermarkStream = new FileStream(watermarkPath, FileMode.Open, FileAccess.Read);
        var watermarker = new ImageWatermarker(imageStream, new JpegEncoder(), true, 10000);
        var resultStream = watermarker.AddWatermark(watermarkStream);

        Assert.NotNull(resultStream);
        Assert.True(resultStream.Length > 0);
        try
        {
            File.Delete(imagePath);
            File.Delete(watermarkPath);
        }
        catch (Exception e)
        {
        }
    }
}