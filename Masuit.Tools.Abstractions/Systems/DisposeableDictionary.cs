using System;
using System.Collections.Generic;
using System.Linq;

namespace Masuit.Tools.Systems;

/// <summary>
/// 值可被Dispose的字典类型
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
public class DisposeableDictionary<TKey, TValue> : NullableDictionary<TKey, TValue>, IDisposable where TValue : IDisposable
{
    private bool _isDisposed;

    /// <summary>
    /// 终结器
    /// </summary>
    ~DisposeableDictionary()
    {
        Dispose(false);
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

    public DisposeableDictionary(IDictionary<NullObject<TKey>, TValue> dictionary) : base(dictionary)
    {
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
