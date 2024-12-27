using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Masuit.Tools.Systems;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Systems;

public class NullableConcurrentDictionaryTests
{
    [Fact]
    public void Test_AddAndRetrieveValue()
    {
        var dictionary = new NullableConcurrentDictionary<string, int>();
        dictionary["key1"] = 1;

        Assert.True(dictionary.TryGetValue("key1", out var value));
        Assert.Equal(1, value);
    }

    [Fact]
    public void Test_FallbackValue()
    {
        var dictionary = new NullableConcurrentDictionary<string, int>(-1);

        Assert.Equal(-1, dictionary["nonexistent"]);
    }

    [Fact]
    public void Test_ContainsKey()
    {
        var dictionary = new NullableConcurrentDictionary<string, int>();
        dictionary["key1"] = 1;

        Assert.True(dictionary.ContainsKey("key1"));
        Assert.False(dictionary.ContainsKey("key2"));
    }

    [Fact]
    public void Test_TryAdd()
    {
        var dictionary = new NullableConcurrentDictionary<string, int>();
        Assert.True(dictionary.TryAdd("key1", 1));
        Assert.False(dictionary.TryAdd("key1", 2));
    }

    [Fact]
    public void Test_TryRemove()
    {
        var dictionary = new NullableConcurrentDictionary<string, int>();
        dictionary["key1"] = 1;

        Assert.True(dictionary.TryRemove("key1", out var value));
        Assert.Equal(1, value);
        Assert.False(dictionary.TryRemove("key1", out _));
    }

    [Fact]
    public void Test_TryUpdate()
    {
        var dictionary = new NullableConcurrentDictionary<string, int>();
        dictionary["key1"] = 1;

        Assert.True(dictionary.TryUpdate("key1", 2, 1));
        Assert.Equal(2, dictionary["key1"]);
        Assert.False(dictionary.TryUpdate("key1", 3, 1));
    }

    [Fact]
    public void Test_ImplicitConversionFromDictionary()
    {
        var dict = new Dictionary<string, int> { { "key1", 1 } };
        NullableConcurrentDictionary<string, int> nullableDict = dict;

        Assert.Equal(1, nullableDict["key1"]);
    }

    [Fact]
    public void Test_ImplicitConversionFromConcurrentDictionary()
    {
        var dict = new ConcurrentDictionary<string, int>();
        dict["key1"] = 1;
        NullableConcurrentDictionary<string, int> nullableDict = dict;

        Assert.Equal(1, nullableDict["key1"]);
    }

    [Fact]
    public void Test_ImplicitConversionToConcurrentDictionary()
    {
        var nullableDict = new NullableConcurrentDictionary<string, int>();
        nullableDict["key1"] = 1;
        ConcurrentDictionary<string, int> dict = nullableDict;

        Assert.Equal(1, dict["key1"]);
    }

    [Fact]
    public void Test_IndexerWithCondition()
    {
        var dictionary = new NullableConcurrentDictionary<string, int>(-1);
        dictionary["key1"] = 1;
        dictionary["key2"] = 2;

        Assert.Equal(2, dictionary[new Func<string, bool>(key => key == "key2")]);
        Assert.Equal(-1, dictionary[new Func<string, bool>(key => key == "key3")]);
    }
}

public class NullableConcurrentDictionaryTestsImpl : NullableConcurrentDictionaryTests
{
}