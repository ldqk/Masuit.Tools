using System;
using System.Threading.Tasks;

namespace Masuit.Tools.Files
{
    /// <summary>
    /// SharpZip
    /// </summary>
    [Obsolete("该类已过时，请使用SevenZipCompressor替代")]
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
}