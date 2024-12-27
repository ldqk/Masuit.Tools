using System.Collections.Concurrent;
using System.Collections.Generic;
using Masuit.Tools.Systems;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Systems;

public class NullableDictionaryTests
{
    [Fact]
    public void Add_ShouldAddKeyValuePair()
    {
        var dictionary = new NullableDictionary<string, int>();
        dictionary.Add("key1", 1);

        Assert.True(dictionary.ContainsKey("key1"));
        Assert.Equal(1, dictionary["key1"]);
    }

    [Fact]
    public void Remove_ShouldRemoveKeyValuePair()
    {
        var dictionary = new NullableDictionary<string, int>();
        dictionary.Add("key1", 1);

        Assert.True(dictionary.Remove("key1"));
        Assert.False(dictionary.ContainsKey("key1"));
    }

    [Fact]
    public void TryGetValue_ShouldReturnTrueIfKeyExists()
    {
        var dictionary = new NullableDictionary<string, int>();
        dictionary.Add("key1", 1);

        var result = dictionary.TryGetValue("key1", out var value);
        Assert.True(result);
        Assert.Equal(1, value);
    }

    [Fact]
    public void TryGetValue_ShouldReturnFalseIfKeyDoesNotExist()
    {
        var dictionary = new NullableDictionary<string, int>();

        var result = dictionary.TryGetValue("key1", out var value);
        Assert.False(result);
    }

    [Fact]
    public void Indexer_ShouldSetAndGetKeyValuePair()
    {
        var dictionary = new NullableDictionary<string, int>();
        dictionary["key1"] = 1;

        Assert.Equal(1, dictionary["key1"]);
    }

    [Fact]
    public void ImplicitConversion_FromDictionary_ShouldConvertCorrectly()
    {
        var dict = new Dictionary<string, int> { { "key1", 1 } };
        NullableDictionary<string, int> nullableDict = dict;

        Assert.True(nullableDict.ContainsKey("key1"));
        Assert.Equal(1, nullableDict["key1"]);
    }

    [Fact]
    public void ImplicitConversion_FromConcurrentDictionary_ShouldConvertCorrectly()
    {
        var dict = new ConcurrentDictionary<string, int>();
        dict["key1"] = 1;
        NullableDictionary<string, int> nullableDict = dict;

        Assert.True(nullableDict.ContainsKey("key1"));
        Assert.Equal(1, nullableDict["key1"]);
    }

    [Fact]
    public void ImplicitConversion_ToDictionary_ShouldConvertCorrectly()
    {
        var nullableDict = new NullableDictionary<string, int>();
        nullableDict["key1"] = 1;
        Dictionary<string, int> dict = nullableDict;

        Assert.True(dict.ContainsKey("key1"));
        Assert.Equal(1, dict["key1"]);
    }
}