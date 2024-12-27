using System;
using Masuit.Tools.DateTimeExt;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.DateTimeExt;

public class ChineseCalendarTest
{
    [Fact]
    public void Can_Test()
    {
        Assert.Equal(new ChineseCalendar(new DateTime(2024, 1, 1)).AnimalString, "龙");
        Assert.Equal(new ChineseCalendar(new DateTime(2025, 1, 29)).ChineseCalendarHoliday, "春节");
        Assert.Equal(new ChineseCalendar(new DateTime(2024, 1, 1)).ChineseConstellation, "毕月乌");
        Assert.Equal(new ChineseCalendar(new DateTime(2024, 1, 1)).ChineseDateString, "二零二三年冬月二十");
        Assert.Equal(new ChineseCalendar(new DateTime(2024, 1, 1)).ChineseHour, "甲子");
        Assert.Equal(new ChineseCalendar(new DateTime(2024, 12, 21)).ChineseTwentyFourDay, "冬至");
        Assert.Equal(new ChineseCalendar(new DateTime(2024, 12, 21)).ChineseTwentyFourNextDay, new ChineseCalendar(new DateTime(2025, 1, 5)));
        Assert.Equal(new ChineseCalendar(new DateTime(2024, 12, 21)).ChineseTwentyFourPrevDay, new ChineseCalendar(new DateTime(2024, 12, 6)));
        Assert.Equal(new ChineseCalendar(new DateTime(2024, 1, 1)).ChineseYear, 2023);
        Assert.Equal(new ChineseCalendar(new DateTime(2024, 1, 1)).DateHoliday, "元旦");
        Assert.Equal(new ChineseCalendar(new DateTime(2024, 1, 1)).DateString, "公元2024年1月1日");
        Assert.Equal(new ChineseCalendar(new DateTime(2024, 1, 1)).GanZhiDateString, "癸卯年甲子月甲子日");
        Assert.True(new ChineseCalendar(new DateTime(2024, 1, 1)).IsChineseLeapYear);
        Assert.Equal(new ChineseCalendar(new DateTime(2024, 1, 1)).NextDay, new ChineseCalendar(new DateTime(2024, 1, 2)));
        Assert.Equal(new ChineseCalendar(new DateTime(2024, 1, 1)).PrevDay, new ChineseCalendar(new DateTime(2023, 12, 31)));
        Assert.Equal(new ChineseCalendar(new DateTime(2024, 1, 1)).WeekDay, DayOfWeek.Monday);
    }

    [Fact]
    public void Constructor_ShouldInitializeWithGregorianDate()
    {
        // Arrange
        var date = new DateTime(2023, 10, 1);

        // Act
        var calendar = new ChineseCalendar(date);

        // Assert
        Assert.Equal(date, calendar.Date);
    }

    [Fact]
    public void Constructor_ShouldInitializeWithLunarDate()
    {
        // Arrange
        var year = 2023;
        var month = 8;
        var day = 15;

        // Act
        var calendar = new ChineseCalendar(year, month, day);

        // Assert
        Assert.Equal(year, calendar.ChineseYear);
        Assert.Equal(month, calendar.ChineseMonth);
        Assert.Equal(day, calendar.ChineseDay);
    }

    [Fact]
    public void IsHoliday_ShouldReturnTrueForHoliday()
    {
        // Arrange
        var date = new DateTime(2023, 10, 1); // 国庆节
        var calendar = new ChineseCalendar(date);

        // Act
        var isHoliday = calendar.IsHoliday;

        // Assert
        Assert.True(isHoliday);
    }

    [Fact]
    public void IsWorkDay_ShouldReturnFalseForHoliday()
    {
        // Arrange
        var date = new DateTime(2023, 10, 1); // 国庆节
        var calendar = new ChineseCalendar(date);

        // Act
        var isWorkDay = calendar.IsWorkDay;

        // Assert
        Assert.False(isWorkDay);
    }

    [Fact]
    public void IsWorkDay_ShouldReturnTrueForWorkDay()
    {
        // Arrange
        var date = new DateTime(2023, 10, 9); // 普通工作日
        var calendar = new ChineseCalendar(date);

        // Act
        var isWorkDay = calendar.IsWorkDay;

        // Assert
        Assert.True(isWorkDay);
    }

    [Fact]
    public void AddDays_ShouldReturnCorrectDate()
    {
        // Arrange
        var date = new DateTime(2023, 10, 1);
        var calendar = new ChineseCalendar(date);

        // Act
        var newCalendar = calendar.AddDays(10);

        // Assert
        Assert.Equal(new DateTime(2023, 10, 11), newCalendar.Date);
    }

    [Fact]
    public void AddMonths_ShouldReturnCorrectDate()
    {
        // Arrange
        var date = new DateTime(2023, 10, 1);
        var calendar = new ChineseCalendar(date);

        // Act
        var newCalendar = calendar.AddMonths(2);

        // Assert
        Assert.Equal(new DateTime(2023, 12, 1), newCalendar.Date);
    }

    [Fact]
    public void Equals_ShouldReturnTrueForSameDate()
    {
        // Arrange
        var date = new DateTime(2023, 10, 1);
        var calendar1 = new ChineseCalendar(date);
        var calendar2 = new ChineseCalendar(date);

        // Act
        var isEqual = calendar1.Equals(calendar2);

        // Assert
        Assert.True(isEqual);
    }

    [Fact]
    public void Equals_ShouldReturnFalseForDifferentDate()
    {
        // Arrange
        var calendar1 = new ChineseCalendar(new DateTime(2023, 10, 1));
        var calendar2 = new ChineseCalendar(new DateTime(2023, 10, 2));

        // Act
        var isEqual = calendar1.Equals(calendar2);

        // Assert
        Assert.False(isEqual);
    }

    [Fact]
    public void ChineseCalendarHoliday_ShouldReturnCorrectHoliday()
    {
        // Arrange
        var date = new DateTime(2023, 1, 22); // 农历春节
        var calendar = new ChineseCalendar(date);

        // Act
        var holiday = calendar.ChineseCalendarHoliday;

        // Assert
        Assert.Equal("春节", holiday);
    }

    [Fact]
    public void WeekDayHoliday_ShouldReturnCorrectHoliday()
    {
        // Arrange
        var date = new DateTime(2023, 5, 14); // 母亲节
        var calendar = new ChineseCalendar(date);

        // Act
        var holiday = calendar.WeekDayHoliday;

        // Assert
        Assert.Equal("母亲节", holiday);
    }

    [Fact]
    public void DateHoliday_ShouldReturnCorrectHoliday()
    {
        // Arrange
        var date = new DateTime(2023, 10, 1); // 国庆节
        var calendar = new ChineseCalendar(date);

        // Act
        var holiday = calendar.DateHoliday;

        // Assert
        Assert.True(holiday.Contains("国庆节"));
    }

    [Fact]
    public void Constellation_ShouldReturnCorrectConstellation()
    {
        // Arrange
        var date = new DateTime(2023, 4, 20); // 金牛座
        var calendar = new ChineseCalendar(date);

        // Act
        var constellation = calendar.Constellation;

        // Assert
        Assert.Equal("金牛座", constellation);
    }

    [Fact]
    public void AnimalString_ShouldReturnCorrectAnimal()
    {
        // Arrange
        var date = new DateTime(2023, 1, 22); // 兔年
        var calendar = new ChineseCalendar(date);

        // Act
        var animal = calendar.AnimalString;

        // Assert
        Assert.Equal("兔", animal);
    }

    [Fact]
    public void GanZhiYearString_ShouldReturnCorrectGanZhiYear()
    {
        // Arrange
        var date = new DateTime(2023, 1, 22); // 癸卯年
        var calendar = new ChineseCalendar(date);

        // Act
        var ganZhiYear = calendar.GanZhiYearString;

        // Assert
        Assert.Equal("癸卯年", ganZhiYear);
    }

    [Fact]
    public void GanZhiMonthString_ShouldReturnCorrectGanZhiMonth()
    {
        // Arrange
        var date = new DateTime(2023, 1, 22); // 甲寅月
        var calendar = new ChineseCalendar(date);

        // Act
        var ganZhiMonth = calendar.GanZhiMonthString;

        // Assert
        Assert.Equal("甲寅月", ganZhiMonth);
    }

    [Fact]
    public void GanZhiDayString_ShouldReturnCorrectGanZhiDay()
    {
        // Arrange
        var date = new DateTime(2023, 1, 22);
        var calendar = new ChineseCalendar(date);

        // Act
        var ganZhiDay = calendar.GanZhiDayString;

        // Assert
        Assert.Equal("庚辰日", ganZhiDay);
    }
}