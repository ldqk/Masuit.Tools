using Xunit;

namespace Masuit.Tools.Abstractions.Test.Extensions.BaseType;

public class ShortExtensionsTests
{
    [Theory]
    [InlineData((short)0, new byte[] { 0, 0 })]
    [InlineData((short)1, new byte[] { 1, 0 })]
    [InlineData((short)256, new byte[] { 0, 1 })]
    [InlineData((short)-1, new byte[] { 255, 255 })]
    public void GetBytes_ShouldReturnCorrectByteArray(short value, byte[] expected)
    {
        // Act
        var result = value.GetBytes();

        // Assert
        Assert.Equal(expected, result);
    }
}