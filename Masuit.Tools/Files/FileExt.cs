using System;
using System.IO;
using System.Threading.Tasks;

namespace Masuit.Tools.Files
{
    /// <summary>
    /// 大文件操作扩展类
    /// </summary>
    public static class FileExt
    {
        /// <summary>
        /// 以文件流的形式复制大文件
        /// </summary>
        /// <param name="fs">源</param>
        /// <param name="dest">目标地址</param>
        public static void CopyToFile(this Stream fs, string dest)
        {
            using (FileStream fsWrite = new FileStream(dest, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                byte[] buf = new byte[1024 * 1024 * 8];
                int len = 0;
                while ((len = fs.Read(buf, 0, buf.Length)) != 0)
                    fsWrite.Write(buf, 0, len);
            }
        }

        /// <summary>
        /// 以文件流的形式复制大文件(异步方式)
        /// </summary>
        /// <param name="fs">源</param>
        /// <param name="dest">目标地址</param>
        public static async void CopyToFileAsync(this Stream fs, string dest)
        {
            using (FileStream fsWrite = new FileStream(dest, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                byte[] buf = new byte[1024 * 1024 * 8];
                int len = 0;
                await Task.Run(() =>
                {
                    while ((len = fs.Read(buf, 0, buf.Length)) != 0)
                        fsWrite.Write(buf, 0, len);
                }).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 计算文件的 MD5 值
        /// </summary>
        /// <param name="fs">源文件流</param>
        /// <returns>MD5 值16进制字符串</returns>
        public static string GetFileMD5(this FileStream fs)
        {
            return HashFile(fs, "md5");
        }

        /// <summary>
        /// 计算文件的 MD5 值
        /// </summary>
        /// <param name="fs">源文件流</param>
        /// <returns>MD5 值16进制字符串</returns>
        public static Task<string> GetFileMD5Async(this FileStream fs)
        {
            return HashFileAsync(fs, "md5");
        }

        /// <summary>
        /// 计算文件的 sha1 值
        /// </summary>
        /// <param name="fs">源文件流</param>
        /// <returns>sha1 值16进制字符串</returns>
        public static string GetFileSha1(this Stream fs)
        {
            return HashFile(fs, "sha1");
        }

        /// <summary>
        /// 计算文件的 sha1 值
        /// </summary>
        /// <param name="fs">源文件流</param>
        /// <returns>sha1 值16进制字符串</returns>
        public static Task<string> GetFileSha1Async(this FileStream fs)
        {
            return HashFileAsync(fs, "sha1");
        }

        /// <summary>
        /// 计算文件的哈希值
        /// </summary>
        /// <param name="fs">被操作的源数据流</param>
        /// <param name="algName">算法:sha1,md5</param>
        /// <returns>哈希值16进制字符串</returns>
        private static string HashFile(Stream fs, string algName)
        {
            byte[] hashBytes = HashData(fs, algName);
            return ByteArrayToHexString(hashBytes);
        }

        /// <summary>
        /// 计算文件的哈希值
        /// </summary>
        /// <param name="fs">被操作的源数据流</param>
        /// <param name="algName">算法:sha1,md5</param>
        /// <returns>哈希值16进制字符串</returns>
        private static async Task<string> HashFileAsync(Stream fs, string algName)
        {
            byte[] hashBytes = await HashDataAsync(fs, algName).ConfigureAwait(false);
            return ByteArrayToHexString(hashBytes);
        }

        /// <summary>
        /// 计算哈希值
        /// </summary>
        /// <param name="stream">要计算哈希值的 Stream</param>
        /// <param name="algName">算法:sha1,md5</param>
        /// <returns>哈希值字节数组</returns>
        private static byte[] HashData(System.IO.Stream stream, string algName)
        {
            System.Security.Cryptography.HashAlgorithm algorithm;
            if (algName == null)
            {
                throw new ArgumentNullException("algName 不能为 null");
            }
            if (string.Compare(algName, "sha1", true) == 0)
            {
                algorithm = System.Security.Cryptography.SHA1.Create();
            }
            else
            {
                if (string.Compare(algName, "md5", true) != 0)
                {
                    throw new Exception("algName 只能使用 sha1 或 md5");
                }
                algorithm = System.Security.Cryptography.MD5.Create();
            }
            return algorithm.ComputeHash(stream);
        }

        /// <summary>
        /// 计算哈希值
        /// </summary>
        /// <param name="stream">要计算哈希值的 Stream</param>
        /// <param name="algName">算法:sha1,md5</param>
        /// <returns>哈希值字节数组</returns>
        private static async Task<byte[]> HashDataAsync(Stream stream, string algName)
        {
            System.Security.Cryptography.HashAlgorithm algorithm;
            if (algName == null)
            {
                throw new ArgumentNullException("algName 不能为 null");
            }
            if (string.Compare(algName, "sha1", true) == 0)
            {
                algorithm = System.Security.Cryptography.SHA1.Create();
            }
            else
            {
                if (string.Compare(algName, "md5", true) != 0)
                {
                    throw new Exception("algName 只能使用 sha1 或 md5");
                }
                algorithm = System.Security.Cryptography.MD5.Create();
            }
            return await Task.Run(() => algorithm.ComputeHash(stream)).ConfigureAwait(false);

        }

        /// <summary>
        /// 字节数组转换为16进制表示的字符串
        /// </summary>
        private static string ByteArrayToHexString(byte[] buf)
        {
            return BitConverter.ToString(buf).Replace("-", "");
        }


    }
}