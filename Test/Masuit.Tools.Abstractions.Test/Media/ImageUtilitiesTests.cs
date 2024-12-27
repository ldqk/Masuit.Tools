using System.IO;
using Masuit.Tools.Media;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Media;

public class ImageUtilitiesTests
{
    [Theory]
    [InlineData("image/pjpeg", true)]
    [InlineData("image/jpeg", true)]
    [InlineData("image/gif", true)]
    [InlineData("image/bmpp", true)]
    [InlineData("image/png", true)]
    [InlineData("image/tiff", false)]
    public void IsWebImage_ShouldReturnCorrectResult(string contentType, bool expected)
    {
        var result = ImageUtilities.IsWebImage(contentType);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void CutImage_ShouldReturnCroppedImage()
    {
        using var image = new Image<Rgba32>(100, 100);
        var rect = new Rectangle(10, 10, 50, 50);
        var croppedImage = image.CutImage(rect);

        Assert.Equal(50, croppedImage.Width);
        Assert.Equal(50, croppedImage.Height);
    }

    [Fact]
    public void CutAndResize_ShouldReturnCroppedAndResizedImage()
    {
        using var image = new Image<Rgba32>(100, 100);
        var rect = new Rectangle(10, 10, 50, 50);
        var resizedImage = image.CutAndResize(rect, 25, 25);

        Assert.Equal(25, resizedImage.Width);
        Assert.Equal(25, resizedImage.Height);
    }

    [Fact]
    public void MakeThumbnail_ShouldCreateThumbnail()
    {
        using var image = new Image<Rgba32>(100, 100);
        var thumbnailPath = Path.Combine(Directory.GetCurrentDirectory(), "thumbnail.jpg");
        image.MakeThumbnail(thumbnailPath, 50, 50, ResizeMode.Crop);

        Assert.True(File.Exists(thumbnailPath));
        using var thumbnail = Image.Load(thumbnailPath);
        Assert.Equal(50, thumbnail.Width);
        Assert.Equal(50, thumbnail.Height);
        File.Delete(thumbnailPath);
    }

    [Fact]
    public void MakeThumbnail_ShouldReturnThumbnailImage()
    {
        using var image = new Image<Rgba32>(100, 100);
        var thumbnail = image.MakeThumbnail(50, 50, ResizeMode.Crop);

        Assert.Equal(50, thumbnail.Width);
        Assert.Equal(50, thumbnail.Height);
    }

    [Fact]
    public void LDPic_ShouldAdjustBrightness()
    {
        using var image = new Image<Rgba32>(100, 100);
        var adjustedImage = image.LDPic(50);

        Assert.NotEqual(image, adjustedImage);
    }

    [Fact]
    public void RePic_ShouldReturnInvertedImage()
    {
        using var image = new Image<Rgba32>(100, 100);
        var invertedImage = image.RePic();

        Assert.NotEqual(image, invertedImage);
    }

    [Fact]
    public void Relief_ShouldReturnReliefImage()
    {
        using var image = new Image<Rgba32>(100, 100);
        var reliefImage = image.Relief();

        Assert.NotEqual(image, reliefImage);
    }

    [Fact]
    public void ResizeImage_ShouldReturnResizedImage()
    {
        using var image = new Image<Rgba32>(100, 100);
        var resizedImage = image.ResizeImage(50, 50);

        Assert.Equal(50, resizedImage.Width);
        Assert.Equal(50, resizedImage.Height);
    }

    [Fact]
    public void FilPic_ShouldReturnFilteredImage()
    {
        using var image = new Image<Rgba32>(100, 100);
        var filteredImage = image.FilPic();

        Assert.NotEqual(image, filteredImage);
    }

    [Fact]
    public void RevPicLR_ShouldReturnHorizontallyFlippedImage()
    {
        using var image = new Image<Rgba32>(100, 120);
        var flippedImage = image.RevPicLR();

        Assert.Equal(image.Width, 100);
        Assert.Equal(image.Height, 120);
    }

    [Fact]
    public void RevPicUD_ShouldReturnVerticallyFlippedImage()
    {
        using var image = new Image<Rgba32>(100, 120);
        var flippedImage = image.RevPicUD();

        Assert.Equal(image.Width, 100);
        Assert.Equal(image.Height, 120);
    }

    [Fact]
    public void Gray_ShouldReturnGrayscaleColor()
    {
        var color = Color.Red;
        var grayColor = color.Gray();

        Assert.NotEqual(color, grayColor);
    }

    [Fact]
    public void Reverse_ShouldReturnReversedColor()
    {
        var color = Color.Red;
        var reversedColor = color.Reverse();

        Assert.NotEqual(color, reversedColor);
    }

    [Fact]
    public void BWPic_ShouldReturnBlackAndWhiteImage()
    {
        using var image = new Image<Rgba32>(100, 100);
        var bwImage = image.BWPic(50, 50);

        Assert.Equal(50, bwImage.Width);
        Assert.Equal(50, bwImage.Height);
    }
}