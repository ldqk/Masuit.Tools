using System;
using Masuit.Tools.Strings;
using Masuit.Tools.Systems;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Systems;

public class SnowFlakeTests
{
    [Fact]
    public void NewId_ShouldReturnUniqueStringId()
    {
        // Arrange
        var snowFlake = SnowFlake.GetInstance();

        // Act
        var id1 = SnowFlake.NewId;
        var id2 = SnowFlake.NewId;

        // Assert
        Assert.NotNull(id1);
        Assert.NotNull(id2);
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void LongId_ShouldReturnUniqueLongId()
    {
        // Arrange
        var snowFlake = SnowFlake.GetInstance();

        // Act
        var id1 = SnowFlake.LongId;
        var id2 = SnowFlake.LongId;

        // Assert
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void GetUniqueId_ShouldReturnUniqueStringId()
    {
        // Arrange
        var snowFlake = new SnowFlake();

        // Act
        var id1 = snowFlake.GetUniqueId();
        var id2 = snowFlake.GetUniqueId();

        // Assert
        Assert.NotNull(id1);
        Assert.NotNull(id2);
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void GetUniqueShortId_ShouldReturnUniqueShortStringId()
    {
        // Arrange
        var snowFlake = new SnowFlake();

        // Act
        var id1 = snowFlake.GetUniqueShortId();
        var id2 = snowFlake.GetUniqueShortId();

        // Assert
        Assert.NotNull(id1);
        Assert.NotNull(id2);
        Assert.NotEqual(id1, id2);
        Assert.True(id1.Length <= 8);
        Assert.True(id2.Length <= 8);
    }

    [Fact]
    public void SetMachienId_ShouldSetMachineId()
    {
        // Arrange
        var machineId = 512;

        // Act
        SnowFlake.SetMachienId(machineId);

        // Assert
        // No exception should be thrown
    }

    [Fact]
    public void SetMachienId_ShouldThrowExceptionForInvalidMachineId()
    {
        // Arrange
        var machineId = 2048;

        // Act & Assert
        Assert.Throws<Exception>(() => SnowFlake.SetMachienId(machineId));
    }

    [Fact]
    public void SetNumberFormater_ShouldSetNumberFormater()
    {
        // Arrange
        var numberFormater = new NumberFormater(36);

        // Act
        SnowFlake.SetNumberFormater(numberFormater);

        // Assert
        // No exception should be thrown
    }

    [Fact]
    public void SetInitialOffset_ShouldSetOffset()
    {
        // Arrange
        var offset = 1000L;

        // Act
        SnowFlake.SetInitialOffset(offset);

        // Assert
        // No exception should be thrown
    }
}