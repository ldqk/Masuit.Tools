using System;
using System.Collections.Generic;
using Masuit.Tools.Maths;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Maths;

public class RadarChartEngineTests
{
    [Fact]
    public void ComputeArea_ShouldReturnCorrectArea()
    {
        var points = new List<Point2D>
        {
            new Point2D(0, 0),
            new Point2D(4, 0),
            new Point2D(4, 3),
            new Point2D(0, 3)
        };

        var area = points.ComputeArea();
        Assert.Equal(12, area, 6);
    }

    [Fact]
    public void ComputeIntersection_ShouldReturnCorrectIntersection()
    {
        var firstChart = new RadarChart(new List<double> { 3, 4, 5 }, 0);
        var secondChart = new RadarChart(new List<double> { 5, 3, 4 }, 0);

        var intersection = firstChart.ComputeIntersection(secondChart);

        Assert.NotNull(intersection);
        Assert.Equal(5, intersection.Count);
    }

    [Fact]
    public void ComputeIntersection_ShouldThrowExceptionForDifferentStartAngles()
    {
        var firstChart = new RadarChart(new List<double> { 3, 4, 5 }, 0);
        var secondChart = new RadarChart(new List<double> { 5, 3, 4 }, 1);

        Assert.Throws<ArgumentException>(() => firstChart.ComputeIntersection(secondChart));
    }

    [Fact]
    public void ComputeIntersection_ShouldThrowExceptionForDifferentDataCounts()
    {
        var firstChart = new RadarChart(new List<double> { 3, 4, 5 }, 0);
        var secondChart = new RadarChart(new List<double> { 5, 3 }, 0);

        Assert.Throws<ArgumentException>(() => firstChart.ComputeIntersection(secondChart));
    }
}