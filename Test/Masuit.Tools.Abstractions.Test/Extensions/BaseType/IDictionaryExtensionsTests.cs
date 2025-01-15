using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Extensions.BaseType;

public class IDictionaryExtensionsTests
{
    [Fact]
    public void AddOrUpdate_ShouldAddOrUpdateValues()
    {
        // Arrange
        var dict = new Dictionary<string, int> { { "key1", 1 } };
        var newDict = new Dictionary<string, int> { { "key1", 2 }, { "key2", 3 } };

        // Act
        dict.AddOrUpdate(newDict);

        // Assert
        Assert.Equal(2, dict["key1"]);
        Assert.Equal(3, dict["key2"]);
    }

    [Fact]
    public void AddOrUpdate_WithFactory_ShouldAddOrUpdateValues()
    {
        // Arrange
        var dict = new Dictionary<string, int> { { "key1", 1 } };

        // Act
        dict.AddOrUpdate("key1", 2, (key, oldValue) => oldValue + 1);
        dict.AddOrUpdate("key2", 3, (key, oldValue) => oldValue + 1);

        // Assert
        Assert.Equal(2, dict["key1"]);
        Assert.Equal(3, dict["key2"]);
    }

    [Fact]
    public void AddOrUpdate_WithValue_ShouldAddOrUpdateValues()
    {
        // Arrange
        var dict = new Dictionary<string, int> { { "key1", 1 } };

        // Act
        dict.AddOrUpdate("key1", 2, 3);
        dict.AddOrUpdate("key2", 3, 4);

        // Assert
        Assert.Equal(3, dict["key1"]);
        Assert.Equal(3, dict["key2"]);
    }

    [Fact]
    public async Task AddOrUpdateAsync_ShouldAddOrUpdateValues()
    {
        // Arrange
        var dict = new Dictionary<string, int> { { "key1", 1 } };

        // Act
        await dict.AddOrUpdateAsync("key1", 2, async (key, oldValue) => await Task.FromResult(oldValue + 1));
        await dict.AddOrUpdateAsync("key2", 3, async (key, oldValue) => await Task.FromResult(oldValue + 1));

        // Assert
        Assert.Equal(2, dict["key1"]);
        Assert.Equal(3, dict["key2"]);
    }

    [Fact]
    public void GetOrAdd_ShouldReturnExistingOrAddNewValue()
    {
        // Arrange
        var dict = new Dictionary<string, int> { { "key1", 1 } };

        // Act
        var value1 = dict.GetOrAdd("key1", () => 2);
        var value2 = dict.GetOrAdd("key2", () => 3);

        // Assert
        Assert.Equal(1, value1);
        Assert.Equal(3, value2);
    }

    [Fact]
    public async Task GetOrAddAsync_ShouldReturnExistingOrAddNewValue()
    {
        // Arrange
        var dict = new Dictionary<string, int> { { "key1", 1 } };

        // Act
        var value1 = await dict.GetOrAddAsync("key1", async () => await Task.FromResult(2));
        var value2 = await dict.GetOrAddAsync("key2", async () => await Task.FromResult(3));

        // Assert
        Assert.Equal(1, value1);
        Assert.Equal(3, value2);
    }

    [Fact]
    public void ToDictionarySafety_ShouldConvertToDictionary()
    {
        // Arrange
        var list = new List<string> { "a", "b", "c" };

        // Act
        var dict = list.ToDictionarySafety(x => x, x => x.ToUpper());

        // Assert
        Assert.Equal("A", dict["a"]);
        Assert.Equal("B", dict["b"]);
        Assert.Equal("C", dict["c"]);
    }

    [Fact]
    public async Task ToDictionarySafetyAsync_ShouldConvertToDictionary()
    {
        // Arrange
        var list = new List<string> { "a", "b", "c" };

        // Act
        var dict = await list.ToDictionarySafetyAsync(x => x, async x => await Task.FromResult(x.ToUpper()));

        // Assert
        Assert.Equal("A", dict["a"]);
        Assert.Equal("B", dict["b"]);
        Assert.Equal("C", dict["c"]);
    }

    [Fact]
    public void ToConcurrentDictionary_ShouldConvertToConcurrentDictionary()
    {
        // Arrange
        var list = new List<string> { "a", "b", "c" };

        // Act
        var dict = list.ToConcurrentDictionary(x => x, x => x.ToUpper());

        // Assert
        Assert.Equal("A", dict["a"]);
        Assert.Equal("B", dict["b"]);
        Assert.Equal("C", dict["c"]);
    }

    [Fact]
    public async Task ToConcurrentDictionaryAsync_ShouldConvertToConcurrentDictionary()
    {
        // Arrange
        var list = new List<string> { "a", "b", "c" };

        // Act
        var dict = await list.ToConcurrentDictionaryAsync(x => x, async x => await Task.FromResult(x.ToUpper()));

        // Assert
        Assert.Equal("A", dict["a"]);
        Assert.Equal("B", dict["b"]);
        Assert.Equal("C", dict["c"]);
    }

    [Fact]
    public void ToDisposableDictionarySafety_ShouldConvertToDisposableDictionary()
    {
        // Arrange
        var list = new List<DisposableObject> { new DisposableObject("a"), new DisposableObject("b"), new DisposableObject("c") };

        // Act
        var dict = list.ToDisposableDictionarySafety(x => x.Name);

        // Assert
        Assert.Equal("a", dict["a"].Name);
        Assert.Equal("b", dict["b"].Name);
        Assert.Equal("c", dict["c"].Name);
    }

    [Fact]
    public async Task ToDisposableDictionarySafetyAsync_ShouldConvertToDisposableDictionary()
    {
        // Arrange
        var list = new List<DisposableObject> { new DisposableObject("a"), new DisposableObject("b"), new DisposableObject("c") };

        // Act
        var dict = await list.ToDisposableDictionarySafetyAsync(x => x.Name, async x => await Task.FromResult(x));

        // Assert
        Assert.Equal("a", dict["a"].Name);
        Assert.Equal("b", dict["b"].Name);
        Assert.Equal("c", dict["c"].Name);
    }

    [Fact]
    public void ToLookupX_ShouldConvertToLookup()
    {
        // Arrange
        var list = new List<string> { "a", "b", "a" };

        // Act
        var lookup = list.ToLookupX(x => x);

        // Assert
        Assert.Equal(2, lookup["a"].Count);
        Assert.Single(lookup["b"]);
    }

    [Fact]
    public void ToLookupX_ShouldConvertToLookup2()
    {
        // Arrange
        var list = new List<string> { "a", "b", "a" };

        // Act
        var lookup = list.ToLookupX(x => x, s => list.IndexOf(s));

        // Assert
        Assert.Equal(2, lookup["a"].Count);
        Assert.Single(lookup["b"]);
    }

    [Fact]
    public async Task ToLookupAsync_ShouldConvertToLookup()
    {
        // Arrange
        var list = new List<string> { "a", "b", "a" };

        // Act
        var lookup = await list.ToLookupAsync(x => x, async x => await Task.FromResult(x.ToUpper()));

        // Assert
        Assert.Equal(2, lookup["a"].Count);
        Assert.Single(lookup["b"]);
    }

    [Fact]
    public void AsConcurrentDictionary_ShouldConvertToConcurrentDictionary()
    {
        // Arrange
        var dict = new Dictionary<string, int> { { "key1", 1 }, { "key2", 2 } };

        // Act
        var concurrentDict = dict.AsConcurrentDictionary();

        // Assert
        Assert.Equal(1, concurrentDict["key1"]);
        Assert.Equal(2, concurrentDict["key2"]);
    }

    [Fact]
    public void AsConcurrentDictionary_WithDefaultValue_ShouldConvertToConcurrentDictionary()
    {
        // Arrange
        var dict = new Dictionary<string, int> { { "key1", 1 }, { "key2", 2 } };

        // Act
        var concurrentDict = dict.AsConcurrentDictionary(0);

        // Assert
        Assert.Equal(1, concurrentDict["key1"]);
        Assert.Equal(2, concurrentDict["key2"]);
        Assert.Equal(0, concurrentDict["key3"]);
    }

    [Fact]
    public void AsDictionary_ShouldConvertToDictionary()
    {
        // Arrange
        var concurrentDict = new ConcurrentDictionary<string, int>();
        concurrentDict["key1"] = 1;
        concurrentDict["key2"] = 2;

        // Act
        var dict = concurrentDict.AsDictionary();

        // Assert
        Assert.Equal(1, dict["key1"]);
        Assert.Equal(2, dict["key2"]);
    }

    [Fact]
    public void AsDictionary_WithDefaultValue_ShouldConvertToDictionary()
    {
        // Arrange
        var concurrentDict = new ConcurrentDictionary<string, int>();
        concurrentDict["key1"] = 1;
        concurrentDict["key2"] = 2;

        // Act
        var dict = concurrentDict.AsDictionary(0);

        // Assert
        Assert.Equal(1, dict["key1"]);
        Assert.Equal(2, dict["key2"]);
        Assert.Equal(0, dict["key3"]);
    }

    private class DisposableObject : IDisposable
    {
        public string Name { get; }

        public DisposableObject(string name)
        {
            Name = name;
        }

        public void Dispose()
        {
            // Dispose logic here
        }
    }
}