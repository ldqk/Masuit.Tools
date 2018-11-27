namespace Masuit.Tools.DateTimeExt
{
    public struct WeekHolidayStruct
    {
        public readonly int Month;
        public readonly int WeekAtMonth;
        public readonly int WeekDay;
        public readonly string HolidayName;

        public WeekHolidayStruct(int month, int weekAtMonth, int weekDay, string name)
        {
            Month = month;
            WeekAtMonth = weekAtMonth;
            WeekDay = weekDay;
            HolidayName = name;
        }
    }
}