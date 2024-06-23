using System;
using System.Collections.Generic;

namespace Masuit.Tools.DateTimeExt
{
    /// <summary>
    /// 日期信息
    /// </summary>
    public readonly record struct DateInfoStruct : IEquatable<DateInfoStruct>
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

        /// <summary>指示当前对象是否等于同一类型的另一个对象。</summary>
        /// <param name="other">一个与此对象进行比较的对象。</param>
        /// <returns>如果当前对象等于 <paramref name="other" /> 参数，则为 true；否则为 false。</returns>
        public bool Equals(DateInfoStruct other)
        {
            return Month == other.Month && Day == other.Day && Recess == other.Recess && HolidayName == other.HolidayName;
        }

        /// <summary>返回此实例的哈希代码。</summary>
        /// <returns>一个 32 位带符号整数，它是此实例的哈希代码。</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Month;
                hashCode = (hashCode * 397) ^ Day;
                hashCode = (hashCode * 397) ^ Recess;
                hashCode = (hashCode * 397) ^ (HolidayName != null ? HolidayName.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}