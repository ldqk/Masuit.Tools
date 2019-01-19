using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Writers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Masuit.Tools.Files
{
    /// <summary>
    /// 7z压缩
    /// </summary>
    public class SevenZipCompressor : ISevenZipCompressor
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpClientFactory"></param>
        public SevenZipCompressor(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        /// <summary>
        /// 将多个文件压缩到一个文件流中，可保存为zip文件，方便于web方式下载
        /// </summary>
        /// <param name="files">多个文件路径，文件或文件夹，或网络路径http/https</param>
        /// <param name="rootdir"></param>
        /// <returns>文件流</returns>
        public MemoryStream ZipStream(List<string> files, string rootdir = "")
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
        public void Zip(List<string> files, string zipFile, string rootdir = "")
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
        public void UnRar(string rar, string dir = "", bool ignoreEmptyDir = true)
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
        public void Extract(string compressedFile, string dir = "", bool ignoreEmptyDir = true) => Decompress(compressedFile, dir, ignoreEmptyDir);

        /// <summary>
        /// 解压文件，自动检测压缩包类型
        /// </summary>
        /// <param name="compressedFile">rar文件</param>
        /// <param name="dir">解压到...</param>
        /// <param name="ignoreEmptyDir">忽略空文件夹</param>
        public void Decompress(string compressedFile, string dir = "", bool ignoreEmptyDir = true)
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
        private ZipArchive CreateZipArchive(List<string> files, string rootdir)
        {
            var archive = ZipArchive.Create();
            var dic = GetFileEntryMaps(files);
            var remoteUrls = files.Distinct().Where(s => s.StartsWith("http")).Select(s =>
            {
                try
                {
                    return new Uri(s);
                }
                catch (UriFormatException)
                {
                    return null;
                }
            }).Where(u => u != null).ToList();
            foreach (var fileEntry in dic)
            {
                archive.AddEntry(Path.Combine(rootdir, fileEntry.Value), fileEntry.Key);
            }

            if (remoteUrls.Any())
            {
                var streams = new ConcurrentDictionary<string, Stream>();
                Parallel.ForEach(remoteUrls, url =>
                {
                    _httpClient.GetAsync(url).ContinueWith(async t =>
                    {
                        if (t.IsCompleted)
                        {
                            var res = await t;
                            if (res.IsSuccessStatusCode)
                            {
                                Stream stream = await res.Content.ReadAsStreamAsync();
                                streams[Path.Combine(rootdir, Path.GetFileName(HttpUtility.UrlDecode(url.AbsolutePath)))] = stream;
                            }
                        }
                    }).Wait();
                });
                foreach (var kv in streams)
                {
                    archive.AddEntry(kv.Key, kv.Value);
                }
            }

            return archive;
        }

        /// <summary>
        /// 获取文件路径和zip-entry的映射
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        private Dictionary<string, string> GetFileEntryMaps(List<string> files)
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
}