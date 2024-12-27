using Masuit.Tools.Core.Validator;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Validator;

public class UnifiedSocialCreditCodeAttributeTests
{
    [Theory]
    [InlineData(null, true, true, null)]
    [InlineData("", true, true, null)]
    [InlineData("91350200752941808B", false, true, null)]
    [InlineData("91350200752941808B", true, true, null)]
    [InlineData("invalid-uscc", false, false, "企业统一社会信用代码格式不正确，请输入有效的企业统一社会信用代码！")]
    [InlineData(null, false, false, "企业统一社会信用代码不能为空")]
    public void IsValid_ShouldValidateUnifiedSocialCreditCode(string uscc, bool allowEmpty, bool expected, string expectedErrorMessage)
    {
        // Arrange
        var attribute = new UnifiedSocialCreditCodeAttribute
        {
            AllowEmpty = allowEmpty
        };

        // Act
        var result = attribute.IsValid(uscc);

        // Assert
        Assert.Equal(expected, result);
        Assert.Equal(expectedErrorMessage, attribute.ErrorMessage);
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenUnifiedSocialCreditCodeIsInvalid()
    {
        // Arrange
        var attribute = new UnifiedSocialCreditCodeAttribute();
        var uscc = "123456";

        // Act
        var result = attribute.IsValid(uscc);

        // Assert
        Assert.False(result);
        Assert.Equal("企业统一社会信用代码格式不正确，请输入有效的企业统一社会信用代码！", attribute.ErrorMessage);
    }

    [Fact]
    public void IsValid_ShouldReturnTrue_WhenUnifiedSocialCreditCodeIsValid()
    {
        // Arrange
        var attribute = new UnifiedSocialCreditCodeAttribute();
        var uscc = "91350200752941808B";

        // Act
        var result = attribute.IsValid(uscc);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_ShouldReturnTrue_WhenUnifiedSocialCreditCodeIsNullAndAllowEmptyIsTrue()
    {
        // Arrange
        var attribute = new UnifiedSocialCreditCodeAttribute
        {
            AllowEmpty = true
        };

        // Act
        var result = attribute.IsValid(null);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenUnifiedSocialCreditCodeIsNullAndAllowEmptyIsFalse()
    {
        // Arrange
        var attribute = new UnifiedSocialCreditCodeAttribute
        {
            AllowEmpty = false
        };

        // Act
        var result = attribute.IsValid(null);

        // Assert
        Assert.False(result);
        Assert.Equal("企业统一社会信用代码不能为空", attribute.ErrorMessage);
    }
}