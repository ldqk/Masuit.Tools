using System;
using System.Diagnostics;
using System.Management;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using static System.String;

namespace Masuit.Tools.Win32
{
    /// <summary>
    /// Windows系统的系列方法
    /// </summary>
    public static class Windows
    {
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
                string ip;
                using (System.Net.Sockets.TcpClient c = new System.Net.Sockets.TcpClient())
                {
                    c.Connect("www.baidu.com", 80);
                    ip = ((IPEndPoint)c.Client.LocalEndPoint).Address.ToString();
                }

                return ip;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>  
        /// 运行一个控制台程序并返回其输出参数。  
        /// </summary>  
        /// <param name="filename">程序名</param>  
        /// <param name="arguments">输入参数</param>
        /// <param name="recordLog">是否在控制台输出日志</param>
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
                    //上面标记的是原文，下面是我自己调试错误后自行修改的  
                    Thread.Sleep(100); //貌似调用系统的nslookup还未返回数据或者数据未编码完成，程序就已经跳过直接执行  
                    //txt = sr.ReadToEnd()了，导致返回的数据为空，故睡眠令硬件反应  
                    if (!proc.HasExited) //在无参数调用nslookup后，可以继续输入命令继续操作，如果进程未停止就直接执行  
                    {
                        //txt = sr.ReadToEnd()程序就在等待输入，而且又无法输入，直接掐住无法继续运行  
                        proc.Kill();
                    }

                    string txt = sr.ReadToEnd();
                    if (recordLog)
                        Trace.WriteLine(txt);
                    return txt;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return ex.Message;
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
        public string[] CpuMhz; //CPU频率  单位：hz

        /// <summary>
        /// mac地址
        /// </summary>
        public string MacAddress; //计算机的MAC地址

        /// <summary>
        /// 硬盘ID
        /// </summary>
        public string DiskId; //硬盘的ID

        /// <summary>
        /// 硬盘大小
        /// </summary>
        public string DiskSize; //硬盘大小  单位：bytes

        /// <summary>
        /// IP地址
        /// </summary>
        public string IpAddress; //计算机的IP地址

        /// <summary>
        /// 系统当前登录用户
        /// </summary>
        public string LoginUserName; //操作系统登录用户名

        /// <summary>
        /// 计算机名
        /// </summary>
        public string ComputerName; //计算机名

        /// <summary>
        /// 操作系统架构
        /// </summary>
        public string SystemType; //系统类型

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
                if (_instance == null) _instance = new WindowsServer();
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
                string cpuInfo = " "; //cpu序列号 
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

        /// <summary>
        /// 获取CPU个数
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// 获取CPU主频
        /// </summary>
        /// <returns></returns>
        public static string[] GetCpuMHZ()
        {
            using (var mc = new ManagementClass("Win32_Processor"))
            {
                using (ManagementObjectCollection cpus = mc.GetInstances())
                {
                    var mhz = new string[cpus.Count];
                    int c = 0;
                    using (var mySearch = new ManagementObjectSearcher("select * from Win32_Processor"))
                    {
                        foreach (ManagementObject mo in mySearch.Get())
                        {
                            using (mo)
                            {
                                mhz[c] = mo.Properties["CurrentClockSpeed"].Value.ToString();
                                c++;
                            }
                        }
                    }

                    return mhz;
                }
            }
        }

        /// <summary>
        /// 获取磁盘大小
        /// </summary>
        /// <returns></returns>
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
                            if ((bool)mo["IPEnabled"])
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
                string st = Empty;
                using (var mc = new ManagementClass("Win32_NetworkAdapterConfiguration"))
                {
                    foreach (ManagementObject mo in mc.GetInstances())
                    {
                        using (mo)
                        {
                            if ((bool)mo["IPEnabled"])
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
        }

        string GetDiskID()
        {
            try
            {
                //获取硬盘ID 
                string hdid = Empty;
                using (var mc = new ManagementClass("Win32_DiskDrive"))
                {
                    foreach (ManagementObject mo in mc.GetInstances())
                    {
                        using (mo)
                        {
                            hdid = (string)mo.Properties["Model"].Value;
                        }
                    }
                }

                return hdid;
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
                string st = Empty;
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
                string st = Empty;
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
                string st = Empty;
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