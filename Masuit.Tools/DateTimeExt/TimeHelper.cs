using System;

namespace Masuit.Tools.DateTimeExt
{
    /// <summary>
    /// 时间相关操作帮助类
    /// </summary>
    public static class TimeHelper
    {
        #region 时间相关操作类

        /// <summary>
        /// 获得一段时间内有多少小时
        /// </summary>
        /// <param name="dtStar">起始时间</param>
        /// <param name="dtEnd">终止时间</param>
        /// <returns>小时差</returns>
        public static string GetTimeDelay(this System.DateTime dtStar, System.DateTime dtEnd)
        {
            long lTicks = (dtEnd.Ticks - dtStar.Ticks) / 10000000;
            string sTemp = (lTicks / 3600).ToString().PadLeft(2, '0') + ":";
            sTemp += ((lTicks % 3600) / 60).ToString().PadLeft(2, '0') + ":";
            sTemp += ((lTicks % 3600) % 60).ToString().PadLeft(2, '0');
            return sTemp;
        }
        /// <summary>
        /// 获得8位时间整型数字
        /// </summary>
        /// <param name="dt">当前的日期时间对象</param>
        /// <returns>8位时间整型数字</returns>
        public static string GetDateString(this System.DateTime dt)
        {
            return dt.Year.ToString() + dt.Month.ToString().PadLeft(2, '0') + dt.Day.ToString().PadLeft(2, '0');
        }
        #endregion

        #region 返回每月的第一天和最后一天

        /// <summary>
        ///  返回每月的第一天和最后一天
        /// </summary>
        /// <param name="_"></param>
        /// <param name="month">月份</param>
        /// <param name="firstDay">第一天</param>
        /// <param name="lastDay">最后一天</param>
        public static void ReturnDateFormat(this DateTime _, int month, out string firstDay, out string lastDay)
        {
            int year = DateTime.Now.Year + (month / 12);
            if (month != 12)
            {
                month %= 12;
            }
            switch (month)
            {
                case 1:
                    firstDay = DateTime.Now.ToString($"{year}-0{month}-01");
                    lastDay = System.DateTime.Now.ToString($"{year}-0{month}-31");
                    break;
                case 2:
                    firstDay = System.DateTime.Now.ToString(year + "-0" + month + "-01");
                    if (System.DateTime.IsLeapYear(System.DateTime.Now.Year))
                        lastDay = System.DateTime.Now.ToString(year + "-0" + month + "-29");
                    else
                        lastDay = System.DateTime.Now.ToString(year + "-0" + month + "-28");
                    break;
                case 3:
                    firstDay = System.DateTime.Now.ToString(year + "-0" + month + "-01");
                    lastDay = System.DateTime.Now.ToString("yyyy-0" + month + "-31");
                    break;
                case 4:
                    firstDay = System.DateTime.Now.ToString(year + "-0" + month + "-01");
                    lastDay = System.DateTime.Now.ToString(year + "-0" + month + "-30");
                    break;
                case 5:
                    firstDay = System.DateTime.Now.ToString(year + "-0" + month + "-01");
                    lastDay = System.DateTime.Now.ToString(year + "-0" + month + "-31");
                    break;
                case 6:
                    firstDay = System.DateTime.Now.ToString(year + "-0" + month + "-01");
                    lastDay = System.DateTime.Now.ToString(year + "-0" + month + "-30");
                    break;
                case 7:
                    firstDay = System.DateTime.Now.ToString(year + "-0" + month + "-01");
                    lastDay = System.DateTime.Now.ToString(year + "-0" + month + "-31");
                    break;
                case 8:
                    firstDay = System.DateTime.Now.ToString(year + "-0" + month + "-01");
                    lastDay = System.DateTime.Now.ToString(year + "-0" + month + "-31");
                    break;
                case 9:
                    firstDay = System.DateTime.Now.ToString(year + "-0" + month + "-01");
                    lastDay = System.DateTime.Now.ToString(year + "-0" + month + "-30");
                    break;
                case 10:
                    firstDay = System.DateTime.Now.ToString(year + "-" + month + "-01");
                    lastDay = System.DateTime.Now.ToString(year + "-" + month + "-31");
                    break;
                case 11:
                    firstDay = System.DateTime.Now.ToString(year + "-" + month + "-01");
                    lastDay = System.DateTime.Now.ToString(year + "-" + month + "-30");
                    break;
                default:
                    firstDay = DateTime.Now.ToString(year + "-" + month + "-01");
                    lastDay = System.DateTime.Now.ToString(year + "-" + month + "-31");
                    break;
            }
        }
        #endregion

        #region  将时间格式化成 年月日 的形式,如果时间为null，返回当前系统时间

        /// <summary>
        /// 将时间格式化成 年月日 的形式,如果时间为null，返回当前系统时间
        /// </summary>
        /// <param name="dt">年月日分隔符</param>
        /// <param name="Separator">分隔符</param>
        /// <returns>xxxx年xx月xx日</returns>
        public static string GetFormatDate(this System.DateTime dt, char Separator)
        {
            if (dt != null && !dt.Equals(DBNull.Value))
            {
                string tem = string.Format("yyyy{0}MM{1}dd", Separator, Separator);
                return dt.ToString(tem);
            }
            else
            {
                return GetFormatDate(System.DateTime.Now, Separator);
            }
        }
        #endregion

        #region 将时间格式化成 时分秒 的形式,如果时间为null，返回当前系统时间
        /// <summary>
        /// 将时间格式化成 时分秒 的形式,如果时间为null，返回当前系统时间
        /// </summary>
        /// <param name="dt">当前日期时间对象</param>
        /// <param name="Separator">分隔符</param>
        /// <returns> xx时xx分xx秒 </returns>
        public static string GetFormatTime(this System.DateTime dt, char Separator)
        {
            if (dt != null && !dt.Equals(DBNull.Value))
            {
                string tem = string.Format("hh{0}mm{1}ss", Separator, Separator);
                return dt.ToString(tem);
            }
            else
            {
                return GetFormatDate(System.DateTime.Now, Separator);
            }
        }
        #endregion

        #region  把秒转换成分钟

        /// <summary>
        /// 把秒转换成分钟
        /// </summary>
        /// <param name="_"></param>
        /// <param name="Second">秒数</param>
        /// <returns>分钟数</returns>
        public static int SecondToMinute(this DateTime _, int Second)
        {
            decimal mm = Second / (decimal)60;
            return Convert.ToInt32(Math.Ceiling(mm));
        }
        #endregion

        #region 返回某年某月最后一天

        /// <summary>
        /// 返回某年某月最后一天
        /// </summary>
        /// <param name="_"></param>
        /// <param name="year">年份</param>
        /// <param name="month">月份</param>
        /// <returns>日</returns>
        public static int GetMonthLastDate(this DateTime _, int year, int month)
        {
            System.DateTime lastDay = new System.DateTime(year, month, new System.Globalization.GregorianCalendar().GetDaysInMonth(year, month));
            int Day = lastDay.Day;
            return Day;
        }
        #endregion

        #region 返回时间差
        /// <summary>
        /// 返回时间差
        /// </summary>
        /// <param name="dateTime1">时间1</param>
        /// <param name="dateTime2">时间2</param>
        /// <returns>时间差</returns>
        public static string DateDiff(this DateTime dateTime1, DateTime dateTime2)
        {
            string dateDiff = null;
            try
            {
                TimeSpan ts = dateTime2 - dateTime1;
                if (ts.Days >= 1)
                {
                    dateDiff = dateTime1.Month + "月" + dateTime1.Day + "日";
                }
                else
                {
                    if (ts.Hours > 1)
                    {
                        dateDiff = ts.Hours + "小时前";
                    }
                    else
                    {
                        dateDiff = ts.Minutes + "分钟前";
                    }
                }
            }
            catch
            {
            }
            return dateDiff;
        }
        /// <summary>
        /// 时间差
        /// </summary>
        /// <param name="beginTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns>时间差</returns>
        public static string GetDiffTime(this DateTime beginTime, DateTime endTime)
        {
            int i = 0;
            return GetDiffTime(beginTime, endTime, ref i);
        }

        /// <summary>
        /// 计算2个时间差
        /// </summary>
        /// <param name="beginTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="mindTime">中间的时间</param>
        /// <returns>时间差</returns>
        public static string GetDiffTime(this DateTime beginTime, System.DateTime endTime, ref int mindTime)
        {
            string strResout = string.Empty;
            //获得2时间的时间间隔秒计算
            //TimeSpan span = endTime - beginTime;
            TimeSpan span = endTime.Subtract(beginTime);

            int iTatol = Convert.ToInt32(span.TotalSeconds);
            int iMinutes = 1 * 60;
            int iHours = iMinutes * 60;
            int iDay = iHours * 24;
            int iMonth = iDay * 30;
            int iYear = iMonth * 12;

            //提醒时间,到了返回1,否则返回0
            if (mindTime > iTatol && iTatol > 0)
            {
                mindTime = 1;
            }
            else
            {
                mindTime = 0;
            }

            if (iTatol > iYear)
            {
                strResout += (iTatol / iYear) + "年";
                iTatol %= iYear;//剩余
            }
            if (iTatol > iMonth)
            {
                strResout += iTatol / iMonth + "月";
                iTatol %= iMonth;
            }
            if (iTatol > iDay)
            {
                strResout += iTatol / iDay + "天";
                iTatol %= iDay;
            }
            if (iTatol > iHours)
            {
                strResout += iTatol / iHours + "小时";
                iTatol %= iHours;
            }
            if (iTatol > iMinutes)
            {
                strResout += iTatol / iMinutes + "分";
                iTatol %= iMinutes;
            }
            strResout += iTatol + "秒";

            return strResout;
        }

        #endregion

        #region 获得两个日期的间隔
        /// <summary>
        /// 获得两个日期的间隔
        /// </summary>
        /// <param name="DateTime1">日期一。</param>
        /// <param name="DateTime2">日期二。</param>
        /// <returns>日期间隔TimeSpan。</returns>
        public static TimeSpan DateDiff2(this System.DateTime DateTime1, System.DateTime DateTime2)
        {
            TimeSpan ts1 = new TimeSpan(DateTime1.Ticks);
            TimeSpan ts2 = new TimeSpan(DateTime2.Ticks);
            TimeSpan ts = ts1.Subtract(ts2).Duration();
            return ts;
        }
        #endregion

        #region 格式化日期时间
        /// <summary>
        /// 格式化日期时间
        /// </summary>
        /// <param name="dateTime1">日期时间</param>
        /// <param name="dateMode">显示模式</param>
        /// <returns>0-9种模式的日期</returns>
        public static string FormatDate(this System.DateTime dateTime1, string dateMode)
        {
            switch (dateMode)
            {
                case "0":
                    return dateTime1.ToString("yyyy-MM-dd");
                case "1":
                    return dateTime1.ToString("yyyy-MM-dd HH:mm:ss");
                case "2":
                    return dateTime1.ToString("yyyy/MM/dd");
                case "3":
                    return dateTime1.ToString("yyyy年MM月dd日");
                case "4":
                    return dateTime1.ToString("MM-dd");
                case "5":
                    return dateTime1.ToString("MM/dd");
                case "6":
                    return dateTime1.ToString("MM月dd日");
                case "7":
                    return dateTime1.ToString("yyyy-MM");
                case "8":
                    return dateTime1.ToString("yyyy/MM");
                case "9":
                    return dateTime1.ToString("yyyy年MM月");
                default:
                    return dateTime1.ToString();
            }
        }
        #endregion

        #region 得到随机日期
        /// <summary>
        /// 得到随机日期
        /// </summary>
        /// <param name="time1">起始日期</param>
        /// <param name="time2">结束日期</param>
        /// <returns>间隔日期之间的 随机日期</returns>
        public static System.DateTime GetRandomTime(this System.DateTime time1, System.DateTime time2)
        {
            Random random = new Random();
            System.DateTime minTime = new System.DateTime();
            System.DateTime maxTime = new System.DateTime();

            System.TimeSpan ts = new System.TimeSpan(time1.Ticks - time2.Ticks);

            // 获取两个时间相隔的秒数
            double dTotalSecontds = ts.TotalSeconds;
            int iTotalSecontds = 0;

            if (dTotalSecontds > System.Int32.MaxValue)
            {
                iTotalSecontds = System.Int32.MaxValue;
            }
            else if (dTotalSecontds < System.Int32.MinValue)
            {
                iTotalSecontds = System.Int32.MinValue;
            }
            else
            {
                iTotalSecontds = (int)dTotalSecontds;
            }
            if (iTotalSecontds > 0)
            {
                minTime = time2;
                maxTime = time1;
            }
            else if (iTotalSecontds < 0)
            {
                minTime = time1;
                maxTime = time2;
            }
            else
            {
                return time1;
            }

            int maxValue = iTotalSecontds;

            if (iTotalSecontds <= System.Int32.MinValue)
                maxValue = System.Int32.MinValue + 1;

            int i = random.Next(System.Math.Abs(maxValue));

            return minTime.AddSeconds(i);
        }
        #endregion

        #region 时间其他转换静态方法
        /// <summary>
        /// C#的时间到Javascript的时间的转换
        /// </summary>
        /// <param name="TheDate">C#的时间</param>
        /// <returns>Javascript的时间</returns>
        public static long CsharpTime2JavascriptTime(this System.DateTime TheDate)
        {
            //string time = (System.DateTime.Now.Subtract(Convert.ToDateTime("1970-01-01 8:0:0"))).TotalMilliseconds.ToString();
            //long d = MilliTimeStamp(DateTime.Now);

            System.DateTime d1 = new System.DateTime(1970, 1, 1);
            System.DateTime d2 = TheDate.ToUniversalTime();
            TimeSpan ts = new TimeSpan(d2.Ticks - d1.Ticks);

            return (long)ts.TotalMilliseconds;
        }

        /// <summary>
        /// PHP的时间转换成C#中的DateTime
        /// </summary>
        /// <param name="_"></param>
        /// <param name="time">php的时间</param>
        /// <returns>C#的时间</returns>
        public static System.DateTime PhpTime2CsharpTime(this DateTime _, long time)
        {
            System.DateTime timeStamp = new System.DateTime(1970, 1, 1);  //得到1970年的时间戳
            long t = (time + 8 * 60 * 60) * 10000000 + timeStamp.Ticks;
            System.DateTime dt = new System.DateTime(t);
            return dt;
        }

        /// <summary>
        ///  C#中的DateTime转换成PHP的时间
        /// </summary>
        /// <param name="time">C#时间</param>
        /// <returns>php时间</returns>
        public static long CsharpTime2PhpTime(this System.DateTime time)
        {
            System.DateTime timeStamp = new System.DateTime(1970, 1, 1);  //得到1970年的时间戳
            long a = (System.DateTime.UtcNow.Ticks - timeStamp.Ticks) / 10000000;  //注意这里有时区问题，用now就要减掉8个小时
            return a;
        }
        #endregion

        #region Rss日期时间转换，将时间全部转换为GMT时间
        /// <summary> 
        /// Rss日期时间转换，将时间全部转换为GMT时间 
        /// </summary> 
        /// <param name="strDateTime">Rss中读取的时间</param> 
        /// <returns>处理后的标准时间格式</returns> 
        public static string DateConvert(this string strDateTime)
        {
            strDateTime = strDateTime.Replace("+0000", "GMT");
            strDateTime = strDateTime.Replace("+0100", "GMT");
            strDateTime = strDateTime.Replace("+0200", "GMT");
            strDateTime = strDateTime.Replace("+0300", "GMT");
            strDateTime = strDateTime.Replace("+0400", "GMT");
            strDateTime = strDateTime.Replace("+0500", "GMT");
            strDateTime = strDateTime.Replace("+0600", "GMT");
            strDateTime = strDateTime.Replace("+0700", "GMT");
            strDateTime = strDateTime.Replace("+0800", "GMT");
            strDateTime = strDateTime.Replace("-0000", "GMT");
            strDateTime = strDateTime.Replace("-0100", "GMT");
            strDateTime = strDateTime.Replace("-0200", "GMT");
            strDateTime = strDateTime.Replace("-0300", "GMT");
            strDateTime = strDateTime.Replace("-0400", "GMT");
            strDateTime = strDateTime.Replace("-0500", "GMT");
            strDateTime = strDateTime.Replace("-0600", "GMT");
            strDateTime = strDateTime.Replace("-0700", "GMT");
            strDateTime = strDateTime.Replace("-0800", "GMT");
            System.DateTime dt = System.DateTime.Parse(strDateTime, null, System.Globalization.DateTimeStyles.AdjustToUniversal);
            return dt.ToString();
        }
        #endregion

    }
}
