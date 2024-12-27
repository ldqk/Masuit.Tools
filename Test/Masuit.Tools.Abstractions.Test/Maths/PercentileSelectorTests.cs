using System;
using Masuit.Tools.Maths;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Maths;

public class PercentileSelectorTests
{
    [Theory]
    [InlineData(new[] { 1, 2, 3, 4, 5 }, 50, 3)]
    [InlineData(new[] { 1, 2, 3, 4, 5 }, 100, 5)]
    [InlineData(new[] { 1, 2, 3, 4, 5, 6 }, 50, 3)]
    [InlineData(new[] { 1, 2, 3, 4, 5, 6 }, 75, 5)]
    public void Percentile_ShouldReturnCorrectElement(int[] arr, double percentile, int expected)
    {
        var result = arr.Percentile(percentile);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Percentile_ShouldReturnDefaultForEmptyArray()
    {
        var arr = new int[] { };
        var result = arr.Percentile(50);
        Assert.Equal(default, result);
    }

    [Theory]
    [InlineData(new[] { 1.0, 2.0, 3.0, 4.0, 5.0 }, 50, 3.0)]
    [InlineData(new[] { 1.0, 2.0, 3.0, 4.0, 5.0 }, 100, 5.0)]
    [InlineData(new[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0 }, 50, 3.0)]
    [InlineData(new[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0 }, 75, 5.0)]
    public void Percentile_ShouldReturnCorrectElementForDoubles(double[] arr, double percentile, double expected)
    {
        var result = arr.Percentile(percentile);
        Assert.Equal(expected, result);
    }
}