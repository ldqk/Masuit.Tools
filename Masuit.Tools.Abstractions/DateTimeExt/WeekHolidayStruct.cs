using System;

namespace Masuit.Tools.DateTimeExt
{
    /// <summary>
    /// 节假日信息
    /// </summary>
    public readonly record struct WeekHolidayStruct : IEquatable<WeekHolidayStruct>
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

        /// <summary>指示当前对象是否等于同一类型的另一个对象。</summary>
        /// <param name="other">一个与此对象进行比较的对象。</param>
        /// <returns>如果当前对象等于 <paramref name="other" /> 参数，则为 true；否则为 false。</returns>
        public bool Equals(WeekHolidayStruct other)
        {
            return Month == other.Month && WeekAtMonth == other.WeekAtMonth && WeekDay == other.WeekDay && HolidayName == other.HolidayName;
        }

        /// <summary>返回此实例的哈希代码。</summary>
        /// <returns>一个 32 位带符号整数，它是此实例的哈希代码。</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Month;
                hashCode = (hashCode * 397) ^ WeekAtMonth;
                hashCode = (hashCode * 397) ^ WeekDay;
                hashCode = (hashCode * 397) ^ (HolidayName != null ? HolidayName.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}