namespace Masuit.Tools.DateTimeExt
{
    public struct DateInfoStruct
    {
        public readonly int Month;
        public readonly int Day;
        public readonly int Recess; //假期长度
        public readonly string HolidayName;

        public DateInfoStruct(int month, int day, int recess, string name)
        {
            Month = month;
            Day = day;
            Recess = recess;
            HolidayName = name;
        }
    }
}