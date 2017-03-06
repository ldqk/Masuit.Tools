using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Web.Script.Serialization;

namespace Masuit.Tools.Hardware
{
    /// <summary>
    /// 硬件信息
    /// </summary>
    public static class SystemInfo
    {
        private const int GW_HWNDFIRST = 0;
        private const int GW_HWNDNEXT = 2;
        private const int GWL_STYLE = -16;
        private const int WS_VISIBLE = 268435456;
        private const int WS_BORDER = 8388608;
        private static readonly PerformanceCounter pcCpuLoad; //CPU计数器 

        #region 构造函数 

        /// <summary>
        /// 静态构造函数
        /// </summary>
        static SystemInfo()
        {
            //初始化CPU计数器 
            pcCpuLoad = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            pcCpuLoad.MachineName = ".";
            pcCpuLoad.NextValue();

            //CPU个数 
            ProcessorCount = Environment.ProcessorCount;

            //获得物理内存 
            ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                if (mo["TotalPhysicalMemory"] != null)
                {
                    PhysicalMemory = long.Parse(mo["TotalPhysicalMemory"].ToString());
                }
            }
        }

        #endregion

        #region CPU核心 

        /// <summary>
        /// 获取CPU核心数 
        /// </summary>
        public static int ProcessorCount { get; }

        #endregion

        #region CPU占用率 

        /// <summary>
        /// 获取CPU占用率 %
        /// </summary>
        public static float CpuLoad
        {
            get { return pcCpuLoad.NextValue(); }
        }

        #endregion

        #region 可用内存 

        /// <summary>
        /// 获取可用内存
        /// </summary>
        public static long MemoryAvailable
        {
            get
            {
                long availablebytes = 0;
                ManagementClass mos = new ManagementClass("Win32_OperatingSystem");
                foreach (ManagementObject mo in mos.GetInstances())
                {
                    if (mo["FreePhysicalMemory"] != null)
                        availablebytes = 1024 * long.Parse(mo["FreePhysicalMemory"].ToString());
                }

                return availablebytes;
            }
        }

        #endregion

        #region 物理内存 

        /// <summary>
        /// 获取物理内存
        /// </summary>
        public static long PhysicalMemory { get; }

        #endregion

        #region 查找所有应用程序标题 

        /// <summary>
        /// 查找所有应用程序标题 
        /// </summary>
        /// <param name="Handle">应用程序标题范型</param>
        /// <returns>所有应用程序集合</returns>
        public static ArrayList FindAllApps(int Handle)
        {
            ArrayList Apps = new ArrayList();

            int hwCurr = GetWindow(Handle, GW_HWNDFIRST);

            while (hwCurr > 0)
            {
                int IsTask = WS_VISIBLE | WS_BORDER;
                int lngStyle = GetWindowLongA(hwCurr, GWL_STYLE);
                bool TaskWindow = (lngStyle & IsTask) == IsTask;
                if (TaskWindow)
                {
                    int length = GetWindowTextLength(new IntPtr(hwCurr));
                    StringBuilder sb = new StringBuilder(2 * length + 1);
                    GetWindowText(hwCurr, sb, sb.Capacity);
                    string strTitle = sb.ToString();
                    if (!string.IsNullOrEmpty(strTitle))
                        Apps.Add(strTitle);
                }
                hwCurr = GetWindow(hwCurr, GW_HWNDNEXT);
            }

            return Apps;
        }

        #endregion

        #region 获取CPU的数量

        /// <summary>
        /// 获取CPU的数量
        /// </summary>
        /// <returns>CPU的数量</returns>
        public static int GetCpuCount()
        {
            ManagementClass m = new ManagementClass("Win32_Processor");
            ManagementObjectCollection mn = m.GetInstances();
            return mn.Count;
        }

        #endregion

        #region 获取CPU信息

        /// <summary>
        /// 获取CPU信息
        /// </summary>
        /// <returns>CPU信息</returns>
        public static List<CpuInfo> GetCpuInfo()
        {
            List<CpuInfo> list = new List<CpuInfo>();
            Dictionary<string, string> dic = new Dictionary<string, string>();
            ManagementObjectSearcher MySearcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
            foreach (ManagementObject MyObject in MySearcher.Get())
            {
                list.Add(new CpuInfo
                {
                    CpuLoad = CpuLoad,
                    NumberOfLogicalProcessors = ProcessorCount,
                    CurrentClockSpeed = MyObject.Properties["CurrentClockSpeed"].Value.ToString(),
                    Manufacturer = MyObject.Properties["Manufacturer"].Value.ToString(),
                    MaxClockSpeed = MyObject.Properties["MaxClockSpeed"].Value.ToString(),
                    Type = MyObject.Properties["Name"].Value.ToString(),
                    DataWidth = MyObject.Properties["DataWidth"].Value.ToString(),
                    DeviceID = MyObject.Properties["DeviceID"].Value.ToString(),
                    NumberOfCores = Convert.ToInt32(MyObject.Properties["NumberOfCores"].Value),
                    Temperature = GetCPUTemperature()
                });
            }

            return list;
        }

        #endregion

        #region 获取内存信息

        /// <summary>
        /// 获取内存信息
        /// </summary>
        /// <returns>内存信息</returns>
        public static RamInfo GetRamInfo()
        {
            MEMORY_INFO MemInfo = new MEMORY_INFO();
            GlobalMemoryStatus(ref MemInfo);
            return new RamInfo
            {
                MemoryAvailable = MemoryAvailable,
                PhysicalMemory = PhysicalMemory,
                TotalPageFile = MemInfo.dwTotalPageFile,
                AvailablePageFile = MemInfo.dwAvailPageFile,
                AvailableVirtual = MemInfo.dwAvailVirtual,
                TotalVirtual = MemInfo.dwTotalVirtual
            };
        }

        #endregion

        #region 获取CPU温度

        /// <summary>
        /// 获取CPU温度
        /// </summary>
        /// <returns>CPU温度</returns>
        public static double GetCPUTemperature()
        {
            string str = "";

            ManagementObjectSearcher vManagementObjectSearcher = new ManagementObjectSearcher(@"root\WMI", @"select * from MSAcpi_ThermalZoneTemperature");

            foreach (ManagementObject managementObject in vManagementObjectSearcher.Get())
            {
                string s = new JavaScriptSerializer().Serialize(managementObject.Properties);

                str += managementObject.Properties["CurrentTemperature"].Value.ToString();
            }

            //这就是CPU的温度了
            double temp = (double.Parse(str) - 2732) / 10;
            return Math.Round(temp, 2);
        }

        #endregion

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

        #endregion

        #region 定义内存的信息结构

        /// <summary>
        /// 定义内存的信息结构
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORY_INFO
        {
#pragma warning disable 1591
            public uint dwLength;
            public uint dwMemoryLoad;
            public uint dwTotalPhys;
            public uint dwAvailPhys;
            public uint dwTotalPageFile;
            public uint dwAvailPageFile;
            public uint dwTotalVirtual;
            public uint dwAvailVirtual;
#pragma warning restore 1591
        }

        #endregion

        #region 定义系统时间的信息结构

        /// <summary>
        /// 定义系统时间的信息结构
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEMTIME_INFO
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

        #endregion

        #region Win32API声明 

#pragma warning disable 1591
        [DllImport("kernel32")]
        public static extern void GetWindowsDirectory(StringBuilder WinDir, int count);

        [DllImport("kernel32")]
        public static extern void GetSystemDirectory(StringBuilder SysDir, int count);

        [DllImport("kernel32")]
        public static extern void GetSystemInfo(ref CPU_INFO cpuinfo);

        [DllImport("kernel32")]
        public static extern void GlobalMemoryStatus(ref MEMORY_INFO meminfo);

        [DllImport("kernel32")]
        public static extern void GetSystemTime(ref SYSTEMTIME_INFO stinfo);

        [DllImport("IpHlpApi.dll")]
        public static extern uint GetIfTable(byte[] pIfTable, ref uint pdwSize, bool bOrder);

        [DllImport("User32")]
        // ReSharper disable once MissingXmlDoc
        public static extern int GetWindow(int hWnd, int wCmd);

        [DllImport("User32")]
        public static extern int GetWindowLongA(int hWnd, int wIndx);

        [DllImport("user32.dll")]
        public static extern bool GetWindowText(int hWnd, StringBuilder title, int maxBufSize);

        [DllImport("user32", CharSet = CharSet.Auto)]
        public static extern int GetWindowTextLength(IntPtr hWnd);
#pragma warning restore 1591

        #endregion



        #region CPU

        /// <summary>
        /// CPU模型
        /// </summary>
        public class CpuInfo
        {
            /// <summary>
            /// 设备ID端口
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
            /// CPU使用率
            /// </summary>
            public double CpuLoad { get; set; }

            /// <summary>
            /// CPU位宽
            /// </summary>
            public string DataWidth { get; set; }

            /// <summary>
            /// 核心温度
            /// </summary>
            public double Temperature { get; set; }
        }

        #endregion

        #region RAM

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

        #endregion
    }
}