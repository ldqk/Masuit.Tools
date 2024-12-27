using Masuit.Tools.Core.Validator;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Validator;

public class MaxValueAttributeTests
{
    [Theory]
    [InlineData(null, true)]
    [InlineData(5, true)]
    [InlineData(10, true)]
    [InlineData(15, false)]
    [InlineData(10.5, false)]
    public void IsValid_ShouldValidateMaxValue(object value, bool expected)
    {
        // Arrange
        var attribute = new MaxValueAttribute(10);

        // Act
        var result = attribute.IsValid(value);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsValid_ShouldReturnTrue_WhenValueIsNull()
    {
        // Arrange
        var attribute = new MaxValueAttribute(10);

        // Act
        var result = attribute.IsValid(null);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_ShouldReturnTrue_WhenValueIsLessThanOrEqualToMaxValue()
    {
        // Arrange
        var attribute = new MaxValueAttribute(10);

        // Act
        var result = attribute.IsValid(10);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenValueIsGreaterThanMaxValue()
    {
        // Arrange
        var attribute = new MaxValueAttribute(10);

        // Act
        var result = attribute.IsValid(15);

        // Assert
        Assert.False(result);
    }
}