using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Xunit;

namespace Masuit.Tools.Systems.Tests
{
    public class ConcurrentHashSetTests
    {
        [Fact]
        public void Add_ShouldAddItem()
        {
            var set = new ConcurrentHashSet<int>();
            set.Add(1);
            Assert.Contains(1, set);
        }

        [Fact]
        public void TryAdd_ShouldReturnTrueForNewItem()
        {
            var set = new ConcurrentHashSet<int>();
            var result = set.TryAdd(1);
            Assert.True(result);
            Assert.Contains(1, set);
        }

        [Fact]
        public void TryAdd_ShouldReturnFalseForExistingItem()
        {
            var set = new ConcurrentHashSet<int>();
            set.Add(1);
            var result = set.TryAdd(1);
            Assert.False(result);
        }

        [Fact]
        public void UnionWith_ShouldUnionSets()
        {
            var set = new ConcurrentHashSet<int> { 1, 2 };
            set.UnionWith(new[] { 2, 3 });
            Assert.Contains(1, set);
            Assert.Contains(2, set);
            Assert.Contains(3, set);
        }

        [Fact]
        public void IntersectWith_ShouldIntersectSets()
        {
            var set = new ConcurrentHashSet<int> { 1, 2 };
            set.IntersectWith(new[] { 2, 3 });
            Assert.DoesNotContain(1, set);
            Assert.Contains(2, set);
            Assert.DoesNotContain(3, set);
        }

        [Fact]
        public void ExceptWith_ShouldExceptSets()
        {
            var set = new ConcurrentHashSet<int> { 1, 2, 3 };
            set.ExceptWith(new[] { 2, 3 });
            Assert.Contains(1, set);
            Assert.DoesNotContain(2, set);
            Assert.DoesNotContain(3, set);
        }

        [Fact]
        public void SymmetricExceptWith_ShouldSymmetricExceptSets()
        {
            var set = new ConcurrentHashSet<int> { 1, 2, 3 };
            set.SymmetricExceptWith(new[] { 2, 3, 4 });
            Assert.Contains(1, set);
            Assert.DoesNotContain(2, set);
            Assert.DoesNotContain(3, set);
            Assert.Contains(4, set);
        }

        [Fact]
        public void IsSubsetOf_ShouldReturnTrueForSubset()
        {
            var set = new ConcurrentHashSet<int> { 1, 2 };
            var result = set.IsSubsetOf(new[] { 1, 2, 3 });
            Assert.True(result);
        }

        [Fact]
        public void IsSupersetOf_ShouldReturnTrueForSuperset()
        {
            var set = new ConcurrentHashSet<int> { 1, 2, 3 };
            var result = set.IsSupersetOf(new[] { 1, 2 });
            Assert.True(result);
        }

        [Fact]
        public void IsProperSupersetOf_ShouldReturnTrueForProperSuperset()
        {
            var set = new ConcurrentHashSet<int> { 1, 2, 3 };
            var result = set.IsProperSupersetOf(new[] { 1, 2 });
            Assert.True(result);
        }

        [Fact]
        public void IsProperSubsetOf_ShouldReturnTrueForProperSubset()
        {
            var set = new ConcurrentHashSet<int> { 1, 2 };
            var result = set.IsProperSubsetOf(new[] { 1, 2, 3 });
            Assert.True(result);
        }

        [Fact]
        public void Overlaps_ShouldReturnTrueForOverlappingSets()
        {
            var set = new ConcurrentHashSet<int> { 1, 2 };
            var result = set.Overlaps(new[] { 2, 3 });
            Assert.True(result);
        }

        [Fact]
        public void SetEquals_ShouldReturnTrueForEqualSets()
        {
            var set = new ConcurrentHashSet<int> { 1, 2, 3 };
            var result = set.SetEquals(new[] { 1, 2, 3 });
            Assert.True(result);
        }

        [Fact]
        public void Clear_ShouldClearSet()
        {
            var set = new ConcurrentHashSet<int> { 1, 2, 3 };
            set.Clear();
            Assert.Empty(set);
        }

        [Fact]
        public void Contains_ShouldReturnTrueForContainedItem()
        {
            var set = new ConcurrentHashSet<int> { 1, 2, 3 };
            var result = set.Contains(2);
            Assert.True(result);
        }

        [Fact]
        public void CopyTo_ShouldCopyItemsToArray()
        {
            var set = new ConcurrentHashSet<int> { 1, 2, 3 };
            var array = new int[3];
            set.CopyTo(array, 0);
            Assert.Contains(1, array);
            Assert.Contains(2, array);
            Assert.Contains(3, array);
        }

        [Fact]
        public void Remove_ShouldRemoveItem()
        {
            var set = new ConcurrentHashSet<int> { 1, 2, 3 };
            var result = set.Remove(2);
            Assert.True(result);
            Assert.DoesNotContain(2, set);
        }

        [Fact]
        public void Count_ShouldReturnCorrectCount()
        {
            var set = new ConcurrentHashSet<int> { 1, 2, 3 };
            Assert.Equal(3, set.Count);
        }

        [Fact]
        public void IsReadOnly_ShouldReturnFalse()
        {
            var set = new ConcurrentHashSet<int>();
            Assert.False(set.IsReadOnly);
        }

        [Fact]
        public void GetEnumerator_ShouldEnumerateItems()
        {
            var set = new ConcurrentHashSet<int> { 1, 2, 3 };
            var enumerator = set.GetEnumerator();
            var items = new List<int>();
            while (enumerator.MoveNext())
            {
                items.Add(enumerator.Current);
            }
            Assert.Contains(1, items);
            Assert.Contains(2, items);
            Assert.Contains(3, items);
        }

        [Fact]
        public void Dispose_ShouldDisposeLock()
        {
            var set = new ConcurrentHashSet<int>();
            set.Dispose();
            Assert.Throws<ObjectDisposedException>(() => set.Add(1));
        }
    }
}