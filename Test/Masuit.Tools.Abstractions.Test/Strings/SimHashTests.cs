using System.Numerics;
using Masuit.Tools.Strings;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Strings;

public class SimHashTests
{
    [Fact]
    public void SimHash_Constructor_WithTokensAndHashBits_ShouldInitializeCorrectly()
    {
        // Arrange
        string tokens = "测试字符串";
        int hashBits = 64;

        // Act
        var simHash = new SimHash(tokens, hashBits);

        // Assert
        Assert.Equal(2374431774038, simHash.StrSimHash);
        Assert.Equal(13, simHash.StrSimHash.ToString().Length);
    }

    [Fact]
    public void SimHash_Constructor_WithTokens_ShouldInitializeCorrectly()
    {
        // Arrange
        string tokens = "测试字符串";

        // Act
        var simHash = new SimHash(tokens);

        // Assert
        Assert.Equal(2374431774038, simHash.StrSimHash);
    }

    [Fact]
    public void GetSimHash_ShouldReturnCorrectSimHash()
    {
        // Arrange
        string tokens = "测试字符串";
        var simHash = new SimHash(tokens);

        // Act
        var result = simHash.StrSimHash;

        // Assert
        Assert.NotEqual(BigInteger.Zero, result);
    }

    [Fact]
    public void Hash_ShouldReturnCorrectHash()
    {
        // Arrange
        string source = "测试";
        var simHash = new SimHash(source);

        // Act
        var result = simHash.GetType().GetMethod("Hash", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(simHash, new object[] { source });

        // Assert
        Assert.NotEqual(BigInteger.Zero, result);
    }

    [Fact]
    public void HammingDistance_ShouldReturnCorrectDistance()
    {
        // Arrange
        string tokens1 = "测试字符串1";
        string tokens2 = "测试字符串2";
        var simHash1 = new SimHash(tokens1);
        var simHash2 = new SimHash(tokens2);

        // Act
        var distance = simHash1.HammingDistance(simHash2);

        // Assert
        Assert.True(distance >= 0);
    }

    [Fact]
    public void SimTokenizer_ShouldTokenizeCorrectly()
    {
        // Arrange
        string source = "测试";
        var tokenizer = new SimTokenizer(source);

        // Act & Assert
        Assert.True(tokenizer.HasMoreTokens());
        Assert.Equal("测", tokenizer.NextToken());
        Assert.True(tokenizer.HasMoreTokens());
        Assert.Equal("试", tokenizer.NextToken());
        Assert.False(tokenizer.HasMoreTokens());
    }
}