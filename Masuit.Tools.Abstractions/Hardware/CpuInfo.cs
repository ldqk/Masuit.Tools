using System.Collections.Generic;

namespace Masuit.Tools.Hardware
{
    /// <summary>
    /// CPU模型
    /// </summary>
    public class CpuInfo
    {
        private static readonly List<CpuInfo> _locals = SystemInfo.GetCpuInfo();

        /// <summary>
        /// 本地实例
        /// </summary>
        public static List<CpuInfo> Locals => _locals;

        /// <summary>
        /// 获取CPU核心数
        /// </summary>
        public static int ProcessorCount => SystemInfo.ProcessorCount;

        /// <summary>
        /// 获取CPU占用率 %
        /// </summary>
        public static float CpuLoad => SystemInfo.CpuLoad;

        /// <summary>
        /// 设备ID
        /// </summary>
        public string DeviceID { get; set; }

        /// <summary>
        /// CPU型号 
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// CPU厂商
        /// </summary>
        public string Manufacturer { get; set; }

        /// <summary>
        /// CPU最大睿频
        /// </summary>
        public string MaxClockSpeed { get; set; }

        /// <summary>
        /// CPU的时钟频率
        /// </summary>
        public string CurrentClockSpeed { get; set; }

        /// <summary>
        /// CPU核心数
        /// </summary>
        public int NumberOfCores { get; set; }

        /// <summary>
        /// 逻辑处理器核心数
        /// </summary>
        public int NumberOfLogicalProcessors { get; set; }

        /// <summary>
        /// CPU位宽
        /// </summary>
        public string DataWidth { get; set; }

        /// <summary>
        /// 核心温度
        /// </summary>
        public double Temperature => SystemInfo.GetCPUTemperature();

        /// <summary>
        /// 序列号
        /// </summary>
        public string SerialNumber { get; set; }
    }
}