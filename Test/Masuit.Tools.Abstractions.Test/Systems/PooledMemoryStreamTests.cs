using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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

    [Fact]
    public void DefaultPool_ShouldLimitRetainedBuffers()
    {
        var smallBuffers = CreateDisposedBuffers(1024 * 1024, 12);
        var largeBuffers = CreateDisposedBuffers(2 * 1024 * 1024, 4);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.True(smallBuffers.Count(reference => reference.IsAlive) <= 4);
        Assert.All(largeBuffers, reference => Assert.False(reference.IsAlive));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference[] CreateDisposedBuffers(int size, int count)
    {
        var streams = new PooledMemoryStream[count];
        var references = new WeakReference[count];
        for (var i = 0; i < count; i++)
        {
            streams[i] = new PooledMemoryStream();
            streams[i].SetLength(size);
            references[i] = new WeakReference(streams[i].GetBuffer());
        }

        foreach (var stream in streams)
        {
            stream.Dispose();
        }

        return references;
    }

    [Fact]
    public void Growth_ShouldPreserveDataBeyondPositionAndReturnRentedBuffers()
    {
        var pool = new TrackingPool();
        var stream = new PooledMemoryStream(pool);
        stream.Write(new byte[] { 1, 2, 3, 4 }, 0, 4);
        stream.Position = 1;
        stream.SetLength(8);

        Assert.Equal(new byte[] { 1, 2, 3, 4 }, stream.ToArray().Take(4));
        stream.Dispose();
        stream.Dispose();
        Assert.Equal(pool.Rented, pool.Returned);
    }

    [Fact]
    public void Dispose_EmptyStream_ShouldNotReturnUnrentedBuffer()
    {
        var pool = new TrackingPool();
        new PooledMemoryStream(pool).Dispose();
        Assert.Empty(pool.Returned);
    }

    [Fact]
    public void LargeInitialWrite_ShouldNotDoubleRequestedCapacity()
    {
        var pool = new TrackingPool();
        var stream = new PooledMemoryStream(pool);
        var data = new byte[2 * 1024 * 1024];
        stream.Write(data, 0, data.Length);
        Assert.Equal(data.Length, stream.Capacity);
        stream.Dispose();
        Assert.Same(pool.Rented[0], Assert.Single(pool.Returned));
    }

    [Fact]
    public void LargeIncrementalWrite_ShouldLimitExtraCapacity()
    {
        var pool = new TrackingPool();
        using var stream = new PooledMemoryStream(pool, 2 * 1024 * 1024);
        stream.Position = stream.Capacity;
        stream.Write(new byte[1], 0, 1);
        Assert.Equal(2 * 1024 * 1024 + 512 * 1024, stream.Capacity);
    }

    [Fact]
    public void AbandonedStream_ShouldNotDelayBufferCollectionForFinalization()
    {
        var reference = CreateAbandonedBuffer();
        GC.Collect();
        Assert.False(reference.IsAlive);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference CreateAbandonedBuffer()
    {
        var stream = new PooledMemoryStream();
        stream.SetLength(2 * 1024 * 1024);
        return new WeakReference(stream.GetBuffer());
    }

    [Fact]
    public void CapacityAboveIntMaxValue_ShouldFailBeforeOverflow()
    {
        using var stream = new PooledMemoryStream();
        Assert.Throws<ArgumentOutOfRangeException>(() => stream.SetLength((long)int.MaxValue + 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => stream.Seek(long.MaxValue, SeekOrigin.End));
        stream.Position = int.MaxValue;
        Assert.Throws<IOException>(() => stream.Write(new byte[1], 0, 1));
    }

    private sealed class TrackingPool : ArrayPool<byte>
    {
        public readonly List<byte[]> Rented = new();
        public readonly List<byte[]> Returned = new();

        public override byte[] Rent(int minimumLength)
        {
            var buffer = new byte[minimumLength];
            Rented.Add(buffer);
            return buffer;
        }

        public override void Return(byte[] array, bool clearArray = false)
        {
            Returned.Add(array);
        }
    }
}