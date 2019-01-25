using System.Runtime.InteropServices;

namespace Masuit.Tools.Hardware
{

    public static partial class SystemInfo
    {
        #region 定义CPU的信息结构

        /// <summary>
        /// 定义CPU的信息结构
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct CPU_INFO
        {
#pragma warning disable 1591
            public uint dwOemId;
            public uint dwPageSize;
            public uint lpMinimumApplicationAddress;
            public uint lpMaximumApplicationAddress;
            public uint dwActiveProcessorMask;
            public uint dwNumberOfProcessors;
            public uint dwProcessorType;
            public uint dwAllocationGranularity;
            public uint dwProcessorLevel;
            public uint dwProcessorRevision;
#pragma warning restore 1591
        }
#pragma warning restore 1591

        #endregion
    }
}