using System.Collections;
using System.Diagnostics;

namespace Masuit.Tools.Systems;

/// <summary>
/// 一个只允许唯一元素的队列，结合了Queue和HashSet的特性
/// </summary>
/// <typeparam name="T">队列中元素的类型</typeparam>
[DebuggerDisplay("Count = {Count}")]
public class HashQueue<T> : IReadOnlyCollection<T>, ICollection<T>, ICollection
{
    private readonly Queue<T> _queue;
    private readonly HashSet<T> _hashSet;
    private readonly IEqualityComparer<T> _comparer;

    #region 构造函数

    /// <summary>
    /// 初始化 UniqueQueue 类的新实例
    /// </summary>
    public HashQueue() : this((IEnumerable<T>)null, null)
    {
    }

    /// <summary>
    /// 初始化 UniqueQueue 类的新实例，使用指定的相等比较器
    /// </summary>
    /// <param name="comparer">用于比较值的 IEqualityComparer</param>
    public HashQueue(IEqualityComparer<T> comparer) : this(null, comparer)
    {
    }

    /// <summary>
    /// 初始化 UniqueQueue 类的新实例，包含从指定集合复制的元素
    /// </summary>
    /// <param name="collection">要复制到 UniqueQueue 中的集合</param>
    public HashQueue(IEnumerable<T> collection) : this(collection, null)
    {
    }

    /// <summary>
    /// 初始化 UniqueQueue 类的新实例，包含从指定集合复制的元素，并使用指定的相等比较器
    /// </summary>
    /// <param name="collection">要复制到 UniqueQueue 中的集合</param>
    /// <param name="comparer">用于比较值的 IEqualityComparer</param>
    public HashQueue(IEnumerable<T> collection, IEqualityComparer<T> comparer)
    {
        _comparer = comparer ?? EqualityComparer<T>.Default;
        _hashSet = new HashSet<T>(_comparer);
        _queue = new Queue<T>();

        if (collection != null)
        {
            foreach (var item in collection)
            {
                Enqueue(item);
            }
        }
    }

    /// <summary>
    /// 初始化 UniqueQueue 类的新实例，具有指定的初始容量
    /// </summary>
    /// <param name="capacity">UniqueQueue 可包含的初始元素数目</param>
    public HashQueue(int capacity) : this(capacity, null)
    {
    }

    /// <summary>
    /// 初始化 UniqueQueue 类的新实例，具有指定的初始容量和使用指定的相等比较器
    /// </summary>
    /// <param name="capacity">UniqueQueue 可包含的初始元素数目</param>
    /// <param name="comparer">用于比较值的 IEqualityComparer</param>
    public HashQueue(int capacity, IEqualityComparer<T> comparer)
    {
        if (capacity < 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), "容量不能为负数。");

        _comparer = comparer ?? EqualityComparer<T>.Default;
        _hashSet = new HashSet<T>(_comparer);
        _queue = new Queue<T>(capacity);
    }

    #endregion 构造函数

    #region 核心方法

    /// <summary>
    /// 将对象添加到 UniqueQueue 的结尾处
    /// </summary>
    /// <param name="item">要添加到 UniqueQueue 的对象</param>
    /// <returns>如果元素成功添加返回 true，如果元素已存在则返回 false</returns>
    public bool Enqueue(T item)
    {
        if (_hashSet.Add(item))
        {
            _queue.Enqueue(item);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 移除并返回位于 UniqueQueue 开始处的对象
    /// </summary>
    /// <returns>从 UniqueQueue 的开头移除的对象</returns>
    /// <exception cref="InvalidOperationException">UniqueQueue 为空</exception>
    public T Dequeue()
    {
        if (_queue.Count == 0)
            throw new InvalidOperationException("队列为空。");

        var item = _queue.Dequeue();
        _hashSet.Remove(item);
        return item;
    }

    /// <summary>
    /// 尝试移除并返回位于 UniqueQueue 开始处的对象
    /// </summary>
    /// <param name="result">如果操作成功，则返回已移除的对象；否则为 T 的默认值</param>
    /// <returns>如果成功移除并返回了对象，则为 true；否则为 false</returns>
    public bool TryDequeue(out T result)
    {
        if (_queue.Count > 0)
        {
            result = _queue.Dequeue();
            _hashSet.Remove(result);
            return true;
        }

        result = default(T);
        return false;
    }

    /// <summary>
    /// 返回位于 UniqueQueue 开始处的对象但不将其移除
    /// </summary>
    /// <returns>位于 UniqueQueue 开头的对象</returns>
    /// <exception cref="InvalidOperationException">UniqueQueue 为空</exception>
    public T Peek()
    {
        if (_queue.Count == 0)
            throw new InvalidOperationException("队列为空。");

        return _queue.Peek();
    }

    /// <summary>
    /// 返回位于 UniqueQueue 开始处的对象但不将其移除
    /// </summary>
    /// <param name="result">如果操作成功，则返回开头的对象；否则为 T 的默认值</param>
    /// <returns>如果成功返回了对象，则为 true；否则为 false</returns>
    public bool TryPeek(out T result)
    {
        if (_queue.Count > 0)
        {
            result = _queue.Peek();
            return true;
        }

        result = default(T);
        return false;
    }

    /// <summary>
    /// 确定某元素是否在 UniqueQueue 中
    /// </summary>
    /// <param name="item">要在 UniqueQueue 中定位的对象</param>
    /// <returns>如果在 UniqueQueue 中找到 item，则为 true；否则为 false</returns>
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
    /// 从 UniqueQueue 中移除所有对象
    /// </summary>
    public void Clear()
    {
        _queue.Clear();
        _hashSet.Clear();
    }

    #endregion 核心方法

    #region 属性和状态

    /// <summary>
    /// 获取 UniqueQueue 中包含的元素数
    /// </summary>
    public int Count => _queue.Count;

    /// <summary>Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.</summary>
    /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only; otherwise, false.</returns>
    public bool IsReadOnly { get; }

    /// <summary>
    /// 获取一个值，该值指示 UniqueQueue 是否为空
    /// </summary>
    public bool IsEmpty => _queue.Count == 0;

    /// <summary>
    /// 获取用于确定集合中值是否相等的 IEqualityComparer
    /// </summary>
    public IEqualityComparer<T> Comparer => _comparer;

    #endregion 属性和状态

    #region 集合操作

    /// <summary>
    /// 从 UniqueQueue 中移除指定项
    /// </summary>
    /// <param name="item">要移除的项</param>
    /// <returns>如果成功找到并移除该项，则为 true；否则为 false</returns>
    public bool Remove(T item)
    {
        if (_hashSet.Remove(item))
        {
            // 重新构建队列，移除指定项
            var tempQueue = new Queue<T>();
            while (_queue.Count > 0)
            {
                var current = _queue.Dequeue();
                if (!_comparer.Equals(current, item))
                {
                    tempQueue.Enqueue(current);
                }
            }

            // 将临时队列的内容复制回原队列
            while (tempQueue.Count > 0)
            {
                _queue.Enqueue(tempQueue.Dequeue());
            }

            return true;
        }
        return false;
    }

    /// <summary>
    /// 将 UniqueQueue 复制到新数组
    /// </summary>
    /// <returns>包含 UniqueQueue 元素副本的新数组</returns>
    public T[] ToArray()
    {
        return _queue.ToArray();
    }

    /// <summary>
    /// 将 UniqueQueue 元素复制到现有一维数组中
    /// </summary>
    /// <param name="array">一维数组，它是从 UniqueQueue 复制的元素的目标</param>
    /// <param name="arrayIndex">array 中从零开始的索引，从此处开始复制</param>
    public void CopyTo(T[] array, int arrayIndex)
    {
        _queue.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// 设置容量为 UniqueQueue 中元素的实际数目
    /// </summary>
    public void TrimExcess()
    {
        _queue.TrimExcess();
        _hashSet.TrimExcess();
    }

    #endregion 集合操作

    #region 接口实现

    /// <summary>
    /// 返回循环访问 UniqueQueue 的枚举数
    /// </summary>
    /// <returns>UniqueQueue 的枚举数</returns>
    public IEnumerator<T> GetEnumerator()
    {
        return _queue.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    bool ICollection.IsSynchronized => false;

    object ICollection.SyncRoot => ((ICollection)_queue).SyncRoot;

    void ICollection.CopyTo(Array array, int index)
    {
        ((ICollection)_queue).CopyTo(array, index);
    }

    #endregion 接口实现

    #region 性能优化方法

    /// <summary>
    /// 批量入队多个元素
    /// </summary>
    /// <param name="items">要入队的元素集合</param>
    /// <returns>成功添加的元素数量</returns>
    public int EnqueueRange(IEnumerable<T> items)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        return items.Count(Enqueue);
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

        var result = new List<T>(Math.Min(count, _queue.Count));
        for (int i = 0; i < count && _queue.Count > 0; i++)
        {
            result.Add(Dequeue());
        }
        return result;
    }

    #endregion 性能优化方法
}

// 扩展方法类，提供额外的便利方法
public static class HashQueueExtensions
{
    /// <summary>
    /// 将元素入队，如果队列已满则移除最旧的元素
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <param name="queue">UniqueQueue 实例</param>
    /// <param name="item">要入队的元素</param>
    /// <param name="maxSize">队列的最大大小</param>
    /// <returns>如果移除了旧元素返回 true，否则返回 false</returns>
    public static bool EnqueueWithLimit<T>(this HashQueue<T> queue, T item, int maxSize)
    {
        if (queue == null)
            throw new ArgumentNullException(nameof(queue));
        if (maxSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxSize), "最大大小必须大于0。");

        var removedOld = false;
        if (queue.Count >= maxSize && !queue.Contains(item))
        {
            queue.Dequeue();
            removedOld = true;
        }

        queue.Enqueue(item);
        return removedOld;
    }

    /// <summary>
    /// 将元素入队，如果队列已满则移除最旧的元素
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <param name="queue">UniqueQueue 实例</param>
    /// <param name="item">要入队的元素</param>
    /// <param name="maxSize">队列的最大大小</param>
    /// <returns>如果移除了旧元素返回 true，否则返回 false</returns>
    public static bool EnqueueWithLimit<T>(this ConcurrentHashQueue<T> queue, T item, int maxSize)
    {
        if (queue == null)
            throw new ArgumentNullException(nameof(queue));
        if (maxSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxSize), "最大大小必须大于0。");

        var removedOld = false;
        if (queue.Count >= maxSize && !queue.Contains(item))
        {
            removedOld = queue.TryDequeue(out _);
        }

        queue.Enqueue(item);
        return removedOld;
    }

    /// <summary>
    /// 批量出队所有元素
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <param name="queue">ConcurrentUniqueQueue 实例</param>
    /// <returns>所有出队的元素</returns>
    public static IEnumerable<T> DequeueAll<T>(this HashQueue<T> queue)
    {
        if (queue == null)
            throw new ArgumentNullException(nameof(queue));

        return queue.DequeueRange(queue.Count);
    }

    /// <summary>
    /// 批量出队所有元素
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <param name="queue">ConcurrentUniqueQueue 实例</param>
    /// <returns>所有出队的元素</returns>
    public static IEnumerable<T> DequeueAll<T>(this ConcurrentHashQueue<T> queue)
    {
        if (queue == null)
            throw new ArgumentNullException(nameof(queue));

        return queue.DequeueRange(queue.Count);
    }

    /// <summary>
    /// 安全地枚举队列元素（快照）
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <param name="queue">ConcurrentUniqueQueue 实例</param>
    /// <returns>队列元素的快照</returns>
    public static IReadOnlyList<T> ToSnapshot<T>(this HashQueue<T> queue)
    {
        if (queue == null)
            throw new ArgumentNullException(nameof(queue));

        return queue.ToArray();
    }

    /// <summary>
    /// 安全地枚举队列元素（快照）
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <param name="queue">ConcurrentUniqueQueue 实例</param>
    /// <returns>队列元素的快照</returns>
    public static IReadOnlyList<T> ToSnapshot<T>(this ConcurrentHashQueue<T> queue)
    {
        if (queue == null)
            throw new ArgumentNullException(nameof(queue));

        return queue.ToArray();
    }
}