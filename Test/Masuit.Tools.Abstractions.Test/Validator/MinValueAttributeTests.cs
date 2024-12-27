using Masuit.Tools.Core.Validator;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Validator;

public class MinValueAttributeTests
{
    [Theory]
    [InlineData(null, true)]
    [InlineData(5, false)]
    [InlineData(10, false)]
    [InlineData(15, true)]
    [InlineData(10.5, true)]
    public void IsValid_ShouldValidateMinValue(object value, bool expected)
    {
        // Arrange
        var attribute = new MinValueAttribute(10);

        // Act
        var result = attribute.IsValid(value);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsValid_ShouldReturnTrue_WhenValueIsNull()
    {
        // Arrange
        var attribute = new MinValueAttribute(10);

        // Act
        var result = attribute.IsValid(null);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_ShouldReturnTrue_WhenValueIsGreaterThanMinValue()
    {
        // Arrange
        var attribute = new MinValueAttribute(10);

        // Act
        var result = attribute.IsValid(15);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenValueIsLessThanOrEqualToMinValue()
    {
        // Arrange
        var attribute = new MinValueAttribute(10);

        // Act
        var result = attribute.IsValid(10);

        // Assert
        Assert.False(result);
    }
}