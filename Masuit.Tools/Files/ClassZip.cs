using System;
using System.Collections.Generic;

namespace Masuit.Tools.Files
{
    /// <summary>
    /// ClassZip
    /// </summary>
    [Obsolete("该类已过时，请使用SevenZipCompressor替代")]
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
}