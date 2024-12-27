using System.Collections.Generic;
using Masuit.Tools.Core.Validator;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Validator;

public class MinItemsCountAttributeTests
{
    [Theory]
    [InlineData(null, false)]
    [InlineData(new int[] { }, false)]
    [InlineData(new int[] { 1 }, true)]
    [InlineData(new int[] { 1, 2 }, true)]
    [InlineData(new int[] { 1, 2, 3 }, true)]
    public void IsValid_ShouldValidateMinItemsCount_Array(IEnumerable<int> value, bool expected)
    {
        // Arrange
        var attribute = new MinItemsCountAttribute(1);

        // Act
        var result = attribute.IsValid(value);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenValueIsNull()
    {
        // Arrange
        var attribute = new MinItemsCountAttribute(1);

        // Act
        var result = attribute.IsValid(null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenCollectionCountIsLessThanMinItems()
    {
        // Arrange
        var attribute = new MinItemsCountAttribute(3);
        var collection = new List<int> { 1, 2 };

        // Act
        var result = attribute.IsValid(collection);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_ShouldReturnTrue_WhenCollectionCountIsGreaterThanOrEqualToMinItems()
    {
        // Arrange
        var attribute = new MinItemsCountAttribute(2);
        var collection = new List<int> { 1, 2, 3 };

        // Act
        var result = attribute.IsValid(collection);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_ShouldReturnTrue_WhenEnumerableCountIsGreaterThanOrEqualToMinItems()
    {
        // Arrange
        var attribute = new MinItemsCountAttribute(2);
        var enumerable = GetEnumerable(3);

        // Act
        var result = attribute.IsValid(enumerable);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenEnumerableCountIsLessThanMinItems()
    {
        // Arrange
        var attribute = new MinItemsCountAttribute(3);
        var enumerable = GetEnumerable(2);

        // Act
        var result = attribute.IsValid(enumerable);

        // Assert
        Assert.False(result);
    }

    private IEnumerable<int> GetEnumerable(int count)
    {
        for (int i = 0; i < count; i++)
        {
            yield return i;
        }
    }
}