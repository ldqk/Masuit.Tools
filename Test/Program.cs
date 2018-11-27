using Masuit.Tools.DateTimeExt;
using System;

namespace Test
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            DateTime dt = DateTime.Parse("2019-2-6");
            ChineseCalendar cc = new ChineseCalendar(dt);
            Console.WriteLine("阳历：" + cc.DateString);
            Console.WriteLine("属相：" + cc.AnimalString);
            Console.WriteLine("农历：" + cc.ChineseDateString);
            Console.WriteLine("时辰：" + cc.ChineseHour);
            Console.WriteLine("节气：" + cc.ChineseTwentyFourDay);
            Console.WriteLine("节日：" + cc.DateHoliday);
            Console.WriteLine("农历节日：" + cc.ChineseCalendarHoliday);
            Console.WriteLine("前一个节气：" + cc.ChineseTwentyFourPrevDay);
            Console.WriteLine("后一个节气：" + cc.ChineseTwentyFourNextDay);
            Console.WriteLine("干支：" + cc.GanZhiDateString);
            Console.WriteLine("星期：" + cc.WeekDayStr);
            Console.WriteLine("星宿：" + cc.ChineseConstellation);
            Console.WriteLine("星座：" + cc.Constellation);
            Console.WriteLine("是否是假期：" + cc.IsHoliday);
            Console.WriteLine("是否是工作日：" + cc.IsWorkDay);

            //DateTime today = DateTime.Parse("2019-1-31");
            //ChineseCalendar.CustomWorkDays.Add(DateTime.Parse("2019-02-02"));
            //ChineseCalendar.CustomWorkDays.Add(DateTime.Parse("2019-02-03"));
            //var cc = new ChineseCalendar(today);
            //var ccEnd = cc.AddWorkDays(4);
            //var endDate = ccEnd.Date;
            //Console.WriteLine(endDate);
        }
    }

}