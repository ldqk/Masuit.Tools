namespace Masuit.Tools.Hardware
{
    /// <summary>
    /// 内存条模型
    /// </summary>
    public class RamInfo
    {
#pragma warning disable 1591
        public double MemoryAvailable { get; set; }
        public double PhysicalMemory { get; set; }
        public double TotalPageFile { get; set; }
        public double AvailablePageFile { get; set; }
        public double TotalVirtual { get; set; }
        public double AvailableVirtual { get; set; }

        public double MemoryUsage
        {
            get { return (1 - MemoryAvailable / PhysicalMemory) * 100; }
        }
#pragma warning restore 1591
    }
}