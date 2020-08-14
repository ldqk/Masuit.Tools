using Masuit.Tools.DateTimeExt;
using System;
using Xunit;

namespace Masuit.Tools.Test
{
    public class ChineseCalendarTest
    {
        [Fact]
        public void GetChineseDateString_ReturnChineseDateString()
        {
            var cc = new ChineseCalendar(DateTime.Parse("2018-11-27"));
            string actual = cc.ChineseDateString;
            Assert.Equal("二零一八年十月二十", actual);
        }

        [Theory, InlineData("2018-11-27"), InlineData("2019-02-02")]
        public void JudgeWorkDay_ReturnTrue(string date)
        {
            ChineseCalendar.CustomWorkDays.Add(DateTime.Parse("2019-02-02"));
            var cc = new ChineseCalendar(DateTime.Parse(date));
            var actual = cc.IsWorkDay;
            Assert.True(actual);
        }

        [Fact]
        public void JudgeHoliday_ReturnTrue()
        {
            ChineseCalendar.CustomHolidays.Add(DateTime.Parse("2019-2-6"), "春节");
            var cc = new ChineseCalendar(DateTime.Parse("2019-2-6"));
            var actual = cc.IsHoliday;
            Assert.True(actual);
        }
    }
}