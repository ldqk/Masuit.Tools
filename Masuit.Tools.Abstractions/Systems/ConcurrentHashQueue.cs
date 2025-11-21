using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace Masuit.Tools.Systems;

/// <summary>
/// 线程安全的唯一元素队列，结合了ConcurrentQueue和ConcurrentDictionary的特性
/// </summary>
/// <typeparam name="T">队列中元素的类型</typeparam>
[DebuggerDisplay("Count = {Count}")]
[ComVisible(false)]
public class ConcurrentHashQueue<T> : IProducerConsumerCollection<T>, IReadOnlyCollection<T>, ICollection, ICollection<T>
{
    private class Node
    {
        public readonly T Value;
        public Node Next;

        public Node(T value)
        {
            Value = value;
        }
    }

    private volatile Node _head;
    private volatile Node _tail;
    private readonly ConcurrentHashSet<T> _hashSet;
    private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.SupportsRecursion);
    private int _count;

    #region 构造函数

    /// <summary>
    /// 初始化 ConcurrentUniqueQueue 类的新实例
    /// </summary>
    public ConcurrentHashQueue() : this(null, null)
    {
    }

    /// <summary>
    /// 初始化 ConcurrentUniqueQueue 类的新实例，使用指定的相等比较器
    /// </summary>
    /// <param name="comparer">用于比较值的 IEqualityComparer</param>
    public ConcurrentHashQueue(IEqualityComparer<T> comparer) : this(null, comparer)
    {
    }

    /// <summary>
    /// 初始化 ConcurrentUniqueQueue 类的新实例，包含从指定集合复制的元素
    /// </summary>
    /// <param name="collection">要复制到 ConcurrentUniqueQueue 中的集合</param>
    public ConcurrentHashQueue(IEnumerable<T> collection) : this(collection, null)
    {
    }

    /// <summary>
    /// 初始化 ConcurrentUniqueQueue 类的新实例，包含从指定集合复制的元素，并使用指定的相等比较器
    /// </summary>
    /// <param name="collection">要复制到 ConcurrentUniqueQueue 中的集合</param>
    /// <param name="comparer">用于比较值的 IEqualityComparer</param>
    public ConcurrentHashQueue(IEnumerable<T> collection, IEqualityComparer<T> comparer)
    {
        _hashSet = new ConcurrentHashSet<T>(comparer ?? EqualityComparer<T>.Default);

        // 创建哨兵节点
        _head = _tail = new Node(default(T));
        _count = 0;

        if (collection != null)
        {
            foreach (var item in collection)
            {
                Enqueue(item);
            }
        }
    }

    #endregion 构造函数

    #region 核心方法

    /// <summary>
    /// 将对象添加到 ConcurrentUniqueQueue 的结尾处
    /// </summary>
    /// <param name="item">要添加到 ConcurrentUniqueQueue 的对象</param>
    /// <returns>如果元素成功添加返回 true，如果元素已存在则返回 false</returns>
    public bool Enqueue(T item)
    {
        if (!_hashSet.TryAdd(item))
            return false;

        // 创建新节点
        var newNode = new Node(item);

        // 使用原子操作添加节点到链表尾部
        while (true)
        {
            var tail = _tail;
            var next = tail.Next;

            if (tail == _tail)
            {
                if (next == null)
                {
                    // 尝试链接新节点
                    if (Interlocked.CompareExchange(ref tail.Next, newNode, null) == null)
                    {
                        // 尝试移动tail指针
                        Interlocked.CompareExchange(ref _tail, newNode, tail);
                        Interlocked.Increment(ref _count);
                        return true;
                    }
                }
                else
                {
                    // 帮助其他线程移动tail指针
                    Interlocked.CompareExchange(ref _tail, next, tail);
                }
            }
        }
    }

    /// <summary>
    /// 尝试移除并返回位于 ConcurrentUniqueQueue 开始处的对象
    /// </summary>
    /// <param name="result">如果操作成功，则返回已移除的对象；否则为 T 的默认值</param>
    /// <returns>如果成功移除并返回了对象，则为 true；否则为 false</returns>
    public bool TryDequeue(out T result)
    {
        while (true)
        {
            var head = _head;
            var tail = _tail;
            var next = head.Next;

            if (head == _head)
            {
                if (head == tail)
                {
                    if (next == null)
                    {
                        result = default(T);
                        return false;
                    }

                    // 帮助其他线程移动tail指针
                    Interlocked.CompareExchange(ref _tail, next, tail);
                }
                else
                {
                    result = next.Value;

                    // 尝试移动head指针
                    if (Interlocked.CompareExchange(ref _head, next, head) == head)
                    {
                        // 从set中移除元素
                        _hashSet.Remove(result);
                        Interlocked.Decrement(ref _count);
                        return true;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 尝试返回位于 ConcurrentUniqueQueue 开始处的对象但不将其移除
    /// </summary>
    /// <param name="result">如果操作成功，则返回开头的对象；否则为 T 的默认值</param>
    /// <returns>如果成功返回了对象，则为 true；否则为 false</returns>
    public bool TryPeek(out T result)
    {
            while (true)
            {
                var head = _head;
                var next = head.Next;

                if (next == null)
                {
                    result = default(T);
                    return false;
                }

                if (head == _head)
                {
                    result = next.Value;
                    return true;
                }
            }
        }

    /// <summary>
    /// 确定某元素是否在 ConcurrentUniqueQueue 中
    /// </summary>
    /// <param name="item">要在 ConcurrentUniqueQueue 中定位的对象</param>
    /// <returns>如果在 ConcurrentUniqueQueue 中找到 item，则为 true；否则为 false</returns>
    public bool Contains(T item)
    {
            return _hashSet.Contains(item);
        }

    /// <summary>Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</summary>
    /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
    /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.</exception>
    public void Add(T item)
    {
        Enqueue(item);
    }

    /// <summary>
    /// 从 ConcurrentUniqueQueue 中移除所有对象
    /// </summary>
    public void Clear()
    {
        _lock.EnterWriteLock();
        try
        {
            _hashSet.Clear();
            _head = _tail = new Node(default(T));
            _count = 0;
        }
        finally
        {
            if (_lock.IsWriteLockHeld)
            {
                _lock.ExitWriteLock();
            }
        }
    }

    #endregion 核心方法

    #region 属性和状态

    /// <summary>Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</summary>
    /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
    /// <returns>true if <paramref name="item">item</paramref> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false. This method also returns false if <paramref name="item">item</paramref> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"></see>.</returns>
    /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.</exception>
    public bool Remove(T item)
    {
        _lock.EnterWriteLock();
        try
        {
            // 双重检查，确保元素仍然存在
            if (!_hashSet.Contains(item))
                return false;

            // 从HashSet中移除
            _hashSet.Remove(item);

            // 遍历链表重新构建，跳过要移除的元素
            var newHead = new Node(default(T));
            var newTail = newHead;
            var current = _head.Next;
            var found = false;

            while (current != null)
            {
                if (!_hashSet.Comparer.Equals(current.Value, item))
                {
                    newTail.Next = new Node(current.Value);
                    newTail = newTail.Next;
                }
                else
                {
                    found = true;
                }
                current = current.Next;
            }

            if (found)
            {
                _head = newHead;
                _tail = newTail;
                Interlocked.Decrement(ref _count);
                return true;
            }

            // 如果没有找到，重新添加到HashSet（理论上不应该发生）
            _hashSet.TryAdd(item);
            return false;
        }
        finally
        {
            if (_lock.IsWriteLockHeld)
            {
                _lock.ExitWriteLock();
            }
        }
    }

    /// <summary>
    /// 获取 ConcurrentUniqueQueue 中包含的元素数
    /// </summary>
    public int Count => _count;

    /// <summary>Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.</summary>
    /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only; otherwise, false.</returns>
    public bool IsReadOnly { get; }

    /// <summary>
    /// 获取一个值，该值指示 ConcurrentUniqueQueue 是否为空
    /// </summary>
    public bool IsEmpty => _head.Next == null;

    #endregion 属性和状态

    #region IProducerConsumerCollection<T> 实现

    /// <summary>
    /// 将项添加到 IProducerConsumerCollection
    /// </summary>
    /// <param name="item">要添加到 IProducerConsumerCollection 的项</param>
    /// <returns>如果成功添加了项，则为 true；否则为 false</returns>
    bool IProducerConsumerCollection<T>.TryAdd(T item)
    {
        return Enqueue(item);
    }

    /// <summary>
    /// 尝试从 IProducerConsumerCollection 中移除和返回对象
    /// </summary>
    /// <param name="item">此方法返回时，如果成功移除并返回了对象，则 item 包含所移除的对象</param>
    /// <returns>如果成功移除并返回了对象，则为 true；否则为 false</returns>
    bool IProducerConsumerCollection<T>.TryTake(out T item)
    {
        return TryDequeue(out item);
    }

    /// <summary>
    /// 将 IProducerConsumerCollection 的元素复制到新数组
    /// </summary>
    /// <returns>包含从 IProducerConsumerCollection 复制的元素的新数组</returns>
    public T[] ToArray()
    {
            var snapshot = new List<T>(Count);
            var current = _head.Next;

            while (current != null)
            {
                snapshot.Add(current.Value);
                current = current.Next;
            }

            return snapshot.ToArray();
        }

    /// <summary>
    /// 将 IProducerConsumerCollection 的元素复制到现有一维数组中
    /// </summary>
    /// <param name="array">一维数组，它是从 IProducerConsumerCollection 复制的元素的目标</param>
    /// <param name="index">array 中从零开始的索引，从此处开始复制</param>
    public void CopyTo(T[] array, int index)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array));
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index), "索引不能为负数。");
        if (index >= array.Length)
            throw new ArgumentOutOfRangeException(nameof(index), "索引等于或大于数组长度。");

            var snapshot = ToArray();
            if (snapshot.Length > array.Length - index)
                throw new ArgumentException("目标数组空间不足。");

            Array.Copy(snapshot, 0, array, index, snapshot.Length);
        }

    /// <summary>
    /// 返回循环访问 ConcurrentUniqueQueue 的枚举数
    /// </summary>
    /// <returns>ConcurrentUniqueQueue 的枚举数</returns>
    public IEnumerator<T> GetEnumerator()
    {
            var current = _head.Next;
            while (current != null)
            {
                yield return current.Value;
                current = current.Next;
            }
        }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion IProducerConsumerCollection<T> 实现

    #region ICollection 实现

    /// <summary>
    /// 从特定的 ICollection 索引处开始，将数组的元素复制到一个数组中
    /// </summary>
    /// <param name="array">一维数组，它是从 ICollection 复制的元素的目标</param>
    /// <param name="index">array 中从零开始的索引，从此处开始复制</param>
    void ICollection.CopyTo(Array array, int index)
    {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (array.Rank != 1)
                throw new ArgumentException("仅支持一维数组。");
            if (index < 0 || index >= array.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            var tArray = array as T[];
            if (tArray != null)
            {
                CopyTo(tArray, index);
                return;
            }

            var elementType = array.GetType().GetElementType();
            if (!elementType.IsAssignableFrom(typeof(T)))
                throw new ArgumentException("数组类型与集合元素类型不兼容。");

            var objects = array as object[];
            if (objects == null)
                throw new ArgumentException("目标数组类型不受支持。");

            try
            {
                var snapshot = ToArray();
                Array.Copy(snapshot, 0, objects, index, snapshot.Length);
            }
            catch (ArrayTypeMismatchException)
            {
                throw new ArgumentException("数组类型与集合元素类型不兼容。");
            }
        }

    /// <summary>
    /// 获取一个值，该值指示是否同步对 ICollection 的访问
    /// </summary>
    bool ICollection.IsSynchronized => false;

    /// <summary>
    /// 获取可用于同步对 ICollection 的访问的对象
    /// </summary>
    object ICollection.SyncRoot => _lock;

    #endregion ICollection 实现

    #region 批量操作方法

    /// <summary>
    /// 批量入队多个元素
    /// </summary>
    /// <param name="items">要入队的元素集合</param>
    /// <returns>成功添加的元素数量</returns>
    public int EnqueueRange(IEnumerable<T> items)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        var count = 0;
        foreach (var item in items)
        {
            if (Enqueue(item))
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// 批量出队多个元素
    /// </summary>
    /// <param name="count">要出队的元素数量</param>
    /// <returns>出队的元素集合</returns>
    public IEnumerable<T> DequeueRange(int count)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), "数量不能为负数。");

        var result = new List<T>(Math.Min(count, Count));
        for (int i = 0; i < count && TryDequeue(out T item); i++)
        {
            result.Add(item);
        }
        return result;
    }

    #endregion 批量操作方法

    #region 原子操作方法

    /// <summary>
    /// 原子性地入队元素，如果队列已满则返回false
    /// </summary>
    /// <param name="item">要入队的元素</param>
    /// <param name="maxSize">队列的最大大小</param>
    /// <returns>如果成功入队返回true，否则返回false</returns>
    public bool TryEnqueueWithLimit(T item, int maxSize)
    {
        if (maxSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxSize), "最大大小必须大于0。");

        // 先检查是否已达到最大大小
        if (Count >= maxSize && !Contains(item))
            return false;

        return Enqueue(item);
    }

    /// <summary>
    /// 原子性地入队元素，如果队列已满则移除最旧的元素
    /// </summary>
    /// <param name="item">要入队的元素</param>
    /// <param name="maxSize">队列的最大大小</param>
    /// <returns>被移除的元素，如果没有移除则返回default(T)</returns>
    public T EnqueueWithEviction(T item, int maxSize)
    {
        if (maxSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxSize), "最大大小必须大于0。");

        T evicted = default(T);

        // 如果元素已存在，先移除旧的实例
        if (Contains(item))
        {
            RemoveExistingAndEnqueue(item);
            return default(T);
        }

        // 检查是否需要驱逐
        if (Count >= maxSize)
        {
            TryDequeue(out evicted);
        }

        Enqueue(item);
        return evicted;
    }

    /// <summary>
    /// 移除已存在的元素并重新入队（用于更新元素位置）
    /// </summary>
    /// <param name="item">要更新的元素</param>
    /// <returns>如果成功更新返回true，否则返回false</returns>
    private bool RemoveExistingAndEnqueue(T item)
    {
        // 这个操作比较复杂，需要锁定整个队列
        _lock.EnterWriteLock();
        try
        {
            if (!_hashSet.Contains(item))
                return false;

            // 从字典中暂时移除
            _hashSet.Remove(item);

            // 重新构建链表，跳过要更新的元素
            var newHead = new Node(default(T));
            var newTail = newHead;
            var current = _head.Next;
            var found = false;

            while (current != null)
            {
                if (!_hashSet.Comparer.Equals(current.Value, item))
                {
                    newTail.Next = new Node(current.Value);
                    newTail = newTail.Next;
                    _hashSet.TryAdd(current.Value);
                }
                else
                {
                    found = true;
                }
                current = current.Next;
            }

            if (found)
            {
                // 添加新元素到尾部
                newTail.Next = new Node(item);
                newTail = newTail.Next;
                _hashSet.TryAdd(item);

                _head = newHead;
                _tail = newTail;
                _count = _hashSet.Count;

                return true;
            }

            return false;
        }
        finally
        {
            if (_lock.IsWriteLockHeld)
            {
                _lock.ExitWriteLock();
            }
        }
    }

    #endregion 原子操作方法
}