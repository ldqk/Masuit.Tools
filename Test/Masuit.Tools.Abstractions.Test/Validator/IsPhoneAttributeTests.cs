using Masuit.Tools.Core.Validator;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Validator;

public class IsPhoneAttributeTests
{
    [Theory]
    [InlineData(null, true, true, null)]
    [InlineData("", true, true, null)]
    [InlineData("13800138000", false, true, null)]
    [InlineData("13800138000", true, true, null)]
    [InlineData("invalid-phone", false, false, "手机号码格式不正确，请输入有效的大陆11位手机号码！")]
    [InlineData(null, false, false, "手机号码不能为空")]
    public void IsValid_ShouldValidatePhoneNumber(string phoneNumber, bool allowEmpty, bool expected, string expectedErrorMessage)
    {
        // Arrange
        var attribute = new IsPhoneAttribute
        {
            AllowEmpty = allowEmpty
        };

        // Act
        var result = attribute.IsValid(phoneNumber);

        // Assert
        Assert.Equal(expected, result);
        Assert.Equal(expectedErrorMessage, attribute.ErrorMessage);
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenPhoneNumberIsInvalid()
    {
        // Arrange
        var attribute = new IsPhoneAttribute();
        var phoneNumber = "123456";

        // Act
        var result = attribute.IsValid(phoneNumber);

        // Assert
        Assert.False(result);
        Assert.Equal("手机号码格式不正确，请输入有效的大陆11位手机号码！", attribute.ErrorMessage);
    }

    [Fact]
    public void IsValid_ShouldReturnTrue_WhenPhoneNumberIsValid()
    {
        // Arrange
        var attribute = new IsPhoneAttribute();
        var phoneNumber = "13800138000";

        // Act
        var result = attribute.IsValid(phoneNumber);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_ShouldReturnTrue_WhenPhoneNumberIsNullAndAllowEmptyIsTrue()
    {
        // Arrange
        var attribute = new IsPhoneAttribute
        {
            AllowEmpty = true
        };

        // Act
        var result = attribute.IsValid(null);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenPhoneNumberIsNullAndAllowEmptyIsFalse()
    {
        // Arrange
        var attribute = new IsPhoneAttribute
        {
            AllowEmpty = false
        };

        // Act
        var result = attribute.IsValid(null);

        // Assert
        Assert.False(result);
        Assert.Equal("手机号码不能为空", attribute.ErrorMessage);
    }
}