using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Masuit.Tools.Win32
{
    /// <summary>
    /// 命令提示符
    /// </summary>
    public static class WindowsCommand
    {
        /// <summary>
        /// 执行一个控制台程序，并获取在控制台返回的数据
        /// </summary>
        /// <param name="dosCommand">dos/cmd命令</param>
        /// <param name="outtime">等待执行时间毫秒值，默认不等待</param>
        /// <returns>控制台输出信息</returns>
        /// <exception cref="SystemException">尚未设置进程 <see cref="P:System.Diagnostics.Process.Id" />，而且不存在可从其确定 <see cref="P:System.Diagnostics.Process.Id" /> 属性的 <see cref="P:System.Diagnostics.Process.Handle" />。- 或 -没有与此 <see cref="T:System.Diagnostics.Process" /> 对象关联的进程。- 或 -您正尝试为远程计算机上运行的进程调用 <see cref="M:System.Diagnostics.Process.WaitForExit(System.Int32)" />。此方法仅对本地计算机上运行的进程可用。</exception>
        /// <exception cref="Win32Exception">未能访问该等待设置。</exception>
        /// <exception cref="Exception">命令参数无效，必须传入一个控制台能被cmd.exe可执行程序; 如：ping 127.0.0.1</exception>
        public static string Execute(string dosCommand, int outtime = 0)
        {
            string output = "";
            if (!string.IsNullOrEmpty(dosCommand))
            {
                using var process = new Process();
                var startinfo = new ProcessStartInfo(); //创建进程时使用的一组值，如下面的属性  
                startinfo.FileName = "cmd.exe"; //设定需要执行的命令程序  
                //以下是隐藏cmd窗口的方法  
                startinfo.Arguments = "/c" + dosCommand; //设定参数，要输入到命令程序的字符，其中"/c"表示执行完命令后马上退出  
                startinfo.UseShellExecute = false; //不使用系统外壳程序启动  
                startinfo.RedirectStandardInput = false; //不重定向输入  
                startinfo.RedirectStandardOutput = true; //重定向输出，而不是默认的显示在dos控制台上  
                startinfo.CreateNoWindow = true; //不创建窗口  
                process.StartInfo = startinfo;
                if (process.Start()) //开始进程  
                {
                    if (outtime == 0)
                    {
                        process.WaitForExit();
                    }
                    else
                    {
                        process.WaitForExit(outtime);
                    }

                    output = process.StandardOutput.ReadToEnd(); //读取进程的输出  
                }

                return output;
            }

            throw new Exception("命令参数无效，必须传入一个控制台能被cmd.exe可执行程序;\n如：ping 127.0.0.1");
        }
    }
}