using System;
using System.Buffers;
using System.IO;
using Masuit.Tools.Systems;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Systems;

public class PooledMemoryStreamTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaultArrayPool()
    {
        using var stream = new PooledMemoryStream();
        Assert.NotNull(stream);
        Assert.Equal(0, stream.Length);
        Assert.Equal(0, stream.Position);
    }

    [Fact]
    public void Constructor_WithBuffer_ShouldInitializeWithBuffer()
    {
        byte[] buffer = { 1, 2, 3, 4, 5 };
        using var stream = new PooledMemoryStream(buffer);
        Assert.Equal(buffer.Length, stream.Length);
        Assert.Equal(buffer, stream.GetBuffer());
    }

    [Fact]
    public void Read_ShouldReadData()
    {
        byte[] buffer = { 1, 2, 3, 4, 5 };
        using var stream = new PooledMemoryStream(buffer);
        byte[] readBuffer = new byte[5];
        int bytesRead = stream.Read(readBuffer, 0, 5);
        Assert.Equal(5, bytesRead);
        Assert.Equal(buffer, readBuffer);
    }

    [Fact]
    public void Write_ShouldWriteData()
    {
        using var stream = new PooledMemoryStream();
        byte[] buffer = { 1, 2, 3, 4, 5 };
        stream.Write(buffer, 0, buffer.Length);
        Assert.Equal(buffer.Length, stream.Length);
        Assert.Equal(buffer, stream.GetBuffer());
    }

    [Fact]
    public void Seek_ShouldChangePosition()
    {
        using var stream = new PooledMemoryStream();
        stream.SetLength(10);
        stream.Seek(5, SeekOrigin.Begin);
        Assert.Equal(5, stream.Position);
    }

    [Fact]
    public void SetLength_ShouldChangeLength()
    {
        using var stream = new PooledMemoryStream();
        stream.SetLength(10);
        Assert.Equal(10, stream.Length);
    }

    [Fact]
    public void GetBuffer_ShouldReturnBuffer()
    {
        byte[] buffer = { 1, 2, 3, 4, 5 };
        using var stream = new PooledMemoryStream(buffer);
        byte[] result = stream.GetBuffer();
        Assert.Equal(buffer, result);
    }

    [Fact]
    public void ToArray_ShouldReturnArray()
    {
        byte[] buffer = { 1, 2, 3, 4, 5 };
        using var stream = new PooledMemoryStream(buffer);
        byte[] result = stream.ToArray();
        Assert.Equal(buffer, result);
    }

    [Fact]
    public void WriteTo_ShouldWriteToAnotherStream()
    {
        byte[] buffer = { 1, 2, 3, 4, 5 };
        using var stream = new PooledMemoryStream(buffer);
        using var memoryStream = new MemoryStream();
        stream.WriteTo(memoryStream);
        Assert.Equal(buffer, memoryStream.ToArray());
    }

    [Fact]
    public void Dispose_ShouldDisposeStream()
    {
        var stream = new PooledMemoryStream();
        stream.Dispose();
        Assert.Throws<ObjectDisposedException>(() => stream.Write(new byte[1], 0, 1));
    }
}