using Masuit.Tools.Systems;
using SharpCompress.Common;
using System.Collections.Generic;
using System.IO;

namespace Masuit.Tools.Files;

/// <summary>
/// 7z压缩
/// </summary>
public interface ISevenZipCompressor
{
    /// <summary>
    /// 解压文件，自动检测压缩包类型
    /// </summary>
    /// <param name="compressedFile">rar文件</param>
    /// <param name="dir">解压到...</param>
    /// <param name="ignoreEmptyDir">忽略空文件夹</param>
    void Decompress(string compressedFile, string dir = "", bool ignoreEmptyDir = true);

    /// <summary>
    /// 压缩多个文件
    /// </summary>
    /// <param name="files">多个文件路径，文件或文件夹</param>
    /// <param name="zipFile">压缩到...</param>
    /// <param name="rootdir">压缩包内部根文件夹</param>
    void Zip(IEnumerable<string> files, string zipFile, string rootdir = "");

    /// <summary>
    /// 压缩多个文件
    /// </summary>
    /// <param name="streams">多个文件流</param>
    /// <param name="zipFile">压缩到...</param>
    /// <param name="disposeAllStreams">是否需要释放所有流</param>
    void Zip(DisposableDictionary<string, Stream> streams, string zipFile, bool disposeAllStreams = false);

    /// <summary>
    /// 压缩文件夹
    /// </summary>
    /// <param name="dir">文件夹</param>
    /// <param name="zipFile">压缩到...</param>
    /// <param name="rootdir">压缩包内部根文件夹</param>
    public void Zip(string dir, string zipFile, string rootdir = "");

    /// <summary>
    /// 将多个文件压缩到一个文件流中，可保存为zip文件，方便于web方式下载
    /// </summary>
    /// <param name="files">多个文件路径，文件或文件夹，或网络路径http/https</param>
    /// <param name="rootdir"></param>
    /// <returns>文件流</returns>
    PooledMemoryStream ZipStream(IEnumerable<string> files, string rootdir = "");

    /// <summary>
    /// 将多个文件压缩到一个文件流中，可保存为zip文件，方便于web方式下载
    /// </summary>
    /// <param name="streams">多个文件流</param>
    /// <param name="disposeAllStreams">是否需要释放所有流</param>
    /// <returns>文件流</returns>
    PooledMemoryStream ZipStream(DisposableDictionary<string, Stream> streams, bool disposeAllStreams = false);

    /// <summary>
    /// 将文件夹压缩到一个文件流中，可保存为zip文件，方便于web方式下载
    /// </summary>
    /// <param name="dir">文件夹</param>
    /// <param name="rootdir"></param>
    /// <returns>文件流</returns>
    public PooledMemoryStream ZipStream(string dir, string rootdir = "");
    /// <summary>将文件或文件夹压缩到 7z 文件。</summary>
    /// <param name="files">文件、文件夹或 HTTP/HTTPS 地址。</param>
    /// <param name="sevenZipFile">目标 7z 文件。</param>
    /// <param name="rootdir">压缩包内部根文件夹。</param>
    void SevenZip(IEnumerable<string> files, string sevenZipFile, string rootdir = "");

    /// <summary>将文件夹压缩到 7z 文件。</summary>
    /// <param name="dir">源文件夹。</param>
    /// <param name="sevenZipFile">目标 7z 文件。</param>
    /// <param name="rootdir">压缩包内部根文件夹。</param>
    void SevenZip(string dir, string sevenZipFile, string rootdir = "");

    /// <summary>将文件流压缩到 7z 文件。</summary>
    /// <param name="streams">条目名称和文件流。</param>
    /// <param name="sevenZipFile">目标 7z 文件。</param>
    /// <param name="disposeAllStreams">是否释放输入流。</param>
    void SevenZip(DisposableDictionary<string, Stream> streams, string sevenZipFile, bool disposeAllStreams = false);

    /// <summary>将文件或文件夹压缩到 7z 流。</summary>
    /// <param name="files">文件、文件夹或 HTTP/HTTPS 地址。</param>
    /// <param name="rootdir">压缩包内部根文件夹。</param>
    /// <returns>包含 7z 压缩包的流。</returns>
    PooledMemoryStream SevenZipStream(IEnumerable<string> files, string rootdir = "");

    /// <summary>将文件夹压缩到 7z 流。</summary>
    /// <param name="dir">源文件夹。</param>
    /// <param name="rootdir">压缩包内部根文件夹。</param>
    /// <returns>包含 7z 压缩包的流。</returns>
    PooledMemoryStream SevenZipStream(string dir, string rootdir = "");

    /// <summary>将文件流压缩到 7z 流。</summary>
    /// <param name="streams">条目名称和文件流。</param>
    /// <param name="disposeAllStreams">是否释放输入流。</param>
    /// <returns>包含 7z 压缩包的流。</returns>
    PooledMemoryStream SevenZipStream(DisposableDictionary<string, Stream> streams, bool disposeAllStreams = false);
}