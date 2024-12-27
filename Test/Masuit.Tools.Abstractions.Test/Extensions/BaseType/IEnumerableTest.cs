using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Masuit.Tools.Abstractions.Test.Tree;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Extensions.BaseType;

public class IEnumerableTest
{
    [Fact]
    public void Can_ICollection_AppendIf()
    {
        // arrange
        var list = new List<string>();

        // act
        list.AppendIf(true, "1");
        list.AppendIf(() => true, "2");

        // assert
        Assert.True(list.Count == 2);
    }

    [Fact]
    public void Can_Enumerable_AppendIf()
    {
        // arrange
        var enumerable = Enumerable.Range(1, 1);

        // act
        enumerable = enumerable.AppendIf(true, 2).AppendIf(() => true, 3);

        // assert
        Assert.Equal(enumerable.Count(), 3);
    }

    [Fact]
    public void Can_ChangeIndex()
    {
        // arrange
        var list = new List<string>()
        {
            "a","c","d","b"
        };

        // act
        list.ChangeIndex("b", 1);

        // assert
        Assert.Equal(list[3], "d");
    }

    [Fact]
    public void Can_CompareChanges()
    {
        // arrange
        var list1 = new List<MyClass3>()
        {
            new MyClass3
            {
                Id = 0,
            },
            new MyClass3()
            {
                Id = 1,
            }
        };
        var list2 = new List<MyClass3>()
        {
            new MyClass3()
            {
                Id = 1,
            },
            new MyClass3()
            {
                Id = 2,
            },
        };

        // act
        var (adds, remove, updates) = list1.CompareChanges(list2);
        var (adds1, remove1, updates1) = list1.CompareChanges(list2, c => c.Id);
        var (adds2, remove2, updates2) = list1.CompareChanges(list2, c => c.Id, c => c.Id);
        var (adds3, remove3, updates3) = list1.CompareChanges(list2, (x, y) => x.Id == y.Id);
        var (adds4, remove4, updates4) = list1.CompareChangesPlus(list2);
        var (adds5, remove5, updates5) = list1.CompareChangesPlus(list2, c => c.Id);
        var (adds6, remove6, updates6) = list1.CompareChangesPlus(list2, c => c.Id, c => c.Id);
        var (adds7, remove7, updates7) = list1.CompareChangesPlus(list2, (x, y) => x.Id == y.Id);

        // assert
        Assert.True(new[] { adds, adds1, adds2, adds3, adds4, adds5, adds6, adds7 }.All(x => x.Count == 1));
        Assert.True(new[] { remove, remove1, remove2, remove3, remove4, remove5, remove6, remove7 }.All(x => x.Count == 1));
        Assert.True(new[] { updates, updates1, updates2, updates3 }.All(x => x.Count == 1));
        Assert.True(new[] { updates4, updates5, updates6, updates7 }.All(x => x.Count == 1));
    }

    [Fact]
    public void IntersectBy_WithCondition_ReturnsCorrectResult()
    {
        var first = new[] { 1, 2, 3, 4 };
        var second = new[] { 3, 4, 5, 6 };
        var result = first.IntersectBy(second, (a, b) => a == b);
        Assert.Equal(new[] { 3, 4 }, result);
    }

    [Fact]
    public void IntersectBy_WithKeySelector_ReturnsCorrectResult()
    {
        var first = new[] { 1, 2, 3, 4 };
        var second = new[] { 3, 4, 5, 6 };
        var result = first.IntersectBy(second, x => x);
        Assert.Equal(new[] { 3, 4 }, result);
    }

    [Fact]
    public void IntersectAll_ReturnsCorrectResult()
    {
        var source = new List<IEnumerable<int>>
            {
                new[] { 1, 2, 3 },
                new[] { 2, 3, 4 },
                new[] { 3, 4, 5 }
            };
        var result = source.IntersectAll();
        Assert.Equal(new[] { 3 }, result);
    }

    [Fact]
    public void ExceptBy_WithCondition_ReturnsCorrectResult()
    {
        var first = new[] { 1, 2, 3, 4 };
        var second = new[] { 3, 4, 5, 6 };
        var result = first.ExceptBy(second, (a, b) => a == b);
        Assert.Equal(new[] { 1, 2 }, result);
    }

    [Fact]
    public void AddRange_AddsElementsToCollection()
    {
        var collection = new List<int> { 1, 2 };
        collection.AddRange(3, 4);
        Assert.Equal(new[] { 1, 2, 3, 4 }, collection);
    }

    [Fact]
    public void AddRangeIf_AddsElementsToCollectionIfConditionIsMet()
    {
        var collection = new List<int> { 1, 2 };
        collection.AddRangeIf(x => x > 2, 3, 4);
        Assert.Equal(new[] { 1, 2, 3, 4 }, collection);
    }

    [Fact]
    public void RemoveWhere_RemovesElementsFromCollection()
    {
        var collection = new List<int> { 1, 2, 3, 4 };
        collection.RemoveWhere(x => x > 2);
        Assert.Equal(new[] { 1, 2 }, collection);
    }

    [Fact]
    public void InsertAfter_InsertsElementAfterCondition()
    {
        var list = new List<int> { 1, 2, 3, 4 };
        list.InsertAfter(x => x == 2, 5);
        Assert.Equal(new[] { 1, 2, 5, 3, 4 }, list);
    }

    [Fact]
    public void ToHashSet_ConvertsToHashSet()
    {
        var source = new[] { 1, 2, 3, 4 };
        var result = source.ToHashSet(x => x);
        Assert.Equal(new HashSet<int> { 1, 2, 3, 4 }, result);
    }

    [Fact]
    public void ToQueue_ConvertsToQueue()
    {
        var source = new[] { 1, 2, 3, 4 };
        var result = source.ToQueue();
        Assert.Equal(new Queue<int>(new[] { 1, 2, 3, 4 }), result);
    }

    [Fact]
    public void ForEach_ExecutesActionOnEachElement()
    {
        var source = new[] { 1, 2, 3, 4 };
        var result = new List<int>();
        source.ForEach(x => result.Add(x));
        Assert.Equal(source, result);
    }

    [Fact]
    public async Task ForeachAsync_ExecutesActionOnEachElement()
    {
        var source = new[] { 1, 2, 3, 4 };
        var result = new ConcurrentBag<int>();
        await source.ForeachAsync(async x =>
        {
            await Task.Delay(10);
            result.Add(x);
        }, 2);
        Assert.Equal(source.Length, result.Count);
    }

    [Fact]
    public void MaxOrDefault_ReturnsMaxValueOrDefault()
    {
        var source = new[] { 1, 2, 3, 4 };
        var result = source.MaxOrDefault();
        Assert.Equal(4, result);
    }

    [Fact]
    public void MinOrDefault_ReturnsMinValueOrDefault()
    {
        var source = new[] { 1, 2, 3, 4 };
        var result = source.MinOrDefault();
        Assert.Equal(1, result);
    }

    [Fact]
    public void StandardDeviation_ReturnsCorrectResult()
    {
        var source = new[] { 1.0, 2.0, 3.0, 4.0 };
        var result = source.StandardDeviation();
        Assert.Equal(1.118033988749895, result, 6);
    }

    [Fact]
    public void OrderByRandom_ReturnsRandomOrder()
    {
        var source = new[] { 1, 2, 3, 4 };
        var result = source.OrderByRandom().ToList();
        Assert.NotEqual(source, result);
    }

    [Fact]
    public void SequenceEqual_WithCondition_ReturnsCorrectResult()
    {
        var first = new[] { 1, 2, 3, 4 };
        var second = new long[] { 1, 2, 3, 4 };
        var result = first.SequenceEqual(second, (a, b) => a == b);
        Assert.True(result);
    }

    [Fact]
    public void CompareChanges_ReturnsCorrectResult()
    {
        var first = new[] { 1, 2, 3, 4 };
        var second = new[] { 3, 4, 5, 6 };
        var (adds, remove, updates) = first.CompareChanges(second);
        Assert.Equal(new[] { 1, 2 }, adds);
        Assert.Equal(new[] { 5, 6 }, remove);
        Assert.Equal(new[] { 3, 4 }, updates);
    }

    [Fact]
    public void AsNotNull_ReturnsNonNullCollection()
    {
        List<int> list = null;
        var result = list.AsNotNull();
        Assert.NotNull(result);
    }

    [Fact]
    public void WhereIf_ReturnsFilteredCollectionIfConditionIsMet()
    {
        var source = new[] { 1, 2, 3, 4 };
        var result = source.WhereIf(true, x => x > 2);
        Assert.Equal(new[] { 3, 4 }, result);
    }
}