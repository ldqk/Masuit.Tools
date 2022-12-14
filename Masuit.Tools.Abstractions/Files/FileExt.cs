using System.IO;
using System.Security.Cryptography;
using System.Text;
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
        /// <param name="bufferSize">缓冲区大小，默认8MB</param>
        public static void CopyToFile(this Stream fs, string dest, int bufferSize = 1024 * 8 * 1024)
        {
            using var fsWrite = new FileStream(dest, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            byte[] buf = new byte[bufferSize];
            int len;
            while ((len = fs.Read(buf, 0, buf.Length)) != 0)
            {
                fsWrite.Write(buf, 0, len);
            }
        }

        /// <summary>
        /// 以文件流的形式复制大文件(异步方式)
        /// </summary>
        /// <param name="fs">源</param>
        /// <param name="dest">目标地址</param>
        /// <param name="bufferSize">缓冲区大小，默认8MB</param>
        public static async Task CopyToFileAsync(this Stream fs, string dest, int bufferSize = 1024 * 1024 * 8)
        {
            using var fsWrite = new FileStream(dest, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            byte[] buf = new byte[bufferSize];
            int len;
            while ((len = await fs.ReadAsync(buf, 0, buf.Length)) != 0)
            {
                await fsWrite.WriteAsync(buf, 0, len);
            }
        }

        /// <summary>
        /// 将内存流转储成文件
        /// </summary>
        /// <param name="ms"></param>
        /// <param name="filename"></param>
        public static void SaveFile(this Stream ms, string filename)
        {
            using var fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
            byte[] buffer = ms.ToArray(); // 转化为byte格式存储
            fs.Write(buffer, 0, buffer.Length);
            fs.Flush();
        }

        /// <summary>
        /// 计算文件的 MD5 值
        /// </summary>
        /// <param name="fs">源文件流</param>
        /// <returns>MD5 值16进制字符串</returns>
        public static string GetFileMD5(this FileStream fs) => HashFile(fs, "md5");

        /// <summary>
        /// 计算文件的 sha1 值
        /// </summary>
        /// <param name="fs">源文件流</param>
        /// <returns>sha1 值16进制字符串</returns>
        public static string GetFileSha1(this Stream fs) => HashFile(fs, "sha1");

        /// <summary>
        /// 计算文件的 sha256 值
        /// </summary>
        /// <param name="fs">源文件流</param>
        /// <returns>sha256 值16进制字符串</returns>
        public static string GetFileSha256(this Stream fs) => HashFile(fs, "sha256");

        /// <summary>
        /// 计算文件的 sha512 值
        /// </summary>
        /// <param name="fs">源文件流</param>
        /// <returns>sha512 值16进制字符串</returns>
        public static string GetFileSha512(this Stream fs) => HashFile(fs, "sha512");

        /// <summary>
        /// 计算文件的哈希值
        /// </summary>
        /// <param name="fs">被操作的源数据流</param>
        /// <param name="algo">加密算法</param>
        /// <returns>哈希值16进制字符串</returns>
        private static string HashFile(Stream fs, string algo)
        {
            using HashAlgorithm crypto = algo switch
            {
                "sha1" => SHA1.Create(),
                "sha256" => SHA256.Create(),
                "sha512" => SHA512.Create(),
                _ => MD5.Create(),
            };
            byte[] retVal = crypto.ComputeHash(fs);
            var sb = new StringBuilder();
            foreach (var t in retVal)
            {
                sb.Append(t.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
