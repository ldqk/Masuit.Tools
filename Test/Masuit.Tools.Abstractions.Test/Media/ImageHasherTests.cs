using System;
using Masuit.Tools.Media;
using SkiaSharp;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Media;

public class ImageHasherTests
{
    private readonly ImageHasher _imageHasher;

    public ImageHasherTests()
    {
        _imageHasher = new ImageHasher();
    }

    [Fact]
    public void AverageHash64_ShouldReturnCorrectHash()
    {
        using var image = new SKBitmap(8, 8);
        var hash = _imageHasher.AverageHash64(image);
        Assert.Equal(0UL, hash);
    }

    [Fact]
    public void MedianHash64_ShouldReturnCorrectHash()
    {
        using var image = new SKBitmap(8, 8);
        var hash = _imageHasher.MedianHash64(image);
        Assert.Equal(0UL, hash);
    }

    [Fact]
    public void MedianHash256_ShouldReturnCorrectHash()
    {
        using var image = new SKBitmap(16, 16);
        var hash = _imageHasher.MedianHash256(image);
        Assert.NotNull(hash);
        Assert.Equal(4, hash.Length);
    }

    [Fact]
    public void DifferenceHash64_ShouldReturnCorrectHash()
    {
        using var image = new SKBitmap(9, 8);
        var hash = _imageHasher.DifferenceHash64(image);
        Assert.Equal(0UL, hash);
    }

    [Fact]
    public void DifferenceHash256_ShouldReturnCorrectHash()
    {
        using var image = new SKBitmap(17, 16);
        var hash = _imageHasher.DifferenceHash256(image);
        Assert.NotNull(hash);
        Assert.Equal(4, hash.Length);
    }

    [Fact]
    public void WaveletAndMarrHildrethHashes_ShouldReturnExpectedSizesAndDistinguishPatterns()
    {
        using var horizontal = new SKBitmap(64, 64);
        using var vertical = new SKBitmap(64, 64);
        for (var y = 0; y < 64; y++)
        {
            for (var x = 0; x < 64; x++)
            {
                horizontal.SetPixel(x, y, new SKColor((byte)(x * 4), (byte)(x * 4), (byte)(x * 4)));
                vertical.SetPixel(x, y, new SKColor((byte)(y * 4), (byte)(y * 4), (byte)(y * 4)));
            }
        }

        Assert.NotEqual(_imageHasher.MHHash64(horizontal), _imageHasher.MHHash64(vertical));
        Assert.Equal(4, _imageHasher.MHHash256(horizontal).Length);
    }

    [Fact]
    public void WaveletAndMarrHildrethStreamAndPathOverloads_ShouldMatchBitmapOverloads()
    {
        using var image = new SKBitmap(64, 64);
        for (var y = 0; y < 64; y++)
        {
            for (var x = 0; x < 64; x++)
            {
                var value = (byte)((x * 3 + y * 5) % 256);
                image.SetPixel(x, y, new SKColor(value, value, value));
            }
        }

        using var encoded = SKImage.FromBitmap(image).Encode(SKEncodedImageFormat.Png, 100);
        var imageBytes = encoded.ToArray();
        using var stream = new System.IO.MemoryStream(imageBytes);
        var path = System.IO.Path.GetTempFileName();
        try
        {
            System.IO.File.WriteAllBytes(path, imageBytes);
            using var waveletStream256 = new System.IO.MemoryStream(imageBytes);
            using var marrHildrethStream64 = new System.IO.MemoryStream(imageBytes);
            Assert.Equal(_imageHasher.MHHash64(image), _imageHasher.MHHash64(marrHildrethStream64));
            using var marrHildrethStream256 = new System.IO.MemoryStream(imageBytes);
            Assert.Equal(_imageHasher.MHHash256(image), _imageHasher.MHHash256(marrHildrethStream256));
            Assert.Equal(_imageHasher.MHHash64(image), _imageHasher.MHHash64(path));
            Assert.Equal(_imageHasher.MHHash256(image), _imageHasher.MHHash256(path));
        }
        finally
        {
            System.IO.File.Delete(path);
        }
    }

    [Fact]
    public void DctHash256_ShouldReturnFourWordsAndComparePatterns()
    {
        using var horizontal = new SKBitmap(64, 64);
        using var vertical = new SKBitmap(64, 64);
        for (var y = 0; y < 64; y++)
        {
            for (var x = 0; x < 64; x++)
            {
                horizontal.SetPixel(x, y, new SKColor((byte)(x * 4), (byte)(x * 4), (byte)(x * 4)));
                vertical.SetPixel(x, y, new SKColor((byte)(y * 4), (byte)(y * 4), (byte)(y * 4)));
            }
        }

        var horizontalHash = _imageHasher.DctHash256(horizontal);
        var verticalHash = _imageHasher.DctHash256(vertical);

        Assert.Equal(4, horizontalHash.Length);
        Assert.Equal(1.0f, ImageHasher.Compare(horizontalHash, _imageHasher.DctHash256(horizontal)));
        Assert.True(ImageHasher.Compare(horizontalHash, verticalHash) < 1.0f);
        Assert.Equal(1.0f, horizontal.Compare(horizontal, ImageHashAlgorithm.DCT256));
    }

    [Fact]
    public void Compare_ShouldReturnCorrectSimilarity()
    {
        var hash1 = 0b1010101010101010101010101010101010101010101010101010101010101010UL;
        var hash2 = 0b0101010101010101010101010101010101010101010101010101010101010101UL;
        var similarity = ImageHasher.Compare(hash1, hash2);
        Assert.Equal(0.0f, similarity);
    }

    [Fact]
    public void Compare256_ShouldReturnCorrectSimilarity()
    {
        var hash1 = new ulong[] { 0b1010101010101010101010101010101010101010101010101010101010101010UL, 0b1010101010101010101010101010101010101010101010101010101010101010UL, 0b1010101010101010101010101010101010101010101010101010101010101010UL, 0b1010101010101010101010101010101010101010101010101010101010101010UL };
        var hash2 = new ulong[] { 0b0101010101010101010101010101010101010101010101010101010101010101UL, 0b0101010101010101010101010101010101010101010101010101010101010101UL, 0b0101010101010101010101010101010101010101010101010101010101010101UL, 0b0101010101010101010101010101010101010101010101010101010101010101UL };
        var similarity = ImageHasher.Compare(hash1, hash2);
        Assert.Equal(0.0f, similarity);
    }

    [Fact]
    public void Compare_ShouldThrowExceptionForDifferentLengthHashes()
    {
        var hash1 = new ulong[] { 0b1010101010101010101010101010101010101010101010101010101010101010UL };
        var hash2 = new ulong[] { 0b0101010101010101010101010101010101010101010101010101010101010101UL, 0b0101010101010101010101010101010101010101010101010101010101010101UL };
        Assert.Throws<ArgumentException>(() => ImageHasher.Compare(hash1, hash2));
    }
}
