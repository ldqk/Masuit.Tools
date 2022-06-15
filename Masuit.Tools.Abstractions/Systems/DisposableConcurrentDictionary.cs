using System;
using System.Collections.Generic;
using System.Linq;

namespace Masuit.Tools.Systems;

/// <summary>
/// 值可被Dispose的字典类型
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
public class DisposableConcurrentDictionary<TKey, TValue> : NullableConcurrentDictionary<TKey, TValue>, IDisposable where TValue : IDisposable
{
    private bool _isDisposed;

    /// <summary>
    /// 终结器
    /// </summary>
    ~DisposableConcurrentDictionary()
    {
        Dispose(false);
    }

    public DisposableConcurrentDictionary() : base()
    {
    }

    public DisposableConcurrentDictionary(TValue fallbackValue) : base()
    {
        FallbackValue = fallbackValue;
    }

    public DisposableConcurrentDictionary(int concurrencyLevel, int capacity) : base(concurrencyLevel, capacity)
    {
    }

    public DisposableConcurrentDictionary(IEqualityComparer<NullObject<TKey>> comparer) : base(comparer)
    {
    }

    /// <summary>
    ///
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        Dispose(true);
        _isDisposed = true;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 释放
    /// </summary>
    /// <param name="disposing"></param>
    public void Dispose(bool disposing)
    {
        foreach (var s in Values.Where(v => v != null))
        {
            s.Dispose();
        }
    }
}
