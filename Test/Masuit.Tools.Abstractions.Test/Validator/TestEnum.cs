using Masuit.Tools.Core.Validator;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Validator;

public enum TestEnum
{
    Value1,
    Value2,
    Value3
}

public class EnumOfAttributeTests
{
    [Theory]
    [InlineData(TestEnum.Value1, true)]
    [InlineData(TestEnum.Value2, true)]
    [InlineData("InvalidValue", false)]
    [InlineData(null, true)]
    public void EnumOfAttribute_ShouldValidateEnumValues(object value, bool expected)
    {
        // Arrange
        var attribute = new EnumOfAttribute(typeof(TestEnum));

        // Act
        var result = attribute.IsValid(value);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("NotEmpty", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void NotNullOrEmptyAttribute_ShouldValidateNonEmptyValues(object value, bool expected)
    {
        // Arrange
        var attribute = new NotNullOrEmptyAttribute();

        // Act
        var result = attribute.IsValid(value);

        // Assert
        Assert.Equal(expected, result);
    }
}