using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Masuit.Tools.Security;
using Masuit.Tools.Systems;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Extensions.BaseType
{
    public class StreamExtensionsTests
    {
        [Fact]
        public void SaveAsMemoryStream_ShouldReturnPooledMemoryStream()
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("Test data"));
            var result = stream.SaveAsMemoryStream();
            Assert.IsType<PooledMemoryStream>(result);
        }

        [Fact]
        public void ToArray_ShouldReturnByteArray()
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("Test data"));
            var result = stream.ToArray();
            Assert.Equal(Encoding.UTF8.GetBytes("Test data"), result);
        }

        [Fact]
        public void ShuffleCode_ShouldAddRandomBytes()
        {
            using var stream = new PooledMemoryStream("Test data"u8.ToArray());
            var md5 = stream.MDString();
            stream.ShuffleCode();
            Assert.NotEqual(stream.MDString(), md5);
        }

        [Fact]
        public void ReadAllLines_StreamReader_ShouldReturnAllLines()
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("Line1\nLine2\nLine3"));
            using var reader = new StreamReader(stream);
            var result = reader.ReadAllLines();
            Assert.Equal(new List<string> { "Line1", "Line2", "Line3" }, result);
        }

        [Fact]
        public void ReadAllLines_FileStream_ShouldReturnAllLines()
        {
            var filePath = Path.GetTempFileName();
            File.WriteAllText(filePath, "Line1\nLine2\nLine3");
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var result = stream.ReadAllLines(Encoding.UTF8);
            Assert.Equal(new List<string> { "Line1", "Line2", "Line3" }, result);
            File.Delete(filePath);
        }

        [Fact]
        public void ReadAllText_ShouldReturnAllText()
        {
            var filePath = Path.GetTempFileName();
            File.WriteAllText(filePath, "Test data");
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var result = stream.ReadAllText(Encoding.UTF8);
            Assert.Equal("Test data", result);
            File.Delete(filePath);
        }

        [Fact]
        public void WriteAllText_ShouldWriteAllText()
        {
            var filePath = Path.GetTempFileName();
            using var stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
            stream.WriteAllText("Test data", Encoding.UTF8);
            var result = File.ReadAllText(filePath);
            Assert.Equal("Test data", result);
            File.Delete(filePath);
        }

        [Fact]
        public void WriteAllLines_ShouldWriteAllLines()
        {
            var filePath = Path.GetTempFileName();
            using var stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
            stream.WriteAllLines(new List<string> { "Line1", "Line2", "Line3" }, Encoding.UTF8);
            var result = File.ReadAllLines(filePath);
            Assert.Equal(new List<string> { "Line1", "Line2", "Line3" }, result);
            File.Delete(filePath);
        }

        [Fact]
        public void ShareReadWrite_ShouldOpenFileWithReadWriteAccess()
        {
            var filePath = Path.GetTempFileName();
            var fileInfo = new FileInfo(filePath);
            using var stream = fileInfo.ShareReadWrite();
            Assert.True(stream.CanRead && stream.CanWrite);
            try
            {
                File.Delete(filePath);
            }
            catch (Exception e)
            {
            }
        }

        [Fact]
        public async Task ReadAllLinesAsync_StreamReader_ShouldReturnAllLines()
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("Line1\nLine2\nLine3"));
            using var reader = new StreamReader(stream);
            var result = await reader.ReadAllLinesAsync();
            Assert.Equal(new List<string> { "Line1", "Line2", "Line3" }, result);
        }

        [Fact]
        public async Task ReadAllLinesAsync_FileStream_ShouldReturnAllLines()
        {
            var filePath = Path.GetTempFileName();
            await File.WriteAllTextAsync(filePath, "Line1\nLine2\nLine3");
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var result = await stream.ReadAllLinesAsync(Encoding.UTF8);
            Assert.Equal(new List<string> { "Line1", "Line2", "Line3" }, result);
            File.Delete(filePath);
        }

        [Fact]
        public async Task ReadAllTextAsync_ShouldReturnAllText()
        {
            var filePath = Path.GetTempFileName();
            await File.WriteAllTextAsync(filePath, "Test data");
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var result = await stream.ReadAllTextAsync(Encoding.UTF8);
            Assert.Equal("Test data", result);
            File.Delete(filePath);
        }

        [Fact]
        public async Task WriteAllTextAsync_ShouldWriteAllText()
        {
            var filePath = Path.GetTempFileName();
            using var stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
            await stream.WriteAllTextAsync("Test data", Encoding.UTF8);
            var result = await File.ReadAllTextAsync(filePath);
            Assert.Equal("Test data", result);
            File.Delete(filePath);
        }

        [Fact]
        public async Task WriteAllLinesAsync_ShouldWriteAllLines()
        {
            var filePath = Path.GetTempFileName();
            using var stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
            await stream.WriteAllLinesAsync(new List<string> { "Line1", "Line2", "Line3" }, Encoding.UTF8);
            var result = await File.ReadAllLinesAsync(filePath);
            Assert.Equal(new List<string> { "Line1", "Line2", "Line3" }, result);
            File.Delete(filePath);
        }

#if NET5_0_OR_GREATER

        [Fact]
        public async Task ToArrayAsync_ShouldReturnByteArray()
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("Test data"));
            var result = await stream.ToArrayAsync();
            Assert.Equal(Encoding.UTF8.GetBytes("Test data"), result);
        }

#endif
    }
}