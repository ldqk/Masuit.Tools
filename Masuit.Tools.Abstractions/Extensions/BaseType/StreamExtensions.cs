using Masuit.Tools.Systems;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#if NET5_0_OR_GREATER
using System;
using System.Buffers;
using System.Runtime.InteropServices;
#endif

namespace Masuit.Tools
{
    public static class StreamExtensions
    {
        /// <summary>
        /// 将流转换为内存流
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static PooledMemoryStream SaveAsMemoryStream(this Stream stream)
        {
            if (stream is PooledMemoryStream pooledMemoryStream)
            {
                return pooledMemoryStream;
            }

            stream.Position = 0;
            var ms = new PooledMemoryStream();
            stream.CopyTo(ms);
            stream.Position = 0;
            return ms;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static byte[] ToArray(this Stream stream)
        {
            stream.Position = 0;
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);

            // 设置当前流的位置为流的开始
            stream.Seek(0, SeekOrigin.Begin);
            return bytes;
        }

        /// <summary>
        /// 流洗码，在流的末端随即增加几个空字节，重要数据请谨慎使用，可能造成流损坏
        /// </summary>
        /// <param name="stream"></param>
        public static void ShuffleCode(this Stream stream)
        {
            if (stream.CanWrite && stream.CanSeek)
            {
                var position = stream.Position;
                stream.Position = stream.Length;
                for (int i = 0; i < new Random().Next(1, 20); i++)
                {
                    stream.WriteByte(0);
                }
                stream.Flush();
                stream.Position = position;
            }
        }

        /// <summary>
        /// 读取所有行
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="closeAfter">读取完毕后关闭流</param>
        /// <returns></returns>
        public static List<string> ReadAllLines(this StreamReader stream, bool closeAfter = true)
        {
            var stringList = new List<string>();
            string str;
            while ((str = stream.ReadLine()) != null)
            {
                stringList.Add(str);
            }

            if (closeAfter)
            {
                stream.Close();
                stream.Dispose();
            }
            return stringList;
        }

        /// <summary>
        /// 读取所有行
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="encoding"></param>
        /// <param name="closeAfter">读取完毕后关闭流</param>
        /// <returns></returns>
        public static List<string> ReadAllLines(this FileStream stream, Encoding encoding, bool closeAfter = true)
        {
            var stringList = new List<string>();
            string str;
            var sr = new StreamReader(stream, encoding);
            while ((str = sr.ReadLine()) != null)
            {
                stringList.Add(str);
            }

            if (closeAfter)
            {
                sr.Close();
                sr.Dispose();
                stream.Close();
                stream.Dispose();
            }

            return stringList;
        }

        /// <summary>
        /// 读取所有文本
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="encoding"></param>
        /// <param name="closeAfter">读取完毕后关闭流</param>
        /// <returns></returns>
        public static string ReadAllText(this FileStream stream, Encoding encoding, bool closeAfter = true)
        {
            var sr = new StreamReader(stream, encoding);
            var text = sr.ReadToEnd();
            if (closeAfter)
            {
                sr.Close();
                sr.Dispose();
                stream.Close();
                stream.Dispose();
            }

            return text;
        }

        /// <summary>
        /// 写入所有文本
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="content"></param>
        /// <param name="encoding"></param>
        /// <param name="closeAfter">读取完毕后关闭流</param>
        /// <returns></returns>
        public static void WriteAllText(this FileStream stream, string content, Encoding encoding, bool closeAfter = true)
        {
            var sw = new StreamWriter(stream, encoding);
            stream.SetLength(0);
            sw.Write(content);
            if (closeAfter)
            {
                sw.Close();
                sw.Dispose();
                stream.Close();
                stream.Dispose();
            }
        }

        /// <summary>
        /// 写入所有文本行
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="lines"></param>
        /// <param name="encoding"></param>
        /// <param name="closeAfter">读取完毕后关闭流</param>
        /// <returns></returns>
        public static void WriteAllLines(this FileStream stream, IEnumerable<string> lines, Encoding encoding, bool closeAfter = true)
        {
            var sw = new StreamWriter(stream, encoding);
            stream.SetLength(0);
            foreach (var line in lines)
            {
                sw.WriteLine(line);
            }

            sw.Flush();
            if (closeAfter)
            {
                sw.Close();
                sw.Dispose();
                stream.Close();
                stream.Dispose();
            }
        }

        /// <summary>
        /// 共享读写打开文件
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static FileStream ShareReadWrite(this FileInfo file)
        {
            return file.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
        }

        /// <summary>
        /// 读取所有行
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="closeAfter">读取完毕后关闭流</param>
        /// <returns></returns>
        public static async Task<List<string>> ReadAllLinesAsync(this StreamReader stream, bool closeAfter = true)
        {
            var stringList = new List<string>();
            string str;
            while ((str = await stream.ReadLineAsync().ConfigureAwait(false)) != null)
            {
                stringList.Add(str);
            }

            if (closeAfter)
            {
                stream.Close();
                stream.Dispose();
            }
            return stringList;
        }

        /// <summary>
        /// 读取所有行
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="encoding"></param>
        /// <param name="closeAfter">读取完毕后关闭流</param>
        /// <returns></returns>
        public static async Task<List<string>> ReadAllLinesAsync(this FileStream stream, Encoding encoding, bool closeAfter = true)
        {
            var stringList = new List<string>();
            string str;
            var sr = new StreamReader(stream, encoding);
            while ((str = await sr.ReadLineAsync().ConfigureAwait(false)) != null)
            {
                stringList.Add(str);
            }

            if (closeAfter)
            {
                sr.Close();
                sr.Dispose();
                stream.Close();
#if NET5_0_OR_GREATER
                await stream.DisposeAsync().ConfigureAwait(false);
#else
                stream.Dispose();
#endif
            }

            return stringList;
        }

        /// <summary>
        /// 读取所有文本
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="encoding"></param>
        /// <param name="closeAfter">读取完毕后关闭流</param>
        /// <returns></returns>
        public static async Task<string> ReadAllTextAsync(this FileStream stream, Encoding encoding, bool closeAfter = true)
        {
            var sr = new StreamReader(stream, encoding);
            var text = await sr.ReadToEndAsync().ConfigureAwait(false);
            if (closeAfter)
            {
                sr.Close();
                sr.Dispose();
                stream.Close();
#if NET5_0_OR_GREATER
                await stream.DisposeAsync().ConfigureAwait(false);
#else
                stream.Dispose();
#endif
            }

            return text;
        }

        /// <summary>
        /// 写入所有文本
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="content"></param>
        /// <param name="encoding"></param>
        /// <param name="closeAfter">读取完毕后关闭流</param>
        /// <returns></returns>
        public static async Task WriteAllTextAsync(this FileStream stream, string content, Encoding encoding, bool closeAfter = true)
        {
            var sw = new StreamWriter(stream, encoding);
            stream.SetLength(0);
            await sw.WriteAsync(content).ConfigureAwait(false);
            await sw.FlushAsync().ConfigureAwait(false);
            if (closeAfter)
            {
                sw.Close();
                stream.Close();
#if NET5_0_OR_GREATER
                await sw.DisposeAsync().ConfigureAwait(false);
                await stream.DisposeAsync().ConfigureAwait(false);
#else
                sw.Dispose();
                stream.Dispose();
#endif
            }
        }

        /// <summary>
        /// 写入所有文本行
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="lines"></param>
        /// <param name="encoding"></param>
        /// <param name="closeAfter">读取完毕后关闭流</param>
        /// <returns></returns>
        public static async Task WriteAllLinesAsync(this FileStream stream, IEnumerable<string> lines, Encoding encoding, bool closeAfter = true)
        {
            var sw = new StreamWriter(stream, encoding);
            stream.SetLength(0);
            foreach (var line in lines)
            {
                await sw.WriteLineAsync(line).ConfigureAwait(false);
            }

            await sw.FlushAsync().ConfigureAwait(false);
            if (closeAfter)
            {
                sw.Close();
                stream.Close();
#if NET5_0_OR_GREATER
                await sw.DisposeAsync().ConfigureAwait(false);
                await stream.DisposeAsync().ConfigureAwait(false);
#else
                sw.Dispose();
                stream.Dispose();
#endif
            }
        }

#if NET5_0_OR_GREATER

        /// <summary>
        ///
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<byte[]> ToArrayAsync(this Stream stream, CancellationToken cancellationToken = default)
        {
            stream.Position = 0;
            byte[] bytes = new byte[stream.Length];
            await stream.ReadAsync(bytes, cancellationToken);
            stream.Seek(0, SeekOrigin.Begin);
            return bytes;
        }
#endif
    }
}
