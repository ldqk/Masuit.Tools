using System;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Masuit.Tools.DateTimeExt
{
    /// <summary>
    /// 日历操作
    /// </summary>
    public static class CNCalendar
    {
        /// <summary>
        /// 格式化日期
        /// </summary>
        /// <param name="m">月份</param>
        /// <param name="d">日期</param>
        /// <returns>x月x日</returns>
        private static string FormatDate(int m, int d)
        {
            return $"{m:00}{d:00}";
        }

        /// <summary>
        /// 从嵌入资源中读取文件内容(e.g: xml).
        /// </summary>
        /// <param name="fileWholeName">嵌入资源文件名，包括项目的命名空间.</param>
        /// <returns>资源中的文件内容.</returns>
        public static string ReadFileFromEmbedded(string fileWholeName)
        {
            //文件属性-生成操作-嵌入的资源
            string result;

            using (TextReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(fileWholeName)))
            {
                result = reader.ReadToEnd();
            }
            return result;
        }

        #region 结构、日期对象

        /// <summary>
        /// 结构、日期对象
        /// </summary>
        private struct structDate
        {
            public int year;
            public int month;
            public int day;
            public bool isLeap; //是否闰月
            public int yearCyl; //年干支
            public int monthCyl; //月干支
            public int dayCyl; //日干支
        }

        #endregion

        #region 结构、完整的日期对象

        /// <summary>
        /// 结构、完整的日期对象
        /// </summary>
        public struct StructDateFullInfo
        {
            /// <summary>
            /// 公历年
            /// </summary>
            public int Year;

            /// <summary>
            /// 公历月
            /// </summary>
            public int Month;

            /// <summary>
            /// 公历日
            /// </summary>
            public int Day;

            /// <summary>
            /// 是否闰月
            /// </summary>
            public bool IsLeap; //是否闰月

            /// <summary>
            /// 农历年
            /// </summary>
            public int Cyear; //农历年

            /// <summary>
            /// 农历年名称
            /// </summary>
            public string Scyear; //农历年名称

            /// <summary>
            /// 干支年
            /// </summary>
            public string CyearCyl; //干支年

            /// <summary>
            /// 农历月
            /// </summary>
            public int Cmonth; //农历月

            /// <summary>
            /// 农历月名称
            /// </summary>
            public string Scmonth; //农历月名称

            /// <summary>
            /// 干支月
            /// </summary>
            public string CmonthCyl; //干支月

            /// <summary>
            /// 农历日
            /// </summary>
            public int Cday; //农历日

            /// <summary>
            /// 农历日名称
            /// </summary>
            public string Scday; //农历日名称

            /// <summary>
            /// 干支日
            /// </summary>
            public string CdayCyl; //干支日

            /// <summary>
            /// 农历属象
            /// </summary>
            public string cnAnm;

            /// <summary>
            /// 节气
            /// </summary>
            public string solarterm; //节气

            /// <summary>
            /// 星期几
            /// </summary>
            public string DayInWeek; //星期几

            /// <summary>
            /// 节日
            /// </summary>
            public string Feast; //节日

            /// <summary>
            /// 完整的日期信息
            /// </summary>
            public string Fullinfo; //完整的日期信息

            /// <summary>
            /// 阴历节日
            /// </summary>
            public string cnFtvl;

            /// <summary>
            /// 阳历节日
            /// </summary>
            public string cnFtvs;

            /// <summary>
            /// 系统问候语
            /// </summary>
            public string Info; //系统问候语

            /// <summary>
            /// 主题图片
            /// </summary>
            public string Image; //主题图片

            /// <summary>
            /// 有特别的问候语吗
            /// </summary>
            public bool SayHello; //有特别的问候语吗？
        }

        #endregion

        #region 私有

        #region  农历月份信息

        /// <summary>
        /// 农历月份信息
        /// </summary>
        private static readonly int[] lunarInfo =
        {
            0x04bd8, 0x04ae0, 0x0a570, 0x054d5, 0x0d260, 0x0d950, 0x16554, 0x056a0, 0x09ad0, 0x055d2,
            0x04ae0, 0x0a5b6, 0x0a4d0, 0x0d250, 0x1d255, 0x0b540, 0x0d6a0, 0x0ada2, 0x095b0, 0x14977,
            0x04970, 0x0a4b0, 0x0b4b5, 0x06a50, 0x06d40, 0x1ab54, 0x02b60, 0x09570, 0x052f2, 0x04970,
            0x06566, 0x0d4a0, 0x0ea50, 0x06e95, 0x05ad0, 0x02b60, 0x186e3, 0x092e0, 0x1c8d7, 0x0c950,
            0x0d4a0, 0x1d8a6, 0x0b550, 0x056a0, 0x1a5b4, 0x025d0, 0x092d0, 0x0d2b2, 0x0a950, 0x0b557,
            0x06ca0, 0x0b550, 0x15355, 0x04da0, 0x0a5d0, 0x14573, 0x052d0, 0x0a9a8, 0x0e950, 0x06aa0,
            0x0aea6, 0x0ab50, 0x04b60, 0x0aae4, 0x0a570, 0x05260, 0x0f263, 0x0d950, 0x05b57, 0x056a0,
            0x096d0, 0x04dd5, 0x04ad0, 0x0a4d0, 0x0d4d4, 0x0d250, 0x0d558, 0x0b540, 0x0b5a0, 0x195a6,
            0x095b0, 0x049b0, 0x0a974, 0x0a4b0, 0x0b27a, 0x06a50, 0x06d40, 0x0af46, 0x0ab60, 0x09570,
            0x04af5, 0x04970, 0x064b0, 0x074a3, 0x0ea50, 0x06b58, 0x055c0, 0x0ab60, 0x096d5, 0x092e0,
            0x0c960, 0x0d954, 0x0d4a0, 0x0da50, 0x07552, 0x056a0, 0x0abb7, 0x025d0, 0x092d0, 0x0cab5,
            0x0a950, 0x0b4a0, 0x0baa4, 0x0ad50, 0x055d9, 0x04ba0, 0x0a5b0, 0x15176, 0x052b0, 0x0a930,
            0x07954, 0x06aa0, 0x0ad50, 0x05b52, 0x04b60, 0x0a6e6, 0x0a4e0, 0x0d260, 0x0ea65, 0x0d530,
            0x05aa0, 0x076a3, 0x096d0, 0x04bd7, 0x04ad0, 0x0a4d0, 0x1d0b6, 0x0d250, 0x0d520, 0x0dd45,
            0x0b5a0, 0x056d0, 0x055b2, 0x049b0, 0x0a577, 0x0a4b0, 0x0aa50, 0x1b255, 0x06d20, 0x0ada0
        };

        #endregion

        /// <summary>
        /// 农历月份名字
        /// </summary>
        private static readonly string[] cMonthName = { "", "正月", "二月", "三月", "四月", "五月", "六月", "七月", "八月", "九月", "十月", "十一月", "十二月" };

        //农历日子
        private static readonly string[] nStr1 = { "日", "一", "二", "三", "四", "五", "六", "七", "八", "九", "十" };
        private static readonly string[] nStr2 = { "初", "十", "廿", "卅", "　" };
        //公历月份名称
        private static string[] monthName = { "JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC" };

        /// <summary>
        /// 天干
        /// </summary>
        private static readonly string[] gan = { "甲", "乙", "丙", "丁", "戊", "己", "庚", "辛", "壬", "癸" };

        /// <summary>
        /// 地支
        /// </summary>
        private static readonly string[] zhi = { "子", "丑", "寅", "卯", "辰", "巳", "午", "未", "申", "酉", "戌", "亥" };

        /// <summary>
        /// 生肖
        /// </summary>
        private static readonly string[] animals
            =
            {
                "鼠", "牛", "虎", "兔",
                "龙", "蛇", "马", "羊",
                "猴", "鸡", "狗", "猪"
            };

        /// <summary>
        /// 节气
        /// </summary>
        private static readonly string[] solarTerm
            =
            {
                "小寒", "大寒", "立春", "雨水",
                "惊蛰", "春分", "清明", "谷雨",
                "立夏", "小满", "芒种", "夏至",
                "小暑", "大暑", "立秋", "处暑",
                "白露", "秋分", "寒露", "霜降",
                "立冬", "小雪", "大雪", "冬至"
            };

        /// <summary>
        /// 节气对应数值？
        /// </summary>
        private static readonly int[] solarTermInfo =
        {
            0, 21208, 42467, 63836, 85337, 107014, 128867, 150921, 173149, 195551, 218072
            , 240693, 263343, 285989, 308563, 331033, 353350, 375494, 397447, 419210, 440795
            , 462224, 483532, 504758
        };

        #region 节日信息

        private static readonly string[] lFtv = { "0101农历春节", "0202 龙抬头节", "0115 元宵节", "0505 端午节", "0707 七夕情人节", "0815 中秋节", "0909 重阳节", "1208 腊八节", "1114 李君先生生日", "1224 小年", "0100除夕" };
        /// <summary>
        /// 节假日信息
        /// </summary>
        private static readonly string[] sFtv =
        {
            "0101 新年元旦",
            "0202 世界湿地日",
            "0207 国际声援南非日",
            "0210 国际气象节",
            "0214 情人节",
            "0301 国际海豹日",
            "0303 全国爱耳日",
            "0308 国际妇女节",
            "0312 植树节 孙中山逝世纪念日",
            "0314 国际警察日",
            "0315 国际消费者权益日",
            "0317 中国国医节 国际航海日",
            "0321 世界森林日 消除种族歧视国际日",
            "0321 世界儿歌日",
            "0322 世界水日",
            "0323 世界气象日",
            "0324 世界防治结核病日",
            "0325 全国中小学生安全教育日",
            "0330 巴勒斯坦国土日",
            "0401 愚人节 全国爱国卫生运动月(四月) 税收宣传月(四月)",
            "0407 世界卫生日",
            "0422 世界地球日",
            "0423 世界图书和版权日",
            "0424 亚非新闻工作者日",
            "0501 国际劳动节",
            "0504 中国五四青年节",
            "0505 碘缺乏病防治日",
            "0508 世界红十字日",
            "0512 国际护士节",
            "0515 国际家庭日",
            "0517 世界电信日",
            "0518 国际博物馆日",
            "0520 全国学生营养日",
            "0523 国际牛奶日",
            "0531 世界无烟日",
            "0601 国际儿童节",
            "0605 世界环境日",
            "0606 全国爱眼日",
            "0617 防治荒漠化和干旱日",
            "0623 国际奥林匹克日",
            "0625 全国土地日",
            "0626 国际反毒品日",
            "0701 中国共产党建党日 世界建筑日",
            "0702 国际体育记者日",
            "0707 中国人民抗日战争纪念日",
            "0711 世界人口日",
            "0730 非洲妇女日",
            "0801 中国建军节",
            "0808 中国男子节(爸爸节)",
            "0815 日本正式宣布无条件投降日",
            "0908 国际扫盲日 国际新闻工作者日",
            "0910 教师节",
            "0914 世界清洁地球日",
            "0916 国际臭氧层保护日",
            "0918 九·一八事变纪念日",
            "0920 全国爱牙日",
            "0927 世界旅游日",
            "1001 国庆节 世界音乐日 国际老人节",
            "1001 国际音乐日",
            "1002 国际和平与民主自由斗争日",
            "1004 世界动物日",
            "1008 全国高血压日",
            "1008 世界视觉日",
            "1009 世界邮政日 万国邮联日",
            "1010 辛亥革命纪念日 世界精神卫生日",
            "1013 世界保健日 国际教师节",
            "1014 世界标准日",
            "1015 国际盲人节(白手杖节)",
            "1016 世界粮食日",
            "1017 世界消除贫困日",
            "1022 世界传统医药日",
            "1024 联合国日 世界发展信息日",
            "1031 世界勤俭日",
            "1107 十月社会主义革命纪念日",
            "1108 中国记者日",
            "1109 全国消防安全宣传教育日",
            "1110 世界青年节",
            "1111 国际科学与和平周(本日所属的一周)",
            "1112 孙中山诞辰纪念日",
            "1114 世界糖尿病日",
            "1117 国际大学生节 世界学生节",
            "1121 世界问候日 世界电视日",
            "1129 国际声援巴勒斯坦人民国际日",
            "1201 世界艾滋病日",
            "1203 世界残疾人日",
            "1205 国际经济和社会发展志愿人员日",
            "1208 国际儿童电视日",
            "1209 世界足球日",
            "1210 世界人权日",
            "1212 西安事变纪念日",
            "1213 南京大屠杀(1937年)纪念日！紧记血泪史！",
            "1221 国际篮球日",
            "1224 平安夜",
            "1225 圣诞节",
            "1226 毛主席诞辰",
            "1229 国际生物多样性日"
        };

        #endregion

        #endregion

        #region 私有方法

        /// <summary>
        /// 传回农历y年的总天数
        /// </summary>
        /// <param name="y">公元年</param>
        private static int GetLYearDays(int y)
        {
            int sum = 348;

            for (int i = 0x8000; i > 0x8; i >>= 1)
                sum += (lunarInfo[y - 1900] & i) > 0 ? 1 : 0;

            return sum + GetLeapDays(y);
        }

        /// <summary>
        /// 传回农历y年闰月的天数
        /// </summary>
        /// <param name="y">公元年</param>
        private static int GetLeapDays(int y)
        {
            if (GetLeapMonth(y) > 0)
                return (lunarInfo[y - 1900] & 0x10000) > 0 ? 30 : 29;
            return 0;
        }

        /// <summary>
        /// 传回农历y年闰哪个月 1-12 , 没闰传回 0
        /// </summary>
        /// <param name="y">公元年</param>
        private static int GetLeapMonth(int y)
        {
            return lunarInfo[y - 1900] & 0xf;
        }

        /// <summary>
        /// 传回农历y年m月的总天数
        /// </summary>
        /// <param name="y">公元年</param>
        /// <param name="m">月份</param> 
        private static int GetLMonthDays(int y, int m)
        {
            return (lunarInfo[y - 1900] & (0x10000 >> m)) > 0 ? 30 : 29;
        }

        /// <summary>
        /// 传回农历y年的生肖
        /// </summary>
        /// <param name="y">公元年</param>
        private static string AnimalsYear(int y)
        {
            return animals[(y - 4) % 12];
        }

        /// <summary>
        ///传入月日的offset 传回天干地支, 0=甲子
        /// </summary>
        /// <param name="num">月日的偏差</param>
        private static string Cyclical(int num)
        {
            return gan[num % 10] + zhi[num % 12];
        }

        /// <summary>
        /// 传入offset 传回干支, 0=甲子
        /// </summary>
        /// <param name="y">公元年</param>
        private static string cyclical(int y)
        {
            int num = y - 1900 + 36;
            return Cyclical(num);
        }

        #region 得到农历日期、年月日的天干地址及是否闰月

        #region  返回一个农历日期结构体

        /// <summary>
        /// 返回一个农历日期结构体
        /// </summary>
        /// <param name="date">日期对象</param>
        /// <returns>农历日期结构体</returns>
        private static structDate GetLunar(DateTime date)
        {
            structDate sd;

            int i = 0, leap = 0, temp = 0;
            DateTime baseDate = new DateTime(1900, 1, 31); //基准时间

            int offset = (date - baseDate).Days; //与基准时间相隔天数

            sd.dayCyl = offset + 40;
            sd.monthCyl = 14;

            for (i = 1900; (i < 2050) && (offset > 0); i++)
            {
                temp = GetLYearDays(i);
                offset -= temp;
                sd.monthCyl += 12;
            }
            if (offset < 0)
            {
                offset += temp;
                i--;
                sd.monthCyl -= 12;
            }

            sd.year = i;
            sd.yearCyl = i - 1864;

            //闰哪个月
            leap = GetLeapMonth(i);
            sd.isLeap = false;
            for (i = 1; (i < 13) && (offset > 0); i++)
            {
                //闰月 
                if ((leap > 0) && (i == leap + 1) && (!sd.isLeap))
                {
                    --i;
                    sd.isLeap = true;
                    temp = GetLeapDays(sd.year);
                }
                else
                {
                    temp = GetLMonthDays(sd.year, i);
                }
                //解除闰月 
                if (sd.isLeap && (i == leap + 1))
                    sd.isLeap = false;
                offset -= temp;
                if (!sd.isLeap)
                    sd.monthCyl++;
            }
            if ((offset == 0) && (leap > 0) && (i == leap + 1))
            {
                if (sd.isLeap)
                {
                    sd.isLeap = false;
                }
                else
                {
                    sd.isLeap = true;
                    --i;
                    --sd.monthCyl;
                }
            }

            if (offset < 0)
            {
                offset += temp;
                --i;
                --sd.monthCyl;
            }

            sd.month = i;
            sd.day = offset + 1;

            return sd;
        }

        #endregion

        #region 传出y年m月d日对应的农历 [0].year [1].month [2].day2 [3].yearCyl [4].monCyl [5].dayCyl [6].isLeap

        /// <summary>
        /// 传出y年m月d日对应的农历[0].year [1].month [2].day2 [3].yearCyl [4].monCyl [5].dayCyl [6].isLeap
        /// </summary>
        /// <param name="y">年</param>
        /// <param name="m">月</param>
        /// <param name="d">日</param>
        private static long[] calElement(int y, int m, int d)
        {
            long[] nongDate = new long[7];

            int i = 0, temp = 0, leap = 0;
            DateTime baseDate = new DateTime(1900, 1, 31);

            DateTime objDate = new DateTime(y, m, d);
            TimeSpan ts = objDate - baseDate;

            long offset = (long)ts.TotalDays;
            nongDate[5] = offset + 40;
            nongDate[4] = 14;

            for (i = 1900; (i < 2050) && (offset > 0); i++)
            {
                temp = GetLYearDays(i);
                offset -= temp;
                nongDate[4] += 12;
            }
            if (offset < 0)
            {
                offset += temp;
                i--;
                nongDate[4] -= 12;
            }
            nongDate[0] = i;
            nongDate[3] = i - 1864;
            leap = GetLeapMonth(i); // 闰哪个月
            nongDate[6] = 0;

            for (i = 1; (i < 13) && (offset > 0); i++)
            {
                // 闰月
                if ((leap > 0) && (i == leap + 1) && (nongDate[6] == 0))
                {
                    --i;
                    nongDate[6] = 1;
                    temp = GetLeapDays((int)nongDate[0]);
                }
                else
                {
                    temp = GetLMonthDays((int)nongDate[0], i);
                }

                // 解除闰月
                if ((nongDate[6] == 1) && (i == leap + 1))
                    nongDate[6] = 0;
                offset -= temp;
                if (nongDate[6] == 0)
                    nongDate[4]++;
            }

            if ((offset == 0) && (leap > 0) && (i == leap + 1))
            {
                if (nongDate[6] == 1)
                {
                    nongDate[6] = 0;
                }
                else
                {
                    nongDate[6] = 1;
                    --i;
                    --nongDate[4];
                }
            }

            if (offset < 0)
            {
                offset += temp;
                --i;
                --nongDate[4];
            }
            nongDate[1] = i;
            nongDate[2] = offset + 1;
            return nongDate;
        }

        #endregion

        #endregion

        /// <summary>
        /// 将值转成农历汉字日子
        /// </summary>
        /// <param name="d">农历日</param>
        /// <returns>中文表示</returns>
        private static string GetCDay(int d)
        {
            string s = "";

            switch (d)
            {
                case 10:
                    s = "初十";
                    break;
                case 20:
                    s = "二十";
                    break;
                case 30:
                    s = "三十";
                    break;
                default:
                    s = nStr2[(int)Math.Floor((double)d / 10)];
                    s += nStr1[d % 10];
                    break;
            }
            return s;
        }

        /// <summary>
        ///  某年的第n个节气为几日(从0,即小寒起算)
        ///	n:节气下标
        /// </summary>
        /// <param name="y">年</param>
        /// <param name="n">节气</param>
        /// <returns>日期时间</returns>
        private static DateTime GetSolarTermDay(int y, int n)
        {
            //按分钟起计算
            double minutes = (525948.766245 * (y - 1900)) + solarTermInfo[n - 1];
            //1900年1月6日：小寒 
            DateTime baseDate = new DateTime(1900, 1, 6, 2, 5, 0);
            DateTime veryDate = baseDate.AddMinutes(minutes);
            return veryDate;
            //按毫秒起计算
            //double ms = 31556925974.7 * (y - 1900);
            // double ms1 = solarTermInfo[n];
            // DateTime baseDate = new DateTime(1900, 1, 6, 2, 5, 0);
            // baseDate = baseDate.AddMilliseconds(ms);
            // baseDate = baseDate.AddMinutes(ms1);
        }

        #endregion

        #region 公有方法

        #region 得到精简日期信息（不含节日）

        /// <summary>
        /// 得到精简日期信息（不含节日）
        /// </summary>
        /// <param name="d">待检查的日子</param>
        /// <returns>日期信息</returns>
        public static StructDateFullInfo GetDateTidyInfo(this DateTime d)
        {
            StructDateFullInfo dayinfo = new StructDateFullInfo();
            structDate day = GetLunar(d);

            dayinfo.IsLeap = day.isLeap;

            dayinfo.Year = d.Year;
            dayinfo.Cyear = day.year;
            dayinfo.Scyear = animals[(day.year - 4) % 12];
            dayinfo.CyearCyl = Cyclical(day.yearCyl); //干支年

            dayinfo.Month = d.Month;
            dayinfo.Cmonth = day.month;
            dayinfo.Scmonth = cMonthName[day.month];
            dayinfo.CmonthCyl = Cyclical(day.monthCyl); //干支月

            dayinfo.Day = d.Day;
            dayinfo.Cday = day.day;
            dayinfo.Scday = GetCDay(day.day); //日子
            dayinfo.CdayCyl = Cyclical(day.dayCyl); //干支日

            switch (d.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    dayinfo.DayInWeek = "星期日";
                    break;
                case DayOfWeek.Monday:
                    dayinfo.DayInWeek = "星期一";
                    break;
                case DayOfWeek.Tuesday:
                    dayinfo.DayInWeek = "星期二";
                    break;
                case DayOfWeek.Wednesday:
                    dayinfo.DayInWeek = "星期三";
                    break;
                case DayOfWeek.Thursday:
                    dayinfo.DayInWeek = "星期四";
                    break;
                case DayOfWeek.Friday:
                    dayinfo.DayInWeek = "星期五";
                    break;
                case DayOfWeek.Saturday:
                    dayinfo.DayInWeek = "星期六";
                    break;
                default:
                    dayinfo.DayInWeek = "星期？";
                    break;
            }

            dayinfo.Info = "";
            dayinfo.Feast = "";
            dayinfo.Image = "";
            dayinfo.SayHello = false;

            //节气
            //每个月有两个节气
            int d1 = GetSolarTermDay(d.Year, (d.Month * 2) - 1).Day;
            int d2 = GetSolarTermDay(d.Year, d.Month * 2).Day;
            if (dayinfo.Day == d1)
            {
                if (solarTerm.Length > d.Month * 2 - 2) dayinfo.solarterm = solarTerm[d.Month * 2 - 2];
            }
            else if (dayinfo.Day == d2)
            {
                dayinfo.solarterm = solarTerm[d.Month * 2 - 1];
            }
            else
            {
                dayinfo.solarterm = "";
            }

            dayinfo.Fullinfo = dayinfo.Year + "年" + dayinfo.Month + "月" + dayinfo.Day + "日";
            dayinfo.Fullinfo += " " + dayinfo.DayInWeek;
            dayinfo.Fullinfo += " 农历" + dayinfo.CyearCyl + "（" + dayinfo.Scyear + "）年";
            if (dayinfo.IsLeap)
                dayinfo.Fullinfo += "闰";
            dayinfo.Fullinfo += dayinfo.Scmonth + dayinfo.Scday;
            if (dayinfo.solarterm != "")
                dayinfo.Fullinfo += " " + dayinfo.solarterm;

            return dayinfo;
        }

        #endregion

        #region    得到日期信息

        /// <summary>
        /// 得到日期信息
        /// </summary>
        /// <param name="d">待检查的日子</param>
        /// <returns>日期信息</returns>
        public static StructDateFullInfo GetDateInfo(this DateTime d)
        {
            // xml文件属性-生成操作-嵌入的资源
            string calendarXmlData = ReadFileFromEmbedded("Core.Common" + "CCalendarData.xml");

            StructDateFullInfo dayinfo = new StructDateFullInfo();
            structDate day = GetLunar(d);

            dayinfo.IsLeap = day.isLeap;

            dayinfo.Year = d.Year;
            dayinfo.Cyear = day.year;
            dayinfo.Scyear = animals[(day.year - 4) % 12];
            dayinfo.CyearCyl = Cyclical(day.yearCyl); //干支年

            dayinfo.Month = d.Month;
            dayinfo.Cmonth = day.month;
            dayinfo.Scmonth = cMonthName[day.month];
            dayinfo.CmonthCyl = Cyclical(day.monthCyl); //干支月

            dayinfo.Day = d.Day;
            dayinfo.Cday = day.day;
            dayinfo.Scday = GetCDay(day.day); //日子
            dayinfo.CdayCyl = Cyclical(day.dayCyl); //干支日

            switch (d.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    dayinfo.DayInWeek = "星期日";
                    break;
                case DayOfWeek.Monday:
                    dayinfo.DayInWeek = "星期一";
                    break;
                case DayOfWeek.Tuesday:
                    dayinfo.DayInWeek = "星期二";
                    break;
                case DayOfWeek.Wednesday:
                    dayinfo.DayInWeek = "星期三";
                    break;
                case DayOfWeek.Thursday:
                    dayinfo.DayInWeek = "星期四";
                    break;
                case DayOfWeek.Friday:
                    dayinfo.DayInWeek = "星期五";
                    break;
                case DayOfWeek.Saturday:
                    dayinfo.DayInWeek = "星期六";
                    break;
                default:
                    dayinfo.DayInWeek = "星期？";
                    break;
            }

            //节气
            //每个月有两个节气
            int d1 = GetSolarTermDay(d.Year, d.Month * 2 - 1).Day;
            int d2 = GetSolarTermDay(d.Year, d.Month * 2).Day;
            if (dayinfo.Day == d1)
                dayinfo.solarterm = solarTerm[d.Month * 2 - 2];
            else if (dayinfo.Day == d2)
                dayinfo.solarterm = solarTerm[d.Month * 2 - 1];
            else
                dayinfo.solarterm = "";

            //节日及问候语
            dayinfo.Info = "";
            dayinfo.Feast = "";
            dayinfo.Image = "";
            dayinfo.SayHello = false;
            XmlDocument feastdoc = new XmlDocument();
            feastdoc.LoadXml(calendarXmlData);

            //公历
            XmlNodeList nodeList = feastdoc.SelectNodes("descendant::AD/feast[@day='" + d.ToString("MMdd") + "']");
            foreach (XmlNode root in nodeList)
            {
                dayinfo.Feast += root.Attributes["name"].InnerText + " ";
                if (root.Attributes["sayhello"].InnerText == "yes")
                {
                    //需要显示节日问候语
                    dayinfo.Info = root["hello"].InnerText;
                    //看看是否需要计算周年
                    if (root["startyear"] != null)
                    {
                        int startyear = Convert.ToInt32(root["startyear"].InnerText);
                        dayinfo.Info = dayinfo.Info.Replace("_YEARS_", (d.Year - startyear).ToString());
                    }
                    dayinfo.Image = root["img"].InnerText;
                    dayinfo.SayHello = true;
                }
            }

            //农历
            string smmdd = "";
            smmdd = dayinfo.Cmonth.ToString().Length == 2 ? dayinfo.Cmonth.ToString() : "0" + dayinfo.Cmonth;
            smmdd += dayinfo.Cday.ToString().Length == 2 ? dayinfo.Cday.ToString() : "0" + dayinfo.Cday;
            XmlNode feast = feastdoc.SelectSingleNode("descendant::LUNAR/feast[@day='" + smmdd + "']");
            if (feast != null)
            {
                dayinfo.Feast += feast.Attributes["name"].InnerText;

                if (feast.Attributes["sayhello"].InnerText == "yes")
                {
                    //需要显示节日问候语
                    dayinfo.Info += feast["hello"].InnerText;
                    dayinfo.Image = feast["img"].InnerText;
                    dayinfo.SayHello = true;
                }
            }
            //普通日子或没有庆贺语
            if (dayinfo.Info?.Length == 0)
            {
                feast = feastdoc.SelectSingleNode("descendant::NORMAL/day[@time1<'" + d.ToString("HHmm") + "']");
                if (feast != null)
                {
                    dayinfo.Info = feast["hello"].InnerText;
                    dayinfo.Image = feast["img"].InnerText;
                }
            }

            dayinfo.Fullinfo = dayinfo.Year + "年" + dayinfo.Month + "月" + dayinfo.Day + "日";
            dayinfo.Fullinfo += dayinfo.DayInWeek;
            dayinfo.Fullinfo += " 农历" + dayinfo.CyearCyl + "[" + dayinfo.Scyear + "]年";
            if (dayinfo.IsLeap)
                dayinfo.Fullinfo += "闰";
            dayinfo.Fullinfo += dayinfo.Scmonth + dayinfo.Scday;
            if (dayinfo.solarterm != "")
                dayinfo.Fullinfo += "  " + dayinfo.solarterm;

            return dayinfo;
        }

        #endregion

        /// <summary>
        /// 获取农历
        /// </summary>
        /// <param name="dt">阳历日期</param>
        public static StructDateFullInfo getChinaDate(this DateTime dt)
        {
            StructDateFullInfo cd = new StructDateFullInfo();
            int year = dt.Year;
            int month = dt.Month;
            int date = dt.Day;
            long[] l = calElement(year, month, date);
            cd.Cyear = (int)l[0];
            cd.Cmonth = (int)l[1];
            cd.Cday = (int)l[2];
            cd.Scyear = cyclical(year);
            cd.cnAnm = AnimalsYear(year);
            cd.Scmonth = nStr1[(int)l[1]];
            cd.Scday = GetCDay((int)l[2]);
            string smd = dt.ToString("MMdd");

            string lmd = FormatDate(cd.Cmonth, cd.Cday);
            for (int i = 0; i < solarTerm.Length; i++)
            {
                string s1 = GetSolarTermDay(dt.Year, i).ToString("MMdd");
                if (s1.Equals(dt.ToString("MMdd")))
                {
                    cd.solarterm = solarTerm[i];
                    break;
                }
            }
            foreach (string s in sFtv)
            {
                string s1 = s.Substring(0, 4);
                if (s1.Equals(smd))
                {
                    cd.cnFtvs = s.Substring(4, s.Length - 4);
                    break;
                }
            }
            foreach (string s in lFtv)
            {
                string s1 = s.Substring(0, 4);
                if (s1.Equals(lmd))
                {
                    cd.cnFtvl = s.Substring(4, s.Length - 4);
                    break;
                }
            }
            dt = dt.AddDays(1);
            year = dt.Year;
            month = dt.Month;
            date = dt.Day;
            l = calElement(year, month, date);
            lmd = FormatDate((int)l[1], (int)l[2]);
            if (lmd.Equals("0101")) cd.cnFtvl = "除夕";
            return cd;
        }

        #endregion
    }
}