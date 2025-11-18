using System;
using Masuit.Tools.Media;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
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
        using var image = new Image<Rgba32>(8, 8);
        var hash = _imageHasher.AverageHash64(image);
        Assert.Equal(0UL, hash);
    }

    [Fact]
    public void MedianHash64_ShouldReturnCorrectHash()
    {
        using var image = new Image<Rgba32>(8, 8);
        var hash = _imageHasher.MedianHash64(image);
        Assert.Equal(0UL, hash);
    }

    [Fact]
    public void MedianHash256_ShouldReturnCorrectHash()
    {
        using var image = new Image<Rgba32>(16, 16);
        var hash = _imageHasher.MedianHash256(image);
        Assert.NotNull(hash);
        Assert.Equal(4, hash.Length);
    }

    [Fact]
    public void DifferenceHash64_ShouldReturnCorrectHash()
    {
        using var image = new Image<Rgba32>(9, 8);
        var hash = _imageHasher.DifferenceHash64(image);
        Assert.Equal(0UL, hash);
    }

    [Fact]
    public void DifferenceHash256_ShouldReturnCorrectHash()
    {
        using var image = new Image<Rgba32>(17, 16);
        var hash = _imageHasher.DifferenceHash256(image);
        Assert.NotNull(hash);
        Assert.Equal(4, hash.Length);
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