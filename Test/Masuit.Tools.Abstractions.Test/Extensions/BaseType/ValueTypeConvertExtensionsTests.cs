using Xunit;

namespace Masuit.Tools.Abstractions.Test.Extensions.BaseType;

public class ValueTypeConvertExtensionsTests
{
    [Theory]
    [InlineData("123", 123)]
    [InlineData("abc", 0)]
    [InlineData(null, 0)]
    public void ToInt32_ShouldConvertStringToInt(string input, int expected)
    {
        var result = input.ToInt32();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("1234567890123", 1234567890123)]
    [InlineData("abc", 0)]
    [InlineData(null, 0)]
    public void ToInt64_ShouldConvertStringToLong(string input, long expected)
    {
        var result = input.ToInt64();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("123.45", 123.45)]
    [InlineData("abc", 0)]
    [InlineData(null, 0)]
    public void ToDouble_ShouldConvertStringToDouble(string input, double expected)
    {
        var result = input.ToDouble();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("123.45", 123.45)]
    [InlineData("abc", 0)]
    [InlineData(null, 0)]
    public void ToDecimal_ShouldConvertStringToDecimal(string input, decimal expected)
    {
        var result = input.ToDecimal();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("123.456", 2, 123.46)]
    [InlineData("abc", 2, 0)]
    [InlineData(null, 2, 0)]
    public void ToDecimal_WithRounding_ShouldConvertStringToDecimal(string input, int round, decimal expected)
    {
        var result = input.ToDecimal(round);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToDouble_ShouldConvertDecimalToDouble()
    {
        decimal input = 123.45M;
        var result = input.ToDouble();
        Assert.Equal(123.45, result);
    }

    [Fact]
    public void ToInt32_ShouldConvertDoubleToInt()
    {
        double input = 123.45;
        var result = input.ToInt32();
        Assert.Equal(123, result);
    }

    [Fact]
    public void ToInt32_ShouldConvertDecimalToInt()
    {
        decimal input = 123.45M;
        var result = input.ToInt32();
        Assert.Equal(123, result);
    }

    [Fact]
    public void ToDouble_ShouldConvertIntToDouble()
    {
        int input = 123;
        var result = input.ToDouble();
        Assert.Equal(123.0, result);
    }

    [Fact]
    public void ToDecimal_ShouldConvertIntToDecimal()
    {
        int input = 123;
        var result = input.ToDecimal();
        Assert.Equal(123M, result);
    }

    [Theory]
    [InlineData(123.456, 2, 123.46)]
    [InlineData(123.451, 2, 123.45)]
    public void Round_ShouldRoundDecimal(decimal input, int decimals, decimal expected)
    {
        var result = input.Round(decimals);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(123.456, 2, 123.46)]
    [InlineData(123.451, 2, 123.45)]
    public void Round_ShouldRoundDouble(double input, int decimals, double expected)
    {
        var result = input.Round(decimals);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, 2, null)]
    public void Round_ShouldRoundNullableDecimal(decimal? input, int decimals, decimal? expected)
    {
        var result = input.Round(decimals);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(123.456, 2, 123.46)]
    [InlineData(null, 2, null)]
    public void Round_ShouldRoundNullableDouble(double? input, int decimals, double? expected)
    {
        var result = input.Round(decimals);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToChineseNumber_ShouldConvertNumberToChinese()
    {
        int input = 123;
        var result = input.ToChineseNumber();
        Assert.Equal("一百二十三", result);
    }

    [Fact]
    public void ToChineseMoney_ShouldConvertNumberToChineseMoney()
    {
        decimal input = 123.45M;
        var result = input.ToChineseMoney();
        Assert.Equal("壹佰贰拾叁元肆角伍分", result);
    }
}