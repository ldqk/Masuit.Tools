using System.Collections.Generic;
using System.Linq;
using Masuit.Tools.Systems;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Systems;

public class ConcurrentLimitedQueueTests
{
    [Fact]
    public void Constructor_WithLimit_SetsLimit()
    {
        // Arrange
        int limit = 5;

        // Act
        var queue = new ConcurrentLimitedQueue<int>(limit);

        // Assert
        Assert.Equal(limit, queue.Limit);
    }

    [Fact]
    public void Constructor_WithList_SetsLimitAndInitialItems()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3 };

        // Act
        var queue = new ConcurrentLimitedQueue<int>(list);

        // Assert
        Assert.Equal(list.Count, queue.Limit);
        Assert.Equal(list, queue.ToList());
    }

    [Fact]
    public void ImplicitOperator_ConvertsListToQueue()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3 };

        // Act
        ConcurrentLimitedQueue<int> queue = list;

        // Assert
        Assert.Equal(list.Count, queue.Limit);
        Assert.Equal(list, queue.ToList());
    }

    [Fact]
    public void Enqueue_AddsItemToQueue()
    {
        // Arrange
        var queue = new ConcurrentLimitedQueue<int>(3);

        // Act
        queue.Enqueue(1);

        // Assert
        Assert.Single(queue);
        Assert.Equal(1, queue.ToList()[0]);
    }

    [Fact]
    public void Enqueue_RemovesOldestItemWhenLimitExceeded()
    {
        // Arrange
        var queue = new ConcurrentLimitedQueue<int>(3);
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);

        // Act
        queue.Enqueue(4);

        // Assert
        Assert.Equal(3, queue.Count);
        Assert.DoesNotContain(1, queue);
        Assert.Equal(new List<int> { 2, 3, 4 }, queue.ToList());
    }
}