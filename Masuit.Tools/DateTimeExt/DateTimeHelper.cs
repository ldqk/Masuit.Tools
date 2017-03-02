using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Masuit.Tools.DateTimeExt
{
    /// <summary>
    /// 日期时间帮助类
    /// </summary>
    public static class DateTimeHelper
    {
        private static System.DateTime dt = System.DateTime.Now;

        /// <summary>
        /// 获取某一年有多少周
        /// </summary>
        /// <param name="year">年份</param>
        /// <returns>该年周数</returns>
        public static int GetWeekAmount(int year)
        {
            var end = new System.DateTime(year, 12, 31); //该年最后一天
            var gc = new GregorianCalendar();
            return gc.GetWeekOfYear(end, CalendarWeekRule.FirstDay, DayOfWeek.Monday); //该年星期数
        }

        /// <summary>
        /// 返回年度第几个星期   默认星期日是第一天
        /// </summary>
        /// <param name="date">时间</param>
        /// <returns>第几周</returns>
        public static int WeekOfYear(System.DateTime date)
        {
            System.Globalization.GregorianCalendar gc = new System.Globalization.GregorianCalendar();
            return gc.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
        }

        /// <summary>
        /// 返回年度第几个星期
        /// </summary>
        /// <param name="date">时间</param>
        /// <param name="week">一周的开始日期</param>
        /// <returns>第几周</returns>
        public static int WeekOfYear(System.DateTime date, DayOfWeek week)
        {
            System.Globalization.GregorianCalendar gc = new System.Globalization.GregorianCalendar();
            return gc.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstDay, week);
        }

        /// <summary>
        /// 得到一年中的某周的起始日和截止日
        /// 年 nYear
        /// 周数 nNumWeek
        /// 周始 out dtWeekStart
        /// 周终 out dtWeekeEnd
        /// </summary>
        /// <param name="nYear">年份</param>
        /// <param name="nNumWeek">第几周</param>
        /// <param name="dtWeekStart">开始日期</param>
        /// <param name="dtWeekeEnd">结束日期</param>
        public static void GetWeekTime(int nYear, int nNumWeek, out System.DateTime dtWeekStart, out System.DateTime dtWeekeEnd)
        {
            System.DateTime dt = new System.DateTime(nYear, 1, 1);
            dt += new TimeSpan((nNumWeek - 1) * 7, 0, 0, 0);
            dtWeekStart = dt.AddDays(-(int)dt.DayOfWeek + (int)DayOfWeek.Monday);
            dtWeekeEnd = dt.AddDays((int)DayOfWeek.Saturday - (int)dt.DayOfWeek + 1);
        }

        /// <summary>
        /// 得到一年中的某周的起始日和截止日    周一到周五  工作日
        /// </summary>
        /// <param name="nYear">年份</param>
        /// <param name="nNumWeek">第几周</param>
        /// <param name="dtWeekStart">开始日期</param>
        /// <param name="dtWeekeEnd">结束日期</param>
        public static void GetWeekWorkTime(int nYear, int nNumWeek, out System.DateTime dtWeekStart, out System.DateTime dtWeekeEnd)
        {
            System.DateTime dt = new System.DateTime(nYear, 1, 1);
            dt += new TimeSpan((nNumWeek - 1) * 7, 0, 0, 0);
            dtWeekStart = dt.AddDays(-(int)dt.DayOfWeek + (int)DayOfWeek.Monday);
            dtWeekeEnd = dt.AddDays((int)DayOfWeek.Saturday - (int)dt.DayOfWeek + 1).AddDays(-2);
        }

        #region P/Invoke 设置本地时间

        [DllImport("kernel32.dll")]
        private static extern bool SetLocalTime(ref SYSTEMTIME time);

        [StructLayout(LayoutKind.Sequential)]
        private struct SYSTEMTIME
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
        /// 设置本地计算机时间
        /// </summary>
        /// <param name="dt">DateTime对象</param>
        public static void SetLocalTime(System.DateTime dt)
        {
            SYSTEMTIME st;

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

        #endregion

        #region 获取网络时间

        /// <summary>
        /// 从指定的字符串中获取整数
        /// </summary>
        /// <param name="origin">原始的字符串</param>
        /// <param name="fullMatch">是否完全匹配，若为false，则返回字符串中的第一个整数数字</param>
        /// <returns>整数数字</returns>
        private static int GetInt(string origin, bool fullMatch)
        {
            if (string.IsNullOrEmpty(origin))
            {
                return 0;
            }
            origin = origin.Trim();
            if (!fullMatch)
            {
                string pat = @"-?\d+";
                Regex reg = new Regex(pat);
                origin = reg.Match(origin.Trim()).Value;
            }
            int res = 0;
            int.TryParse(origin, out res);
            return res;
        }
        #endregion

    }
}
