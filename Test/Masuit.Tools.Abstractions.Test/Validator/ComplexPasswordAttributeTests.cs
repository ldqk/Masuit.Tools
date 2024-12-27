using Masuit.Tools.Core.Validator;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Validator;

public class ComplexPasswordAttributeTests
{
    [Theory]
    [InlineData("Password123!", true)]
    [InlineData("Pass123!", true)]
    [InlineData("123456", false)]
    [InlineData("abcdef", false)]
    [InlineData("ABCDEF", false)]
    [InlineData("!@#$%^", false)]
    [InlineData("Pass!1", true)]
    [InlineData("P@ssw0rd", true)]
    [InlineData("P@ss", false)]
    public void IsValid_DefaultSettings(string password, bool expected)
    {
        // Arrange
        var attribute = new ComplexPasswordAttribute();

        // Act
        var result = attribute.IsValid(password);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Password123!", true)]
    [InlineData("Pass123!", true)]
    [InlineData("123456", false)]
    [InlineData("abcdef", false)]
    [InlineData("ABCDEF", false)]
    [InlineData("!@#$%^", false)]
    [InlineData("P@ssw0rd", true)]
    [InlineData("P@ss", false)]
    public void IsValid_CustomSettings(string password, bool expected)
    {
        // Arrange
        var attribute = new ComplexPasswordAttribute(8, 20)
        {
            MustNumber = true,
            MustLetter = true,
            MustSymbol = true
        };

        // Act
        var result = attribute.IsValid(password);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Password123", true)]
    [InlineData("Pass123", true)]
    [InlineData("123456", false)]
    [InlineData("abcdef", false)]
    [InlineData("ABCDEF", false)]
    [InlineData("!@#$%^", false)]
    [InlineData("Pass1", false)]
    [InlineData("P@ssw0rd", true)]
    [InlineData("P@ss", false)]
    public void IsValid_NoSymbolRequirement(string password, bool expected)
    {
        // Arrange
        var attribute = new ComplexPasswordAttribute
        {
            MustNumber = true,
            MustLetter = true,
            MustSymbol = false
        };

        // Act
        var result = attribute.IsValid(password);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Password!", true)]
    [InlineData("Pass!", false)]
    [InlineData("123456", false)]
    [InlineData("abcdef", false)]
    [InlineData("ABCDEF", false)]
    [InlineData("!@#$%^", false)]
    [InlineData("P@ssw0rd", true)]
    [InlineData("P@ss", false)]
    public void IsValid_NoNumberRequirement(string password, bool expected)
    {
        // Arrange
        var attribute = new ComplexPasswordAttribute
        {
            MustNumber = false,
            MustLetter = true,
            MustSymbol = true
        };

        // Act
        var result = attribute.IsValid(password);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Password123", false)]
    [InlineData("Pass123", false)]
    [InlineData("123456", false)]
    [InlineData("abcdef", false)]
    [InlineData("ABCDEF", false)]
    [InlineData("!@#$%^", false)]
    [InlineData("Pass1", false)]
    [InlineData("P@ssw0rd", true)]
    [InlineData("P@ss", false)]
    public void IsValid_NoLetterRequirement(string password, bool expected)
    {
        // Arrange
        var attribute = new ComplexPasswordAttribute
        {
            MustNumber = true,
            MustLetter = false,
            MustSymbol = true
        };

        // Act
        var result = attribute.IsValid(password);

        // Assert
        Assert.Equal(expected, result);
    }
}