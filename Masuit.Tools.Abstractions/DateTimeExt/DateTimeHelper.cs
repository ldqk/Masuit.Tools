using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Masuit.Tools.DateTimeExt
{
    /// <summary>
    /// 日期操作工具类
    /// </summary>
    public static class DateUtil
    {
        /// <summary>
        /// 获取某一年有多少周
        /// </summary>
        /// <param name="_"></param>
        /// <param name="year">年份</param>
        /// <returns>该年周数</returns>
        public static int GetWeekAmount(this DateTime _, int year)
        {
            var end = new DateTime(year, 12, 31); //该年最后一天
            var gc = new GregorianCalendar();
            return gc.GetWeekOfYear(end, CalendarWeekRule.FirstDay, DayOfWeek.Monday); //该年星期数
        }

        /// <summary>
        /// 返回年度第几个星期   默认星期日是第一天
        /// </summary>
        /// <param name="date">时间</param>
        /// <returns>第几周</returns>
        public static int WeekOfYear(this in DateTime date)
        {
            var gc = new GregorianCalendar();
            return gc.GetWeekOfYear(date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
        }

        /// <summary>
        /// 返回年度第几个星期
        /// </summary>
        /// <param name="date">时间</param>
        /// <param name="week">一周的开始日期</param>
        /// <returns>第几周</returns>
        public static int WeekOfYear(this in DateTime date, DayOfWeek week)
        {
            var gc = new GregorianCalendar();
            return gc.GetWeekOfYear(date, CalendarWeekRule.FirstDay, week);
        }

        /// <summary>
        /// 得到一年中的某周的起始日和截止日
        /// 年 nYear
        /// 周数 nNumWeek
        /// 周始 out dtWeekStart
        /// 周终 out dtWeekeEnd
        /// </summary>
        /// <param name="_"></param>
        /// <param name="nYear">年份</param>
        /// <param name="nNumWeek">第几周</param>
        /// <param name="dtWeekStart">开始日期</param>
        /// <param name="dtWeekeEnd">结束日期</param>
        public static void GetWeekTime(this DateTime _, int nYear, int nNumWeek, out DateTime dtWeekStart, out DateTime dtWeekeEnd)
        {
            var dt = new DateTime(nYear, 1, 1);
            dt += new TimeSpan((nNumWeek - 1) * 7, 0, 0, 0);
            dtWeekStart = dt.AddDays(-(int)dt.DayOfWeek + (int)DayOfWeek.Monday);
            dtWeekeEnd = dt.AddDays((int)DayOfWeek.Saturday - (int)dt.DayOfWeek + 1);
        }

        /// <summary>
        /// 得到一年中的某周的起始日和截止日    周一到周五  工作日
        /// </summary>
        /// <param name="_"></param>
        /// <param name="nYear">年份</param>
        /// <param name="nNumWeek">第几周</param>
        /// <param name="dtWeekStart">开始日期</param>
        /// <param name="dtWeekeEnd">结束日期</param>
        public static void GetWeekWorkTime(this DateTime _, int nYear, int nNumWeek, out DateTime dtWeekStart, out DateTime dtWeekeEnd)
        {
            var dt = new DateTime(nYear, 1, 1);
            dt += new TimeSpan((nNumWeek - 1) * 7, 0, 0, 0);
            dtWeekStart = dt.AddDays(-(int)dt.DayOfWeek + (int)DayOfWeek.Monday);
            dtWeekeEnd = dt.AddDays((int)DayOfWeek.Saturday - (int)dt.DayOfWeek + 1).AddDays(-2);
        }

        #region P/Invoke 设置本地时间

        [DllImport("kernel32.dll")]
        private static extern bool SetLocalTime(ref SystemTime time);

        [StructLayout(LayoutKind.Sequential)]
        private struct SystemTime
        {
            public short year;
            public short month;
            public short dayOfWeek;
            public short day;
            public short hour;
            public short minute;
            public short second;
            public short milliseconds;
        }

        /// <summary>
        /// 设置本地计算机系统时间，仅支持Windows系统
        /// </summary>
        /// <param name="dt">DateTime对象</param>
        public static void SetLocalTime(this in DateTime dt)
        {
            SystemTime st;
            st.year = (short)dt.Year;
            st.month = (short)dt.Month;
            st.dayOfWeek = (short)dt.DayOfWeek;
            st.day = (short)dt.Day;
            st.hour = (short)dt.Hour;
            st.minute = (short)dt.Minute;
            st.second = (short)dt.Second;
            st.milliseconds = (short)dt.Millisecond;
            SetLocalTime(ref st);
        }

        #endregion P/Invoke 设置本地时间

        /// <summary>
        /// 返回相对于当前时间的相对天数
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="relativeday">相对天数</param>
        public static string GetDateTime(this in DateTime dt, int relativeday)
        {
            return dt.AddDays(relativeday).ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// 返回标准时间格式string
        /// </summary>
        public static string GetDateTimeF(this in DateTime dt) => dt.ToString("yyyy-MM-dd HH:mm:ss:fffffff");

        /// <summary>
        /// 获取该时间相对于1970-01-01T00:00:00Z的秒数
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static long GetTotalSeconds(this in DateTime dt) => new DateTimeOffset(dt).UtcDateTime.Ticks / 10_000_000L - 62135596800L;

        /// <summary>
        /// 获取该时间相对于1970-01-01T00:00:00Z的毫秒数
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static long GetTotalMilliseconds(this in DateTime dt) => new DateTimeOffset(dt).UtcDateTime.Ticks / 10000L - 62135596800000L;

        /// <summary>
        /// 获取该时间相对于1970-01-01T00:00:00Z的微秒时间戳
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static long GetTotalMicroseconds(this in DateTime dt) => (new DateTimeOffset(dt).UtcTicks - 621355968000000000) / 10;

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        /// <summary>
        /// 获取该时间相对于1970-01-01T00:00:00Z的纳秒时间戳
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static long GetTotalNanoseconds(this in DateTime dt)
        {
            var ticks = (new DateTimeOffset(dt).UtcTicks - 621355968000000000) * 100;
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                QueryPerformanceCounter(out var timestamp);
                return ticks + timestamp % 100;
            }

            return ticks + Stopwatch.GetTimestamp() % 100;
        }

        /// <summary>
        /// 获取该时间相对于1970-01-01T00:00:00Z的分钟数
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static double GetTotalMinutes(this in DateTime dt) => new DateTimeOffset(dt).Offset.TotalMinutes;

        /// <summary>
        /// 获取该时间相对于1970-01-01T00:00:00Z的小时数
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static double GetTotalHours(this in DateTime dt) => new DateTimeOffset(dt).Offset.TotalHours;

        /// <summary>
        /// 获取该时间相对于1970-01-01T00:00:00Z的天数
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static double GetTotalDays(this in DateTime dt) => new DateTimeOffset(dt).Offset.TotalDays;

        /// <summary>
        /// 返回本年有多少天
        /// </summary>
        /// <param name="_"></param>
        /// <param name="iYear">年份</param>
        /// <returns>本年的天数</returns>
        public static int GetDaysOfYear(this DateTime _, int iYear)
        {
            return IsRuYear(iYear) ? 366 : 365;
        }

        /// <summary>本年有多少天</summary>
        /// <param name="dt">日期</param>
        /// <returns>本天在当年的天数</returns>
        public static int GetDaysOfYear(this in DateTime dt)
        {
            //取得传入参数的年份部分，用来判断是否是闰年
            int n = dt.Year;
            return IsRuYear(n) ? 366 : 365;
        }

        /// <summary>本月有多少天</summary>
        /// <param name="_"></param>
        /// <param name="iYear">年</param>
        /// <param name="month">月</param>
        /// <returns>天数</returns>
        public static int GetDaysOfMonth(this DateTime _, int iYear, int month)
        {
            return month switch
            {
                1 => 31,
                2 => (IsRuYear(iYear) ? 29 : 28),
                3 => 31,
                4 => 30,
                5 => 31,
                6 => 30,
                7 => 31,
                8 => 31,
                9 => 30,
                10 => 31,
                11 => 30,
                12 => 31,
                _ => 0
            };
        }

        /// <summary>本月有多少天</summary>
        /// <param name="dt">日期</param>
        /// <returns>天数</returns>
        public static int GetDaysOfMonth(this in DateTime dt)
        {
            //--利用年月信息，得到当前月的天数信息。
            return dt.Month switch
            {
                1 => 31,
                2 => (IsRuYear(dt.Year) ? 29 : 28),
                3 => 31,
                4 => 30,
                5 => 31,
                6 => 30,
                7 => 31,
                8 => 31,
                9 => 30,
                10 => 31,
                11 => 30,
                12 => 31,
                _ => 0
            };
        }

        /// <summary>返回当前日期的星期名称</summary>
        /// <param name="idt">日期</param>
        /// <returns>星期名称</returns>
        public static string GetWeekNameOfDay(this in DateTime idt)
        {
            return idt.DayOfWeek switch
            {
                DayOfWeek.Monday => "星期一",
                DayOfWeek.Tuesday => "星期二",
                DayOfWeek.Wednesday => "星期三",
                DayOfWeek.Thursday => "星期四",
                DayOfWeek.Friday => "星期五",
                DayOfWeek.Saturday => "星期六",
                DayOfWeek.Sunday => "星期日",
                _ => ""
            };
        }

        /// <summary>判断当前年份是否是闰年，私有函数</summary>
        /// <param name="iYear">年份</param>
        /// <returns>是闰年：True ，不是闰年：False</returns>
        private static bool IsRuYear(int iYear)
        {
            //形式参数为年份
            //例如：2003
            var n = iYear;
            return n % 400 == 0 || n % 4 == 0 && n % 100 != 0;
        }

        /// <summary>
        /// 判断是否为合法日期，必须大于1800年1月1日
        /// </summary>
        /// <param name="strDate">输入日期字符串</param>
        /// <returns>True/False</returns>
        public static bool IsDateTime(this string strDate)
        {
            DateTime.TryParse(strDate, out var result);
            return result.CompareTo(DateTime.Parse("1800-1-1")) > 0;
        }

        /// <summary>
        /// 判断时间是否在区间内
        /// </summary>
        /// <param name="this"></param>
        /// <param name="start">开始</param>
        /// <param name="end">结束</param>
        /// <param name="mode">模式</param>
        /// <returns></returns>
        public static bool In(this in DateTime @this, DateTime start, DateTime end, RangeMode mode = RangeMode.Close)
        {
            return mode switch
            {
                RangeMode.Open => start < @this && end > @this,
                RangeMode.Close => start <= @this && end >= @this,
                RangeMode.OpenClose => start < @this && end >= @this,
                RangeMode.CloseOpen => start <= @this && end > @this,
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
            };
        }

        /// <summary>
        ///  返回每月的第一天和最后一天
        /// </summary>
        /// <param name="_"></param>
        /// <param name="month">月份</param>
        /// <param name="firstDay">第一天</param>
        /// <param name="lastDay">最后一天</param>
        public static void ReturnDateFormat(this DateTime _, int month, out string firstDay, out string lastDay)
        {
            int year = DateTime.Now.Year + month / 12;
            if (month != 12)
            {
                month %= 12;
            }

            switch (month)
            {
                case 1:
                    firstDay = DateTime.Now.ToString($"{year}-0{month}-01");
                    lastDay = DateTime.Now.ToString($"{year}-0{month}-31");
                    break;

                case 2:
                    firstDay = DateTime.Now.ToString(year + "-0" + month + "-01");
                    lastDay = DateTime.IsLeapYear(DateTime.Now.Year) ? DateTime.Now.ToString(year + "-0" + month + "-29") : DateTime.Now.ToString(year + "-0" + month + "-28");
                    break;

                case 3:
                    firstDay = DateTime.Now.ToString(year + "-0" + month + "-01");
                    lastDay = DateTime.Now.ToString("yyyy-0" + month + "-31");
                    break;

                case 4:
                    firstDay = DateTime.Now.ToString(year + "-0" + month + "-01");
                    lastDay = DateTime.Now.ToString(year + "-0" + month + "-30");
                    break;

                case 5:
                    firstDay = DateTime.Now.ToString(year + "-0" + month + "-01");
                    lastDay = DateTime.Now.ToString(year + "-0" + month + "-31");
                    break;

                case 6:
                    firstDay = DateTime.Now.ToString(year + "-0" + month + "-01");
                    lastDay = DateTime.Now.ToString(year + "-0" + month + "-30");
                    break;

                case 7:
                    firstDay = DateTime.Now.ToString(year + "-0" + month + "-01");
                    lastDay = DateTime.Now.ToString(year + "-0" + month + "-31");
                    break;

                case 8:
                    firstDay = DateTime.Now.ToString(year + "-0" + month + "-01");
                    lastDay = DateTime.Now.ToString(year + "-0" + month + "-31");
                    break;

                case 9:
                    firstDay = DateTime.Now.ToString(year + "-0" + month + "-01");
                    lastDay = DateTime.Now.ToString(year + "-0" + month + "-30");
                    break;

                case 10:
                    firstDay = DateTime.Now.ToString(year + "-" + month + "-01");
                    lastDay = DateTime.Now.ToString(year + "-" + month + "-31");
                    break;

                case 11:
                    firstDay = DateTime.Now.ToString(year + "-" + month + "-01");
                    lastDay = DateTime.Now.ToString(year + "-" + month + "-30");
                    break;

                default:
                    firstDay = DateTime.Now.ToString(year + "-" + month + "-01");
                    lastDay = DateTime.Now.ToString(year + "-" + month + "-31");
                    break;
            }
        }

        /// <summary>
        /// 返回某年某月最后一天
        /// </summary>
        /// <param name="_"></param>
        /// <param name="year">年份</param>
        /// <param name="month">月份</param>
        /// <returns>日</returns>
        public static int GetMonthLastDate(this DateTime _, int year, int month)
        {
            DateTime lastDay = new DateTime(year, month, new GregorianCalendar().GetDaysInMonth(year, month));
            int day = lastDay.Day;
            return day;
        }

        /// <summary>
        /// 获得一段时间内有多少小时
        /// </summary>
        /// <param name="dtStar">起始时间</param>
        /// <param name="dtEnd">终止时间</param>
        /// <returns>小时差</returns>
        public static string GetTimeDelay(this in DateTime dtStar, DateTime dtEnd)
        {
            long lTicks = (dtEnd.Ticks - dtStar.Ticks) / 10000000;
            string sTemp = (lTicks / 3600).ToString().PadLeft(2, '0') + ":";
            sTemp += (lTicks % 3600 / 60).ToString().PadLeft(2, '0') + ":";
            sTemp += (lTicks % 3600 % 60).ToString().PadLeft(2, '0');
            return sTemp;
        }

        /// <summary>
        /// 获得8位时间整型数字
        /// </summary>
        /// <param name="dt">当前的日期时间对象</param>
        /// <returns>8位时间整型数字</returns>
        public static string GetDateString(this in DateTime dt)
        {
            return dt.Year + dt.Month.ToString().PadLeft(2, '0') + dt.Day.ToString().PadLeft(2, '0');
        }

        /// <summary>
        /// 返回时间差
        /// </summary>
        /// <param name="dateTime1">时间1</param>
        /// <param name="dateTime2">时间2</param>
        /// <returns>时间差</returns>
        public static string DateDiff(this in DateTime dateTime1, in DateTime dateTime2)
        {
            string dateDiff;
            var ts = dateTime2 - dateTime1;
            if (ts.Days >= 1)
            {
                dateDiff = dateTime1.Month + "月" + dateTime1.Day + "日";
            }
            else
            {
                dateDiff = ts.Hours > 1 ? ts.Hours + "小时前" : ts.Minutes + "分钟前";
            }

            return dateDiff;
        }

        /// <summary>
        /// 计算2个时间差
        /// </summary>
        /// <param name="beginTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns>时间差</returns>
        public static string GetDiffTime(this in DateTime beginTime, in DateTime endTime)
        {
            string strResout = string.Empty;

            //获得2时间的时间间隔秒计算
            TimeSpan span = endTime.Subtract(beginTime);
            int sec = Convert.ToInt32(span.TotalSeconds);
            int minutes = 1 * 60;
            int hours = minutes * 60;
            int day = hours * 24;
            int month = day * 30;
            int year = month * 12;

            //提醒时间,到了返回1,否则返回0
            if (sec > year)
            {
                strResout += sec / year + "年";
                sec %= year; //剩余
            }

            if (sec > month)
            {
                strResout += sec / month + "月";
                sec %= month;
            }

            if (sec > day)
            {
                strResout += sec / day + "天";
                sec %= day;
            }

            if (sec > hours)
            {
                strResout += sec / hours + "小时";
                sec %= hours;
            }

            if (sec > minutes)
            {
                strResout += sec / minutes + "分";
                sec %= minutes;
            }

            strResout += sec + "秒";
            return strResout;
        }
    }

    /// <summary>
    /// 区间模式
    /// </summary>
    public enum RangeMode
    {
        /// <summary>
        /// 开区间
        /// </summary>
        Open,

        /// <summary>
        /// 闭区间
        /// </summary>
        Close,

        /// <summary>
        /// 左开右闭区间
        /// </summary>
        OpenClose,

        /// <summary>
        /// 左闭右开区间
        /// </summary>
        CloseOpen
    }
}
