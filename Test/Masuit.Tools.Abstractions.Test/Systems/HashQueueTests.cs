using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Masuit.Tools.Systems;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Systems;

/// <summary>
/// HashQueue&lt;T&gt; 的单元测试类
/// </summary>
public class HashQueueTests
{
    #region 构造函数测试

    [Fact]
    public void Constructor_Default_CreatesEmptyQueue()
    {
        // Arrange & Act
        var queue = new HashQueue<int>();

        // Assert
        Assert.Empty(queue);
        Assert.Equal(0, queue.Count);
        Assert.True(queue.IsEmpty);
    }

    [Fact]
    public void Constructor_WithComparer_UsesProvidedComparer()
    {
        // Arrange
        var comparer = EqualityComparer<int>.Default;

        // Act
        var queue = new HashQueue<int>(comparer);

        // Assert
        Assert.NotNull(queue);
        Assert.Equal(comparer, queue.Comparer);
        Assert.True(queue.IsEmpty);
    }

    [Fact]
    public void Constructor_WithCollection_InitializesWithItems()
    {
        // Arrange
        var items = new[] { 1, 2, 3 };

        // Act
        var queue = new HashQueue<int>(items);

        // Assert
        Assert.Equal(3, queue.Count);
        Assert.Contains(1, queue);
        Assert.Contains(2, queue);
        Assert.Contains(3, queue);
    }

    [Fact]
    public void Constructor_WithCollectionAndComparer_InitializesWithItemsAndComparer()
    {
        // Arrange
        var items = new[] { 1, 2, 3 };
        var comparer = EqualityComparer<int>.Default;

        // Act
        var queue = new HashQueue<int>(items, comparer);

        // Assert
        Assert.Equal(3, queue.Count);
        Assert.Equal(comparer, queue.Comparer);
    }

    [Fact]
    public void Constructor_WithCapacity_CreatesQueueWithCapacity()
    {
        // Arrange & Act
        var queue = new HashQueue<int>(10);

        // Assert
        Assert.Empty(queue);
        Assert.Equal(0, queue.Count);
    }

    [Fact]
    public void Constructor_WithCapacityAndComparer_CreatesQueueWithCapacityAndComparer()
    {
        // Arrange
        var comparer = EqualityComparer<int>.Default;

        // Act
        var queue = new HashQueue<int>(10, comparer);

        // Assert
        Assert.Empty(queue);
        Assert.Equal(comparer, queue.Comparer);
    }

    [Fact]
    public void Constructor_WithNegativeCapacity_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new HashQueue<int>(-1));
    }

    [Fact]
    public void Constructor_WithNegativeCapacityAndComparer_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new HashQueue<int>(-1, EqualityComparer<int>.Default));
    }

    #endregion 构造函数测试

    #region Enqueue 方法测试

    [Fact]
    public void Enqueue_AddsSingleItem()
    {
        // Arrange
        var queue = new HashQueue<int>();

        // Act
        var result = queue.Enqueue(1);

        // Assert
        Assert.True(result);
        Assert.Single(queue);
        Assert.Contains(1, queue);
    }

    [Fact]
    public void Enqueue_ReturnsFalseForDuplicateItem()
    {
        // Arrange
        var queue = new HashQueue<int>();
        queue.Enqueue(1);

        // Act
        var result = queue.Enqueue(1);

        // Assert
        Assert.False(result);
        Assert.Single(queue);
    }

    [Fact]
    public void Enqueue_AddsMultipleUniqueItems()
    {
        // Arrange
        var queue = new HashQueue<int>();

        // Act
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);

        // Assert
        Assert.Equal(3, queue.Count);
        Assert.Contains(1, queue);
        Assert.Contains(2, queue);
        Assert.Contains(3, queue);
    }

    [Fact]
    public void Enqueue_MaintainsQueueOrder()
    {
        // Arrange
        var queue = new HashQueue<int>();

        // Act
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);

        // Assert
        var array = queue.ToArray();
        Assert.Equal(1, array[0]);
        Assert.Equal(2, array[1]);
        Assert.Equal(3, array[2]);
    }

    #endregion Enqueue 方法测试

    #region Dequeue 方法测试

    [Fact]
    public void Dequeue_RemovesAndReturnsFirstItem()
    {
        // Arrange
        var queue = new HashQueue<int>(new[] { 1, 2, 3 });

        // Act
        var item = queue.Dequeue();

        // Assert
        Assert.Equal(1, item);
        Assert.Equal(2, queue.Count);
        Assert.DoesNotContain(1, queue);
    }

    [Fact]
    public void Dequeue_ThrowsInvalidOperationExceptionWhenEmpty()
    {
        // Arrange
        var queue = new HashQueue<int>();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => queue.Dequeue());
    }

    [Fact]
    public void Dequeue_RemovesItemsInFifoOrder()
    {
        // Arrange
        var queue = new HashQueue<int>(new[] { 1, 2, 3 });

        // Act
        var first = queue.Dequeue();
        var second = queue.Dequeue();
        var third = queue.Dequeue();

        // Assert
        Assert.Equal(1, first);
        Assert.Equal(2, second);
        Assert.Equal(3, third);
        Assert.Empty(queue);
    }

    [Fact]
    public void Dequeue_UpdatesHashSet()
    {
        // Arrange
        var queue = new HashQueue<int>(new[] { 1, 2, 3 });

        // Act
        queue.Dequeue();

        // Assert
        Assert.False(queue.Contains(1));
    }

    #endregion Dequeue 方法测试

    #region TryDequeue 方法测试

    [Fact]
    public void TryDequeue_ReturnsTrueAndRemovesItem()
    {
        // Arrange
        var queue = new HashQueue<int>(new[] { 1, 2, 3 });

        // Act
        var result = queue.TryDequeue(out var item);

        // Assert
        Assert.True(result);
        Assert.Equal(1, item);
        Assert.Equal(2, queue.Count);
    }

    [Fact]
    public void TryDequeue_ReturnsFalseWhenEmpty()
    {
        // Arrange
        var queue = new HashQueue<int>();

        // Act
        var result = queue.TryDequeue(out var item);

        // Assert
        Assert.False(result);
        Assert.Equal(default(int), item);
    }

    [Fact]
    public void TryDequeue_RemovesItemsInFifoOrder()
    {
        // Arrange
        var queue = new HashQueue<int>(new[] { 1, 2, 3 });

        // Act
        queue.TryDequeue(out var first);
        queue.TryDequeue(out var second);
        queue.TryDequeue(out var third);

        // Assert
        Assert.Equal(1, first);
        Assert.Equal(2, second);
        Assert.Equal(3, third);
    }

    #endregion TryDequeue 方法测试

    #region Peek 方法测试

    [Fact]
    public void Peek_ReturnsFirstItemWithoutRemoving()
    {
        // Arrange
        var queue = new HashQueue<int>(new[] { 1, 2, 3 });

        // Act
        var item = queue.Peek();

        // Assert
        Assert.Equal(1, item);
        Assert.Equal(3, queue.Count);
        Assert.Contains(1, queue);
    }

    [Fact]
    public void Peek_ThrowsInvalidOperationExceptionWhenEmpty()
    {
        // Arrange
        var queue = new HashQueue<int>();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => queue.Peek());
    }

    [Fact]
    public void Peek_MultipleCallsReturnSameItem()
    {
        // Arrange
        var queue = new HashQueue<int>(new[] { 1, 2, 3 });

        // Act
        var first = queue.Peek();
        var second = queue.Peek();

        // Assert
        Assert.Equal(first, second);
        Assert.Equal(3, queue.Count);
    }

    #endregion Peek 方法测试

    #region TryPeek 方法测试

    [Fact]
    public void TryPeek_ReturnsTrueAndItem()
    {
        // Arrange
        var queue = new HashQueue<int>(new[] { 1, 2, 3 });

        // Act
        var result = queue.TryPeek(out var item);

        // Assert
        Assert.True(result);
        Assert.Equal(1, item);
    }

    [Fact]
    public void TryPeek_ReturnsFalseWhenEmpty()
    {
        // Arrange
        var queue = new HashQueue<int>();

        // Act
        var result = queue.TryPeek(out var item);

        // Assert
        Assert.False(result);
        Assert.Equal(default(int), item);
    }

    [Fact]
    public void TryPeek_DoesNotRemoveItem()
    {
        // Arrange
        var queue = new HashQueue<int>(new[] { 1, 2, 3 });

        // Act
        queue.TryPeek(out var item);

        // Assert
        Assert.Equal(3, queue.Count);
        Assert.Contains(1, queue);
    }

    #endregion TryPeek 方法测试

    #region Contains 方法测试

    [Fact]
    public void Contains_ReturnsTrueForExistingItem()
    {
        // Arrange
        var queue = new HashQueue<int>(new[] { 1, 2, 3 });

        // Act
        var result = queue.Contains(2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Contains_ReturnsFalseForNonExistingItem()
    {
        // Arrange
        var queue = new HashQueue<int>(new[] { 1, 2, 3 });

        // Act
        var result = queue.Contains(4);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Contains_ReturnsFalseForRemovedItem()
    {
        // Arrange
        var queue = new HashQueue<int>(new[] { 1, 2, 3 });

        // Act
        queue.Dequeue();
        var result = queue.Contains(1);

        // Assert
        Assert.False(result);
    }

    #endregion Contains 方法测试

    #region Clear 方法测试

    [Fact]
    public void Clear_RemovesAllItems()
    {
        // Arrange
        var queue = new HashQueue<int>(new[] { 1, 2, 3 });

        // Act
        queue.Clear();

        // Assert
        Assert.Empty(queue);
        Assert.Equal(0, queue.Count);
        Assert.True(queue.IsEmpty);
    }

    [Fact]
    public void Clear_AllowsReenqueueAfterClear()
    {
        // Arrange
        var queue = new HashQueue<int>(new[] { 1, 2, 3 });
        queue.Clear();

        // Act
        queue.Enqueue(4);

        // Assert
        Assert.Single(queue);
        Assert.Contains(4, queue);
    }

    #endregion Clear 方法测试

    #region Remove 方法测试

    [Fact]
    public void Remove_RemovesSpecificItem()
    {
        // Arrange
        var queue = new HashQueue<int>(new[] { 1, 2, 3 });

        // Act
        var result = queue.Remove(2);

        // Assert
        Assert.True(result);
        Assert.Equal(2, queue.Count);
        Assert.DoesNotContain(2, queue);
        Assert.Contains(1, queue);
        Assert.Contains(3, queue);
    }

    [Fact]
    public void Remove_ReturnsFalseForNonExistingItem()
    {
        // Arrange
        var queue = new HashQueue<int>(new[] { 1, 2, 3 });

        // Act
        var result = queue.Remove(4);

        // Assert
        Assert.False(result);
        Assert.Equal(3, queue.Count);
    }

    [Fact]
    public void Remove_MaintainsRemainingItemsOrder()
    {
        // Arrange
        var queue = new HashQueue<int>(new[] { 1, 2, 3, 4, 5 });

        // Act
        queue.Remove(3);

        // Assert
        var array = queue.ToArray();
        Assert.Equal(new[] { 1, 2, 4, 5 }, array);
    }

    [Fact]
    public void Remove_RemovesFirstOccurrenceOnly()
    {
        // Arrange
        var queue = new HashQueue<int>(new[] { 1, 2, 3 });

        // Act
        var result = queue.Remove(1);

        // Assert
        Assert.True(result);
        Assert.DoesNotContain(1, queue);
    }

    #endregion Remove 方法测试

    #region 属性测试

    [Fact]
    public void Count_ReturnsCorrectCount()
    {
        // Arrange
        var queue = new HashQueue<int>();

        // Act & Assert
        Assert.Equal(0, queue.Count);
        queue.Enqueue(1);
        Assert.Equal(1, queue.Count);
        queue.Enqueue(2);
        Assert.Equal(2, queue.Count);
        queue.Dequeue();
        Assert.Equal(1, queue.Count);
    }

    [Fact]
    public void IsEmpty_ReturnsTrueForEmptyQueue()
    {
        // Arrange
        var queue = new HashQueue<int>();

        // Act & Assert
        Assert.True(queue.IsEmpty);
    }

    [Fact]
    public void IsEmpty_ReturnsFalseForNonEmptyQueue()
    {
        // Arrange
        var queue = new HashQueue<int>(new[] { 1 });

        // Act & Assert
        Assert.False(queue.IsEmpty);
    }

    [Fact]
    public void Comparer_ReturnsDefaultComparerWhenNotSpecified()
    {
        // Arrange
        var queue = new HashQueue<int>();

        // Act
        var comparer = queue.Comparer;

        // Assert
        Assert.NotNull(comparer);
        Assert.Equal(EqualityComparer<int>.Default, comparer);
    }

    [Fact]
    public void Comparer_ReturnsSpecifiedComparer()
    {
        // Arrange
        var customComparer = new AbsoluteValueComparer();
        var queue = new HashQueue<int>(customComparer);

        // Act
        var comparer = queue.Comparer;

        // Assert
        Assert.Equal(customComparer, comparer);
    }

    #endregion 属性测试

    #region ToArray 方法测试

    [Fact]
    public void ToArray_ReturnsArrayWithAllItems()
    {
        // Arrange
        var queue = new HashQueue<int>(new[] { 1, 2, 3 });

        // Act
        var array = queue.ToArray();

        // Assert
        Assert.Equal(3, array.Length);
        Assert.Equal(new[] { 1, 2, 3 }, array);
    }

    [Fact]
    public void ToArray_ReturnsEmptyArrayForEmptyQueue()
    {
        // Arrange
        var queue = new HashQueue<int>();

        // Act
        var array = queue.ToArray();

        // Assert
        Assert.Empty(array);
    }

    [Fact]
    public void ToArray_ReturnsItemsInQueueOrder()
    {
        // Arrange
        var queue = new HashQueue<int>();
        queue.Enqueue(5);
        queue.Enqueue(3);
        queue.Enqueue(8);

        // Act
        var array = queue.ToArray();

        // Assert
        Assert.Equal(new[] { 5, 3, 8 }, array);
    }

    #endregion ToArray 方法测试

    #region CopyTo 方法测试

    [Fact]
    public void CopyTo_CopiesItemsToArray()
    {
        // Arrange
        var queue = new HashQueue<int>(new[] { 1, 2, 3 });
        var targetArray = new int[5];

        // Act
        queue.CopyTo(targetArray, 0);

        // Assert
        Assert.Equal(1, targetArray[0]);
        Assert.Equal(2, targetArray[1]);
        Assert.Equal(3, targetArray[2]);
        Assert.Equal(0, targetArray[3]);
        Assert.Equal(0, targetArray[4]);
    }

    [Fact]
    public void CopyTo_CopiesItemsWithOffset()
    {
        // Arrange
        var queue = new HashQueue<int>(new[] { 1, 2, 3 });
        var targetArray = new int[5];

        // Act
        queue.CopyTo(targetArray, 1);

        // Assert
        Assert.Equal(0, targetArray[0]);
        Assert.Equal(1, targetArray[1]);
        Assert.Equal(2, targetArray[2]);
        Assert.Equal(3, targetArray[3]);
        Assert.Equal(0, targetArray[4]);
    }

    #endregion CopyTo 方法测试

    #region TrimExcess 方法测试

    [Fact]
    public void TrimExcess_DoesNotThrow()
    {
        // Arrange
        var queue = new HashQueue<int>(new[] { 1, 2, 3 });

        // Act & Assert
        queue.TrimExcess();
    }

    #endregion TrimExcess 方法测试

    #region 集合接口测试

    [Fact]
    public void IEnumerable_CanIterateOverItems()
    {
        // Arrange
        var queue = new HashQueue<int>(new[] { 1, 2, 3 });
        var items = new List<int>();

        // Act
        foreach (var item in queue)
        {
            items.Add(item);
        }

        // Assert
        Assert.Equal(new[] { 1, 2, 3 }, items);
    }

    [Fact]
    public void IEnumerable_GetEnumerator_ReturnsEnumerator()
    {
        // Arrange
        var queue = new HashQueue<int>(new[] { 1, 2, 3 });

        // Act
        var enumerator = queue.GetEnumerator();

        // Assert
        Assert.NotNull(enumerator);
    }

    [Fact]
    public void IEnumerableNonGeneric_CanIterateOverItems()
    {
        // Arrange
        var queue = new HashQueue<int>(new[] { 1, 2, 3 });
        IEnumerable enumerable = queue;
        var items = new List<int>();

        // Act
        foreach (var item in enumerable)
        {
            items.Add((int)item);
        }

        // Assert
        Assert.Equal(new[] { 1, 2, 3 }, items);
    }

    [Fact]
    public void ICollection_CopyTo_CopiesItems()
    {
        // Arrange
        var queue = new HashQueue<int>(new[] { 1, 2, 3 });
        ICollection collection = queue;
        var array = new object[5];

        // Act
        collection.CopyTo(array, 0);

        // Assert
        Assert.Equal(1, (int)array[0]);
        Assert.Equal(2, (int)array[1]);
        Assert.Equal(3, (int)array[2]);
    }

    [Fact]
    public void ICollection_Count_ReturnsCount()
    {
        // Arrange
        var queue = new HashQueue<int>(new[] { 1, 2, 3 });
        ICollection collection = queue;

        // Act
        var count = collection.Count;

        // Assert
        Assert.Equal(3, count);
    }

    [Fact]
    public void ICollection_IsSynchronized_ReturnsFalse()
    {
        // Arrange
        var queue = new HashQueue<int>();
        ICollection collection = queue;

        // Act
        var isSynchronized = collection.IsSynchronized;

        // Assert
        Assert.False(isSynchronized);
    }

    [Fact]
    public void ICollection_SyncRoot_ReturnsNonNull()
    {
        // Arrange
        var queue = new HashQueue<int>();
        ICollection collection = queue;

        // Act
        var syncRoot = collection.SyncRoot;

        // Assert
        Assert.NotNull(syncRoot);
    }

    #endregion 集合接口测试

    #region EnqueueRange 方法测试

    [Fact]
    public void EnqueueRange_AddsMultipleItems()
    {
        // Arrange
        var queue = new HashQueue<int>();
        var items = new[] { 1, 2, 3, 4, 5 };

        // Act
        var count = queue.EnqueueRange(items);

        // Assert
        Assert.Equal(5, count);
        Assert.Equal(5, queue.Count);
    }

    [Fact]
    public void EnqueueRange_IgnoresDuplicates()
    {
        // Arrange
        var queue = new HashQueue<int>(new[] { 1, 2 });
        var items = new[] { 2, 3, 4 };

        // Act
        var count = queue.EnqueueRange(items);

        // Assert
        Assert.Equal(2, count); // Only 3 and 4 are new
        Assert.Equal(4, queue.Count); // 1, 2, 3, 4
        Assert.True(queue.Contains(1));
        Assert.True(queue.Contains(2));
        Assert.True(queue.Contains(3));
        Assert.True(queue.Contains(4));
    }

    [Fact]
    public void EnqueueRange_WithNullCollection_ThrowsArgumentNullException()
    {
        // Arrange
        var queue = new HashQueue<int>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => queue.EnqueueRange(null));
    }

    [Fact]
    public void EnqueueRange_WithEmptyCollection_ReturnsZero()
    {
        // Arrange
        var queue = new HashQueue<int>(new[] { 1, 2, 3 });
        var items = new int[] { };

        // Act
        var count = queue.EnqueueRange(items);

        // Assert
        Assert.Equal(0, count);
        Assert.Equal(3, queue.Count);
    }

    #endregion EnqueueRange 方法测试

    #region DequeueRange 方法测试

    [Fact]
    public void DequeueRange_RemovesMultipleItems()
    {
        // Arrange
        var queue = new HashQueue<int>(new[] { 1, 2, 3, 4, 5 });

        // Act
        var items = queue.DequeueRange(3).ToList();

        // Assert
        Assert.Equal(3, items.Count);
        Assert.Equal(new[] { 1, 2, 3 }, items);
        Assert.Equal(2, queue.Count);
    }

    [Fact]
    public void DequeueRange_WithCountGreaterThanQueueSize_RemovesAllItems()
    {
        // Arrange
        var queue = new HashQueue<int>(new[] { 1, 2, 3 });

        // Act
        var items = queue.DequeueRange(10).ToList();

        // Assert
        Assert.Equal(3, items.Count);
        Assert.Empty(queue);
    }

    [Fact]
    public void DequeueRange_WithZeroCount_ReturnsEmptyCollection()
    {
        // Arrange
        var queue = new HashQueue<int>(new[] { 1, 2, 3 });

        // Act
        var items = queue.DequeueRange(0).ToList();

        // Assert
        Assert.Empty(items);
        Assert.Equal(3, queue.Count);
    }

    [Fact]
    public void DequeueRange_WithNegativeCount_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var queue = new HashQueue<int>(new[] { 1, 2, 3 });

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => queue.DequeueRange(-1));
    }

    [Fact]
    public void DequeueRange_RemovesItemsInFifoOrder()
    {
        // Arrange
        var queue = new HashQueue<int>(new[] { 1, 2, 3, 4, 5 });

        // Act
        var items = queue.DequeueRange(3).ToList();

        // Assert
        Assert.Equal(1, items[0]);
        Assert.Equal(2, items[1]);
        Assert.Equal(3, items[2]);
    }

    [Fact]
    public void DequeueRange_UpdatesQueueState()
    {
        // Arrange
        var queue = new HashQueue<int>(new[] { 1, 2, 3, 4, 5 });

        // Act
        queue.DequeueRange(2);

        // Assert
        Assert.False(queue.Contains(1));
        Assert.False(queue.Contains(2));
        Assert.True(queue.Contains(3));
        Assert.Equal(3, queue.Count);
    }

    #endregion DequeueRange 方法测试

    #region 自定义比较器测试

    [Fact]
    public void WithCustomComparer_IgnoresDuplicatesAccordingToComparer()
    {
        // Arrange
        var comparer = new AbsoluteValueComparer();
        var queue = new HashQueue<int>(comparer);

        // Act
        var result1 = queue.Enqueue(5);
        var result2 = queue.Enqueue(-5);

        // Assert
        Assert.True(result1);
        Assert.False(result2);
        Assert.Single(queue);
    }

    #endregion 自定义比较器测试

    #region 综合场景测试

    [Fact]
    public void ComplexScenario_MixedOperations()
    {
        // Arrange
        var queue = new HashQueue<string>(new[] { "a", "b", "c" });

        // Act & Assert
        Assert.Equal(3, queue.Count);

        queue.Enqueue("d");
        Assert.Equal(4, queue.Count);

        var peeked = queue.Peek();
        Assert.Equal("a", peeked);
        Assert.Equal(4, queue.Count);

        var dequeued = queue.Dequeue();
        Assert.Equal("a", dequeued);
        Assert.Equal(3, queue.Count);

        queue.Enqueue("e");
        queue.Enqueue("b"); // Duplicate, should not be added
        Assert.Equal(4, queue.Count);

        queue.Remove("c");
        Assert.False(queue.Contains("c"));
        Assert.Equal(3, queue.Count);

        queue.Clear();
        Assert.True(queue.IsEmpty);
    }

    [Fact]
    public void ComplexScenario_EnqueueAndDequeueWithDuplicates()
    {
        // Arrange
        var queue = new HashQueue<int>();

        // Act
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(1); // Duplicate
        queue.Enqueue(3);
        queue.Enqueue(2); // Duplicate

        // Assert
        Assert.Equal(3, queue.Count);
        Assert.Equal(1, queue.Dequeue());
        Assert.Equal(2, queue.Dequeue());
        Assert.Equal(3, queue.Dequeue());
        Assert.True(queue.IsEmpty);
    }

    [Fact]
    public void ComplexScenario_RemoveMiddleItem()
    {
        // Arrange
        var queue = new HashQueue<int>(new[] { 1, 2, 3, 4, 5 });

        // Act
        queue.Remove(3);

        // Assert
        Assert.Equal(4, queue.Count);
        var array = queue.ToArray();
        Assert.Equal(new[] { 1, 2, 4, 5 }, array);
    }

    #endregion 综合场景测试
}

/// <summary>
/// 自定义比较器，用于测试，比较绝对值
/// </summary>
public class AbsoluteValueComparer : IEqualityComparer<int>
{
    public bool Equals(int x, int y)
    {
        return Math.Abs(x) == Math.Abs(y);
    }

    public int GetHashCode(int obj)
    {
        return Math.Abs(obj).GetHashCode();
    }
}
