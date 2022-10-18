namespace Masuit.Tools.Hardware
{
    /// <summary>
    /// 内存条模型
    /// </summary>
    public class RamInfo
    {
        public static RamInfo Local => SystemInfo.GetRamInfo();

        /// <summary>
        /// 可用物理内存
        /// </summary>
        public double MemoryAvailable { get; set; }

        /// <summary>
        /// 物理总内存
        /// </summary>
        public double PhysicalMemory { get; set; }

        /// <summary>
        /// 分页内存总数
        /// </summary>
        public double TotalPageFile { get; set; }

        /// <summary>
        /// 分页内存可用
        /// </summary>
        public double AvailablePageFile { get; set; }

        /// <summary>
        /// 虚拟内存总数
        /// </summary>
        public double TotalVirtual { get; set; }

        /// <summary>
        /// 虚拟内存可用
        /// </summary>
        public double AvailableVirtual { get; set; }

        /// <summary>
        /// 内存使用率
        /// </summary>
        public double MemoryUsage => (1 - MemoryAvailable / PhysicalMemory) * 100;
    }
}