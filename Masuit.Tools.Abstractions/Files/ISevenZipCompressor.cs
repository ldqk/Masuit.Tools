using System.Collections.Generic;
using System.IO;

namespace Masuit.Tools.Files
{
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
        /// 解压文件，自动检测压缩包类型
        /// </summary>
        /// <param name="compressedFile">rar文件</param>
        /// <param name="dir">解压到...</param>
        /// <param name="ignoreEmptyDir">忽略空文件夹</param>
        void Extract(string compressedFile, string dir = "", bool ignoreEmptyDir = true);

        /// <summary>
        /// 解压rar文件
        /// </summary>
        /// <param name="rar">rar文件</param>
        /// <param name="dir">解压到...</param>
        /// <param name="ignoreEmptyDir">忽略空文件夹</param>
        void UnRar(string rar, string dir = "", bool ignoreEmptyDir = true);

        /// <summary>
        /// 压缩多个文件
        /// </summary>
        /// <param name="files">多个文件路径，文件或文件夹</param>
        /// <param name="zipFile">压缩到...</param>
        /// <param name="rootdir">压缩包内部根文件夹</param>
        void Zip(List<string> files, string zipFile, string rootdir = "");

        /// <summary>
        /// 将多个文件压缩到一个文件流中，可保存为zip文件，方便于web方式下载
        /// </summary>
        /// <param name="files">多个文件路径，文件或文件夹，或网络路径http/https</param>
        /// <param name="rootdir"></param>
        /// <returns>文件流</returns>
        MemoryStream ZipStream(List<string> files, string rootdir = "");
    }
}