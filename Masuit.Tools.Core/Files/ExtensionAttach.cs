using Microsoft.Win32;
using System;
using System.IO;
using System.Security;

namespace Masuit.Tools.Files
{
    /// <summary>
    /// 文件关联
    /// </summary>
    public static class ExtensionAttach
    {
        /// <summary>
        /// 关联文件
        /// </summary>
        /// <param name="filePathString">应用程序路径</param>
        /// <param name="pFileTypeName">文件类型</param>
        /// <exception cref="SecurityException">The user does not have the permissions required to access the registry key in the specified mode. </exception>
        /// <exception cref="UnauthorizedAccessException">The user does not have the necessary registry rights.</exception>
        /// <exception cref="IOException">The nesting level exceeds 510.-or-A system error occurred, such as deletion of the key, or an attempt to create a key in the <see cref="F:Microsoft.Win32.Registry.LocalMachine" /> root.</exception>
        public static void SaveReg(string filePathString, string pFileTypeName)
        {
            RegistryKey regKey = Registry.ClassesRoot.OpenSubKey("", true); //打开注册表
            RegistryKey vrPkey = regKey?.OpenSubKey(pFileTypeName, true);
            if (vrPkey != null)
            {
                regKey.DeleteSubKey(pFileTypeName, true);
            }

            regKey?.CreateSubKey(pFileTypeName);
            vrPkey = regKey?.OpenSubKey(pFileTypeName, true);
            vrPkey?.SetValue("", "Exec");
            vrPkey = regKey?.OpenSubKey("Exec", true);
            if (vrPkey != null) regKey.DeleteSubKeyTree("Exec"); //如果等于空就删除注册表DSKJIVR

            regKey?.CreateSubKey("Exec");
            vrPkey = regKey?.OpenSubKey("Exec", true);
            vrPkey?.CreateSubKey("shell");
            vrPkey = vrPkey?.OpenSubKey("shell", true); //写入必须路径
            vrPkey?.CreateSubKey("open");
            vrPkey = vrPkey?.OpenSubKey("open", true);
            vrPkey?.CreateSubKey("command");
            vrPkey = vrPkey?.OpenSubKey("command", true);
            string pathString = "\"" + filePathString + "\" \"%1\"";
            vrPkey?.SetValue("", pathString); //写入数据
        }

        /// <summary>
        /// 取消文件关联
        /// </summary>
        /// <param name="pFileTypeName">文件类型</param>
        /// <exception cref="SecurityException">The user does not have the permissions required to access the registry key in the specified mode. </exception>
        /// <exception cref="UnauthorizedAccessException">The user does not have the necessary registry rights.</exception>
        /// <exception cref="IOException">An I/O error has occurred.</exception>
        public static void DelReg(string pFileTypeName)
        {
            RegistryKey regkey = Registry.ClassesRoot.OpenSubKey("", true);
            RegistryKey vrPkey = regkey?.OpenSubKey(pFileTypeName);
            if (vrPkey != null)
            {
                regkey.DeleteSubKey(pFileTypeName, true);
            }

            if (vrPkey != null)
            {
                regkey.DeleteSubKeyTree("Exec");
            }
        }
    }
}