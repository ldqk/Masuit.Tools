using Masuit.Tools.Systems;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Writers.Zip;
using System.Net.Http;
using System.Text;
using System.Web;
using SharpCompress.Archives.Zip;
using SharpCompress.Writers;
using SharpCompress.Writers.SevenZip;

namespace Masuit.Tools.Files;

/// <summary>
/// 7z压缩
/// </summary>
public class SevenZipCompressor : ISevenZipCompressor
{
    private readonly HttpClient _httpClient;

    /// <summary>
    ///
    /// </summary>
    /// <param name="httpClient"></param>
    public SevenZipCompressor(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// 将多个文件压缩到一个文件流中，可保存为zip文件，方便于web方式下载
    /// </summary>
    /// <param name="files">多个文件路径，文件或文件夹，或网络路径http/https</param>
    /// <param name="rootdir"></param>
    /// <returns>文件流</returns>
    public PooledMemoryStream ZipStream(IEnumerable<string> files, string rootdir = "")
    {
        using var archive = CreateZipArchive(files, rootdir);
        var ms = new PooledMemoryStream();
        archive.SaveTo(ms, new ZipWriterOptions(CompressionType.Deflate)
        {
            LeaveStreamOpen = true,
            ArchiveEncoding = new ArchiveEncoding()
            {
                Default = Encoding.UTF8
            }
        });
        return ms;
    }

    /// <summary>
    /// 将文件夹压缩到一个文件流中，可保存为zip文件，方便于web方式下载
    /// </summary>
    /// <param name="dir">文件夹</param>
    /// <param name="rootdir"></param>
    /// <returns>文件流</returns>
    public PooledMemoryStream ZipStream(string dir, string rootdir = "")
    {
        return ZipStream(new[] { dir }, rootdir);
    }

    /// <summary>
    /// 将多个文件压缩到一个文件流中，可保存为zip文件，方便于web方式下载
    /// </summary>
    /// <param name="streams">多个文件流</param>
    /// <param name="disposeAllStreams">是否需要释放所有流</param>
    /// <returns>文件流</returns>
    public PooledMemoryStream ZipStream(DisposableDictionary<string, Stream> streams, bool disposeAllStreams = false)
    {
        using var archive = CreateStreamArchive(streams);

        var ms = new PooledMemoryStream();
        archive.SaveTo(ms, new ZipWriterOptions(CompressionType.Deflate)
        {
            LeaveStreamOpen = true,
            ArchiveEncoding = new ArchiveEncoding()
            {
                Default = Encoding.UTF8
            }
        });
        if (disposeAllStreams)
        {
            streams.Dispose();
        }
        return ms;
    }

    /// <summary>
    /// 压缩文件夹
    /// </summary>
    /// <param name="dir">文件夹</param>
    /// <param name="zipFile">压缩到...</param>
    /// <param name="rootdir">压缩包内部根文件夹</param>
    public void Zip(string dir, string zipFile, string rootdir = "")
    {
        Zip(new[] { dir }, zipFile, rootdir);
    }

    /// <summary>
    /// 压缩多个文件
    /// </summary>
    /// <param name="files">多个文件路径，文件或文件夹</param>
    /// <param name="zipFile">压缩到...</param>
    /// <param name="rootdir">压缩包内部根文件夹</param>
    public void Zip(IEnumerable<string> files, string zipFile, string rootdir = "")
    {
        using var archive = CreateZipArchive(files, rootdir);
        archive.SaveTo(zipFile, new ZipWriterOptions(CompressionType.Deflate)
        {
            LeaveStreamOpen = true,
            ArchiveEncoding = new ArchiveEncoding()
            {
                Default = Encoding.UTF8
            }
        });
    }

    /// <summary>
    /// 压缩多个文件
    /// </summary>
    /// <param name="streams">多个文件流</param>
    /// <param name="zipFile">压缩到...</param>
    /// <param name="disposeAllStreams">是否需要释放所有流</param>
    public void Zip(DisposableDictionary<string, Stream> streams, string zipFile, bool disposeAllStreams = false)
    {
        using var archive = CreateStreamArchive(streams);
        archive.SaveTo(zipFile, new ZipWriterOptions(CompressionType.Deflate)
        {
            LeaveStreamOpen = true,
            ArchiveEncoding = new ArchiveEncoding()
            {
                Default = Encoding.UTF8
            }
        });
        if (disposeAllStreams)
        {
            streams.Dispose();
        }
    }

    /// <summary>
    /// 解压文件，自动检测压缩包类型
    /// </summary>
    /// <param name="compressedFile">rar文件</param>
    /// <param name="dir">解压到...</param>
    /// <param name="ignoreEmptyDir">忽略空文件夹</param>
    public void Decompress(string compressedFile, string dir, bool ignoreEmptyDir = true)
    {
        if (string.IsNullOrEmpty(dir))
        {
            dir = Path.GetDirectoryName(Path.GetFullPath(compressedFile));
        }

        ArchiveFactory.WriteToDirectory(compressedFile, Directory.CreateDirectory(dir).FullName, new ExtractionOptions(true, true));
    }

    /// <inheritdoc />
    public void SevenZip(IEnumerable<string> files, string sevenZipFile, string rootdir = "")
    {
        using var archive = CreateZipArchive(files, rootdir);
        using var output = File.Create(sevenZipFile);
        WriteSevenZip(archive, output);
    }

    /// <inheritdoc />
    public void SevenZip(string dir, string sevenZipFile, string rootdir = "")
    {
        SevenZip(new[] { dir }, sevenZipFile, rootdir);
    }

    /// <inheritdoc />
    public void SevenZip(DisposableDictionary<string, Stream> streams, string sevenZipFile, bool disposeAllStreams = false)
    {
        using var archive = CreateStreamArchive(streams);
        using var output = File.Create(sevenZipFile);
        WriteSevenZip(archive, output);
        if (disposeAllStreams)
        {
            streams.Dispose();
        }
    }

    /// <inheritdoc />
    public PooledMemoryStream SevenZipStream(IEnumerable<string> files, string rootdir = "")
    {
        using var archive = CreateZipArchive(files, rootdir);
        return CreateSevenZipStream(archive);
    }

    /// <inheritdoc />
    public PooledMemoryStream SevenZipStream(string dir, string rootdir = "")
    {
        return SevenZipStream(new[] { dir }, rootdir);
    }

    /// <inheritdoc />
    public PooledMemoryStream SevenZipStream(DisposableDictionary<string, Stream> streams, bool disposeAllStreams = false)
    {
        using var archive = CreateStreamArchive(streams);
        var result = CreateSevenZipStream(archive);
        if (disposeAllStreams)
        {
            streams.Dispose();
        }
        return result;
    }

    private static IWritableArchive<ZipWriterOptions> CreateStreamArchive(DisposableDictionary<string, Stream> streams)
    {
        var archive = ZipArchive.CreateArchive();
        foreach (var pair in streams)
        {
            archive.AddEntry(((string)pair.Key).Replace('\\', '/'), pair.Value, false);
        }
        return archive;
    }

    private static PooledMemoryStream CreateSevenZipStream(IWritableArchive<ZipWriterOptions> archive)
    {
        var output = new PooledMemoryStream();
        try
        {
            WriteSevenZip(archive, output);
            return output;
        }
        catch
        {
            output.Dispose();
            throw;
        }
    }

    private static void WriteSevenZip(IWritableArchive<ZipWriterOptions> archive, Stream output)
    {
        using var writer = WriterFactory.OpenWriter(output, ArchiveType.SevenZip,
            new SevenZipWriterOptions(CompressionType.LZMA2) { LeaveStreamOpen = true,CompressionLevel = 9});
        foreach (var entry in archive.Entries)
        {
            using var source = entry.OpenEntryStream();
            writer.Write(entry.Key, source, entry.LastModifiedTime);
        }
    }

    /// <summary>
    /// 创建zip包
    /// </summary>
    /// <param name="files"></param>
    /// <param name="rootdir"></param>
    /// <returns></returns>
    private IWritableArchive<ZipWriterOptions> CreateZipArchive(IEnumerable<string> files, string rootdir)
    {
        var inputs = files.ToList();
        var archive = ZipArchive.CreateArchive();
        try
        {
            foreach (var pair in GetFileEntryMaps(inputs.Where(s => !IsRemote(s))))
            {
                archive.AddEntry(GetEntryName(rootdir, pair.Value), pair.Key);
            }

            foreach (var address in inputs.Where(IsRemote).Distinct())
            {
                var url = new Uri(address);
                using var response = _httpClient.GetAsync(url).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
                var bytes = response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                var name = Path.GetFileName(HttpUtility.UrlDecode(url.AbsolutePath));
                archive.AddEntry(GetEntryName(rootdir, name), new MemoryStream(bytes), true);
            }
            return archive;
        }
        catch
        {
            archive.Dispose();
            throw;
        }
    }

    private static bool IsRemote(string path) =>
        path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("https://", StringComparison.OrdinalIgnoreCase);

    private static string GetEntryName(string rootdir, string name) =>
        (rootdir.Trim('/', '\\') + "/" + name.TrimStart('/', '\\')).TrimStart('/').Replace('\\', '/');

    /// <summary>
    /// 获取文件路径和zip-entry的映射
    /// </summary>
    /// <param name="files"></param>
    /// <returns></returns>
    private Dictionary<string, string> GetFileEntryMaps(IEnumerable<string> files)
    {
        var fileList = new List<string>();
        var baseDirectories = new List<string>();
        void GetFilesRecurs(string path)
        {
            //遍历目标文件夹的所有文件
            fileList.AddRange(Directory.GetFiles(path));

            //遍历目标文件夹的所有文件夹
            foreach (var directory in Directory.GetDirectories(path))
            {
                GetFilesRecurs(directory);
            }
        }

        files.ForEach(s =>
        {
            if (Directory.Exists(s))
            {
                baseDirectories.Add(Path.GetFullPath(s));
                GetFilesRecurs(s);
            }
            else
            {
                var path = Path.GetFullPath(s);
                baseDirectories.Add(Path.GetDirectoryName(path));
                fileList.Add(path);
            }
        });

        if (!fileList.Any())
        {
            return new Dictionary<string, string>();
        }

        fileList = fileList.Select(Path.GetFullPath).ToList();
        var comparison = Path.DirectorySeparatorChar == '\\' ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        var dirname = baseDirectories[0];
        while (baseDirectories.Any(path => !path.Equals(dirname, comparison) &&
            !path.StartsWith(dirname.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar, comparison)))
        {
            var parent = Directory.GetParent(dirname);
            if (parent == null)
            {
                throw new ArgumentException("Files must share a common directory root.", nameof(files));
            }
            dirname = parent.FullName;
        }

        var prefixLength = dirname.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Length + 1;
        return fileList.ToDictionary(s => s, s => s.Substring(prefixLength));
    }
}