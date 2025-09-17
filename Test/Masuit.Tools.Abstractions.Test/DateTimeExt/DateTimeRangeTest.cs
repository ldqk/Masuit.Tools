using System;
using Masuit.Tools.Models;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.DateTimeExt;

public class DateTimeRangeTest
{
    [Fact]
    public void Constructor_WithValidDates_ShouldCreateInstance()
    {
        var start = new DateTime(2025, 1, 1);
        var end = new DateTime(2025, 1, 10);
        var range = new DateTimeRange(start, end);
        Assert.Equal(start, range.Start);
        Assert.Equal(end, range.End);
    }

    [Fact]
    public void Constructor_StartAfterEnd_ShouldThrowException()
    {
        var start = new DateTime(2025, 1, 10);
        var end = new DateTime(2025, 1, 1);
        Assert.Throws<Exception>(() => new DateTimeRange(start, end));
    }

    [Fact]
    public void ParameterlessConstructor_ShouldCreateInstanceWithNulls()
    {
        var range = new DateTimeRange();
        Assert.Null(range.Start);
        Assert.Null(range.End);
    }

    [Theory]
    [InlineData("2025-01-05", "2025-01-15", "2025-01-10", "2025-01-20", true)] // Overlap
    [InlineData("2025-01-05", "2025-01-15", "2025-01-15", "2025-01-20", true)] // Touch at end
    [InlineData("2025-01-05", "2025-01-15", "2025-01-01", "2025-01-05", true)] // Touch at start
    [InlineData("2025-01-05", "2025-01-15", "2025-01-20", "2025-01-25", false)] // No overlap
    [InlineData("2025-01-01", "2025-01-31", "2025-01-10", "2025-01-20", true)] // Contain
    [InlineData("2025-01-10", "2025-01-20", "2025-01-01", "2025-01-31", true)] // Inside
    public void HasIntersect_ShouldReturnCorrectly(string s1, string e1, string s2, string e2, bool expected)
    {
        var range1 = new DateTimeRange(DateTime.Parse(s1), DateTime.Parse(e1));
        var range2 = new DateTimeRange(DateTime.Parse(s2), DateTime.Parse(e2));
        Assert.Equal(expected, range1.HasIntersect(range2));
    }

    [Fact]
    public void Intersect_WithOverlappingRanges_ShouldReturnIntersection()
    {
        var range1 = new DateTimeRange(new DateTime(2025, 1, 5), new DateTime(2025, 1, 15));
        var range2 = new DateTimeRange(new DateTime(2025, 1, 10), new DateTime(2025, 1, 20));
        var (intersected, range) = range1.Intersect(range2);
        Assert.True(intersected);
        Assert.Equal(new DateTime(2025, 1, 10), range.Start);
        Assert.Equal(new DateTime(2025, 1, 15), range.End);
    }

    [Fact]
    public void Intersect_WithNonOverlappingRanges_ShouldReturnFalse()
    {
        var range1 = new DateTimeRange(new DateTime(2025, 1, 5), new DateTime(2025, 1, 15));
        var range2 = new DateTimeRange(new DateTime(2025, 1, 20), new DateTime(2025, 1, 25));
        var (intersected, range) = range1.Intersect(range2);
        Assert.False(intersected);
        Assert.Null(range);
    }

    [Fact]
    public void Contains_WithOverlappingRange_ShouldReturnTrue_DueToIncorrectImplementation()
    {
        var range1 = new DateTimeRange(new DateTime(2025, 1, 1), new DateTime(2025, 1, 20));
        var range2 = new DateTimeRange(new DateTime(2025, 1, 15), new DateTime(2025, 1, 25));
        Assert.True(range1.Contains(range2));
    }

    [Fact]
    public void Contains_WithInnerRange_ShouldReturnTrue()
    {
        var outer = new DateTimeRange(new DateTime(2025, 1, 1), new DateTime(2025, 1, 31));
        var inner = new DateTimeRange(new DateTime(2025, 1, 10), new DateTime(2025, 1, 20));
        Assert.True(outer.Contains(inner));
    }

    [Fact]
    public void In_WithOuterRange_ShouldReturnTrue()
    {
        var inner = new DateTimeRange(new DateTime(2025, 1, 10), new DateTime(2025, 1, 20));
        var outer = new DateTimeRange(new DateTime(2025, 1, 1), new DateTime(2025, 1, 31));
        Assert.True(inner.In(outer));
    }

    [Fact]
    public void In_WithOverlappingRange_ShouldReturnTrue_DueToIncorrectImplementation()
    {
        var range1 = new DateTimeRange(new DateTime(2025, 1, 15), new DateTime(2025, 1, 25));
        var range2 = new DateTimeRange(new DateTime(2025, 1, 1), new DateTime(2025, 1, 20));
        Assert.True(range1.In(range2));
    }

    [Fact]
    public void Union_WithOverlappingRanges_ShouldReturnCombinedRange()
    {
        var range1 = new DateTimeRange(new DateTime(2025, 1, 5), new DateTime(2025, 1, 15));
        var range2 = new DateTimeRange(new DateTime(2025, 1, 10), new DateTime(2025, 1, 20));
        var union = range1.Union(range2);
        Assert.Equal(new DateTime(2025, 1, 5), union.Start);
        Assert.Equal(new DateTime(2025, 1, 20), union.End);
    }

    [Fact]
    public void Union_WithNonOverlappingRanges_ShouldThrowException()
    {
        var range1 = new DateTimeRange(new DateTime(2025, 1, 5), new DateTime(2025, 1, 15));
        var range2 = new DateTimeRange(new DateTime(2025, 1, 20), new DateTime(2025, 1, 25));
        Assert.Throws<Exception>(() => range1.Union(range2));
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        var start = new DateTime(2025, 1, 1, 10, 30, 0);
        var end = new DateTime(2025, 1, 10, 18, 45, 0);
        var range = new DateTimeRange(start, end);
        var expected = "2025-01-01 10:30:00~2025-01-10 18:45:00";
        Assert.Equal(expected, range.ToString());
    }

    [Fact]
    public void Equals_WithSameDatesAndTimes_ShouldReturnTrue()
    {
        var start = new DateTime(2025, 1, 1, 10, 0, 0);
        var end = new DateTime(2025, 1, 10, 20, 0, 0);
        var range1 = new DateTimeRange(start, end);
        var range2 = new DateTimeRange(start, end);
        Assert.True(range1.Equals(range2));
    }

    [Fact]
    public void Equals_WithSameDatesDifferentTimes_ShouldReturnTrue_DueToDateOnlyComparison()
    {
        var start1 = new DateTime(2025, 1, 1, 10, 0, 0);
        var end1 = new DateTime(2025, 1, 10, 20, 0, 0);
        var start2 = new DateTime(2025, 1, 1, 12, 0, 0);
        var end2 = new DateTime(2025, 1, 10, 22, 0, 0);
        var range1 = new DateTimeRange(start1, end1);
        var range2 = new DateTimeRange(start2, end2);
        Assert.True(range1.Equals(range2));
    }

    [Fact]
    public void Equals_WithDifferentDates_ShouldReturnFalse()
    {
        var range1 = new DateTimeRange(new DateTime(2025, 1, 1), new DateTime(2025, 1, 10));
        var range2 = new DateTimeRange(new DateTime(2025, 2, 1), new DateTime(2025, 2, 10));
        Assert.False(range1.Equals(range2));
    }

    [Fact]
    public void Equals_WithDifferentObject_ShouldReturnFalse()
    {
        var range = new DateTimeRange(new DateTime(2025, 1, 1), new DateTime(2025, 1, 10));
        var other = new object();
        Assert.False(range.Equals(other));
    }
}