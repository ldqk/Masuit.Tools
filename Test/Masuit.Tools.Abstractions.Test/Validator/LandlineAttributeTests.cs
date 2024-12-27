using Masuit.Tools.Core.Validator;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Validator;

public class LandlineAttributeTests
{
    [Theory]
    [InlineData(null, true, true, null)]
    [InlineData("", true, true, null)]
    [InlineData("010-12345678", false, true, null)]
    [InlineData("010-12345678", true, true, null)]
    [InlineData("invalid-landline", false, false, "固定电话格式不正确，请输入有效的大陆11/12位固定电话！")]
    [InlineData(null, false, false, "固定电话不能为空")]
    public void IsValid_ShouldValidateLandline(string landline, bool allowEmpty, bool expected, string expectedErrorMessage)
    {
        // Arrange
        var attribute = new LandlineAttribute
        {
            AllowEmpty = allowEmpty
        };

        // Act
        var result = attribute.IsValid(landline);

        // Assert
        Assert.Equal(expected, result);
        Assert.Equal(expectedErrorMessage, attribute.ErrorMessage);
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenLandlineIsInvalid()
    {
        // Arrange
        var attribute = new LandlineAttribute();
        var landline = "123456";

        // Act
        var result = attribute.IsValid(landline);

        // Assert
        Assert.False(result);
        Assert.Equal("固定电话格式不正确，请输入有效的大陆11/12位固定电话！", attribute.ErrorMessage);
    }

    [Fact]
    public void IsValid_ShouldReturnTrue_WhenLandlineIsValid()
    {
        // Arrange
        var attribute = new LandlineAttribute();
        var landline = "010-12345678";

        // Act
        var result = attribute.IsValid(landline);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_ShouldReturnTrue_WhenLandlineIsNullAndAllowEmptyIsTrue()
    {
        // Arrange
        var attribute = new LandlineAttribute
        {
            AllowEmpty = true
        };

        // Act
        var result = attribute.IsValid(null);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenLandlineIsNullAndAllowEmptyIsFalse()
    {
        // Arrange
        var attribute = new LandlineAttribute
        {
            AllowEmpty = false
        };

        // Act
        var result = attribute.IsValid(null);

        // Assert
        Assert.False(result);
        Assert.Equal("固定电话不能为空", attribute.ErrorMessage);
    }
}