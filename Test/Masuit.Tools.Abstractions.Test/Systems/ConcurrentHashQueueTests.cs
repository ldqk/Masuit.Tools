using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Masuit.Tools.Systems;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Systems;

/// <summary>
/// ConcurrentHashQueue&lt;T&gt; 的单元测试类
/// </summary>
public class ConcurrentHashQueueTests
{
    #region 构造函数测试

    [Fact]
    public void Constructor_Default_CreatesEmptyQueue()
    {
        // Arrange & Act
        var queue = new ConcurrentHashQueue<int>();

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
        var queue = new ConcurrentHashQueue<int>(comparer);

        // Assert
        Assert.NotNull(queue);
        Assert.True(queue.IsEmpty);
    }

    [Fact]
    public void Constructor_WithCollection_InitializesWithItems()
    {
        // Arrange
        var items = new[] { 1, 2, 3 };

        // Act
        var queue = new ConcurrentHashQueue<int>(items);

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
        var queue = new ConcurrentHashQueue<int>(items, comparer);

        // Assert
        Assert.Equal(3, queue.Count);
    }

    [Fact]
    public void Constructor_WithCollection_IgnoresDuplicates()
    {
        // Arrange
        var items = new[] { 1, 2, 2, 3, 1 };

        // Act
        var queue = new ConcurrentHashQueue<int>(items);

        // Assert
        Assert.Equal(3, queue.Count);
    }

    #endregion 构造函数测试

    #region Enqueue 方法测试

    [Fact]
    public void Enqueue_AddsSingleItem()
    {
        // Arrange
        var queue = new ConcurrentHashQueue<int>();

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
        var queue = new ConcurrentHashQueue<int>();
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
        var queue = new ConcurrentHashQueue<int>();

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
        var queue = new ConcurrentHashQueue<int>();

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

    #region TryDequeue 方法测试

    [Fact]
    public void TryDequeue_ReturnsTrueAndRemovesItem()
    {
        // Arrange
        var queue = new ConcurrentHashQueue<int>(new[] { 1, 2, 3 });

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
        var queue = new ConcurrentHashQueue<int>();

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
        var queue = new ConcurrentHashQueue<int>(new[] { 1, 2, 3 });

        // Act
        queue.TryDequeue(out var first);
        queue.TryDequeue(out var second);
        queue.TryDequeue(out var third);

        // Assert
        Assert.Equal(1, first);
        Assert.Equal(2, second);
        Assert.Equal(3, third);
    }

    [Fact]
    public void TryDequeue_RemovesItemFromHashSet()
    {
        // Arrange
        var queue = new ConcurrentHashQueue<int>(new[] { 1, 2, 3 });

        // Act
        queue.TryDequeue(out var item);

        // Assert
        Assert.False(queue.Contains(1));
    }

    #endregion TryDequeue 方法测试

    #region TryPeek 方法测试

    [Fact]
    public void TryPeek_ReturnsTrueAndItem()
    {
        // Arrange
        var queue = new ConcurrentHashQueue<int>(new[] { 1, 2, 3 });

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
        var queue = new ConcurrentHashQueue<int>();

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
        var queue = new ConcurrentHashQueue<int>(new[] { 1, 2, 3 });

        // Act
        queue.TryPeek(out var item);

        // Assert
        Assert.Equal(3, queue.Count);
        Assert.Contains(1, queue);
    }

    [Fact]
    public void TryPeek_MultipleCallsReturnSameItem()
    {
        // Arrange
        var queue = new ConcurrentHashQueue<int>(new[] { 1, 2, 3 });

        // Act
        queue.TryPeek(out var first);
        queue.TryPeek(out var second);

        // Assert
        Assert.Equal(first, second);
        Assert.Equal(3, queue.Count);
    }

    #endregion TryPeek 方法测试

    #region Contains 方法测试

    [Fact]
    public void Contains_ReturnsTrueForExistingItem()
    {
        // Arrange
        var queue = new ConcurrentHashQueue<int>(new[] { 1, 2, 3 });

        // Act
        var result = queue.Contains(2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Contains_ReturnsFalseForNonExistingItem()
    {
        // Arrange
        var queue = new ConcurrentHashQueue<int>(new[] { 1, 2, 3 });

        // Act
        var result = queue.Contains(4);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Contains_ReturnsFalseForRemovedItem()
    {
        // Arrange
        var queue = new ConcurrentHashQueue<int>(new[] { 1, 2, 3 });

        // Act
        queue.TryDequeue(out _);
        var result = queue.Contains(1);

        // Assert
        Assert.False(result);
    }

    #endregion Contains 方法测试

    #region Add 方法测试

    [Fact]
    public void Add_AddsItem()
    {
        // Arrange
        var queue = new ConcurrentHashQueue<int>();

        // Act
        queue.Add(1);

        // Assert
        Assert.Single(queue);
        Assert.Contains(1, queue);
    }

    [Fact]
    public void Add_DoesNotThrowOnDuplicate()
    {
        // Arrange
        var queue = new ConcurrentHashQueue<int>();
        queue.Add(1);

        // Act & Assert (should not throw)
        queue.Add(1);
    }

    #endregion Add 方法测试

    #region Clear 方法测试

    [Fact]
    public void Clear_RemovesAllItems()
    {
        // Arrange
        var queue = new ConcurrentHashQueue<int>(new[] { 1, 2, 3 });

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
        var queue = new ConcurrentHashQueue<int>(new[] { 1, 2, 3 });
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
        var queue = new ConcurrentHashQueue<int>(new[] { 1, 2, 3 });

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
        var queue = new ConcurrentHashQueue<int>(new[] { 1, 2, 3 });

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
        var queue = new ConcurrentHashQueue<int>(new[] { 1, 2, 3, 4, 5 });

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
        var queue = new ConcurrentHashQueue<int>(new[] { 1, 2, 3 });

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
        var queue = new ConcurrentHashQueue<int>();

        // Act & Assert
        Assert.Equal(0, queue.Count);
        queue.Enqueue(1);
        Assert.Equal(1, queue.Count);
        queue.Enqueue(2);
        Assert.Equal(2, queue.Count);
        queue.TryDequeue(out _);
        Assert.Equal(1, queue.Count);
    }

    [Fact]
    public void IsEmpty_ReturnsTrueForEmptyQueue()
    {
        // Arrange
        var queue = new ConcurrentHashQueue<int>();

        // Act & Assert
        Assert.True(queue.IsEmpty);
    }

    [Fact]
    public void IsEmpty_ReturnsFalseForNonEmptyQueue()
    {
        // Arrange
        var queue = new ConcurrentHashQueue<int>(new[] { 1 });

        // Act & Assert
        Assert.False(queue.IsEmpty);
    }

    [Fact]
    public void IsReadOnly_ReturnsFalse()
    {
        // Arrange
        var queue = new ConcurrentHashQueue<int>();

        // Act
        var isReadOnly = queue.IsReadOnly;

        // Assert
        Assert.False(isReadOnly);
    }

    #endregion 属性测试

    #region ToArray 方法测试

    [Fact]
    public void ToArray_ReturnsArrayWithAllItems()
    {
        // Arrange
        var queue = new ConcurrentHashQueue<int>(new[] { 1, 2, 3 });

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
        var queue = new ConcurrentHashQueue<int>();

        // Act
        var array = queue.ToArray();

        // Assert
        Assert.Empty(array);
    }

    [Fact]
    public void ToArray_ReturnsItemsInQueueOrder()
    {
        // Arrange
        var queue = new ConcurrentHashQueue<int>();
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
        var queue = new ConcurrentHashQueue<int>(new[] { 1, 2, 3 });
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
        var queue = new ConcurrentHashQueue<int>(new[] { 1, 2, 3 });
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

    [Fact]
    public void CopyTo_WithNullArray_ThrowsArgumentNullException()
    {
        // Arrange
        var queue = new ConcurrentHashQueue<int>(new[] { 1, 2, 3 });

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => queue.CopyTo(null, 0));
    }

    [Fact]
    public void CopyTo_WithNegativeIndex_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var queue = new ConcurrentHashQueue<int>(new[] { 1, 2, 3 });
        var targetArray = new int[5];

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => queue.CopyTo(targetArray, -1));
    }

    [Fact]
    public void CopyTo_WithInsufficientSpace_ThrowsArgumentException()
    {
        // Arrange
        var queue = new ConcurrentHashQueue<int>(new[] { 1, 2, 3 });
        var targetArray = new int[2];

        // Act & Assert
        Assert.Throws<ArgumentException>(() => queue.CopyTo(targetArray, 0));
    }

    #endregion CopyTo 方法测试

    #region 集合接口测试

    [Fact]
    public void IEnumerable_CanIterateOverItems()
    {
        // Arrange
        var queue = new ConcurrentHashQueue<int>(new[] { 1, 2, 3 });
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
    public void IProducerConsumerCollection_TryAdd_AddsItem()
    {
        // Arrange
        var queue = new ConcurrentHashQueue<int>();
        IProducerConsumerCollection<int> collection = queue;

        // Act
        var result = collection.TryAdd(1);

        // Assert
        Assert.True(result);
        Assert.Single(queue);
    }

    [Fact]
    public void IProducerConsumerCollection_TryTake_RemovesItem()
    {
        // Arrange
        var queue = new ConcurrentHashQueue<int>(new[] { 1, 2, 3 });
        IProducerConsumerCollection<int> collection = queue;

        // Act
        var result = collection.TryTake(out var item);

        // Assert
        Assert.True(result);
        Assert.Equal(1, item);
        Assert.Equal(2, queue.Count);
    }

    #endregion 集合接口测试

    #region EnqueueRange 方法测试

    [Fact]
    public void EnqueueRange_AddsMultipleItems()
    {
        // Arrange
        var queue = new ConcurrentHashQueue<int>();
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
        var queue = new ConcurrentHashQueue<int>(new[] { 1, 2 });
        var items = new[] { 2, 3, 4 };

        // Act
        var count = queue.EnqueueRange(items);

        // Assert
        Assert.Equal(2, count);
        Assert.Equal(4, queue.Count);
        Assert.True(queue.Contains(1));
        Assert.True(queue.Contains(2));
        Assert.True(queue.Contains(3));
        Assert.True(queue.Contains(4));
    }

    [Fact]
    public void EnqueueRange_WithNullCollection_ThrowsArgumentNullException()
    {
        // Arrange
        var queue = new ConcurrentHashQueue<int>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => queue.EnqueueRange(null));
    }

    [Fact]
    public void EnqueueRange_WithEmptyCollection_ReturnsZero()
    {
        // Arrange
        var queue = new ConcurrentHashQueue<int>(new[] { 1, 2, 3 });
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
        var queue = new ConcurrentHashQueue<int>(new[] { 1, 2, 3, 4, 5 });

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
        var queue = new ConcurrentHashQueue<int>(new[] { 1, 2, 3 });

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
        var queue = new ConcurrentHashQueue<int>(new[] { 1, 2, 3 });

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
        var queue = new ConcurrentHashQueue<int>(new[] { 1, 2, 3 });

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => queue.DequeueRange(-1));
    }

    [Fact]
    public void DequeueRange_RemovesItemsInFifoOrder()
    {
        // Arrange
        var queue = new ConcurrentHashQueue<int>(new[] { 1, 2, 3, 4, 5 });

        // Act
        var items = queue.DequeueRange(3).ToList();

        // Assert
        Assert.Equal(1, items[0]);
        Assert.Equal(2, items[1]);
        Assert.Equal(3, items[2]);
    }

    #endregion DequeueRange 方法测试

    #region TryEnqueueWithLimit 方法测试

    [Fact]
    public void TryEnqueueWithLimit_AddsItemWhenBelowLimit()
    {
        // Arrange
        var queue = new ConcurrentHashQueue<int>(new[] { 1, 2 });

        // Act
        var result = queue.TryEnqueueWithLimit(3, 5);

        // Assert
        Assert.True(result);
        Assert.Equal(3, queue.Count);
        Assert.Contains(3, queue);
    }

    [Fact]
    public void TryEnqueueWithLimit_ReturnsFalseWhenAtLimitWithNewItem()
    {
        // Arrange
        var queue = new ConcurrentHashQueue<int>(new[] { 1, 2, 3 });

        // Act
        var result = queue.TryEnqueueWithLimit(4, 3);

        // Assert
        Assert.False(result);
        Assert.Equal(3, queue.Count);
        Assert.DoesNotContain(4, queue);
    }

    [Fact]
    public void TryEnqueueWithLimit_AddsExistingItemWhenAtLimit()
    {
        // Arrange
        var queue = new ConcurrentHashQueue<int>(new[] { 1, 2, 3 });

        // Act
        var result = queue.TryEnqueueWithLimit(1, 3);

        // Assert
        Assert.False(result);
        Assert.Equal(3, queue.Count);
    }

    [Fact]
    public void TryEnqueueWithLimit_WithInvalidLimit_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var queue = new ConcurrentHashQueue<int>();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => queue.TryEnqueueWithLimit(1, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => queue.TryEnqueueWithLimit(1, -1));
    }

    #endregion TryEnqueueWithLimit 方法测试

    #region EnqueueWithEviction 方法测试

    [Fact]
    public void EnqueueWithEviction_AddsItemWhenBelowLimit()
    {
        // Arrange
        var queue = new ConcurrentHashQueue<int>(new[] { 1, 2 });

        // Act
        var evicted = queue.EnqueueWithEviction(3, 5);

        // Assert
        Assert.Equal(default(int), evicted);
        Assert.Equal(3, queue.Count);
        Assert.Contains(3, queue);
    }

    [Fact]
    public void EnqueueWithEviction_EvictsOldestWhenAtLimit()
    {
        // Arrange
        var queue = new ConcurrentHashQueue<int>(new[] { 1, 2, 3 });

        // Act
        var evicted = queue.EnqueueWithEviction(4, 3);

        // Assert
        Assert.Equal(1, evicted);
        Assert.Equal(3, queue.Count);
        Assert.Contains(2, queue);
        Assert.Contains(3, queue);
        Assert.Contains(4, queue);
        Assert.DoesNotContain(1, queue);
    }

    [Fact]
    public void EnqueueWithEviction_RequeuesExistingItem()
    {
        // Arrange
        var queue = new ConcurrentHashQueue<int>(new[] { 1, 2, 3 });

        // Act
        var evicted = queue.EnqueueWithEviction(2, 3);

        // Assert
        Assert.Equal(default(int), evicted);
        Assert.Equal(3, queue.Count);
        var array = queue.ToArray();
        // Item 2 should be moved to the end
        Assert.Equal(2, array[2]);
    }

    [Fact]
    public void EnqueueWithEviction_WithInvalidLimit_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var queue = new ConcurrentHashQueue<int>();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => queue.EnqueueWithEviction(1, 0));
    }

    #endregion EnqueueWithEviction 方法测试

    #region 并发操作测试

    [Fact]
    public void ConcurrentEnqueue_MultipleThreads_AllItemsAdded()
    {
        // Arrange
        var queue = new ConcurrentHashQueue<int>();
        var tasks = new Task[10];

        // Act
        for (int i = 0; i < 10; i++)
        {
            int value = i;
            tasks[i] = Task.Run(() => queue.Enqueue(value));
        }
        Task.WaitAll(tasks);

        // Assert
        Assert.Equal(10, queue.Count);
    }

    [Fact]
    public void ConcurrentDequeue_MultipleThreads_AllItemsRemoved()
    {
        // Arrange
        var queue = new ConcurrentHashQueue<int>(Enumerable.Range(1, 10).ToArray());
        var tasks = new Task[10];
        var results = new List<int>();
        var lockObj = new object();

        // Act
        for (int i = 0; i < 10; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                if (queue.TryDequeue(out var item))
                {
                    lock (lockObj)
                    {
                        results.Add(item);
                    }
                }
            });
        }
        Task.WaitAll(tasks);

        // Assert
        Assert.Equal(10, results.Count);
        Assert.Empty(queue);
    }

    [Fact]
    public void ConcurrentEnqueueDequeue_ProducerConsumer()
    {
        // Arrange
        var queue = new ConcurrentHashQueue<int>();
        var tasks = new List<Task>();
        var enqueued = 0;
        var dequeued = 0;

        // Act - Producer
        var producerTask = Task.Run(() =>
        {
            for (int i = 0; i < 50; i++)
            {
                if (queue.Enqueue(i))
                {
                    Interlocked.Increment(ref enqueued);
                }
            }
        });

        // Act - Consumers
        var consumerTasks = new Task[3];
        for (int c = 0; c < 3; c++)
        {
            consumerTasks[c] = Task.Run(() =>
            {
                while (queue.TryDequeue(out _))
                {
                    Interlocked.Increment(ref dequeued);
                }
            });
        }

        Task.WaitAll(new[] { producerTask }.Concat(consumerTasks).ToArray());

        // Assert
        Assert.Equal(enqueued, dequeued);
    }

    [Fact]
    public void ConcurrentEnqueueWithDuplicates_OnlyUniqueItemsAdded()
    {
        // Arrange
        var queue = new ConcurrentHashQueue<int>();
        var tasks = new Task[10];

        // Act - All threads try to enqueue the same values
        for (int t = 0; t < 10; t++)
        {
            int threadId = t;
            tasks[t] = Task.Run(() =>
            {
                for (int i = 0; i < 5; i++)
                {
                    queue.Enqueue(i);
                }
            });
        }
        Task.WaitAll(tasks);

        // Assert - Only 5 unique items should be in queue
        Assert.Equal(5, queue.Count);
    }

    [Fact]
    public void ConcurrentRemove_WhileEnqueuing()
    {
        // Arrange
        var queue = new ConcurrentHashQueue<int>(new[] { 1, 2, 3, 4, 5 });

        // Act - Enqueue and Remove concurrently
        var enqueueTask = Task.Run(() =>
        {
            for (int i = 6; i < 15; i++)
            {
                queue.Enqueue(i);
            }
        });

        var removeTask = Task.Run(() =>
        {
            for (int i = 1; i < 6; i++)
            {
                queue.Remove(i);
                Thread.Sleep(10);
            }
        });

        Task.WaitAll(enqueueTask, removeTask);

        // Assert - Queue should have items from 6 to 14
        Assert.Equal(9, queue.Count);
        for (int i = 6; i < 15; i++)
        {
            Assert.Contains(i, queue);
        }
    }

    #endregion 并发操作测试

    #region ICollection 接口测试

    [Fact]
    public void ICollection_CopyTo_CopiesItems()
    {
        // Arrange
        var queue = new ConcurrentHashQueue<int>(new[] { 1, 2, 3 });
        System.Collections.ICollection collection = queue;
        var array = new object[5];

        // Act
        collection.CopyTo(array, 0);

        // Assert
        Assert.Equal(1, (int)array[0]);
        Assert.Equal(2, (int)array[1]);
        Assert.Equal(3, (int)array[2]);
    }

    [Fact]
    public void ICollection_IsSynchronized_ReturnsFalse()
    {
        // Arrange
        var queue = new ConcurrentHashQueue<int>();
        System.Collections.ICollection collection = queue;

        // Act
        var isSynchronized = collection.IsSynchronized;

        // Assert
        Assert.False(isSynchronized);
    }

    [Fact]
    public void ICollection_SyncRoot_ReturnsNonNull()
    {
        // Arrange
        var queue = new ConcurrentHashQueue<int>();
        System.Collections.ICollection collection = queue;

        // Act
        var syncRoot = collection.SyncRoot;

        // Assert
        Assert.NotNull(syncRoot);
    }

    #endregion ICollection 接口测试

    #region 综合场景测试

    [Fact]
    public void ComplexScenario_MixedOperations()
    {
        // Arrange
        var queue = new ConcurrentHashQueue<string>(new[] { "a", "b", "c" });

        // Act & Assert
        Assert.Equal(3, queue.Count);

        queue.Add("d");
        Assert.Equal(4, queue.Count);

        queue.TryPeek(out var peeked);
        Assert.Equal("a", peeked);
        Assert.Equal(4, queue.Count);

        queue.TryDequeue(out var dequeued);
        Assert.Equal("a", dequeued);
        Assert.Equal(3, queue.Count);

        queue.Add("e");
        queue.Add("b"); // Duplicate, silently ignored
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
        var queue = new ConcurrentHashQueue<int>();

        // Act
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(1); // Duplicate
        queue.Enqueue(3);
        queue.Enqueue(2); // Duplicate

        // Assert
        Assert.Equal(3, queue.Count);
        queue.TryDequeue(out var first);
        queue.TryDequeue(out var second);
        queue.TryDequeue(out var third);
        Assert.Equal(1, first);
        Assert.Equal(2, second);
        Assert.Equal(3, third);
        Assert.True(queue.IsEmpty);
    }

    [Fact]
    public void ComplexScenario_RemoveMiddleItem()
    {
        // Arrange
        var queue = new ConcurrentHashQueue<int>(new[] { 1, 2, 3, 4, 5 });

        // Act
        queue.Remove(3);

        // Assert
        Assert.Equal(4, queue.Count);
        var array = queue.ToArray();
        Assert.Equal(new[] { 1, 2, 4, 5 }, array);
    }

    #endregion 综合场景测试

    #region 自定义比较器测试

    [Fact]
    public void WithCustomComparer_IgnoresDuplicatesAccordingToComparer()
    {
        // Arrange
        var comparer = new AbsoluteValueComparer();
        var queue = new ConcurrentHashQueue<int>(comparer);

        // Act
        var result1 = queue.Enqueue(5);
        var result2 = queue.Enqueue(-5);

        // Assert
        Assert.True(result1);
        Assert.False(result2);
        Assert.Single(queue);
    }

    #endregion 自定义比较器测试
}