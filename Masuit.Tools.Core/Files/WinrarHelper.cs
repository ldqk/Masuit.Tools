using System.Diagnostics;
using Microsoft.Win32;

namespace Masuit.Tools.Files
{
    /// <summary>
    /// WinRAR压缩操作
    /// </summary>
    public static class WinrarHelper
    {
        #region 压缩

        /// <summary>
        /// 压缩
        /// </summary>
        /// <param name="zipname">要解压的文件名</param>
        /// <param name="zippath">要压缩的文件目录</param>
        /// <param name="dirpath">初始目录</param>
        public static void Rar(string zipname, string zippath, string dirpath)
        {
            _theReg = Registry.ClassesRoot.OpenSubKey(@"Applications\WinRAR.exe\Shell\Open\Command");
            if (_theReg != null)
            {
                _theObj = _theReg.GetValue("");
                _theRar = _theObj.ToString();
                _theReg?.Close();
            }

            _theRar = _theRar.Substring(1, _theRar.Length - 7);
            _theInfo = " a  " + zipname + " " + zippath;
            _theStartInfo = new ProcessStartInfo
            {
                FileName = _theRar,
                Arguments = _theInfo,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = dirpath
            };
            _theProcess = new Process
            {
                StartInfo = _theStartInfo
            };
            _theProcess.Start();
        }

        #endregion

        #region 解压缩

        /// <summary>
        /// 解压缩
        /// </summary>
        /// <param name="zipname">要解压的文件名</param>
        /// <param name="zippath">要解压的文件路径</param>
        public static void UnRar(string zipname, string zippath)
        {
            _theReg = Registry.ClassesRoot.OpenSubKey(@"Applications\WinRar.exe\Shell\Open\Command");
            if (_theReg != null)
            {
                _theObj = _theReg.GetValue("");
                _theRar = _theObj.ToString();
                _theReg.Close();
            }

            _theRar = _theRar.Substring(1, _theRar.Length - 7);
            _theInfo = " X " + zipname + " " + zippath;
            _theStartInfo = new ProcessStartInfo
            {
                FileName = _theRar,
                Arguments = _theInfo,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            _theProcess = new Process
            {
                StartInfo = _theStartInfo
            };
            _theProcess.Start();
        }

        #endregion

        #region 私有变量

        private static string _theRar;
        private static RegistryKey _theReg;
        private static object _theObj;
        private static string _theInfo;
        private static ProcessStartInfo _theStartInfo;
        private static Process _theProcess;

        #endregion
    }
}