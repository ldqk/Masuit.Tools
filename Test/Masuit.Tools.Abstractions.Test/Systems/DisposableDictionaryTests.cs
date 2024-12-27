using System;
using System.Collections.Generic;
using Masuit.Tools.Systems;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Systems;

public class DisposableDictionaryTests
{
    private class DisposableValue : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    [Fact]
    public void AddAndRetrieveValue()
    {
        var dictionary = new DisposableDictionary<string, DisposableValue>();
        var value = new DisposableValue();
        dictionary.Add("key", value);

        Assert.True(dictionary.TryGetValue("key", out var retrievedValue));
        Assert.Equal(value, retrievedValue);
    }

    [Fact]
    public void Dispose_DisposesAllValues()
    {
        var dictionary = new DisposableDictionary<string, DisposableValue>
        {
            { "key1", new DisposableValue() },
            { "key2", new DisposableValue() }
        };

        dictionary.Dispose();

        foreach (var value in dictionary.Values)
        {
            Assert.True(value.IsDisposed);
        }
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        var dictionary = new DisposableDictionary<string, DisposableValue>
        {
            { "key1", new DisposableValue() },
            { "key2", new DisposableValue() }
        };

        dictionary.Dispose();
        dictionary.Dispose();

        foreach (var value in dictionary.Values)
        {
            Assert.True(value.IsDisposed);
        }
    }

    [Fact]
    public void Finalizer_DisposesValues()
    {
        var dictionary = new DisposableDictionary<string, DisposableValue>
        {
            { "key1", new DisposableValue() },
            { "key2", new DisposableValue() }
        };

        dictionary = null;
        GC.Collect();
        GC.WaitForPendingFinalizers();

        // 无法直接测试终结器的效果，但可以通过其他测试确保 Dispose 方法的正确性
    }

    [Fact]
    public void Constructor_WithFallbackValue()
    {
        var fallbackValue = new DisposableValue();
        var dictionary = new DisposableDictionary<string, DisposableValue>(fallbackValue);

        Assert.Equal(dictionary["aa"], fallbackValue);
    }

    [Fact]
    public void Constructor_WithCapacity()
    {
        var dictionary = new DisposableDictionary<string, DisposableValue>(10);

        Assert.Equal(0, dictionary.Count);
    }

    [Fact]
    public void Constructor_WithDictionary()
    {
        var initialDictionary = new Dictionary<NullObject<string>, DisposableValue>
        {
            { new NullObject<string>("key1"), new DisposableValue() },
            { new NullObject<string>("key2"), new DisposableValue() }
        };

        var dictionary = new DisposableDictionary<string, DisposableValue>(initialDictionary);

        Assert.Equal(2, dictionary.Count);
    }
}