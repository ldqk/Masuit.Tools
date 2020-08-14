namespace Masuit.Tools.DateTimeExt
{
    /// <summary>
    /// 节假日信息
    /// </summary>
    public struct WeekHolidayStruct
    {
        /// <summary>
        /// 月
        /// </summary>
        public readonly int Month;

        /// <summary>
        /// 这个月第几周
        /// </summary>
        public readonly int WeekAtMonth;

        /// <summary>
        /// 周末日
        /// </summary>
        public readonly int WeekDay;

        /// <summary>
        /// 假日名
        /// </summary>
        public readonly string HolidayName;

        /// <summary>
        /// 节假日信息
        /// </summary>
        /// <param name="month"></param>
        /// <param name="weekAtMonth"></param>
        /// <param name="weekDay"></param>
        /// <param name="name"></param>
        public WeekHolidayStruct(int month, int weekAtMonth, int weekDay, string name)
        {
            Month = month;
            WeekAtMonth = weekAtMonth;
            WeekDay = weekDay;
            HolidayName = name;
        }
    }
}