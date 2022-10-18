using Masuit.Tools.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Masuit.Tools.Hardware
{
    /// <summary>
    /// 硬件信息，部分功能需要C++支持，仅支持Windows系统
    /// </summary>
    public static partial class SystemInfo
    {
        #region 字段

        private const int GwHwndfirst = 0;
        private const int GwHwndnext = 2;
        private const int GwlStyle = -16;
        private const int WsVisible = 268435456;
        private const int WsBorder = 8388608;
        private static readonly PerformanceCounter PcCpuLoad; //CPU计数器

        private static readonly PerformanceCounter MemoryCounter = new PerformanceCounter();
        private static readonly PerformanceCounter CpuCounter = new PerformanceCounter();
        private static readonly PerformanceCounter DiskReadCounter = new PerformanceCounter();
        private static readonly PerformanceCounter DiskWriteCounter = new PerformanceCounter();

        private static readonly string[] InstanceNames;
        private static readonly PerformanceCounter[] NetRecvCounters;
        private static readonly PerformanceCounter[] NetSentCounters;
        private static readonly Dictionary<string, dynamic> _cache = new();

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// 静态构造函数
        /// </summary>
        static SystemInfo()
        {
            //初始化CPU计数器
            PcCpuLoad = new PerformanceCounter("Processor", "% Processor Time", "_Total")
            {
                MachineName = "."
            };
            PcCpuLoad.NextValue();

            //CPU个数
            ProcessorCount = Environment.ProcessorCount;

            //获得物理内存
            try
            {
                using var mc = new ManagementClass("Win32_ComputerSystem");
                using var moc = mc.GetInstances();
                foreach (var mo in moc)
                {
                    using (mo)
                    {
                        if (mo["TotalPhysicalMemory"] != null)
                        {
                            PhysicalMemory = long.Parse(mo["TotalPhysicalMemory"].ToString());
                        }
                    }
                }

                var cat = new PerformanceCounterCategory("Network Interface");
                InstanceNames = cat.GetInstanceNames();
                NetRecvCounters = new PerformanceCounter[InstanceNames.Length];
                NetSentCounters = new PerformanceCounter[InstanceNames.Length];
                for (int i = 0; i < InstanceNames.Length; i++)
                {
                    NetRecvCounters[i] = new PerformanceCounter();
                    NetSentCounters[i] = new PerformanceCounter();
                }

                CompactFormat = false;
            }
            catch (Exception e)
            {
                LogManager.Error(e);
            }
        }

        #endregion 构造函数

        private static bool CompactFormat { get; set; }

        #region CPU相关

        /// <summary>
        /// 获取CPU核心数
        /// </summary>
        public static int ProcessorCount { get; }

        /// <summary>
        /// 获取CPU占用率 %
        /// </summary>
        public static float CpuLoad => PcCpuLoad.NextValue();

        /// <summary>
        /// 获取当前进程的CPU使用率（至少需要0.5s）
        /// </summary>
        /// <returns></returns>
        public static async Task<double> GetCpuUsageForProcess()
        {
            var startTime = DateTime.UtcNow;
            using var p1 = Process.GetCurrentProcess();
            var startCpuUsage = p1.TotalProcessorTime;
            await Task.Delay(500);
            var endTime = DateTime.UtcNow;
            using var p2 = Process.GetCurrentProcess();
            var endCpuUsage = p2.TotalProcessorTime;
            var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;
            return cpuUsedMs / (Environment.ProcessorCount * totalMsPassed) * 100;
        }

        /// <summary>
        /// WMI接口获取CPU使用率
        /// </summary>
        /// <returns></returns>
        public static string GetProcessorData()
        {
            var d = GetCounterValue(CpuCounter, "Processor", "% Processor Time", "_Total");
            return CompactFormat ? (int)d + "%" : d.ToString("F") + "%";
        }

        /// <summary>
        /// 获取CPU温度
        /// </summary>
        /// <returns>CPU温度</returns>
        public static float GetCPUTemperature()
        {
            try
            {
                using var mos = new ManagementObjectSearcher(@"root\WMI", "select * from MSAcpi_ThermalZoneTemperature");
                using var moc = mos.Get();
                foreach (var mo in moc)
                {
                    using (mo)
                    {
                        //这就是CPU的温度了
                        var temp = (float.Parse(mo.Properties["CurrentTemperature"].Value.ToString()) - 2732) / 10;
                        return (float)Math.Round(temp, 2);
                    }
                }
            }
            catch (Exception)
            {
                return 0;
            }

            return 0;
        }

        /// <summary>
        /// 获取CPU的数量
        /// </summary>
        /// <returns>CPU的数量</returns>
        public static int GetCpuCount()
        {
            try
            {
                return _cache.GetOrAdd(nameof(GetCpuCount), () =>
                {
                    using var m = new ManagementClass("Win32_Processor");
                    using var moc = m.GetInstances();
                    return moc.Count;
                });
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private static readonly Lazy<List<ManagementBaseObject>> CpuObjects = new Lazy<List<ManagementBaseObject>>(() =>
        {
            using var mos = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
            using var moc = mos.Get();
            return moc.AsParallel().Cast<ManagementBaseObject>().ToList();
        });

        /// <summary>
        /// 获取CPU信息
        /// </summary>
        /// <returns>CPU信息</returns>
        public static List<CpuInfo> GetCpuInfo()
        {
            try
            {
                return CpuObjects.Value.Select(mo => new CpuInfo
                {
                    NumberOfLogicalProcessors = ProcessorCount,
                    CurrentClockSpeed = mo.Properties["CurrentClockSpeed"].Value.ToString(),
                    Manufacturer = mo.Properties["Manufacturer"].Value.ToString(),
                    MaxClockSpeed = mo.Properties["MaxClockSpeed"].Value.ToString(),
                    Type = mo.Properties["Name"].Value.ToString(),
                    DataWidth = mo.Properties["DataWidth"].Value.ToString(),
                    SerialNumber = mo.Properties["ProcessorId"].Value.ToString(),
                    DeviceID = mo.Properties["DeviceID"].Value.ToString(),
                    NumberOfCores = Convert.ToInt32(mo.Properties["NumberOfCores"].Value)
                }).ToList();
            }
            catch (Exception)
            {
                return new List<CpuInfo>();
            }
        }

        #endregion CPU核心

        #region 内存相关

        /// <summary>
        /// 获取可用内存
        /// </summary>
        public static long MemoryAvailable
        {
            get
            {
                try
                {
                    using var mc = new ManagementClass("Win32_OperatingSystem");
                    using var moc = mc.GetInstances();
                    foreach (var mo in moc)
                    {
                        using (mo)
                        {
                            if (mo["FreePhysicalMemory"] != null)
                            {
                                return 1024 * long.Parse(mo["FreePhysicalMemory"].ToString());
                            }
                        }
                    }

                    return 0;
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// 获取物理内存
        /// </summary>
        public static long PhysicalMemory { get; }

        /// <summary>
        /// 获取内存信息
        /// </summary>
        /// <returns>内存信息</returns>
        public static RamInfo GetRamInfo()
        {
            return new RamInfo
            {
                MemoryAvailable = GetFreePhysicalMemory(),
                PhysicalMemory = GetTotalPhysicalMemory(),
                TotalPageFile = GetTotalVirtualMemory(),
                AvailablePageFile = GetTotalVirtualMemory() - GetUsedVirtualMemory(),
                AvailableVirtual = 1 - GetUsageVirtualMemory(),
                TotalVirtual = 1 - GetUsedPhysicalMemory()
            };
        }

        /// <summary>
        /// 获取虚拟内存使用率详情
        /// </summary>
        /// <returns></returns>
        public static string GetMemoryVData()
        {
            float d = GetCounterValue(MemoryCounter, "Memory", "% Committed Bytes In Use", null);
            var str = d.ToString("F") + "% (";
            d = GetCounterValue(MemoryCounter, "Memory", "Committed Bytes", null);
            str += FormatBytes(d) + " / ";
            d = GetCounterValue(MemoryCounter, "Memory", "Commit Limit", null);
            return str + FormatBytes(d) + ") ";
        }

        /// <summary>
        /// 获取虚拟内存使用率
        /// </summary>
        /// <returns></returns>
        public static float GetUsageVirtualMemory()
        {
            return GetCounterValue(MemoryCounter, "Memory", "% Committed Bytes In Use", null);
        }

        /// <summary>
        /// 获取虚拟内存已用大小
        /// </summary>
        /// <returns></returns>
        public static float GetUsedVirtualMemory()
        {
            return GetCounterValue(MemoryCounter, "Memory", "Committed Bytes", null);
        }

        /// <summary>
        /// 获取虚拟内存总大小
        /// </summary>
        /// <returns></returns>
        public static float GetTotalVirtualMemory()
        {
            return GetCounterValue(MemoryCounter, "Memory", "Commit Limit", null);
        }

        /// <summary>
        /// 获取物理内存使用率详情描述
        /// </summary>
        /// <returns></returns>
        public static string GetMemoryPData()
        {
            string s = QueryComputerSystem("totalphysicalmemory");
            float totalphysicalmemory = Convert.ToSingle(s);
            float d = GetCounterValue(MemoryCounter, "Memory", "Available Bytes", null);
            d = totalphysicalmemory - d;
            s = CompactFormat ? "%" : "% (" + FormatBytes(d) + " / " + FormatBytes(totalphysicalmemory) + ")";
            d /= totalphysicalmemory;
            d *= 100;
            return CompactFormat ? (int)d + s : d.ToString("F") + s;
        }

        /// <summary>
        /// 获取物理内存总数，单位B
        /// </summary>
        /// <returns></returns>
        public static float GetTotalPhysicalMemory()
        {
            return _cache.GetOrAdd(nameof(GetTotalPhysicalMemory), () =>
            {
                var s = QueryComputerSystem("totalphysicalmemory");
                return s.TryConvertTo<float>();
            });
        }

        /// <summary>
        /// 获取空闲的物理内存数，单位B
        /// </summary>
        /// <returns></returns>
        public static float GetFreePhysicalMemory()
        {
            return GetCounterValue(MemoryCounter, "Memory", "Available Bytes", null);
        }

        /// <summary>
        /// 获取已经使用了的物理内存数，单位B
        /// </summary>
        /// <returns></returns>
        public static float GetUsedPhysicalMemory()
        {
            return GetTotalPhysicalMemory() - GetFreePhysicalMemory();
        }

        #endregion 可用内存

        #region 硬盘相关

        /// <summary>
        /// 获取硬盘的读写速率
        /// </summary>
        /// <param name="dd">读或写</param>
        /// <returns></returns>
        public static float GetDiskData(DiskData dd) => dd == DiskData.Read ? GetCounterValue(DiskReadCounter, "PhysicalDisk", "Disk Read Bytes/sec", "_Total") : dd == DiskData.Write ? GetCounterValue(DiskWriteCounter, "PhysicalDisk", "Disk Write Bytes/sec", "_Total") : dd == DiskData.ReadAndWrite ? GetCounterValue(DiskReadCounter, "PhysicalDisk", "Disk Read Bytes/sec", "_Total") + GetCounterValue(DiskWriteCounter, "PhysicalDisk", "Disk Write Bytes/sec", "_Total") : 0;

        private static readonly List<DiskInfo> DiskInfos = new();

        /// <summary>
        /// 获取磁盘可用空间
        /// </summary>
        /// <returns></returns>
        public static List<DiskInfo> GetDiskInfo()
        {
            try
            {
                if (DiskInfos.Count > 0)
                {
                    return DiskInfos;
                }

                using var mc = new ManagementClass("Win32_DiskDrive");
                using var moc = mc.GetInstances();
                foreach (var mo in moc)
                {
                    using (mo)
                    {
                        DiskInfos.Add(new DiskInfo()
                        {
                            Total = float.Parse(mo["Size"].ToString()),
                            Model = mo["Model"].ToString(),
                            SerialNumber = mo["SerialNumber"].ToString(),
                        });
                    }
                }

                return DiskInfos;
            }
            catch (Exception)
            {
                return new List<DiskInfo>();
            }
        }

        #endregion 硬盘相关

        #region 网络相关

        /// <summary>
        /// 获取网络的传输速率
        /// </summary>
        /// <param name="nd">上传或下载</param>
        /// <returns></returns>
        public static float GetNetData(NetData nd)
        {
            if (InstanceNames.Length == 0)
            {
                return 0;
            }

            float d = 0;
            for (int i = 0; i < InstanceNames.Length; i++)
            {
                float receied = GetCounterValue(NetRecvCounters[i], "Network Interface", "Bytes Received/sec", InstanceNames[i]);
                float send = GetCounterValue(NetSentCounters[i], "Network Interface", "Bytes Sent/sec", InstanceNames[i]);
                switch (nd)
                {
                    case NetData.Received:
                        d += receied;
                        break;

                    case NetData.Sent:
                        d += send;
                        break;

                    case NetData.ReceivedAndSent:
                        d += receied + send;
                        break;

                    default:
                        d += 0;
                        break;
                }
            }

            return d;
        }

        /// <summary>
        /// 获取网卡硬件地址
        /// </summary>
        /// <returns></returns>
        public static IList<string> GetMacAddress()
        {
            //获取网卡硬件地址
            try
            {
                return _cache.GetOrAdd(nameof(GetMacAddress), () =>
                {
                    IList<string> list = new List<string>();
                    using var mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                    using var moc = mc.GetInstances();
                    foreach (var mo in moc)
                    {
                        using (mo)
                        {
                            if ((bool)mo["IPEnabled"])
                            {
                                list.Add(mo["MacAddress"].ToString());
                            }
                        }
                    }

                    return list;
                });
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// 获取当前使用的IP
        /// </summary>
        /// <returns></returns>
        public static IPAddress GetLocalUsedIP()
        {
            return GetLocalUsedIP(AddressFamily.InterNetwork);
        }

        /// <summary>
        /// 获取IP地址WMI
        /// </summary>
        /// <returns></returns>
        public static string GetIPAddressWMI()
        {
            try
            {
                return _cache.GetOrAdd(nameof(GetIPAddressWMI), () =>
                {
                    using var mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                    using var moc = mc.GetInstances();
                    foreach (var mo in moc)
                    {
                        if ((bool)mo["IPEnabled"])
                        {
                            return ((string[])mo.Properties["IpAddress"].Value)[0];
                        }
                    }
                    return "";
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return "";
        }

        /// <summary>
        /// 获取当前使用的IP
        /// </summary>
        /// <returns></returns>
        public static IPAddress GetLocalUsedIP(AddressFamily family)
        {
            return NetworkInterface.GetAllNetworkInterfaces().OrderByDescending(c => c.Speed).Where(c => c.NetworkInterfaceType != NetworkInterfaceType.Loopback && c.OperationalStatus == OperationalStatus.Up).Select(t => t.GetIPProperties()).Where(p => p.DhcpServerAddresses.Count > 0).SelectMany(p => p.UnicastAddresses).Select(p => p.Address).FirstOrDefault(p => !(p.IsIPv6Teredo || p.IsIPv6LinkLocal || p.IsIPv6Multicast || p.IsIPv6SiteLocal) && p.AddressFamily == family);
        }

        /// <summary>
        /// 获取本机所有的ip地址
        /// </summary>
        /// <returns></returns>
        public static List<UnicastIPAddressInformation> GetLocalIPs()
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces().OrderByDescending(c => c.Speed).Where(c => c.NetworkInterfaceType != NetworkInterfaceType.Loopback && c.OperationalStatus == OperationalStatus.Up); //所有网卡信息
            return interfaces.SelectMany(n => n.GetIPProperties().UnicastAddresses).ToList();
        }

        /// <summary>
        /// 获取网卡地址
        /// </summary>
        /// <returns></returns>
        public static string GetNetworkCardAddress()
        {
            try
            {
                return _cache.GetOrAdd(nameof(GetNetworkCardAddress), () =>
                {
                    using var mos = new ManagementObjectSearcher("select * from Win32_NetworkAdapter where ((MACAddress Is Not NULL) and (Manufacturer <> 'Microsoft'))");
                    using var moc = mos.Get();
                    foreach (var mo in moc)
                    {
                        return mo["MACAddress"].ToString().Trim();
                    }
                    return "";
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return "";
        }

        #endregion 网络相关

        #region 系统相关

        /// <summary>
        /// 获取计算机开机时间
        /// </summary>
        /// <returns>datetime</returns>
        public static DateTime BootTime()
        {
            var query = new SelectQuery("SELECT LastBootUpTime FROM Win32_OperatingSystem WHERE Primary='true'");
            using var searcher = new ManagementObjectSearcher(query);
            using var moc = searcher.Get();
            foreach (var mo in moc)
            {
                using (mo)
                {
                    return ManagementDateTimeConverter.ToDateTime(mo.Properties["LastBootUpTime"].Value.ToString());
                }
            }

            return DateTime.Now - TimeSpan.FromMilliseconds(Environment.TickCount & int.MaxValue);
        }

        /// <summary>
        /// 查询计算机系统信息
        /// </summary>
        /// <param name="type">类型名</param>
        /// <returns></returns>
        public static string QueryComputerSystem(string type)
        {
            try
            {
                var mos = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");
                using var moc = mos.Get();
                foreach (var mo in moc)
                {
                    using (mo)
                    {
                        return mo[type].ToString();
                    }
                }
            }
            catch (Exception e)
            {
                return "未能获取到当前计算机系统信息，可能是当前程序无管理员权限，如果是web应用程序，请将应用程序池的高级设置中的进程模型下的标识设置为：LocalSystem；如果是普通桌面应用程序，请提升管理员权限后再操作。异常信息：" + e.Message;
            }
            return string.Empty;
        }

        /// <summary>
        /// 查找所有应用程序标题
        /// </summary>
        /// <param name="handle">应用程序标题范型</param>
        /// <returns>所有应用程序集合</returns>
        public static ArrayList FindAllApps(int handle)
        {
            var apps = new ArrayList();
            int hwCurr = GetWindow(handle, GwHwndfirst);

            while (hwCurr > 0)
            {
                int IsTask = WsVisible | WsBorder;
                int lngStyle = GetWindowLongA(hwCurr, GwlStyle);
                bool taskWindow = (lngStyle & IsTask) == IsTask;
                if (taskWindow)
                {
                    int length = GetWindowTextLength(new IntPtr(hwCurr));
                    StringBuilder sb = new StringBuilder(2 * length + 1);
                    GetWindowText(hwCurr, sb, sb.Capacity);
                    string strTitle = sb.ToString();
                    if (!string.IsNullOrEmpty(strTitle))
                    {
                        apps.Add(strTitle);
                    }
                }

                hwCurr = GetWindow(hwCurr, GwHwndnext);
            }

            return apps;
        }

        /// <summary>
        /// 操作系统类型
        /// </summary>
        /// <returns></returns>
        public static string GetSystemType()
        {
            try
            {
                return _cache.GetOrAdd(nameof(GetSystemType), () =>
                {
                    using var mc = new ManagementClass("Win32_ComputerSystem");
                    using var moc = mc.GetInstances();
                    foreach (var mo in moc)
                    {
                        return mo["SystemType"].ToString().Trim();
                    }
                    return "";
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return "";
        }

        #endregion 系统相关

        #region 主板相关

        /// <summary>
        /// 获取主板序列号
        /// </summary>
        /// <returns></returns>
        public static string GetBiosSerialNumber()
        {
            try
            {
                return _cache.GetOrAdd(nameof(GetBiosSerialNumber), () =>
                {
                    using var searcher = new ManagementObjectSearcher("select * from Win32_BIOS");
                    using var mos = searcher.Get();
                    foreach (var mo in mos)
                    {
                        return mo["SerialNumber"].ToString().Trim();
                    }
                    return "";
                });

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return "";
        }

        /// <summary>
        /// 主板编号
        /// </summary>
        /// <returns></returns>
        public static BiosInfo GetBiosInfo()
        {
            return _cache.GetOrAdd(nameof(GetBiosInfo), () =>
            {
                using var searcher = new ManagementObjectSearcher("select * from Win32_BaseBoard");
                using var mos = searcher.Get();
                foreach (var mo in mos)
                {
                    return new BiosInfo
                    {
                        Manufacturer = mo.GetPropertyValue("Manufacturer").ToString(),
                        ID = mo["SerialNumber"].ToString(),
                        Model = mo["Product"].ToString(),
                        SerialNumber = GetBiosSerialNumber()
                    };
                }

                return new BiosInfo();
            });
        }

        #endregion

        #region 公共函数

        /// <summary>
        /// 将速度值格式化成字节单位
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string FormatBytes(this double bytes)
        {
            int unit = 0;
            while (bytes > 1024)
            {
                bytes /= 1024;
                ++unit;
            }

            string s = CompactFormat ? ((int)bytes).ToString() : bytes.ToString("F") + " ";
            return s + (Unit)unit;
        }

        private static float GetCounterValue(PerformanceCounter pc, string categoryName, string counterName, string instanceName)
        {
            pc.CategoryName = categoryName;
            pc.CounterName = counterName;
            pc.InstanceName = instanceName;
            return pc.NextValue();
        }

        #endregion 公共函数

        #region Win32API声明

#pragma warning disable 1591

        [DllImport("User32")]
        public static extern int GetWindow(int hWnd, int wCmd);

        [DllImport("User32")]
        public static extern int GetWindowLongA(int hWnd, int wIndx);

        [DllImport("user32.dll")]
        public static extern bool GetWindowText(int hWnd, StringBuilder title, int maxBufSize);

        [DllImport("user32", CharSet = CharSet.Auto)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

#pragma warning restore 1591

        #endregion Win32API声明
    }
}
