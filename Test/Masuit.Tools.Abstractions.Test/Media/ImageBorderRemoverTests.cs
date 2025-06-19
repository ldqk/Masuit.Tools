using System;
using System.IO;
using Masuit.Tools.Media;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Media;

public class ImageBorderRemoverTests
{
    private Image<Rgba32> CreateTestImage(int width, int height, Rgba32 borderColor, int borderSize = 5)
    {
        var image = new Image<Rgba32>(width, height);
        // 填充边框
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                if (x < borderSize || x >= width - borderSize || y < borderSize || y >= height - borderSize)
                    image[x, y] = borderColor;
                else
                    image[x, y] = new Rgba32(Random.Shared.Next(255), Random.Shared.Next(255), Random.Shared.Next(255));
            }
        return image;
    }

    [Fact]
    public void DetectBorders_ImageObject_ShouldDetectBorder()
    {
        var remover = new ImageBorderRemover(ToleranceMode.Channel);
        using var image = CreateTestImage(30, 30, new Rgba32(255, 0, 0), 3);

        var result = remover.DetectBorders(image, 10);

        Assert.True(result.HasAnyBorder);
    }

    [Fact]
    public void RemoveBorders_Stream_ShouldReturnCroppedStream()
    {
        var remover = new ImageBorderRemover(ToleranceMode.Channel);
        using var image = CreateTestImage(60, 60, new Rgba32(255, 0, 0), 6);
        using var ms = new MemoryStream();
        image.SaveAsPng(ms);
        ms.Position = 0;

        using var resultStream = remover.RemoveBorders(ms, 0);
        resultStream.Position = 0;
        using var cropped = Image.Load<Rgba32>(resultStream);

        Assert.True(48 >= cropped.Width);
        Assert.True(48 >= cropped.Height);
    }
}