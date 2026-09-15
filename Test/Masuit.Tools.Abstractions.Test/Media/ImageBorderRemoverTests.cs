using System;
using System.IO;
using Masuit.Tools.Media;
using SkiaSharp;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Media;

public class ImageBorderRemoverTests
{
    private SKBitmap CreateTestImage(int width, int height, SKColor borderColor, int borderSize = 5)
    {
        var image = new SKBitmap(width, height);
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                if (x < borderSize || x >= width - borderSize || y < borderSize || y >= height - borderSize)
                    image.SetPixel(x, y, borderColor);
                else
                    image.SetPixel(x, y, new SKColor((byte)Random.Shared.Next(255), (byte)Random.Shared.Next(255), (byte)Random.Shared.Next(255)));
            }
        return image;
    }

    [Fact]
    public void DetectBorders_ImageObject_ShouldDetectBorder()
    {
        var remover = new ImageBorderRemover(ToleranceMode.Channel);
        using var image = CreateTestImage(30, 30, new SKColor(255, 0, 0), 3);

        var result = remover.DetectBorders(image, 10);

        Assert.True(result.HasAnyBorder);
    }
}
