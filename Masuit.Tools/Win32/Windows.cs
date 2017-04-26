using System;
using System.Diagnostics;
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
}
