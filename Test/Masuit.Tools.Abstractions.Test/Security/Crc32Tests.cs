using Masuit.Tools.Security;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Security;

public class Crc32Tests
{
    [Fact]
    public void Crc32_DefaultConstructor_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var crc32 = new Crc32();

        // Assert
        Assert.Equal(32, crc32.HashSize);
    }

    [Fact]
    public void Crc32_ParameterizedConstructor_ShouldInitializeWithGivenValues()
    {
        // Arrange
        uint polynomial = 0x04C11DB7;
        uint seed = 0xFFFFFFFF;

        // Act
        var crc32 = new Crc32(polynomial, seed);

        // Assert
        Assert.Equal(32, crc32.HashSize);
    }

    [Fact]
    public void HashFinal_ShouldReturnCorrectHash()
    {
        // Arrange
        var crc32 = new Crc32();
        var data = new byte[] { 1, 2, 3 };

        // Act
        crc32.ComputeHash(data);
        var hash = crc32.Hash;

        // Assert
        Assert.NotNull(hash);
        Assert.Equal(4, hash.Length);
    }

    [Fact]
    public void Compute_WithBuffer_ShouldReturnCorrectHash()
    {
        // Arrange
        var data = new byte[] { 1, 2, 3 };

        // Act
        var hash = Crc32.Compute(data);

        // Assert
        Assert.Equal((uint)1438416925, hash);
    }

    [Fact]
    public void Compute_WithSeedAndBuffer_ShouldReturnCorrectHash()
    {
        // Arrange
        var data = new byte[] { 1, 2, 3 };
        uint seed = 0xFFFFFFFF;

        // Act
        var hash = Crc32.Compute(seed, data);

        // Assert
        Assert.Equal((uint)1438416925, hash);
    }

    [Fact]
    public void Compute_WithPolynomialSeedAndBuffer_ShouldReturnCorrectHash()
    {
        // Arrange
        var data = new byte[] { 1, 2, 3 };
        uint polynomial = 0x04C11DB7;
        uint seed = 0xFFFFFFFF;

        // Act
        var hash = Crc32.Compute(polynomial, seed, data);

        // Assert
        Assert.Equal((uint)4185564129, hash);
    }
}