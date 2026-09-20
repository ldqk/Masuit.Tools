using System;
using System.IO;
using Masuit.Tools.Media;
using SkiaSharp;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Media;

public class ImageBorderRemoverTests
{
    private static ImageBorderRemover CreateRemover(int croppedBorderCount = 2) => new(ToleranceMode.Channel, croppedBorderCount, new ImageBorderRemoverOptions
    {
        EnableContourRefinement = false,
        MinimumBorderThickness = 2,
        MinimumContentSize = 8
    });

    private static SKBitmap CreateTestImage(int width, int height, Func<int, int, SKColor> borderColor, int borderSize = 5, bool allSides = true)
    {
        var image = new SKBitmap(width, height);
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                if (y < borderSize || allSides && (x < borderSize || x >= width - borderSize || y >= height - borderSize))
                    image.SetPixel(x, y, borderColor(x, y));
                else
                    image.SetPixel(x, y, new SKColor((byte)((x * 17 + y * 13) % 255), (byte)((x * 31 + y * 7) % 255), (byte)((x * 11 + y * 29) % 255)));
            }
        return image;
    }

    [Fact]
    public void DetectBorders_ImageObject_ShouldDetectBorder()
    {
        var remover = CreateRemover();
        using var image = CreateTestImage(48, 48, (_, _) => new SKColor(255, 0, 0), 3);

        var result = remover.DetectBorders(image, 10);

        Assert.Equal(3, result.TopBorderWidth);
        Assert.Equal(3, result.BottomBorderWidth);
        Assert.Equal(3, result.LeftBorderWidth);
        Assert.Equal(3, result.RightBorderWidth);
    }

    [Fact]
    public void DetectBorders_GradientBorder_ShouldDetectBorder()
    {
        var remover = CreateRemover();
        using var image = CreateTestImage(64, 64, (x, y) => new SKColor((byte)(x * 4), (byte)(y * 4), 100), 4);

        var result = remover.DetectBorders(image, 20);

        Assert.True(result.CanBeCropped);
        Assert.Equal(4, result.TopBorderWidth);
    }

    [Fact]
    public void DetectBorders_TransparentBorder_ShouldIncludeAlphaChannel()
    {
        var remover = CreateRemover();
        using var image = CreateTestImage(48, 48, (_, _) => new SKColor(20, 40, 60, 96), 3);

        var result = remover.DetectBorders(image, 5);

        Assert.True(result.CanBeCropped);
        Assert.Equal(3, result.LeftBorderWidth);
    }

    [Fact]
    public void DetectBorders_NoBorder_ShouldKeepOriginalBounds()
    {
        var remover = CreateRemover();
        using var image = CreateTestImage(48, 48, (x, y) => new SKColor((byte)(x * 5), (byte)(y * 5), 100), 0);

        var result = remover.DetectBorders(image, 10);

        Assert.False(result.HasAnyBorder);
        Assert.Equal(image.Width, result.ContentWidth);
        Assert.Equal(image.Height, result.ContentHeight);
    }

    [Fact]
    public void DetectBorders_TexturedImageWithoutBorder_ShouldNotUseContourAlone()
    {
        var remover = new ImageBorderRemover(ToleranceMode.Channel, options: new ImageBorderRemoverOptions
        {
            EnableContourRefinement = true,
            MinimumContentSize = 8
        });
        using var image = new SKBitmap(96, 96);
        for (var y = 0; y < image.Height; y++)
        {
            for (var x = 0; x < image.Width; x++)
            {
                image.SetPixel(x, y, (x / 8 + y / 8) % 2 == 0 ? SKColors.Black : SKColors.White);
            }
        }

        var result = remover.DetectBorders(image, 5);

        Assert.False(result.HasAnyBorder);
        Assert.Equal(image.Width, result.ContentWidth);
        Assert.Equal(image.Height, result.ContentHeight);
    }

    [Fact]
    public void DetectBorders_TopAndBottomBorders_ShouldKeepSideBackground()
    {
        var remover = new ImageBorderRemover(ToleranceMode.Channel, options: new ImageBorderRemoverOptions
        {
            EnableContourRefinement = true,
            MinimumContentSize = 8
        });
        using var image = new SKBitmap(160, 160);
        for (var y = 0; y < image.Height; y++)
        {
            for (var x = 0; x < image.Width; x++)
            {
                var isHorizontalBorder = y < 8 || y >= image.Height - 8;
                var isSubject = x >= 60 && x < 100 && y >= 35 && y < 130;
                image.SetPixel(x, y, isHorizontalBorder
                    ? SKColors.White
                    : isSubject ? new SKColor(40, 40, 40) : new SKColor(180, 190, 190));
            }
        }

        var result = remover.DetectBorders(image, 5);

        Assert.Equal(8, result.TopBorderWidth);
        Assert.Equal(8, result.BottomBorderWidth);
        Assert.Equal(0, result.LeftBorderWidth);
        Assert.Equal(0, result.RightBorderWidth);
        Assert.Equal(image.Width, result.ContentWidth);
    }

    [Fact]
    public void RemoveBorders_AlreadyCroppedImage_ShouldBeIdempotent()
    {
        var remover = CreateRemover();
        using var source = CreateTestImage(64, 64, (_, _) => SKColors.Black, 6);
        using var cropped = remover.RemoveBorders(source, 5);
        Assert.NotNull(cropped);

        using var secondCrop = remover.RemoveBorders(cropped!, 5);

        Assert.Null(secondCrop);
    }

    [Fact]
    public void RemoveBorders_SingleBorder_ShouldNotCrop()
    {
        var remover = CreateRemover();
        using var image = CreateTestImage(48, 48, (_, _) => SKColors.Black, 4, false);

        using var result = remover.RemoveBorders(image, 5);

        Assert.Null(result);
    }

    [Fact]
    public void DetectBorders_LargeImage_ShouldUseSampledScan()
    {
        var remover = CreateRemover();
        using var image = CreateTestImage(1024, 768, (_, _) => new SKColor(120, 20, 30), 8);

        var result = remover.DetectBorders(image, 5);

        Assert.True(result.CanBeCropped);
        Assert.Equal(8, result.TopBorderWidth);
    }
}
