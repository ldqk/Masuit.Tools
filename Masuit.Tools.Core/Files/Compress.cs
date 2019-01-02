using Microsoft.Win32;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Writers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Masuit.Tools.Files
{
    /// <summary>
    /// 7z压缩
    /// </summary>
    public static class SevenZipCompressor
    {
        /// <summary>
        /// 将多个文件压缩到一个文件流中，可保存为zip文件，方便于web方式下载
        /// </summary>
        /// <param name="files">多个文件路径，文件或文件夹，或网络路径http/https</param>
        /// <param name="rootdir"></param>
        /// <returns>文件流</returns>
        public static MemoryStream ZipStream(List<string> files, string rootdir = "")
        {
            using (var archive = CreateZipArchive(files, rootdir))
            {
                var ms = new MemoryStream();
                archive.SaveTo(ms, new WriterOptions(CompressionType.Deflate)
                {
                    LeaveStreamOpen = true,
                    ArchiveEncoding = new ArchiveEncoding()
                    {
                        Default = Encoding.UTF8
                    }
                });
                return ms;
            }
        }

        /// <summary>
        /// 压缩多个文件
        /// </summary>
        /// <param name="files">多个文件路径，文件或文件夹</param>
        /// <param name="zipFile">压缩到...</param>
        /// <param name="rootdir">压缩包内部根文件夹</param>
        public static void Zip(List<string> files, string zipFile, string rootdir = "")
        {
            using (var archive = CreateZipArchive(files, rootdir))
            {
                archive.SaveTo(zipFile, new WriterOptions(CompressionType.Deflate)
                {
                    LeaveStreamOpen = true,
                    ArchiveEncoding = new ArchiveEncoding()
                    {
                        Default = Encoding.UTF8
                    }
                });
            }
        }

        /// <summary>
        /// 解压rar文件
        /// </summary>
        /// <param name="rar">rar文件</param>
        /// <param name="dir">解压到...</param>
        /// <param name="ignoreEmptyDir">忽略空文件夹</param>
        public static void UnRar(string rar, string dir = "", bool ignoreEmptyDir = true)
        {
            if (string.IsNullOrEmpty(dir))
            {
                dir = Path.GetDirectoryName(rar);
            }
            using (var archive = RarArchive.Open(rar))
            {
                var entries = ignoreEmptyDir ? archive.Entries.Where(entry => !entry.IsDirectory) : archive.Entries;
                foreach (var entry in entries)
                {
                    entry.WriteToDirectory(dir, new ExtractionOptions()
                    {
                        ExtractFullPath = true,
                        Overwrite = true
                    });
                }
            }
        }

        /// <summary>
        /// 解压文件，自动检测压缩包类型
        /// </summary>
        /// <param name="compressedFile">rar文件</param>
        /// <param name="dir">解压到...</param>
        /// <param name="ignoreEmptyDir">忽略空文件夹</param>
        public static void Decompress(string compressedFile, string dir = "", bool ignoreEmptyDir = true)
        {
            if (string.IsNullOrEmpty(dir))
            {
                dir = Path.GetDirectoryName(compressedFile);
            }
            using (Stream stream = File.OpenRead(compressedFile))
            {
                using (var reader = ReaderFactory.Open(stream))
                {
                    while (reader.MoveToNextEntry())
                    {
                        if (ignoreEmptyDir)
                        {
                            reader.WriteEntryToDirectory(dir, new ExtractionOptions()
                            {
                                ExtractFullPath = true,
                                Overwrite = true
                            });
                        }
                        else
                        {
                            if (!reader.Entry.IsDirectory)
                            {
                                reader.WriteEntryToDirectory(dir, new ExtractionOptions()
                                {
                                    ExtractFullPath = true,
                                    Overwrite = true
                                });
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 创建zip包
        /// </summary>
        /// <param name="files"></param>
        /// <param name="rootdir"></param>
        /// <returns></returns>
        private static ZipArchive CreateZipArchive(List<string> files, string rootdir)
        {
            var archive = ZipArchive.Create();
            var dic = GetFileEntryMaps(files);
            var remoteFiles = files.Where(s => s.StartsWith("http")).ToList();
            foreach (var fileEntry in dic)
            {
                archive.AddEntry(Path.Combine(rootdir, fileEntry.Value), fileEntry.Key);
            }
            if (remoteFiles.Any())
            {
                //var paths = remoteFiles.Select(s => new Uri(s).PathAndQuery).ToList();
                //string pathname = new string(paths.First().Substring(0, paths.Min(s => s.Length)).TakeWhile((c, i) => paths.All(s => s[i] == c)).ToArray());
                //Dictionary<string, string> pathDic = paths.ToDictionary(s => s, s => s.Substring(pathname.Length));
                using (var httpClient = new HttpClient())
                {
                    Parallel.ForEach(remoteFiles, url =>
                    {
                        httpClient.GetAsync(url).ContinueWith(async t =>
                        {
                            if (t.IsCompleted)
                            {
                                var res = await t;
                                if (res.IsSuccessStatusCode)
                                {
                                    Stream stream = await res.Content.ReadAsStreamAsync();
                                    //archive.AddEntry(Path.Combine(rootdir, pathDic[new Uri(url).AbsolutePath.Trim('/')]), stream);
                                    archive.AddEntry(Path.Combine(rootdir, Path.GetFileName(new Uri(url).AbsolutePath.Trim('/'))), stream);
                                }
                            }
                        }).Wait();
                    });
                }
            }
            return archive;
        }

        /// <summary>
        /// 获取文件路径和zip-entry的映射
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        private static Dictionary<string, string> GetFileEntryMaps(List<string> files)
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

            files.Where(s => !s.StartsWith("http")).ForEach(s =>
            {
                if (Directory.Exists(s))
                {
                    GetFilesRecurs(s);
                }
                else
                {
                    fileList.Add(s);
                }
            });
            if (!fileList.Any())
            {
                return new Dictionary<string, string>();
            }
            string dirname = new string(fileList.First().Substring(0, fileList.Min(s => s.Length)).TakeWhile((c, i) => fileList.All(s => s[i] == c)).ToArray());
            Dictionary<string, string> dic = fileList.ToDictionary(s => s, s => s.Substring(dirname.Length));
            return dic;
        }
    }
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
        [Obsolete("该方法已过时，请使用SevenZipCompressor.Zip方法替代")]
        public static void PackFiles(string filename, string directory)
        {
            throw new NotImplementedException("该方法已过时，请使用SevenZipCompressor.Zip方法替代");
        }

        /// <summary>
        /// 文件压缩
        /// </summary> 
        /// <param name="filename"> 压缩后的文件名(包含物理路径)</param>
        /// <param name="directory">待压缩的文件夹(包含物理路径)</param>
        [Obsolete("该方法已过时，请使用SevenZipCompressor.Zip方法替代")]
        public static async void PackFilesAsync(string filename, string directory)
        {
            await Task.Delay(0);
            throw new NotImplementedException("该方法已过时，请使用SevenZipCompressor.Zip方法替代");
        }

        #endregion

        #region 文件解压缩

        /// <summary>
        /// 文件解压缩
        /// </summary>
        /// <param name="file">待解压文件名(包含物理路径)</param>
        /// <param name="dir"> 解压到哪个目录中(包含物理路径)</param>
        [Obsolete("该方法已过时，请使用SevenZipCompressor.Decompress方法替代")]
        public static bool UnpackFiles(string file, string dir)
        {
            throw new NotImplementedException("该方法已过时，请使用SevenZipCompressor.Decompress方法替代");

        }

        /// <summary>
        /// 文件解压缩
        /// </summary>
        /// <param name="file">待解压文件名(包含物理路径)</param>
        /// <param name="dir"> 解压到哪个目录中(包含物理路径)</param>
        [Obsolete("该方法已过时，请使用SevenZipCompressor.Decompress方法替代")]
        public static async Task<bool> UnpackFilesAsync(string file, string dir)
        {
            await Task.Delay(0);
            throw new NotImplementedException("该方法已过时，请使用SevenZipCompressor.Decompress方法替代");
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
        [Obsolete("该方法已过时，请使用SevenZipCompressor.Zip方法替代")]
        public static bool Zip(string fileToZip, string zipedFile, int level = 6)
        {
            throw new NotImplementedException("该方法已过时，请使用SevenZipCompressor.Zip方法替代");
        }

        /// <summary>
        /// 将多个文件压缩到一个文件流中，可保存为zip文件，方便于web方式下载
        /// </summary>
        /// <param name="files">多个文件路径，文件或文件夹</param>
        /// <returns>文件流</returns>
        [Obsolete("该方法已过时，请使用SevenZipCompressor.ZipStream方法替代")]
        public static byte[] ZipStream(List<string> files)
        {
            throw new NotImplementedException("该方法已过时，请使用SevenZipCompressor.ZipStream方法替代");
        }

        #endregion

        #region 解压

        /// <summary>
        /// 解压
        /// </summary>
        /// <param name="fileToUpZip">待解压的文件</param>
        /// <param name="zipedFolder">解压目标存放目录</param>
        [Obsolete("该方法已过时，请使用SevenZipCompressor.Decompress方法替代")]
        public static void UnZip(string fileToUpZip, string zipedFolder)
        {
            throw new NotImplementedException("该方法已过时，请使用SevenZipCompressor.Decompress方法替代");
        }

        #endregion
    }

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