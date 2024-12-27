using System;
using Masuit.Tools.Strings;
using Masuit.Tools.Systems;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Systems;

public class SnowFlakeNewTests
{
    [Fact]
    public void NewId_ShouldReturnUniqueStringId()
    {
        // Arrange
        var snowFlake = SnowFlakeNew.GetInstance();

        // Act
        var id1 = SnowFlakeNew.NewId;
        var id2 = SnowFlakeNew.NewId;

        // Assert
        Assert.NotNull(id1);
        Assert.NotNull(id2);
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void LongId_ShouldReturnUniqueLongId()
    {
        // Arrange
        var snowFlake = SnowFlakeNew.GetInstance();

        // Act
        var id1 = SnowFlakeNew.LongId;
        var id2 = SnowFlakeNew.LongId;

        // Assert
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void GetInstance_ShouldReturnSameInstance()
    {
        // Act
        var instance1 = SnowFlakeNew.GetInstance();
        var instance2 = SnowFlakeNew.GetInstance();

        // Assert
        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void SetMachienId_ShouldSetWorkerId()
    {
        // Arrange
        var machineId = 100;

        // Act
        SnowFlakeNew.SetMachienId(machineId);

        // Assert
        // 使用反射来检查私有字段 _workerId
        var field = typeof(SnowFlakeNew).GetField("_workerId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var workerId = (long)field.GetValue(null);
        Assert.Equal(machineId, workerId);
    }

    [Fact]
    public void SetInitialOffset_ShouldSetOffset()
    {
        // Arrange
        var offset = 123456789L;

        // Act
        SnowFlakeNew.SetInitialOffset(offset);

        // Assert
        // 使用反射来检查私有字段 _offset
        var field = typeof(SnowFlakeNew).GetField("_offset", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var actualOffset = (long)field.GetValue(null);
        Assert.Equal(offset, actualOffset);
    }

    [Fact]
    public void SetNumberFormater_ShouldSetNumberFormater()
    {
        // Arrange
        var numberFormater = new NumberFormater(36);

        // Act
        SnowFlakeNew.SetNumberFormater(numberFormater);

        // Assert
        // 使用反射来检查私有字段 _numberFormater
        var field = typeof(SnowFlakeNew).GetField("_numberFormater", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var actualNumberFormater = (NumberFormater)field.GetValue(null);
        Assert.Same(numberFormater, actualNumberFormater);
    }

    [Fact]
    public void GetUniqueId_ShouldReturnUniqueStringId()
    {
        // Arrange
        var snowFlake = SnowFlakeNew.GetInstance();

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
        var snowFlake = SnowFlakeNew.GetInstance();
        var maxLength = 8;

        // Act
        var id1 = snowFlake.GetUniqueShortId(maxLength);
        var id2 = snowFlake.GetUniqueShortId(maxLength);

        // Assert
        Assert.NotNull(id1);
        Assert.NotNull(id2);
        Assert.NotEqual(id1, id2);
        Assert.True(id1.Length <= maxLength);
        Assert.True(id2.Length <= maxLength);
    }

    [Fact]
    public void GetUniqueShortId_ShouldThrowException_WhenMaxLengthIsLessThan6()
    {
        // Arrange
        var snowFlake = SnowFlakeNew.GetInstance();
        var maxLength = 5;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => snowFlake.GetUniqueShortId(maxLength));
    }
}