using System;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using Masuit.Tools.Hardware;

namespace Masuit.Tools.Win32
{
    class Windows
    {
        /// <summary>
        /// 跨平台调用C++的方法
        /// </summary>
        /// <param name="hwProc">程序句柄</param>
        /// <returns></returns>
        [DllImport("psapi.dll")]
        static extern int EmptyWorkingSet(IntPtr hwProc);

        /// <summary>
        /// 清理系统内存，返回优化内存后的内存占用率
        /// </summary>
        /// <returns>优化内存后的内存占用率</returns>
        public static double ClearMemory()
        {
            ClearMemorySilent();
            Thread.Sleep(1000);
            return SystemInfo.GetRamInfo().MemoryUsage;
        }

        /// <summary>
        /// 静默清理系统内存
        /// </summary>
        public static void ClearMemorySilent()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            foreach (Process p in Process.GetProcesses())
            {
                using (p)
                {
                    if ((p.ProcessName.Equals("System")) && (p.ProcessName.Equals("Idle")))
                    {
                        //两个系统的关键进程，不整理
                        continue;
                    }
                    try
                    {
                        EmptyWorkingSet(p.Handle);
                    }
                    catch (Exception e)
                    {
                        // ignored
                    }
                }
            }
        }
    }

    /// <summary>
    /// 服务器信息
    /// </summary>
    public class WindowsServer
    {
        /// <summary>
        /// CPUID
        /// </summary>
        public string CpuId; //CPU的ID

        /// <summary>
        /// CPU插槽数
        /// </summary>
        public int CpuCount; //CPU的个数

        /// <summary>
        /// CPU主频
        /// </summary>
        public string[] CpuMhz;//CPU频率  单位：hz

        /// <summary>
        /// mac地址
        /// </summary>
        public string MacAddress;//计算机的MAC地址

        /// <summary>
        /// 硬盘ID
        /// </summary>
        public string DiskId;//硬盘的ID

        /// <summary>
        /// 硬盘大小
        /// </summary>
        public string DiskSize;//硬盘大小  单位：bytes

        /// <summary>
        /// IP地址
        /// </summary>
        public string IpAddress;//计算机的IP地址

        /// <summary>
        /// 系统当前登录用户
        /// </summary>
        public string LoginUserName;//操作系统登录用户名

        /// <summary>
        /// 计算机名
        /// </summary>
        public string ComputerName;//计算机名

        /// <summary>
        /// 操作系统架构
        /// </summary>
        public string SystemType;//系统类型

        /// <summary>
        /// 物理内存，单位MB
        /// </summary>
        public string TotalPhysicalMemory; //总共的内存  单位：M 
        private static WindowsServer _instance;

        /// <summary>
        /// 获取实例
        /// </summary>
        public static WindowsServer Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new WindowsServer();
                return _instance;
            }
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        public WindowsServer()
        {
            CpuId = GetCpuID();
            CpuCount = GetCpuCount();
            CpuMhz = GetCpuMHZ();
            MacAddress = GetMacAddress();
            DiskId = GetDiskID();
            DiskSize = GetSizeOfDisk();
            IpAddress = GetIPAddress();
            LoginUserName = GetUserName();
            SystemType = GetSystemType();
            TotalPhysicalMemory = GetTotalPhysicalMemory();
            ComputerName = GetComputerName();
        }
        string GetCpuID()
        {
            try
            {
                //获取CPU序列号代码 
                string cpuInfo = " ";//cpu序列号 
                using (var mc = new ManagementClass("Win32_Processor"))
                {
                    foreach (ManagementObject mo in mc.GetInstances())
                    {
                        using (mo)
                        {
                            cpuInfo = mo.Properties["ProcessorId"].Value.ToString();
                        }
                    }
                }
                return cpuInfo;
            }
            catch
            {
                return "unknow ";
            }
        }
        public static int GetCpuCount()
        {
            try
            {
                using (var mCpu = new ManagementClass("Win32_Processor"))
                {
                    using (ManagementObjectCollection cpus = mCpu.GetInstances())
                    {
                        return cpus.Count;
                    }
                }
            }
            catch
            {
                // ignored
            }
            return -1;
        }
        public static string[] GetCpuMHZ()
        {
            using (var mc = new ManagementClass("Win32_Processor"))
            {
                using (ManagementObjectCollection cpus = mc.GetInstances())
                {
                    var MHz = new string[cpus.Count];
                    int c = 0;
                    using (var mySearch = new ManagementObjectSearcher("select * from Win32_Processor"))
                    {
                        foreach (ManagementObject mo in mySearch.Get())
                        {
                            using (mo)
                            {
                                MHz[c] = mo.Properties["CurrentClockSpeed"].Value.ToString();
                                c++;
                            }
                        }
                    }
                    return MHz;
                }
            }
        }
        public static string GetSizeOfDisk()
        {
            using (var mc = new ManagementClass("Win32_DiskDrive"))
            {
                foreach (ManagementObject m in mc.GetInstances())
                {
                    using (m)
                    {
                        return m.Properties["Size"].Value.ToString();
                    }
                }
            }
            return "-1";
        }
        string GetMacAddress()
        {
            try
            {
                //获取网卡硬件地址 
                string mac = " ";
                using (var mc = new ManagementClass("Win32_NetworkAdapterConfiguration"))
                {
                    foreach (ManagementObject mo in mc.GetInstances())
                    {
                        using (mo)
                        {
                            if ((bool)mo["IPEnabled"] == true)
                            {
                                mac = mo["MacAddress"].ToString();
                                break;
                            }
                        }
                    }
                }
                return mac;
            }
            catch
            {
                return "unknow ";
            }
        }
        string GetIPAddress()
        {
            try
            {
                //获取IP地址 
                string st = String.Empty;
                using (var mc = new ManagementClass("Win32_NetworkAdapterConfiguration"))
                {
                    foreach (ManagementObject mo in mc.GetInstances())
                    {
                        using (mo)
                        {
                            if ((bool)mo["IPEnabled"] == true)
                            {
                                //st=mo[ "IpAddress "].ToString(); 
                                Array ar;
                                ar = (Array)(mo.Properties["IpAddress"].Value);
                                st = ar.GetValue(0).ToString();
                                break;
                            }
                        }
                    }
                }
                return st;
            }
            catch
            {
                return "unknow ";
            }
            finally
            {
            }

        }
        string GetDiskID()
        {
            try
            {
                //获取硬盘ID 
                string HDid = String.Empty;
                using (var mc = new ManagementClass("Win32_DiskDrive"))
                {
                    foreach (ManagementObject mo in mc.GetInstances())
                    {
                        using (mo)
                        {
                            HDid = (string)mo.Properties["Model"].Value;
                        }
                    }
                }
                return HDid;
            }
            catch
            {
                return "unknow ";
            }
        }
        ///    <summary>  
        ///   操作系统的登录用户名 
        ///    </summary>  
        ///    <returns>  </returns>  
        string GetUserName()
        {
            try
            {
                string st = String.Empty;
                using (var mc = new ManagementClass("Win32_ComputerSystem"))
                {
                    foreach (ManagementObject mo in mc.GetInstances())
                    {
                        using (mo)
                        {
                            st = mo["UserName"].ToString();
                        }
                    }
                }
                return st;
            }
            catch
            {
                return "unknow ";
            }

        }
        string GetSystemType()
        {
            try
            {
                string st = String.Empty;
                using (var mc = new ManagementClass("Win32_ComputerSystem"))
                {
                    foreach (var o in mc.GetInstances())
                    {
                        using (o)
                        {
                            var mo = (ManagementObject)o;
                            st = mo["SystemType"].ToString();
                        }
                    }
                }
                return st;
            }
            catch
            {
                return "unknow ";
            }
        }
        string GetTotalPhysicalMemory()
        {
            try
            {

                string st = String.Empty;
                using (var mc = new ManagementClass("Win32_ComputerSystem"))
                {
                    using (ManagementObjectCollection moc = mc.GetInstances())
                    {
                        foreach (var o in moc)
                        {
                            var mo = (ManagementObject)o;

                            st = mo["TotalPhysicalMemory"].ToString();
                        }
                    }
                }
                return st;
            }
            catch
            {
                return "unknow ";
            }
        }
        string GetComputerName()
        {
            try
            {
                return Environment.GetEnvironmentVariable("ComputerName");
            }
            catch
            {
                return "unknow ";
            }
        }
    }
}
