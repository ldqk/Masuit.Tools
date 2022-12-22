using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;

namespace Masuit.Tools.Systems;

/// <summary>
/// 大型内存流，最大可支持1TB数据，推荐当数据流大于2GB时使用
/// </summary>
public class LargeMemoryStream : Stream
{
    /// <summary>
    /// 终结器
    /// </summary>
    ~LargeMemoryStream()
    {
        Dispose(true);
    }

    private const int PageSize = 1024000000;
    private const int AllocStep = 1024;

    private byte[][] _streamBuffers;

    private int _pageCount = 0;
    private long _allocatedBytes = 0;

    private long _position = 0;
    private long _length = 0;
    private bool _isDisposed;

    private int GetPageCount(long length)
    {
        int pageCount = (int)(length / PageSize) + 1;
        if (length % PageSize == 0)
        {
            pageCount--;
        }

        return pageCount;
    }

    private void ExtendPages()
    {
        if (_streamBuffers == null)
        {
            _streamBuffers = new byte[AllocStep][];
        }
        else
        {
            var streamBuffers = new byte[_streamBuffers.Length + AllocStep][];
            Buffer.BlockCopy(_streamBuffers, 0, streamBuffers, 0, _streamBuffers.Length);
            _streamBuffers = streamBuffers;
        }

        _pageCount = _streamBuffers.Length;
    }

    private void AllocSpaceIfNeeded(long value)
    {
        switch (value)
        {
            case < 0:
                throw new InvalidOperationException("AllocSpaceIfNeeded < 0");
            case 0:
                return;
        }

        int currentPageCount = GetPageCount(_allocatedBytes);
        int neededPageCount = GetPageCount(value);
        while (currentPageCount < neededPageCount)
        {
            if (currentPageCount == _pageCount)
                ExtendPages();

            _streamBuffers[currentPageCount++] = ArrayPool<byte>.Shared.Rent(PageSize);
        }

        _allocatedBytes = (long)currentPageCount * PageSize;
        value = Math.Max(value, _length);
        if (_position > (_length = value))
        {
            _position = _length;
        }
    }

    public override bool CanRead => true;

    public override bool CanSeek => true;

    public override bool CanWrite => true;

    public override long Length => _length;

    public override long Position
    {
        get => _position;
        set
        {
            if (value > _length)
            {
                throw new InvalidOperationException("Position > Length");
            }

            if (value < 0)
            {
                throw new InvalidOperationException("Position < 0");
            }

            _position = value;
        }
    }

#if NETCOREAPP || NET452
    public Span<byte[]> GetSpan()
    {
        return _streamBuffers.AsSpan(0, _streamBuffers.Length);
    }

    public Memory<byte[]> GetMemory()
    {
        return _streamBuffers.AsMemory(0, _streamBuffers.Length);
    }
    public ArraySegment<byte[]> ToArraySegment()
    {
        return new ArraySegment<byte[]>(_streamBuffers, 0, _streamBuffers.Length);
    }
#endif

    public override void Flush()
    {
        AssertNotDisposed();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        AssertNotDisposed();
        int currentPage = (int)(_position / PageSize);
        int currentOffset = (int)(_position % PageSize);
        int currentLength = PageSize - currentOffset;
        long startPosition = _position;
        if (startPosition + count > _length)
        {
            count = (int)(_length - startPosition);
        }

        while (count != 0 && _position < _length)
        {
            if (currentLength > count)
            {
                currentLength = count;
            }

            Buffer.BlockCopy(_streamBuffers[currentPage++], currentOffset, buffer, offset, currentLength);
            offset += currentLength;
            _position += currentLength;
            count -= currentLength;
            currentOffset = 0;
            currentLength = PageSize;
        }

        return (int)(_position - startPosition);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        AssertNotDisposed();
        switch (origin)
        {
            case SeekOrigin.Begin:
                break;

            case SeekOrigin.Current:
                offset += _position;
                break;

            case SeekOrigin.End:
                offset = _length - offset;
                break;

            default:
                throw new ArgumentOutOfRangeException("origin");
        }

        return Position = offset;
    }

    public override void SetLength(long value)
    {
        switch (value)
        {
            case < 0:
                throw new InvalidOperationException("SetLength < 0");
            case 0:
                _streamBuffers = null;
                _allocatedBytes = _position = _length = 0;
                _pageCount = 0;
                return;
        }

        int currentPageCount = GetPageCount(_allocatedBytes);
        int neededPageCount = GetPageCount(value);
        while (currentPageCount > neededPageCount)
        {
            ArrayPool<byte>.Shared.Return(_streamBuffers[--currentPageCount], true);
            _streamBuffers[currentPageCount] = null;
        }

        AllocSpaceIfNeeded(value);
        if (_position > (_length = value))
        {
            _position = _length;
        }
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        AssertNotDisposed();
        int currentPage = (int)(_position / PageSize);
        int currentOffset = (int)(_position % PageSize);
        int currentLength = PageSize - currentOffset;
        AllocSpaceIfNeeded(_position + count);
        while (count != 0)
        {
            if (currentLength > count)
            {
                currentLength = count;
            }

            Buffer.BlockCopy(buffer, offset, _streamBuffers[currentPage++], currentOffset, currentLength);
            offset += currentLength;
            _position += currentLength;
            count -= currentLength;
            currentOffset = 0;
            currentLength = PageSize;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _isDisposed = true;
            Position = 0;
            _length = 0;
            if (_streamBuffers != null)
            {
                foreach (var bytes in _streamBuffers)
                {
                    if (bytes != null)
                    {
                        ArrayPool<byte>.Shared.Return(bytes);
                    }
                }

                _streamBuffers = null;
            }
        }

        base.Dispose(disposing);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AssertNotDisposed()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(PooledMemoryStream));
        }
    }
}