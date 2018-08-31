using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Checksum;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Win32;

namespace Masuit.Tools.Files
{
    /// <summary>
    /// SharpZip
    /// </summary>
    public static class SharpZip
    {
        #region 文件压缩

        /// <summary>
        /// 文件压缩
        /// </summary> 
        /// <param name="filename"> 压缩后的文件名(包含物理路径)</param>
        /// <param name="directory">待压缩的文件夹(包含物理路径)</param>
        public static void PackFiles(string filename, string directory)
        {
            FastZip fz = new FastZip { CreateEmptyDirectories = true };
            fz.CreateZip(filename, directory, true, "");
        }

        /// <summary>
        /// 文件压缩
        /// </summary> 
        /// <param name="filename"> 压缩后的文件名(包含物理路径)</param>
        /// <param name="directory">待压缩的文件夹(包含物理路径)</param>
        public static async void PackFilesAsync(string filename, string directory)
        {
            FastZip fz = new FastZip { CreateEmptyDirectories = true };
            await Task.Run(() =>
            {
                fz.CreateZip(filename, directory, true, "");
                fz = null;
            }).ConfigureAwait(false);
        }

        #endregion

        #region 文件解压缩

        /// <summary>
        /// 文件解压缩
        /// </summary>
        /// <param name="file">待解压文件名(包含物理路径)</param>
        /// <param name="dir"> 解压到哪个目录中(包含物理路径)</param>
        public static bool UnpackFiles(string file, string dir)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            using (ZipInputStream s = new ZipInputStream(File.OpenRead(file)))
            {
                ZipEntry theEntry;
                while ((theEntry = s.GetNextEntry()) != null)
                {
                    string directoryName = Path.GetDirectoryName(theEntry.Name);
                    string fileName = Path.GetFileName(theEntry.Name);
                    if (directoryName != string.Empty)
                        Directory.CreateDirectory(dir + directoryName);
                    if (fileName != string.Empty)
                    {
                        using (FileStream streamWriter = File.Create(dir + theEntry.Name))
                        {
                            byte[] data = new byte[2048];
                            while (true)
                            {
                                var size = s.Read(data, 0, data.Length);
                                if (size > 0)
                                    streamWriter.Write(data, 0, size);
                                else
                                    break;
                            }
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 文件解压缩
        /// </summary>
        /// <param name="file">待解压文件名(包含物理路径)</param>
        /// <param name="dir"> 解压到哪个目录中(包含物理路径)</param>
        public static async Task<bool> UnpackFilesAsync(string file, string dir)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            using (ZipInputStream s = new ZipInputStream(File.OpenRead(file)))
            {
                ZipEntry theEntry;
                await Task.Run(() =>
                {
                    while ((theEntry = s.GetNextEntry()) != null)
                    {
                        string directoryName = Path.GetDirectoryName(theEntry.Name);
                        string fileName = Path.GetFileName(theEntry.Name);
                        if (directoryName != string.Empty)
                            Directory.CreateDirectory(dir + directoryName);
                        if (fileName != string.Empty)
                        {
                            using (FileStream streamWriter = File.Create(dir + theEntry.Name))
                            {
                                byte[] data = new byte[2048];
                                while (true)
                                {
                                    var size = s.Read(data, 0, data.Length);
                                    if (size > 0)
                                        streamWriter.Write(data, 0, size);
                                    else
                                        break;
                                }
                            }
                        }
                    }
                }).ConfigureAwait(false);
            }
            return true;
        }

        #endregion
    }

    /// <summary>
    /// ClassZip
    /// </summary>
    public static class ClassZip
    {
        #region 压缩

        /// <summary>
        /// 压缩
        /// </summary>
        /// <param name="fileToZip">待压缩的文件目录或文件</param>
        /// <param name="zipedFile">生成的目标文件</param>
        /// <param name="level">压缩级别，默认值6</param>
        public static bool Zip(string fileToZip, string zipedFile, int level = 6)
        {
            if (Directory.Exists(fileToZip))
                return ZipFileDictory(fileToZip, zipedFile, level);
            return File.Exists(fileToZip) && ZipFile(fileToZip, zipedFile, level);
        }

        /// <summary>
        /// 将多个文件压缩到一个文件流中，可保存为zip文件，方便于web方式下载
        /// </summary>
        /// <param name="files">多个文件路径，文件或文件夹</param>
        /// <returns>文件流</returns>
        public static byte[] ZipStream(List<string> files)
        {
            List<string> fileList = new List<string>();

            void GetFilesRecurs(string path)
            {
                //遍历目标文件夹的所有文件
                foreach (string fileName in Directory.GetFiles(path))
                {
                    fileList.Add(fileName);
                }

                //遍历目标文件夹的所有文件夹
                foreach (string directory in Directory.GetDirectories(path))
                {
                    GetFilesRecurs(directory);
                }
            }
            MemoryStream ms = new MemoryStream();
            byte[] buffer;
            using (ZipFile f = ICSharpCode.SharpZipLib.Zip.ZipFile.Create(ms))
            {
                f.BeginUpdate();
                string dirname = null;
                files.ForEach(s =>
                {
                    if (Directory.Exists(s))
                    {
                        GetFilesRecurs(s);
                    }
                    else
                    {
                        fileList.Add(s);
                        dirname = Path.GetDirectoryName(s);
                    }
                });
                if (string.IsNullOrEmpty(dirname))
                {
                    dirname = Directory.GetParent(fileList[0]).FullName;
                }
                f.NameTransform = new ZipNameTransform(dirname); //通过这个名称格式化器，可以将里面的文件名进行一些处理。默认情况下，会自动根据文件的路径在zip中创建有关的文件夹。  
                fileList.ForEach(s => { f.Add(s); });
                f.CommitUpdate();
                buffer = new byte[ms.Length];
                ms.Position = 0;
                ms.Read(buffer, 0, buffer.Length);
            }
            return buffer;
        }
        #endregion

        #region 解压

        /// <summary>
        /// 解压
        /// </summary>
        /// <param name="fileToUpZip">待解压的文件</param>
        /// <param name="zipedFolder">解压目标存放目录</param>
        public static void UnZip(string fileToUpZip, string zipedFolder)
        {
            if (!File.Exists(fileToUpZip))
                return;
            if (!Directory.Exists(zipedFolder))
                Directory.CreateDirectory(zipedFolder);
            using (ZipInputStream s = new ZipInputStream(File.OpenRead(fileToUpZip)))
            {
                ZipEntry theEntry;
                while ((theEntry = s.GetNextEntry()) != null)
                {
                    if (theEntry.Name != string.Empty)
                    {
                        var fileName = Path.Combine(zipedFolder, theEntry.Name);
                        if (fileName.EndsWith("/") || fileName.EndsWith("\\"))
                        {
                            Directory.CreateDirectory(fileName);
                            continue;
                        }
                        using (FileStream streamWriter = File.Create(fileName))
                        {
                            byte[] data = new byte[2048];
                            while (true)
                            {
                                var size = s.Read(data, 0, data.Length);
                                if (size > 0)
                                    streamWriter.Write(data, 0, size);
                                else
                                    break;
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region 私有方法

        #region 递归压缩文件夹方法

        /// <summary>
        /// 递归压缩文件夹方法
        /// </summary>
        /// <param name="folderToZip">需要压缩的文件夹</param>
        /// <param name="s">压缩流</param>
        /// <param name="parentFolderName">父级文件夹</param>
        private static bool ZipFileDictory(string folderToZip, ZipOutputStream s, string parentFolderName)
        {
            Crc32 crc = new Crc32();
            var entry = new ZipEntry(Path.Combine(parentFolderName, Path.GetFileName(folderToZip) + "/"));
            s.PutNextEntry(entry);
            s.Flush();
            var filenames = Directory.GetFiles(folderToZip);
            foreach (string file in filenames)
            {
                using (FileStream fs = File.OpenRead(file))
                {
                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);
                    entry = new ZipEntry(Path.Combine(parentFolderName, Path.GetFileName(folderToZip) + "/" + Path.GetFileName(file)))
                    {
                        DateTime = DateTime.Now,
                        Size = fs.Length
                    };
                    crc.Reset();
                    crc.Update(buffer);
                    entry.Crc = crc.Value;
                    s.PutNextEntry(entry);
                    s.Write(buffer, 0, buffer.Length);
                }
            }
            var folders = Directory.GetDirectories(folderToZip);
            foreach (string folder in folders)
            {
                if (!ZipFileDictory(folder, s, Path.Combine(parentFolderName, Path.GetFileName(folderToZip))))
                    return false;
            }

            return true;
        }

        #endregion

        #region 压缩目录

        /// <summary>
        /// 压缩目录
        /// </summary>
        /// <param name="folderToZip">待压缩的文件夹，全路径格式</param>
        /// <param name="zipedFile">压缩后的文件名，全路径格式</param>
        /// <param name="level">压缩等级</param>
        private static bool ZipFileDictory(string folderToZip, string zipedFile, int level)
        {
            bool res;
            if (!Directory.Exists(folderToZip))
                return false;
            using (ZipOutputStream s = new ZipOutputStream(File.Create(zipedFile)))
            {
                s.SetLevel(level);
                res = ZipFileDictory(folderToZip, s, "");
                s.Finish();
            }
            return res;
        }

        #endregion

        #region 压缩文件

        /// <summary>
        /// 压缩文件
        /// </summary>
        /// <param name="fileToZip">要进行压缩的文件名</param>
        /// <param name="zipedFile">压缩后生成的压缩文件名</param>
        /// <param name="level">压缩等级</param>
        /// <exception cref="FileNotFoundException"></exception>
        private static bool ZipFile(string fileToZip, string zipedFile, int level)
        {
            if (!File.Exists(fileToZip))
                throw new FileNotFoundException("指定要压缩的文件: " + fileToZip + " 不存在!");
            FileStream zipFile = File.OpenRead(fileToZip);
            byte[] buffer = new byte[zipFile.Length];
            zipFile.Read(buffer, 0, buffer.Length);
            zipFile = File.Create(zipedFile);
            using (zipFile)
            {
                using (ZipOutputStream zipStream = new ZipOutputStream(zipFile))
                {
                    var zipEntry = new ZipEntry(Path.GetFileName(fileToZip));
                    zipStream.PutNextEntry(zipEntry);
                    zipStream.SetLevel(level);
                    zipStream.Write(buffer, 0, buffer.Length);
                }
            }
            return true;
        }

        #endregion

        #endregion
    }

    /// <summary>
    /// WinRAR压缩操作
    /// </summary>
    public static class ZipHelper
    {
        #region 压缩

        /// <summary>
        /// 压缩
        /// </summary>
        /// <param name="zipname">要解压的文件名</param>
        /// <param name="zippath">要压缩的文件目录</param>
        /// <param name="dirpath">初始目录</param>
        public static void EnZip(string zipname, string zippath, string dirpath)
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
        public static void DeZip(string zipname, string zippath)
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