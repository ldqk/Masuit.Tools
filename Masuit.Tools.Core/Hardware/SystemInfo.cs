using Masuit.Tools.Logging;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Masuit.Tools.Hardware
{
    /// <summary>
    /// 硬件信息，部分功能需要C++支持
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

        #endregion

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
                ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementBaseObject mo in moc)
                {
                    if (mo["TotalPhysicalMemory"] != null)
                    {
                        PhysicalMemory = long.Parse(mo["TotalPhysicalMemory"].ToString());
                    }
                }

                PerformanceCounterCategory cat = new PerformanceCounterCategory("Network Interface");
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

        #endregion

        private static bool CompactFormat { get; set; }

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
        public static float CpuLoad => PcCpuLoad.NextValue();

        #endregion

        #region 可用内存 

        /// <summary>
        /// 获取可用内存
        /// </summary>
        public static long MemoryAvailable
        {
            get
            {
                try
                {
                    long availablebytes = 0;
                    ManagementClass mos = new ManagementClass("Win32_OperatingSystem");
                    foreach (var o in mos.GetInstances())
                    {
                        var mo = (ManagementObject)o;
                        if (mo["FreePhysicalMemory"] != null)
                        {
                            availablebytes = 1024 * long.Parse(mo["FreePhysicalMemory"].ToString());
                        }
                    }

                    return availablebytes;
                }
                catch (Exception)
                {
                    return 0;
                }
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
        /// <param name="handle">应用程序标题范型</param>
        /// <returns>所有应用程序集合</returns>
        public static ArrayList FindAllApps(int handle)
        {
            ArrayList apps = new ArrayList();
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

        #endregion

        #region 获取CPU的数量

        /// <summary>
        /// 获取CPU的数量
        /// </summary>
        /// <returns>CPU的数量</returns>
        public static int GetCpuCount()
        {
            try
            {
                ManagementClass m = new ManagementClass("Win32_Processor");
                ManagementObjectCollection mn = m.GetInstances();
                return mn.Count;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        #endregion

        #region 获取CPU信息

        /// <summary>
        /// 获取CPU信息
        /// </summary>
        /// <returns>CPU信息</returns>
        public static List<CpuInfo> GetCpuInfo()
        {
            try
            {
                List<CpuInfo> list = new List<CpuInfo>();
                ManagementObjectSearcher mySearcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
                foreach (var o in mySearcher.Get())
                {
                    var myObject = (ManagementObject)o;
                    list.Add(new CpuInfo
                    {
                        CpuLoad = CpuLoad,
                        NumberOfLogicalProcessors = ProcessorCount,
                        CurrentClockSpeed = myObject.Properties["CurrentClockSpeed"].Value.ToString(),
                        Manufacturer = myObject.Properties["Manufacturer"].Value.ToString(),
                        MaxClockSpeed = myObject.Properties["MaxClockSpeed"].Value.ToString(),
                        Type = myObject.Properties["Name"].Value.ToString(),
                        DataWidth = myObject.Properties["DataWidth"].Value.ToString(),
                        DeviceID = myObject.Properties["DeviceID"].Value.ToString(),
                        NumberOfCores = Convert.ToInt32(myObject.Properties["NumberOfCores"].Value),
                        Temperature = GetCPUTemperature()
                    });
                }

                return list;
            }
            catch (Exception)
            {
                return new List<CpuInfo>();
            }
        }

        #endregion

        #region 获取内存信息

        /// <summary>
        /// 获取内存信息
        /// </summary>
        /// <returns>内存信息</returns>
        public static RamInfo GetRamInfo()
        {
            var info = new RamInfo();
            info.MemoryAvailable = GetFreePhysicalMemory();
            info.PhysicalMemory = GetTotalPhysicalMemory();
            info.TotalPageFile = GetTotalVirtualMemory();
            info.AvailablePageFile = GetTotalVirtualMemory() - GetUsedVirtualMemory();
            info.AvailableVirtual = 1 - GetUsageVirtualMemory();
            info.TotalVirtual = 1 - GetUsedPhysicalMemory();
            return info;
        }

        #endregion

        #region 获取CPU温度

        /// <summary>
        /// 获取CPU温度
        /// </summary>
        /// <returns>CPU温度</returns>
        public static double GetCPUTemperature()
        {
            try
            {
                string str = "";
                ManagementObjectSearcher vManagementObjectSearcher = new ManagementObjectSearcher(@"root\WMI", @"select * from MSAcpi_ThermalZoneTemperature");
                foreach (ManagementObject managementObject in vManagementObjectSearcher.Get())
                {
                    str += managementObject.Properties["CurrentTemperature"].Value.ToString();
                }

                //这就是CPU的温度了
                double temp = (double.Parse(str) - 2732) / 10;
                return Math.Round(temp, 2);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        #endregion

        #region WMI接口获取CPU使用率

        /// <summary>
        /// WMI接口获取CPU使用率
        /// </summary>
        /// <returns></returns>
        public static string GetProcessorData()
        {
            double d = GetCounterValue(CpuCounter, "Processor", "% Processor Time", "_Total");
            return CompactFormat ? (int)d + "%" : d.ToString("F") + "%";
        }

        #endregion

        #region 获取虚拟内存使用率详情

        /// <summary>
        /// 获取虚拟内存使用率详情
        /// </summary>
        /// <returns></returns>
        public static string GetMemoryVData()
        {
            double d = GetCounterValue(MemoryCounter, "Memory", "% Committed Bytes In Use", null);
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
        public static double GetUsageVirtualMemory()
        {
            return GetCounterValue(MemoryCounter, "Memory", "% Committed Bytes In Use", null);
        }

        /// <summary>
        /// 获取虚拟内存已用大小
        /// </summary>
        /// <returns></returns>
        public static double GetUsedVirtualMemory()
        {
            return GetCounterValue(MemoryCounter, "Memory", "Committed Bytes", null);
        }

        /// <summary>
        /// 获取虚拟内存总大小
        /// </summary>
        /// <returns></returns>
        public static double GetTotalVirtualMemory()
        {
            return GetCounterValue(MemoryCounter, "Memory", "Commit Limit", null);
        }

        #endregion

        #region 获取物理内存使用率详情

        /// <summary>
        /// 获取物理内存使用率详情描述
        /// </summary>
        /// <returns></returns>
        public static string GetMemoryPData()
        {
            string s = QueryComputerSystem("totalphysicalmemory");
            double totalphysicalmemory = Convert.ToDouble(s);

            double d = GetCounterValue(MemoryCounter, "Memory", "Available Bytes", null);
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
        public static double GetTotalPhysicalMemory()
        {
            string s = QueryComputerSystem("totalphysicalmemory");
            return s.ToDouble();
        }

        /// <summary>
        /// 获取空闲的物理内存数，单位B
        /// </summary>
        /// <returns></returns>
        public static double GetFreePhysicalMemory()
        {
            return GetCounterValue(MemoryCounter, "Memory", "Available Bytes", null);
        }

        /// <summary>
        /// 获取已经使用了的物理内存数，单位B
        /// </summary>
        /// <returns></returns>
        public static double GetUsedPhysicalMemory()
        {
            return GetTotalPhysicalMemory() - GetFreePhysicalMemory();
        }

        #endregion

        #region 获取硬盘的读写速率

        /// <summary>
        /// 获取硬盘的读写速率
        /// </summary>
        /// <param name="dd">读或写</param>
        /// <returns></returns>
        public static double GetDiskData(DiskData dd) => dd == DiskData.Read ? GetCounterValue(DiskReadCounter, "PhysicalDisk", "Disk Read Bytes/sec", "_Total") : dd == DiskData.Write ? GetCounterValue(DiskWriteCounter, "PhysicalDisk", "Disk Write Bytes/sec", "_Total") : dd == DiskData.ReadAndWrite ? GetCounterValue(DiskReadCounter, "PhysicalDisk", "Disk Read Bytes/sec", "_Total") + GetCounterValue(DiskWriteCounter, "PhysicalDisk", "Disk Write Bytes/sec", "_Total") : 0;

        #endregion

        #region 获取网络的传输速率

        /// <summary>
        /// 获取网络的传输速率
        /// </summary>
        /// <param name="nd">上传或下载</param>
        /// <returns></returns>
        public static double GetNetData(NetData nd)
        {
            if (InstanceNames.Length == 0)
            {
                return 0;
            }

            double d = 0;
            for (int i = 0; i < InstanceNames.Length; i++)
            {
                double receied = GetCounterValue(NetRecvCounters[i], "Network Interface", "Bytes Received/sec", InstanceNames[i]);
                double send = GetCounterValue(NetSentCounters[i], "Network Interface", "Bytes Sent/sec", InstanceNames[i]);
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

        #endregion

        /// <summary>
        /// 获取网卡硬件地址
        /// </summary>
        /// <returns></returns>
        public static IList<string> GetMacAddress()
        {
            //获取网卡硬件地址       
            try
            {
                IList<string> list = new List<string>();
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                using (mc)
                {
                    ManagementObjectCollection moc = mc.GetInstances();
                    using (moc)
                    {
                        foreach (ManagementObject mo in moc)
                        {
                            if ((bool)mo["IPEnabled"])
                            {
                                list.Add(mo["MacAddress"].ToString());
                            }
                        }
                    }
                }

                return list;
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// 获取IP地址 
        /// </summary>
        /// <returns></returns>
        public static IList<string> GetIPAddress()
        {
            //获取IP地址        
            try
            {
                IList<string> list = new List<string>();
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                using (mc)
                {
                    ManagementObjectCollection moc = mc.GetInstances();
                    using (moc)
                    {
                        foreach (ManagementObject mo in moc)
                        {
                            if ((bool)mo["IPEnabled"])
                            {
                                var ar = (Array)(mo.Properties["IpAddress"].Value);
                                var st = ar.GetValue(0).ToString();
                                list.Add(st);
                            }
                        }

                        return list;
                    }
                }
            }
            catch (Exception)
            {
                return new List<string>()
                {
                    "未能获取到当前计算机的IP地址，可能是当前程序无管理员权限，如果是web应用程序，请将应用程序池的高级设置中的进程模型下的标识设置为：LocalSystem；如果是普通桌面应用程序，请提升管理员权限后再操作。"
                };
            }
        }

        /// <summary>  
        /// 获取当前使用的IP  
        /// </summary>  
        /// <returns></returns>  
        public static string GetLocalUsedIP()
        {
            string result = RunApp("route", "print", true);
            Match m = Regex.Match(result, @"0.0.0.0\s+0.0.0.0\s+(\d+.\d+.\d+.\d+)\s+(\d+.\d+.\d+.\d+)");
            if (m.Success)
            {
                return m.Groups[2].Value;
            }

            try
            {
                using (System.Net.Sockets.TcpClient c = new System.Net.Sockets.TcpClient())
                {
                    c.Connect("www.baidu.com", 80);
                    var ip = ((IPEndPoint)c.Client.LocalEndPoint).Address.ToString();
                    return ip;
                }
            }
            catch (Exception)
            {
                return "未能获取当前使用的IP，可能是当前程序无管理员权限，如果是web应用程序，请将应用程序池的高级设置中的进程模型下的标识设置为：LocalSystem；如果是普通桌面应用程序，请提升管理员权限后再操作。";
            }
        }

        /// <summary>  
        /// 运行一个控制台程序并返回其输出参数。  
        /// </summary>  
        /// <param name="filename">程序名</param>  
        /// <param name="arguments">输入参数</param>
        /// <param name="recordLog">是否记录日志</param>
        /// <returns></returns>  
        public static string RunApp(string filename, string arguments, bool recordLog)
        {
            try
            {
                if (recordLog)
                {
                    Trace.WriteLine(filename + " " + arguments);
                }

                Process proc = new Process
                {
                    StartInfo =
                    {
                        FileName = filename,
                        CreateNoWindow = true,
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        UseShellExecute = false
                    }
                };
                proc.Start();

                using (System.IO.StreamReader sr = new System.IO.StreamReader(proc.StandardOutput.BaseStream, Encoding.Default))
                {
                    Thread.Sleep(100); //貌似调用系统的nslookup还未返回数据或者数据未编码完成，程序就已经跳过直接执行  
                    //txt = sr.ReadToEnd()了，导致返回的数据为空，故睡眠令硬件反应  
                    if (!proc.HasExited) //在无参数调用nslookup后，可以继续输入命令继续操作，如果进程未停止就直接执行  
                    {
                        //txt = sr.ReadToEnd()程序就在等待输入，而且又无法输入，直接掐住无法继续运行  
                        proc.Kill();
                    }

                    string txt = sr.ReadToEnd();
                    sr.Close();
                    if (recordLog)
                    {
                        Trace.WriteLine(txt);
                    }
                    return txt;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return ex.Message;
            }
        }

        /// <summary>
        /// 获取操作系统版本
        /// </summary>
        /// <returns></returns>
        public static string GetOsVersion()
        {
            try
            {
                return Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion")?.GetValue("ProductName").ToString();
            }
            catch (Exception)
            {
                return "未能获取到操作系统版本，可能是当前程序无管理员权限，如果是web应用程序，请将应用程序池的高级设置中的进程模型下的标识设置为：LocalSystem；如果是普通桌面应用程序，请提升管理员权限后再操作。";
            }
        }

        #region 将速度值格式化成字节单位

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

        #endregion

        #region 查询计算机系统信息

        /// <summary>
        /// 获取计算机开机时间
        /// </summary>
        /// <returns>datetime</returns>
        public static DateTime BootTime()
        {
            var query = new SelectQuery("SELECT LastBootUpTime FROM Win32_OperatingSystem WHERE Primary='true'");
            var searcher = new ManagementObjectSearcher(query);

            foreach (ManagementObject mo in searcher.Get())
            {
                return ManagementDateTimeConverter.ToDateTime(mo.Properties["LastBootUpTime"].Value.ToString());
            }

            return DateTime.Now - TimeSpan.FromMilliseconds(Environment.TickCount & Int32.MaxValue);
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
                string str = null;
                ManagementObjectSearcher objCS = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");
                foreach (ManagementObject objMgmt in objCS.Get())
                {
                    str = objMgmt[type].ToString();
                }
                return str;
            }
            catch (Exception e)
            {
                return "未能获取到当前计算机系统信息，可能是当前程序无管理员权限，如果是web应用程序，请将应用程序池的高级设置中的进程模型下的标识设置为：LocalSystem；如果是普通桌面应用程序，请提升管理员权限后再操作。异常信息：" + e.Message;
            }
        }

        #endregion

        #region 获取环境变量

        /// <summary>
        /// 获取环境变量
        /// </summary>
        /// <param name="type">环境变量名</param>
        /// <returns></returns>
        public static string QueryEnvironment(string type) => Environment.ExpandEnvironmentVariables(type);

        #endregion

        #region 获取磁盘空间

        /// <summary>
        /// 获取磁盘可用空间
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> DiskFree()
        {
            try
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                ManagementObjectSearcher objCS = new ManagementObjectSearcher("SELECT * FROM Win32_LogicalDisk");
                foreach (ManagementObject objMgmt in objCS.Get())
                {
                    var device = objMgmt["DeviceID"];
                    if (null != device)
                    {
                        var space = objMgmt["FreeSpace"];
                        if (null != space)
                        {
                            dic.Add(device.ToString(), FormatBytes(double.Parse(space.ToString())));
                        }
                    }
                }

                return dic;
            }
            catch (Exception)
            {
                return new Dictionary<string, string>()
                {
                    { "null", "未能获取到当前计算机的磁盘信息，可能是当前程序无管理员权限，如果是web应用程序，请将应用程序池的高级设置中的进程模型下的标识设置为：LocalSystem；如果是普通桌面应用程序，请提升管理员权限后再操作。" }
                };
            }
        }

        /// <summary>
        /// 获取磁盘总空间
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> DiskTotalSpace()
        {
            try
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                ManagementObjectSearcher objCS = new ManagementObjectSearcher("SELECT * FROM Win32_LogicalDisk");
                foreach (ManagementObject objMgmt in objCS.Get())
                {
                    var device = objMgmt["DeviceID"];
                    if (null != device)
                    {
                        var space = objMgmt["Size"];
                        if (null != space)
                        {
                            dic.Add(device.ToString(), FormatBytes(double.Parse(space.ToString())));
                        }
                    }
                }

                return dic;
            }
            catch (Exception)
            {
                return new Dictionary<string, string>();
            }
        }


        /// <summary>
        /// 获取磁盘已用空间
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> DiskUsedSpace()
        {
            try
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                ManagementObjectSearcher objCS = new ManagementObjectSearcher("SELECT * FROM Win32_LogicalDisk");
                foreach (ManagementObject objMgmt in objCS.Get())
                {
                    var device = objMgmt["DeviceID"];
                    if (null != device)
                    {
                        var free = objMgmt["FreeSpace"];
                        var total = objMgmt["Size"];
                        if (null != total)
                        {
                            dic.Add(device.ToString(), FormatBytes(double.Parse(total.ToString()) - free.ToString().ToDouble()));
                        }
                    }
                }

                return dic;
            }
            catch (Exception)
            {
                return new Dictionary<string, string>()
                {
                    { "null", "未能获取到当前计算机的磁盘信息，可能是当前程序无管理员权限，如果是web应用程序，请将应用程序池的高级设置中的进程模型下的标识设置为：LocalSystem；如果是普通桌面应用程序，请提升管理员权限后再操作。" }
                };
            }
        }

        /// <summary>
        /// 获取磁盘使用率
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, double> DiskUsage()
        {
            try
            {
                Dictionary<string, double> dic = new Dictionary<string, double>();
                ManagementObjectSearcher objCS = new ManagementObjectSearcher("SELECT * FROM Win32_LogicalDisk");
                foreach (ManagementObject objMgmt in objCS.Get())
                {
                    var device = objMgmt["DeviceID"];
                    if (null != device)
                    {
                        var free = objMgmt["FreeSpace"];
                        var total = objMgmt["Size"];
                        if (null != total && total.ToString().ToDouble() > 0)
                        {
                            dic.Add(device.ToString(), 1 - free.ToString().ToDouble() / total.ToString().ToDouble());
                        }
                    }
                }

                return dic;
            }
            catch (Exception)
            {
                return new Dictionary<string, double>()
                {
                    { "未能获取到当前计算机的磁盘信息，可能是当前程序无管理员权限，如果是web应用程序，请将应用程序池的高级设置中的进程模型下的标识设置为：LocalSystem；如果是普通桌面应用程序，请提升管理员权限后再操作。", 0 }
                };
            }
        }

        #endregion

        private static double GetCounterValue(PerformanceCounter pc, string categoryName, string counterName, string instanceName)
        {
            pc.CategoryName = categoryName;
            pc.CounterName = counterName;
            pc.InstanceName = instanceName;
            return pc.NextValue();
        }

        #region Win32API声明 

#pragma warning disable 1591
        [DllImport("kernel32")]
        public static extern void GetWindowsDirectory(StringBuilder winDir, int count);

        [DllImport("kernel32")]
        public static extern void GetSystemDirectory(StringBuilder sysDir, int count);

        [DllImport("kernel32")]
        public static extern void GetSystemInfo(ref CPU_INFO cpuinfo);

        [DllImport("kernel32")]
        public static extern void GlobalMemoryStatus(ref MemoryInfo meminfo);

        [DllImport("kernel32")]
        public static extern void GetSystemTime(ref SystemtimeInfo stinfo);

        [DllImport("IpHlpApi.dll")]
        public static extern uint GetIfTable(byte[] pIfTable, ref uint pdwSize, bool bOrder);

        [DllImport("User32")]
        public static extern int GetWindow(int hWnd, int wCmd);

        [DllImport("User32")]
        public static extern int GetWindowLongA(int hWnd, int wIndx);

        [DllImport("user32.dll")]
        public static extern bool GetWindowText(int hWnd, StringBuilder title, int maxBufSize);

        [DllImport("user32", CharSet = CharSet.Auto)]
        public static extern int GetWindowTextLength(IntPtr hWnd);
#pragma warning restore 1591

        #endregion
    }
}