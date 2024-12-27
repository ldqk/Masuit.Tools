using System;
using System.IO;
using Masuit.Tools.Systems;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Systems;

public class LargeMemoryStreamTests
{
    [Fact]
    public void CanRead_ShouldReturnTrue()
    {
        var stream = new LargeMemoryStream();
        Assert.True(stream.CanRead);
    }

    [Fact]
    public void CanSeek_ShouldReturnTrue()
    {
        var stream = new LargeMemoryStream();
        Assert.True(stream.CanSeek);
    }

    [Fact]
    public void CanWrite_ShouldReturnTrue()
    {
        var stream = new LargeMemoryStream();
        Assert.True(stream.CanWrite);
    }

    [Fact]
    public void Length_ShouldReturnZeroInitially()
    {
        var stream = new LargeMemoryStream();
        Assert.Equal(0, stream.Length);
    }

    [Fact]
    public void Position_ShouldReturnZeroInitially()
    {
        var stream = new LargeMemoryStream();
        Assert.Equal(0, stream.Position);
    }

    [Fact]
    public void Position_SetValidValue_ShouldUpdatePosition()
    {
        var stream = new LargeMemoryStream();
        stream.SetLength(100);
        stream.Position = 50;
        Assert.Equal(50, stream.Position);
    }

    [Fact]
    public void Position_SetInvalidValue_ShouldThrowException()
    {
        var stream = new LargeMemoryStream();
        Assert.Throws<InvalidOperationException>(() => stream.Position = -1);
        Assert.Throws<InvalidOperationException>(() => stream.Position = 1);
    }

    [Fact]
    public void Flush_ShouldNotThrowException()
    {
        var stream = new LargeMemoryStream();
        stream.Flush();
    }

    [Fact]
    public void Read_ShouldReturnCorrectData()
    {
        var stream = new LargeMemoryStream();
        var buffer = new byte[100];
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = (byte)i;
        }

        stream.Write(buffer, 0, buffer.Length);
        stream.Position = 0;

        var readBuffer = new byte[100];
        int bytesRead = stream.Read(readBuffer, 0, readBuffer.Length);

        Assert.Equal(buffer.Length, bytesRead);
        Assert.Equal(buffer, readBuffer);
    }

    [Fact]
    public void Seek_ShouldUpdatePositionCorrectly()
    {
        var stream = new LargeMemoryStream();
        stream.SetLength(100);
        stream.Seek(50, SeekOrigin.Begin);
        Assert.Equal(50, stream.Position);

        stream.Seek(10, SeekOrigin.Current);
        Assert.Equal(60, stream.Position);
    }

    [Fact]
    public void SetLength_ShouldUpdateLengthCorrectly()
    {
        var stream = new LargeMemoryStream();
        stream.SetLength(100);
        Assert.Equal(100, stream.Length);

        stream.SetLength(50);
        Assert.Equal(50, stream.Length);
    }

    [Fact]
    public void Write_ShouldWriteDataCorrectly()
    {
        var stream = new LargeMemoryStream();
        var buffer = new byte[100];
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = (byte)i;
        }

        stream.Write(buffer, 0, buffer.Length);
        stream.Position = 0;

        var readBuffer = new byte[100];
        int bytesRead = stream.Read(readBuffer, 0, readBuffer.Length);

        Assert.Equal(buffer.Length, bytesRead);
        Assert.Equal(buffer, readBuffer);
    }

    [Fact]
    public void Dispose_ShouldDisposeCorrectly()
    {
        var stream = new LargeMemoryStream();
        stream.Dispose();
        Assert.Throws<ObjectDisposedException>(() => stream.Write(new byte[1], 0, 1));
    }
}