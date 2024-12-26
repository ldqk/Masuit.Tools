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
}