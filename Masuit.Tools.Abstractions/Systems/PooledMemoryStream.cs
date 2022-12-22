using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace Masuit.Tools.Systems;

/// <summary>
/// 池化内存流
/// </summary>
public sealed class PooledMemoryStream : Stream, IEnumerable<byte>
{
    /// <summary>
    /// 终结器
    /// </summary>
    ~PooledMemoryStream()
    {
        Dispose(true);
    }

    private const float OverExpansionFactor = 2;

    private byte[] _data = Array.Empty<byte>();
    private int _length;
    private readonly ArrayPool<byte> _pool;
    private bool _isDisposed;

    public PooledMemoryStream() : this(ArrayPool<byte>.Shared)
    {
    }

    public PooledMemoryStream(byte[] buffer) : this(ArrayPool<byte>.Shared, buffer.Length)
    {
        Buffer.BlockCopy(buffer, 0, _data, 0, buffer.Length);
    }

    public PooledMemoryStream(ArrayPool<byte> arrayPool, int capacity = 0)
    {
        _pool = arrayPool ?? throw new ArgumentNullException(nameof(arrayPool));
        if (capacity > 0)
        {
            _data = _pool.Rent(capacity);
        }
    }

    public override bool CanRead => true;

    public override bool CanSeek => true;

    public override bool CanWrite => true;

    public override long Length => _length;

    public override long Position { get; set; }

    public long Capacity => _data?.Length ?? 0;

#if NETCOREAPP || NET452
    public Span<byte> GetSpan()
    {
        return _data.AsSpan(0, _length);
    }

    public Memory<byte> GetMemory()
    {
        return _data.AsMemory(0, _length);
    }
    public ArraySegment<byte> ToArraySegment()
    {
        return new ArraySegment<byte>(_data, 0, (int)Length);
    }
#endif

    public override void Flush()
    {
        AssertNotDisposed();
    }

    /// <summary>
    /// 读取到字节数组
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public override int Read(byte[] buffer, int offset, int count)
    {
        AssertNotDisposed();
        if (count == 0)
        {
            return 0;
        }

        var available = Math.Min(count, Length - Position);
        Array.Copy(_data, Position, buffer, offset, available);
        Position += available;
        return (int)available;
    }

    /// <summary>
    /// 改变游标位置
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="origin"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override long Seek(long offset, SeekOrigin origin)
    {
        AssertNotDisposed();
        switch (origin)
        {
            case SeekOrigin.Current:
                if (Position + offset < 0 || Position + offset > Capacity)
                {
                    throw new ArgumentOutOfRangeException(nameof(offset));
                }

                Position += offset;
                _length = (int)Math.Max(Position, _length);
                return Position;

            case SeekOrigin.Begin:
                if (offset < 0 || offset > Capacity)
                {
                    throw new ArgumentOutOfRangeException(nameof(offset));
                }

                Position = offset;
                _length = (int)Math.Max(Position, _length);
                return Position;

            case SeekOrigin.End:
                if (Length + offset < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(offset));
                }

                if (Length + offset > Capacity)
                {
                    SetCapacity((int)(Length + offset));
                }

                Position = Length + offset;
                _length = (int)Math.Max(Position, _length);
                return Position;

            default:
                throw new ArgumentOutOfRangeException(nameof(origin));
        }
    }

    /// <summary>
    /// 设置内容长度
    /// </summary>
    /// <param name="value"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public override void SetLength(long value)
    {
        AssertNotDisposed();
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value));
        }

        if (value > Capacity)
        {
            SetCapacity((int)value);
        }

        _length = (int)value;
        if (Position > Length)
        {
            Position = Length;
        }
    }

    /// <summary>
    /// 写入到字节数组
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    /// <param name="count"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Write(byte[] buffer, int offset, int count)
    {
        AssertNotDisposed();
        if (count == 0)
        {
            return;
        }

        if (Capacity - Position < count)
        {
            SetCapacity((int)(OverExpansionFactor * (Position + count)));
        }

        Array.Copy(buffer, offset, _data, Position, count);
        Position += count;
        _length = (int)Math.Max(Position, _length);
    }

    /// <summary>
    /// 写入到另一个流
    /// </summary>
    /// <param name="stream"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public void WriteTo(Stream stream)
    {
        if (stream == null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        AssertNotDisposed();
        stream.Write(_data, 0, (int)Length);
    }

    /// <summary>
    /// 获取流的字节数组
    /// </summary>
    /// <returns></returns>
    public byte[] GetBuffer()
    {
        AssertNotDisposed();
        if (_data.Length == Length)
        {
            return _data;
        }

        var buffer = new byte[Length];
        Buffer.BlockCopy(_data, 0, buffer, 0, buffer.Length);
        return buffer;
    }

    /// <summary>
    /// 获取流的字节数组
    /// </summary>
    /// <returns></returns>
    public byte[] ToArray()
    {
        return GetBuffer();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="disposing"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _isDisposed = true;
            Position = 0;
            _length = 0;

            if (_data != null)
            {
                _pool.Return(_data);
                _data = null;
            }
        }

        base.Dispose(disposing);
    }

    private void SetCapacity(int newCapacity)
    {
        var newData = _pool.Rent(newCapacity);
        if (_data != null)
        {
            Array.Copy(_data, 0, newData, 0, Position);
            _pool.Return(_data);
        }

        _data = newData;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AssertNotDisposed()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(PooledMemoryStream));
        }
    }

    /// <summary>Returns an enumerator that iterates through a collection.</summary>
    /// <returns>An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public IEnumerator<byte> GetEnumerator()
    {
        for (var i = 0; i < Length; i++)
        {
            yield return _data[i];
        }
    }
}