namespace Masuit.Tools.DateTimeExt
{
    /// <summary>
    /// 日期信息
    /// </summary>
    public struct DateInfoStruct
    {
        /// <summary>
        /// 月
        /// </summary>
        public readonly int Month;

        /// <summary>
        /// 日
        /// </summary>
        public readonly int Day;

        /// <summary>
        /// 假期长度
        /// </summary>
        public readonly int Recess;

        /// <summary>
        /// 节假日名
        /// </summary>
        public readonly string HolidayName;

        /// <summary>
        /// 日期信息
        /// </summary>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <param name="recess"></param>
        /// <param name="name"></param>
        public DateInfoStruct(int month, int day, int recess, string name)
        {
            Month = month;
            Day = day;
            Recess = recess;
            HolidayName = name;
        }
    }
}