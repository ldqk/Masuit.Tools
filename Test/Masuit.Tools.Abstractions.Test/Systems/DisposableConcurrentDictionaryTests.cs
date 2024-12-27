using System.Collections.Generic;
using Masuit.Tools.Systems;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Systems;

public class DisposableConcurrentDictionaryTests
{
    [Fact]
    public void Constructor_Default_ShouldInitialize()
    {
        var dictionary = new DisposableConcurrentDictionary<string, DisposableValue>();
        Assert.NotNull(dictionary);
    }

    [Fact]
    public void Constructor_WithFallbackValue_ShouldInitialize()
    {
        var fallbackValue = new DisposableValue();
        var dictionary = new DisposableConcurrentDictionary<string, DisposableValue>(fallbackValue);
        Assert.NotNull(dictionary);
    }

    [Fact]
    public void Constructor_WithConcurrencyLevelAndCapacity_ShouldInitialize()
    {
        var dictionary = new DisposableConcurrentDictionary<string, DisposableValue>(4, 100);
        Assert.NotNull(dictionary);
    }

    [Fact]
    public void Constructor_WithComparer_ShouldInitialize()
    {
        var comparer = EqualityComparer<NullObject<string>>.Default;
        var dictionary = new DisposableConcurrentDictionary<string, DisposableValue>(comparer);
        Assert.NotNull(dictionary);
    }

    [Fact]
    public void Dispose_ShouldDisposeAllValues()
    {
        var value1 = new DisposableValue();
        var value2 = new DisposableValue();
        var dictionary = new DisposableConcurrentDictionary<string, DisposableValue>
        {
            ["key1"] = value1,
            ["key2"] = value2
        };

        dictionary.Dispose();

        Assert.True(value1.IsDisposed);
        Assert.True(value2.IsDisposed);
    }

    [Fact]
    public void Dispose_MultipleTimes_ShouldNotThrow()
    {
        var dictionary = new DisposableConcurrentDictionary<string, DisposableValue>();
        dictionary.Dispose();
        var exception = Record.Exception(() => dictionary.Dispose());
        Assert.Null(exception);
    }
}

public class DisposableValue : Disposable
{
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// 释放
    /// </summary>
    /// <param name="disposing"></param>
    public override void Dispose(bool disposing)
    {
        IsDisposed = true;
    }
}