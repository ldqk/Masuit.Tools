using System.Runtime.InteropServices;

namespace Masuit.Tools.Hardware
{
    /// <summary>
    /// 定义系统时间的信息结构
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SystemtimeInfo
    {
#pragma warning disable 1591
        public ushort wYear;
        public ushort wMonth;
        public ushort wDayOfWeek;
        public ushort wDay;
        public ushort wHour;
        public ushort wMinute;
        public ushort wSecond;
        public ushort wMilliseconds;
#pragma warning restore 1591
    }
}