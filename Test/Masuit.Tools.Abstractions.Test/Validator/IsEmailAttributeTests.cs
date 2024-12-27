using Masuit.Tools.Core.Validator;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Validator;

public class IsEmailAttributeTests
{
    [Theory]
    [InlineData(null, true, true)]
    [InlineData("", true, true)]
    [InlineData("test@example.com", true, true)]
    [InlineData("invalid-email", false, false)]
    [InlineData("test@invalid-domain", false, false)]
    public void IsValid_ShouldValidateEmail(string email, bool allowEmpty, bool expected)
    {
        // Arrange
        var attribute = new IsEmailAttribute(validDns: false)
        {
            AllowEmpty = allowEmpty
        };

        // Act
        var result = attribute.IsValid(email);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenEmailIsTooShort()
    {
        // Arrange
        var attribute = new IsEmailAttribute();
        var email = "a@b.c";

        // Act
        var result = attribute.IsValid(email);

        // Assert
        Assert.False(result);
        Assert.Equal("您输入的邮箱格式不正确！", attribute.ErrorMessage);
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenEmailIsTooLong()
    {
        // Arrange
        var attribute = new IsEmailAttribute();
        var email = new string('a', 257) + "@example.com";

        // Act
        var result = attribute.IsValid(email);

        // Assert
        Assert.False(result);
        Assert.Equal("您输入的邮箱无效，请使用真实有效的邮箱地址！", attribute.ErrorMessage);
    }
}