using System;
using Masuit.Tools.DateTimeExt;
using Masuit.Tools.Models;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.DateTimeExt;

public class DateTimeHelperTest
{
    [Fact]
    public static void GetWeekAmount()
    {
        Assert.Equal(new DateTime(2024, 12, 1).GetWeekAmount(), 53);
    }

    [Fact]
    public static void WeekOfYear()
    {
        Assert.Equal(new DateTime(2024, 12, 1).WeekOfYear(), 49);
        Assert.Equal(new DateTime(2024, 12, 1).WeekOfYear(DayOfWeek.Friday), 49);
    }

    [Fact]
    public static void GetWeekTime()
    {
        Assert.Equal(new DateTime(2024, 12, 1).GetWeekTime(20).ToString(), new DateTimeRange(new DateTime(2024, 5, 13), new DateTime(2024, 5, 19, 23, 59, 59)).ToString());
    }

    [Fact]
    public static void GetCurrentWeek()
    {
        Assert.Equal(new DateTime(2024, 12, 1).GetCurrentWeek().ToString(), new DateTimeRange(new DateTime(2024, 11, 25), new DateTime(2024, 12, 1, 23, 59, 59)).ToString());
    }

    [Fact]
    public static void GetCurrentMonth()
    {
        Assert.Equal(new DateTime(2024, 12, 1).GetCurrentMonth().ToString(), new DateTimeRange(new DateTime(2024, 12, 1), new DateTime(2024, 12, 31, 23, 59, 59)).ToString());
    }

    [Fact]
    public static void GetCurrentLunarMonth()
    {
        Assert.Equal(new DateTime(2024, 12, 1).GetCurrentLunarMonth().ToString(), new DateTimeRange(new DateTime(2024, 12, 1), new DateTime(2024, 12, 30, 23, 59, 59)).ToString());
    }

    [Fact]
    public static void GetCurrentYear()
    {
        Assert.Equal(new DateTime(2024, 12, 1).GetCurrentYear().ToString(), new DateTimeRange(new DateTime(2024, 1, 1), new DateTime(2024, 12, 31, 23, 59, 59)).ToString());
    }

    [Fact]
    public static void GetCurrentLunarYear()
    {
        Assert.Equal(new DateTime(2024, 12, 1).GetCurrentLunarYear().ToString(), new DateTimeRange(new DateTime(2024, 2, 10), new DateTime(2025, 1, 28, 23, 59, 59)).ToString());
    }

    [Fact]
    public static void GetCurrentQuarter()
    {
        Assert.Equal(new DateTime(2024, 12, 1).GetCurrentQuarter().ToString(), new DateTimeRange(new DateTime(2024, 10, 1), new DateTime(2024, 12, 31, 23, 59, 59)).ToString());
    }

    [Fact]
    public static void GetCurrentLunarQuarter()
    {
        Assert.Equal(new DateTime(2024, 12, 1).GetCurrentLunarQuarter().ToString(), new DateTimeRange(new DateTime(2024, 11, 1), new DateTime(2025, 1, 28, 23, 59, 59)).ToString());
    }

    [Fact]
    public static void GetCurrentSolar()
    {
        Assert.Equal(new DateTime(2024, 12, 1).GetCurrentSolar().ToString(), new DateTimeRange(new DateTime(2024, 11, 7), new DateTime(2025, 2, 2, 23, 59, 59)).ToString());
    }

    [Fact]
    public static void GetCurrentRange()
    {
        Assert.Equal(new DateTime(2024, 12, 1).GetCurrentRange(DateRangeType.LunarYear).ToString(), new DateTimeRange(new DateTime(2024, 2, 10), new DateTime(2025, 1, 28, 23, 59, 59)).ToString());
    }

    [Fact]
    public static void GetTotalSeconds()
    {
        Assert.Equal(new DateTime(2024, 12, 1).GetTotalSeconds(), 1732982400);
    }

    [Fact]
    public static void GetTotalMilliseconds()
    {
        Assert.Equal(new DateTime(2024, 12, 1).GetTotalMilliseconds(), 1732982400000);
    }

    [Fact]
    public static void GetTotalMicroseconds()
    {
        Assert.Equal(new DateTime(2024, 12, 1).GetTotalMicroseconds(), 1732982400000000);
    }

    [Fact]
    public static void GetTotalNanoseconds()
    {
        Assert.True(new DateTime(2024, 12, 1).GetTotalNanoseconds() >= 1732982400000000000);
    }

    [Fact]
    public static void GetTotalMinutes()
    {
        Assert.Equal(new DateTime(2024, 12, 1).GetTotalMinutes(), 28883040);
    }

    [Fact]
    public static void GetDaysOfYear()
    {
        Assert.Equal(new DateTime(2024, 12, 1).GetDaysOfYear(), 366);
    }

    [Fact]
    public static void GetDaysOfMonth()
    {
        Assert.Equal(new DateTime(2024, 12, 1).GetDaysOfMonth(), 31);
    }

    [Fact]
    public static void InRange()
    {
        Assert.True(new DateTime(2024, 12, 1).In(new DateTime(2024, 12, 1), new DateTime(2024, 12, 31)));
    }

    [Fact]
    public static void GetMonthLastDate()
    {
        Assert.Equal(new DateTime(2024, 12, 1).GetMonthLastDate(), 31);
    }

    [Fact]
    public static void GetUnionSet()
    {
        Assert.Equal(new DateTimeRange(new DateTime(2024, 12, 10), new DateTime(2024, 12, 20)).GetMaxTimePeriod([new DateTimeRange(new DateTime(2024, 12, 15), new DateTime(2024, 12, 30))]), new DateTimeRange(new DateTime(2024, 12, 15), new DateTime(2024, 12, 30)));
    }
}