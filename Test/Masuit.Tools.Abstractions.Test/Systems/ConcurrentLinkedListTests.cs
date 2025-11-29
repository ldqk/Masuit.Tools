using Masuit.Tools.Abstractions.Systems;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Systems
{
    public class ConcurrentLinkedListTests
    {
        [Fact]
        public void Add_ShouldAddItemToLast()
        {
            var list = new ConcurrentLinkedList<int>();
            list.Add(1);
            list.Add(2);

            Assert.Equal(2, list.Count);
            Assert.Equal(1, list.First?.Value);
            Assert.Equal(2, list.Last?.Value);
        }

        [Fact]
        public void AddFirst_Value_ShouldAddAtBeginning()
        {
            var list = new ConcurrentLinkedList<int>();
            list.AddLast(1);
            list.AddFirst(0);

            Assert.Equal(0, list.First?.Value);
            Assert.Equal(2, list.Count);
        }

        [Fact]
        public void AddLast_Value_ShouldAddAtEnd()
        {
            var list = new ConcurrentLinkedList<int>();
            list.AddLast(1);
            list.AddLast(2);

            Assert.Equal(2, list.Last?.Value);
            Assert.Equal(2, list.Count);
        }

        [Fact]
        public void AddFirst_Node_ShouldAddAtBeginning()
        {
            var list = new ConcurrentLinkedList<int>();
            list.AddLast(1);
            var node = new LinkedListNode<int>(0);
            list.AddFirst(node);

            Assert.Equal(node, list.First);
            Assert.Equal(0, list.First?.Value);
        }

        [Fact]
        public void AddLast_Node_ShouldAddAtEnd()
        {
            var list = new ConcurrentLinkedList<int>();
            list.AddFirst(1);
            var node = new LinkedListNode<int>(2);
            list.AddLast(node);

            Assert.Equal(node, list.Last);
            Assert.Equal(2, list.Last?.Value);
        }

        [Fact]
        public void AddAfter_Value_ShouldInsertCorrectly()
        {
            var list = new ConcurrentLinkedList<int>();
            var node1 = list.AddFirst(1);
            list.AddAfter(node1, 2);

            Assert.Equal(2, list.Last?.Value);
            Assert.Equal(2, list.Count);
        }

        [Fact]
        public void AddAfter_Node_ShouldInsertCorrectly()
        {
            var list = new ConcurrentLinkedList<int>();
            var node1 = list.AddFirst(1);
            var node2 = new LinkedListNode<int>(2);
            list.AddAfter(node1, node2);

            Assert.Equal(node2, list.Last);
            Assert.Equal(2, list.Count);
        }

        [Fact]
        public void AddBefore_Value_ShouldInsertCorrectly()
        {
            var list = new ConcurrentLinkedList<int>();
            var node2 = list.AddFirst(2);
            list.AddBefore(node2, 1);

            Assert.Equal(1, list.First?.Value);
            Assert.Equal(2, list.Count);
        }

        [Fact]
        public void AddBefore_Node_ShouldInsertCorrectly()
        {
            var list = new ConcurrentLinkedList<int>();
            var node2 = list.AddFirst(2);
            var node1 = new LinkedListNode<int>(1);
            list.AddBefore(node2, node1);

            Assert.Equal(node1, list.First);
            Assert.Equal(2, list.Count);
        }

        [Fact]
        public void Remove_Value_ShouldRemoveItem()
        {
            var list = new ConcurrentLinkedList<int>();
            list.Add(1);
            list.Add(2);

            var removed = list.Remove(1);

            Assert.True(removed);
            Assert.Single(list);
            Assert.Equal(2, list.First?.Value);
        }

        [Fact]
        public void Remove_Value_ShouldReturnFalseIfNotFound()
        {
            var list = new ConcurrentLinkedList<int>();
            list.Add(1);

            var removed = list.Remove(2);

            Assert.False(removed);
            Assert.Single(list);
        }

        [Fact]
        public void Remove_Node_ShouldRemoveItem()
        {
            var list = new ConcurrentLinkedList<int>();
            var node = list.AddFirst(1);
            list.AddLast(2);

            list.Remove(node);

            Assert.Single(list);
            Assert.Equal(2, list.First?.Value);
        }

        [Fact]
        public void RemoveFirst_ShouldRemoveHead()
        {
            var list = new ConcurrentLinkedList<int>();
            list.Add(1);
            list.Add(2);

            list.RemoveFirst();

            Assert.Single(list);
            Assert.Equal(2, list.First?.Value);
        }

        [Fact]
        public void RemoveLast_ShouldRemoveTail()
        {
            var list = new ConcurrentLinkedList<int>();
            list.Add(1);
            list.Add(2);

            list.RemoveLast();

            Assert.Single(list);
            Assert.Equal(1, list.Last?.Value);
        }

        [Fact]
        public void Clear_ShouldEmptyList()
        {
            var list = new ConcurrentLinkedList<int>();
            list.Add(1);
            list.Add(2);

            list.Clear();

            Assert.Empty(list);
            Assert.Null(list.First);
            Assert.Null(list.Last);
        }

        [Fact]
        public void Contains_ShouldReturnTrueForExistingItem()
        {
            var list = new ConcurrentLinkedList<int>();
            list.Add(1);

            Assert.Contains(1, list);
        }

        [Fact]
        public void Contains_ShouldReturnFalseForNonExistingItem()
        {
            var list = new ConcurrentLinkedList<int>();
            list.Add(1);

            Assert.DoesNotContain(2, list);
        }

        [Fact]
        public void Find_ShouldReturnNode()
        {
            var list = new ConcurrentLinkedList<int>();
            list.Add(1);
            list.Add(2);

            var node = list.Find(2);

            Assert.NotNull(node);
            Assert.Equal(2, node?.Value);
        }

        [Fact]
        public void FindLast_ShouldReturnLastMatchingNode()
        {
            var list = new ConcurrentLinkedList<int>();
            list.Add(1);
            list.Add(2);
            list.Add(1);

            var node = list.FindLast(1);

            Assert.NotNull(node);
            Assert.Equal(node, list.Last);
        }

        [Fact]
        public void CopyTo_ShouldCopyElements()
        {
            var list = new ConcurrentLinkedList<int>();
            list.Add(1);
            list.Add(2);
            var array = new int[2];

            list.CopyTo(array, 0);

            Assert.Equal(1, array[0]);
            Assert.Equal(2, array[1]);
        }

        [Fact]
        public void GetEnumerator_ShouldIterateAllItems()
        {
            var list = new ConcurrentLinkedList<int> { 1, 2, 3 };
            var result = list.ToList();
            Assert.Equal(new[] { 1, 2, 3 }, result);
        }

        [Fact]
        public void Constructor_WithCollection_ShouldInitialize()
        {
            var initialData = new[] { 1, 2, 3 };
            var list = new ConcurrentLinkedList<int>(initialData);

            Assert.Equal(3, list.Count);
            Assert.Equal(1, list.First?.Value);
            Assert.Equal(3, list.Last?.Value);
        }
    }
}