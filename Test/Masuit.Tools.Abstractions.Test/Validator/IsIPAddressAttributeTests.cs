using Masuit.Tools.Core.Validator;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Validator;

public class IsIPAddressAttributeTests
{
    [Theory]
    [InlineData(null, false, "IP地址不能为空！")]
    [InlineData("", false, "IP地址格式不正确，请输入有效的IPv4地址")]
    [InlineData("192.168.1.1", true, null)]
    [InlineData("255.255.255.255", true, null)]
    [InlineData("0.0.0.0", true, null)]
    [InlineData("256.256.256.256", false, "IP地址格式不正确，请输入有效的IPv4地址")]
    [InlineData("invalid-ip", false, "IP地址格式不正确，请输入有效的IPv4地址")]
    public void IsValid_ShouldValidateIPAddress(string ipAddress, bool expected, string expectedErrorMessage)
    {
        // Arrange
        var attribute = new IsIPAddressAttribute();

        // Act
        var result = attribute.IsValid(ipAddress);

        // Assert
        Assert.Equal(expected, result);
        Assert.Equal(expectedErrorMessage, attribute.ErrorMessage);
    }
}