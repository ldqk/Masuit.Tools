using System;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Extensions.BaseType;

public class DoubleExtensionsTests
{
    [Theory]
    [InlineData(0.0, 0.0)]
    [InlineData(1.23, 1.23)]
    [InlineData(-1.23, -1.23)]
    [InlineData(123456789.123456789, 123456789.123456789)]
    public void ToDecimal_FromDouble_ShouldReturnCorrectDecimal(double value, decimal expected)
    {
        // Act
        var result = value.ToDecimal();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(1.2345, 2, MidpointRounding.AwayFromZero, 1.23)]
    [InlineData(1.2355, 2, MidpointRounding.AwayFromZero, 1.24)]
    [InlineData(1.2345, 2, MidpointRounding.ToEven, 1.23)]
    [InlineData(1.2355, 2, MidpointRounding.ToEven, 1.24)]
    [InlineData(-1.2345, 2, MidpointRounding.AwayFromZero, -1.23)]
    [InlineData(-1.2355, 2, MidpointRounding.AwayFromZero, -1.24)]
    public void ToDecimal_FromDouble_WithPrecision_ShouldReturnCorrectDecimal(double value, int precision, MidpointRounding mode, decimal expected)
    {
        // Act
        var result = value.ToDecimal(precision, mode);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0.0f, 0.0)]
    [InlineData(1.23f, 1.23)]
    [InlineData(-1.23f, -1.23)]
    public void ToDecimal_FromFloat_ShouldReturnCorrectDecimal(float value, decimal expected)
    {
        // Act
        var result = value.ToDecimal();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(1.2345f, 2, MidpointRounding.AwayFromZero, 1.23)]
    [InlineData(1.2355f, 2, MidpointRounding.AwayFromZero, 1.24)]
    [InlineData(1.2345f, 2, MidpointRounding.ToEven, 1.23)]
    [InlineData(1.2355f, 2, MidpointRounding.ToEven, 1.24)]
    [InlineData(-1.2345f, 2, MidpointRounding.AwayFromZero, -1.23)]
    [InlineData(-1.2355f, 2, MidpointRounding.AwayFromZero, -1.24)]
    public void ToDecimal_FromFloat_WithPrecision_ShouldReturnCorrectDecimal(float value, int precision, MidpointRounding mode, decimal expected)
    {
        // Act
        var result = value.ToDecimal(precision, mode);

        // Assert
        Assert.Equal(expected, result);
    }
}