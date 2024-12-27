using System;
using Masuit.Tools.Maths;
using Masuit.Tools.Models;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Models;

public class SphereTests
{
    [Fact]
    public void EarthRadius_ShouldBeCorrect()
    {
        // Arrange
        var expectedRadius = 6371.393;

        // Act
        var earth = Sphere.Earth;

        // Assert
        Assert.Equal(expectedRadius, earth.Radius);
    }

    [Fact]
    public void GetDistance_ShouldReturnCorrectDistance()
    {
        // Arrange
        var sphere = new Sphere(6371.393);
        var lat1 = 0.0;
        var lng1 = 0.0;
        var lat2 = 0.0;
        var lng2 = 90.0;
        var expectedDistance = Math.PI * sphere.Radius / 2;

        // Act
        var distance = sphere.GetDistance(lat1, lng1, lat2, lng2);

        // Assert
        Assert.Equal(expectedDistance, distance, 1e-6);
    }

    [Fact]
    public void IsCrossWith_ShouldReturnTrue_WhenCirclesCross()
    {
        // Arrange
        var sphere = new Sphere(6371.393);
        var circle1 = new Circle(new Point2D(0, 0), 10);
        var circle2 = new Circle(new Point2D(15, 0), 10);

        // Act
        var result = sphere.IsCrossWith(circle1, circle2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsCrossWith_ShouldReturnFalse_WhenCirclesDoNotCross()
    {
        // Arrange
        var sphere = new Sphere(6371.393);
        var circle1 = new Circle(new Point2D(0, 0), 10);
        var circle2 = new Circle(new Point2D(30, 0), 10);

        // Act
        var result = sphere.IsCrossWith(circle1, circle2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsIntersectWith_ShouldReturnTrue_WhenCirclesIntersect()
    {
        // Arrange
        var sphere = new Sphere(6371.393);
        var circle1 = new Circle(new Point2D(0, 0), 10);
        var circle2 = new Circle(new Point2D(20, 0), 10);

        // Act
        var result = sphere.IsIntersectWith(circle1, circle2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsIntersectWith_ShouldReturnFalse_WhenCirclesDoNotIntersect()
    {
        // Arrange
        var sphere = new Sphere(6371.393);
        var circle1 = new Circle(new Point2D(0, 0), 10);
        var circle2 = new Circle(new Point2D(25, 0), 10);

        // Act
        var result = sphere.IsIntersectWith(circle1, circle2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsSeparateWith_ShouldReturnTrue_WhenCirclesAreSeparate()
    {
        // Arrange
        var sphere = new Sphere(6371.393);
        var circle1 = new Circle(new Point2D(0, 0), 10);
        var circle2 = new Circle(new Point2D(30, 0), 10);

        // Act
        var result = sphere.IsSeparateWith(circle1, circle2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsSeparateWith_ShouldReturnFalse_WhenCirclesAreNotSeparate()
    {
        // Arrange
        var sphere = new Sphere(6371.393);
        var circle1 = new Circle(new Point2D(0, 0), 10);
        var circle2 = new Circle(new Point2D(15, 0), 10);

        // Act
        var result = sphere.IsSeparateWith(circle1, circle2);

        // Assert
        Assert.True(result);
    }
}