﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Masuit.Tools;

/// <summary>
///
/// </summary>
public static class IEnumerableExtensions
{
#if NETFRAMEWORK

    public static IEnumerable<T> Append<T>(this IEnumerable<T> source, T item)
    {
        return source.Concat([item]);
    }

#endif

    /// <summary>
    /// 按字段属性判等取交集
    /// </summary>
    /// <typeparam name="TFirst"></typeparam>
    /// <typeparam name="TSecond"></typeparam>
    /// <param name="second"></param>
    /// <param name="condition"></param>
    /// <param name="first"></param>
    /// <returns></returns>
    public static IEnumerable<TFirst> IntersectBy<TFirst, TSecond>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, bool> condition)
    {
        return first.Where(f => second.Any(s => condition(f, s)));
    }

    /// <summary>
    /// 按字段属性判等取交集
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <param name="keySelector"></param>
    /// <returns></returns>
    public static IEnumerable<TSource> IntersectBy<TSource, TKey>(this IEnumerable<TSource> first, IEnumerable<TSource> second, Func<TSource, TKey> keySelector)
    {
        return first.IntersectBy(second, keySelector, null);
    }

    /// <summary>
    /// 按字段属性判等取交集
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <param name="keySelector"></param>
    /// <param name="comparer"></param>
    /// <returns></returns>
    public static IEnumerable<TSource> IntersectBy<TSource, TKey>(this IEnumerable<TSource> first, IEnumerable<TSource> second, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
    {
        if (first == null)
            throw new ArgumentNullException(nameof(first));
        if (second == null)
            throw new ArgumentNullException(nameof(second));
        if (keySelector == null)
            throw new ArgumentNullException(nameof(keySelector));
        return IntersectByIterator(first, second, keySelector, comparer);
    }

    private static IEnumerable<TSource> IntersectByIterator<TSource, TKey>(IEnumerable<TSource> first, IEnumerable<TSource> second, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
    {
        var set = new HashSet<TKey>(second.Select(keySelector), comparer);
        foreach (var item in first.Where(source => set.Remove(keySelector(source))))
        {
            yield return item;
        }
    }

    /// <summary>
    /// 多个集合取交集元素
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static IEnumerable<T> IntersectAll<T>(this IEnumerable<IEnumerable<T>> source)
    {
        return source.Aggregate((current, item) => current.Intersect(item));
    }

    /// <summary>
    /// 多个集合取交集元素
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="source"></param>
    /// <param name="keySelector"></param>
    /// <returns></returns>
    public static IEnumerable<TSource> IntersectAll<TSource, TKey>(this IEnumerable<IEnumerable<TSource>> source, Func<TSource, TKey> keySelector)
    {
        return source.Aggregate((current, item) => current.IntersectBy(item, keySelector));
    }

    /// <summary>
    /// 多个集合取交集元素
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="source"></param>
    /// <param name="keySelector"></param>
    /// <param name="comparer"></param>
    /// <returns></returns>
    public static IEnumerable<TSource> IntersectAll<TSource, TKey>(this IEnumerable<IEnumerable<TSource>> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
    {
        return source.Aggregate((current, item) => current.IntersectBy(item, keySelector, comparer));
    }

    /// <summary>
    /// 多个集合取交集元素
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="comparer"></param>
    /// <returns></returns>
    public static IEnumerable<T> IntersectAll<T>(this IEnumerable<IEnumerable<T>> source, IEqualityComparer<T> comparer)
    {
        return source.Aggregate((current, item) => current.Intersect(item, comparer));
    }

    /// <summary>
    /// 按字段属性判等取差集
    /// </summary>
    /// <typeparam name="TFirst"></typeparam>
    /// <typeparam name="TSecond"></typeparam>
    /// <param name="second"></param>
    /// <param name="condition"></param>
    /// <param name="first"></param>
    /// <returns></returns>
    public static IEnumerable<TFirst> ExceptBy<TFirst, TSecond>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, bool> condition)
    {
        return first.Where(f => !second.Any(s => condition(f, s)));
    }

    /// <summary>
    /// 按字段属性判等取差集
    /// </summary>
    /// <param name="second"></param>
    /// <param name="first"></param>
    /// <param name="keySelector"></param>
    /// <returns></returns>
    public static IEnumerable<T> ExceptBy<T, TKey>(this IEnumerable<T> first, IEnumerable<T> second, Func<T, TKey> keySelector)
    {
        // 将 second 集合转换为 HashSet 以提高查找效率
        var keys = new HashSet<TKey>(second.Select(keySelector));

        // 使用 Lookup 来分组 first 集合，并过滤掉 second 集合中存在的元素
        var firstLookup = first.ToLookup(keySelector);
        return firstLookup.Where(g => !keys.Contains(g.Key)).SelectMany(grouping => grouping);
    }

#if NET6_0_OR_GREATER
#else

    /// <summary>
    /// 按字段去重
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="source"></param>
    /// <param name="keySelector"></param>
    /// <returns></returns>
    public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
        var set = new HashSet<TKey>();
        return source.Where(item => set.Add(keySelector(item)));
    }

    /// <summary>
    /// 按字段属性判等取交集
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <param name="keySelector"></param>
    /// <returns></returns>
    public static IEnumerable<TSource> IntersectBy<TSource, TKey>(this IEnumerable<TSource> first, IEnumerable<TKey> second, Func<TSource, TKey> keySelector)
    {
        return first.IntersectBy(second, keySelector, null);
    }

    /// <summary>
    /// 按字段属性判等取交集
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <param name="keySelector"></param>
    /// <param name="comparer"></param>
    /// <returns></returns>
    public static IEnumerable<TSource> IntersectBy<TSource, TKey>(this IEnumerable<TSource> first, IEnumerable<TKey> second, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
    {
        if (first == null) throw new ArgumentNullException(nameof(first));
        if (second == null) throw new ArgumentNullException(nameof(second));
        if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
        return IntersectByIterator(first, second, keySelector, comparer);
    }

    private static IEnumerable<TSource> IntersectByIterator<TSource, TKey>(IEnumerable<TSource> first, IEnumerable<TKey> second, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
    {
        var set = new HashSet<TKey>(second, comparer);
        return first.Where(source => set.Remove(keySelector(source)));
    }

    /// <summary>
    /// 按字段属性判等取差集
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <param name="keySelector"></param>
    /// <returns></returns>
    public static IEnumerable<TSource> ExceptBy<TSource, TKey>(this IEnumerable<TSource> first, IEnumerable<TKey> second, Func<TSource, TKey> keySelector)
    {
        return first.ExceptBy(second, keySelector, null);
    }

    /// <summary>
    /// 按字段属性判等取差集
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <param name="keySelector"></param>
    /// <param name="comparer"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IEnumerable<TSource> ExceptBy<TSource, TKey>(this IEnumerable<TSource> first, IEnumerable<TKey> second, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
    {
        if (first == null) throw new ArgumentNullException(nameof(first));
        if (second == null) throw new ArgumentNullException(nameof(second));
        if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
        return ExceptByIterator(first, second, keySelector, comparer);
    }

    private static IEnumerable<TSource> ExceptByIterator<TSource, TKey>(IEnumerable<TSource> first, IEnumerable<TKey> second, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
    {
        var set = new HashSet<TKey>(second, comparer);
        return first.Where(source => set.Add(keySelector(source)));
    }

#endif

    /// <summary>
    /// 添加多个元素
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this"></param>
    /// <param name="values"></param>
    public static void AddRange<T>(this ICollection<T> @this, params T[] values)
    {
        foreach (var obj in values)
        {
            @this.Add(obj);
        }
    }

    /// <summary>
    /// 添加多个元素
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this"></param>
    /// <param name="values"></param>
    public static void AddRange<T>(this ICollection<T> @this, IEnumerable<T> values)
    {
        foreach (var obj in values)
        {
            @this.Add(obj);
        }
    }

    /// <summary>
    /// 添加多个元素
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this"></param>
    /// <param name="values"></param>
    public static void AddRange<T>(this ConcurrentBag<T> @this, params T[] values)
    {
        foreach (var obj in values)
        {
            @this.Add(obj);
        }
    }

    /// <summary>
    /// 添加多个元素
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this"></param>
    /// <param name="values"></param>
    public static void AddRange<T>(this ConcurrentQueue<T> @this, params T[] values)
    {
        foreach (var obj in values)
        {
            @this.Enqueue(obj);
        }
    }

    /// <summary>
    /// 添加符合条件的多个元素
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this"></param>
    /// <param name="predicate"></param>
    /// <param name="values"></param>
    public static void AddRangeIf<T>(this ICollection<T> @this, Func<T, bool> predicate, params T[] values)
    {
        foreach (var obj in values.Where(predicate))
        {
            @this.Add(obj);
        }
    }

    /// <summary>
    /// 添加符合条件的多个元素
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this"></param>
    /// <param name="predicate"></param>
    /// <param name="values"></param>
    public static void AddRangeIf<T>(this ConcurrentBag<T> @this, Func<T, bool> predicate, params T[] values)
    {
        foreach (var obj in values.Where(predicate))
        {
            @this.Add(obj);
        }
    }

    /// <summary>
    /// 添加符合条件的多个元素
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this"></param>
    /// <param name="predicate"></param>
    /// <param name="values"></param>
    public static void AddRangeIf<T>(this ConcurrentQueue<T> @this, Func<T, bool> predicate, params T[] values)
    {
        foreach (var obj in values.Where(predicate))
        {
            @this.Enqueue(obj);
        }
    }

    /// <summary>
    /// 符合条件则添加元素
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this"></param>
    /// <param name="predicate"></param>
    /// <param name="values"></param>
    public static ICollection<T> AddRangeIf<T>(this ICollection<T> @this, Func<bool> predicate, IEnumerable<T> values)
    {
        if (predicate())
        {
            foreach (var value in values)
            {
                @this.Add(value);
            }
        }

        return @this;
    }

    /// <summary>
    /// 符合条件则添加元素
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this"></param>
    /// <param name="predicate"></param>
    /// <param name="values"></param>
    public static ICollection<T> AddRangeIf<T>(this ICollection<T> @this, bool predicate, IEnumerable<T> values)
    {
        if (predicate)
        {
            foreach (var value in values)
            {
                @this.Add(value);
            }
        }

        return @this;
    }

    /// <summary>
    /// 符合条件则添加元素
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this"></param>
    /// <param name="predicate"></param>
    /// <param name="values"></param>
    public static ConcurrentBag<T> AddRangeIf<T>(this ConcurrentBag<T> @this, Func<bool> predicate, IEnumerable<T> values)
    {
        if (predicate())
        {
            foreach (var value in values)
            {
                @this.Add(value);
            }
        }

        return @this;
    }

    /// <summary>
    /// 符合条件则添加元素
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this"></param>
    /// <param name="predicate"></param>
    /// <param name="values"></param>
    public static ConcurrentBag<T> AddRangeIf<T>(this ConcurrentBag<T> @this, bool predicate, IEnumerable<T> values)
    {
        if (predicate)
        {
            foreach (var value in values)
            {
                @this.Add(value);
            }
        }

        return @this;
    }

    /// <summary>
    /// 符合条件则添加元素
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this"></param>
    /// <param name="predicate"></param>
    /// <param name="values"></param>
    public static ConcurrentQueue<T> AddRangeIf<T>(this ConcurrentQueue<T> @this, Func<bool> predicate, IEnumerable<T> values)
    {
        if (predicate())
        {
            foreach (var value in values)
            {
                @this.Enqueue(value);
            }
        }

        return @this;
    }

    /// <summary>
    /// 符合条件则添加元素
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this"></param>
    /// <param name="predicate"></param>
    /// <param name="values"></param>
    public static ConcurrentQueue<T> AddRangeIf<T>(this ConcurrentQueue<T> @this, bool predicate, IEnumerable<T> values)
    {
        if (predicate)
        {
            foreach (var value in values)
            {
                @this.Enqueue(value);
            }
        }

        return @this;
    }

    /// <summary>
    /// 添加不重复的元素
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this"></param>
    /// <param name="values"></param>
    public static void AddRangeIfNotContains<T>(this ICollection<T> @this, params T[] values)
    {
        foreach (var obj in values.Where(obj => !@this.Contains(obj)))
        {
            @this.Add(obj);
        }
    }

    /// <summary>
    /// 符合条件则添加一个元素
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this"></param>
    /// <param name="predicate"></param>
    /// <param name="value"></param>
    public static ICollection<T> AppendIf<T>(this ICollection<T> @this, Func<bool> predicate, T value)
    {
        if (predicate())
        {
            @this.Add(value);
        }

        return @this;
    }

    /// <summary>
    /// 符合条件则添加一个元素
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this"></param>
    /// <param name="predicate"></param>
    /// <param name="value"></param>
    public static ICollection<T> AppendIf<T>(this ICollection<T> @this, bool predicate, T value)
    {
        if (predicate)
        {
            @this.Add(value);
        }

        return @this;
    }

    /// <summary>
    /// 符合条件则添加一个元素
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this"></param>
    /// <param name="predicate"></param>
    /// <param name="value"></param>
    public static IEnumerable<T> AppendIf<T>(this IEnumerable<T> @this, Func<bool> predicate, T value)
    {
        return predicate() ? @this.Append(value) : @this;
    }

    /// <summary>
    /// 符合条件则添加一个元素
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this"></param>
    /// <param name="predicate"></param>
    /// <param name="value"></param>
    public static IEnumerable<T> AppendIf<T>(this IEnumerable<T> @this, bool predicate, T value)
    {
        return predicate ? @this.Append(value) : @this;
    }

    /// <summary>
    /// 移除符合条件的元素
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this"></param>
    /// <param name="where"></param>
    public static void RemoveWhere<T>(this ICollection<T> @this, Func<T, bool> @where)
    {
        var list = @this.Where(where).ToList();
        foreach (var obj in list)
        {
            @this.Remove(obj);
        }
    }

    /// <summary>
    /// 在元素之后添加元素
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="condition">条件</param>
    /// <param name="value">值</param>
    public static void InsertAfter<T>(this IList<T> list, Func<T, bool> condition, T value)
    {
        foreach (var index in list.Select((item, index) => new
        {
            item,
            index
        }).Where(p => condition(p.item)).OrderByDescending(p => p.index).Select(t => t.index))
        {
            if (index + 1 == list.Count)
            {
                list.Add(value);
            }
            else
            {
                list.Insert(index + 1, value);
            }
        }
    }

    /// <summary>
    /// 在元素之后添加元素
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="index">索引位置</param>
    /// <param name="value">值</param>
    public static void InsertAfter<T>(this IList<T> list, int index, T value)
    {
        foreach (var i in list.Select((v, i) => new
        {
            Value = v,
            Index = i
        }).Where(p => p.Index == index).OrderByDescending(p => p.Index).Select(t => t.Index))
        {
            if (i + 1 == list.Count)
            {
                list.Add(value);
            }
            else
            {
                list.Insert(i + 1, value);
            }
        }
    }

    /// <summary>
    /// 转HashSet
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <returns></returns>
    public static HashSet<TResult> ToHashSet<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector)
    {
        return new HashSet<TResult>(source.Select(selector));
    }

    /// <summary>
    /// 转SortedSet
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="comparer"></param>
    /// <returns></returns>
    public static SortedSet<T> ToSortedSet<T>(this IEnumerable<T> source, IComparer<T> comparer = null)
    {
        return comparer == null ? [.. source] : new SortedSet<T>(source, comparer);
    }

    /// <summary>
    /// 转SortedSet
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <param name="comparer"></param>
    /// <returns></returns>
    public static SortedSet<TResult> ToSortedSet<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector, IComparer<TResult> comparer = null)
    {
        return comparer == null ? [.. source.Select(selector)] : new SortedSet<TResult>(source.Select(selector), comparer);
    }

    /// <summary>
    /// 转Queue
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static Queue<T> ToQueue<T>(this IEnumerable<T> source)
    {
        return new Queue<T>(source);
    }

    /// <summary>
    /// 转Queue
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <returns></returns>
    public static Queue<TResult> ToQueue<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector)
    {
        return new Queue<TResult>(source.Select(selector));
    }

    /// <summary>
    /// 遍历IEnumerable
    /// </summary>
    /// <param name="objs"></param>
    /// <param name="action">回调方法</param>
    /// <typeparam name="T"></typeparam>
    public static void ForEach<T>(this IEnumerable<T> objs, Action<T> action)
    {
        foreach (var o in objs)
        {
            action(o);
        }
    }

    /// <summary>
    /// 异步foreach
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="maxParallelCount">最大并行数</param>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task ForeachAsync<T>(this IEnumerable<T> source, Func<T, Task> action, int maxParallelCount, CancellationToken cancellationToken = default)
    {
        if (Debugger.IsAttached)
        {
            foreach (var item in source)
            {
                await action(item);
            }

            return;
        }

        var list = new List<Task>();
        foreach (var item in source)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            list.Add(action(item));
            if (list.Count(t => !t.IsCompleted) >= maxParallelCount)
            {
                await Task.WhenAny(list);
                list.RemoveAll(t => t.IsCompleted);
            }
        }

        await Task.WhenAll(list);
    }

    /// <summary>
    /// 异步foreach
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static Task ForeachAsync<T>(this IEnumerable<T> source, Func<T, Task> action, CancellationToken cancellationToken = default)
    {
        if (source is ICollection<T> collection)
        {
            return ForeachAsync(collection, action, collection.Count, cancellationToken);
        }

        var list = source.ToList();
        return ForeachAsync(list, action, list.Count, cancellationToken);
    }

    /// <summary>
    /// 异步Select
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <returns></returns>
    public static Task<TResult[]> SelectAsync<T, TResult>(this IEnumerable<T> source, Func<T, Task<TResult>> selector)
    {
        return Task.WhenAll(source.Select(selector));
    }

    /// <summary>
    /// 异步Select
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <returns></returns>
    public static Task<TResult[]> SelectAsync<T, TResult>(this IEnumerable<T> source, Func<T, int, Task<TResult>> selector)
    {
        return Task.WhenAll(source.Select(selector));
    }

    /// <summary>
    /// 异步Select
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <param name="maxParallelCount">最大并行数</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<List<TResult>> SelectAsync<T, TResult>(this IEnumerable<T> source, Func<T, Task<TResult>> selector, int maxParallelCount, CancellationToken cancellationToken = default)
    {
        var results = new List<TResult>();
        var tasks = new List<Task<TResult>>();
        foreach (var item in source)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var task = selector(item);
            tasks.Add(task);
            if (tasks.Count >= maxParallelCount)
            {
                await Task.WhenAny(tasks);
                var completedTasks = tasks.Where(t => t.IsCompleted).ToArray();
                results.AddRange(completedTasks.Select(t => t.Result));
                tasks.RemoveWhere(t => completedTasks.Contains(t));
            }
        }

        results.AddRange(await Task.WhenAll(tasks));
        return results;
    }

    /// <summary>
    /// 异步Select
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <param name="maxParallelCount">最大并行数</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<List<TResult>> SelectAsync<T, TResult>(this IEnumerable<T> source, Func<T, int, Task<TResult>> selector, int maxParallelCount, CancellationToken cancellationToken = default)
    {
        var results = new List<TResult>();
        var tasks = new List<Task<TResult>>();
        int index = 0;
        foreach (var item in source)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var task = selector(item, index);
            tasks.Add(task);
            Interlocked.Add(ref index, 1);
            if (tasks.Count >= maxParallelCount)
            {
                await Task.WhenAny(tasks);
                var completedTasks = tasks.Where(t => t.IsCompleted).ToArray();
                results.AddRange(completedTasks.Select(t => t.Result));
                tasks.RemoveWhere(t => completedTasks.Contains(t));
            }
        }

        results.AddRange(await Task.WhenAll(tasks));
        return results;
    }

    /// <summary>
    /// 异步SelectMany
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <returns></returns>
    public static Task<IEnumerable<TResult>> SelectManyAsync<T, TResult>(this IEnumerable<T> source, Func<T, Task<IEnumerable<TResult>>> selector)
    {
        return Task.WhenAll(source.Select(selector)).ContinueWith(t => t.Result.SelectMany(_ => _));
    }

    /// <summary>
    /// 异步SelectMany
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <returns></returns>
    public static Task<IEnumerable<TResult>> SelectManyAsync<T, TResult>(this IEnumerable<T> source, Func<T, int, Task<IEnumerable<TResult>>> selector)
    {
        return Task.WhenAll(source.Select(selector)).ContinueWith(t => t.Result.SelectMany(_ => _));
    }

    /// <summary>
    /// 异步SelectMany
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <param name="maxParallelCount">最大并行数</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<List<TResult>> SelectManyAsync<T, TResult>(this IEnumerable<T> source, Func<T, Task<IEnumerable<TResult>>> selector, int maxParallelCount, CancellationToken cancellationToken = default)
    {
        var results = new List<TResult>();
        var tasks = new List<Task<IEnumerable<TResult>>>();
        foreach (var item in source)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var task = selector(item);
            tasks.Add(task);
            if (tasks.Count >= maxParallelCount)
            {
                await Task.WhenAny(tasks);
                var completedTasks = tasks.Where(t => t.IsCompleted).ToArray();
                results.AddRange(completedTasks.SelectMany(t => t.Result));
                tasks.RemoveWhere(t => completedTasks.Contains(t));
            }
        }

        results.AddRange(await Task.WhenAll(tasks).ContinueWith(t => t.Result.SelectMany(_ => _), cancellationToken));
        return results;
    }

    /// <summary>
    /// 异步SelectMany
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <param name="maxParallelCount">最大并行数</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<List<TResult>> SelectManyAsync<T, TResult>(this IEnumerable<T> source, Func<T, int, Task<IEnumerable<TResult>>> selector, int maxParallelCount, CancellationToken cancellationToken = default)
    {
        var results = new List<TResult>();
        var tasks = new List<Task<IEnumerable<TResult>>>();
        int index = 0;
        foreach (var item in source)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var task = selector(item, index);
            tasks.Add(task);
            Interlocked.Add(ref index, 1);
            if (tasks.Count >= maxParallelCount)
            {
                await Task.WhenAny(tasks);
                var completedTasks = tasks.Where(t => t.IsCompleted).ToArray();
                results.AddRange(completedTasks.SelectMany(t => t.Result));
                tasks.RemoveWhere(t => completedTasks.Contains(t));
            }
        }

        results.AddRange(await Task.WhenAll(tasks).ContinueWith(t => t.Result.SelectMany(_ => _), cancellationToken));
        return results;
    }

    /// <summary>
    /// 异步For
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <param name="maxParallelCount">最大并行数</param>
    /// <param name="cancellationToken">取消口令</param>
    /// <returns></returns>
    public static async Task ForAsync<T>(this IEnumerable<T> source, Func<T, int, Task> selector, int maxParallelCount, CancellationToken cancellationToken = default)
    {
        int index = 0;
        if (Debugger.IsAttached)
        {
            foreach (var item in source)
            {
                await selector(item, index);
                index++;
            }

            return;
        }

        var list = new List<Task>();
        foreach (var item in source)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            list.Add(selector(item, index));
            Interlocked.Add(ref index, 1);
            if (list.Count >= maxParallelCount)
            {
                await Task.WhenAny(list);
                list.RemoveAll(t => t.IsCompleted);
            }
        }

        await Task.WhenAll(list);
    }

    /// <summary>
    /// 异步For
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <param name="cancellationToken">取消口令</param>
    /// <returns></returns>
    public static Task ForAsync<T>(this IEnumerable<T> source, Func<T, int, Task> selector, CancellationToken cancellationToken = default)
    {
        if (source is ICollection<T> collection)
        {
            return ForAsync(collection, selector, collection.Count, cancellationToken);
        }

        var list = source.ToList();
        return ForAsync(list, selector, list.Count, cancellationToken);
    }

    /// <summary>
    /// 取最大值
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <returns></returns>
    public static TResult MaxOrDefault<TSource, TResult>(this IQueryable<TSource> source, Expression<Func<TSource, TResult>> selector) => source.Select(selector).DefaultIfEmpty().Max();

    /// <summary>
    /// 取最大值
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static TResult MaxOrDefault<TSource, TResult>(this IQueryable<TSource> source, Expression<Func<TSource, TResult>> selector, TResult defaultValue) => source.Select(selector).DefaultIfEmpty(defaultValue).Max();

    /// <summary>
    /// 取最大值
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static TSource MaxOrDefault<TSource>(this IQueryable<TSource> source) => source.DefaultIfEmpty().Max();

    /// <summary>
    /// 取最大值
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="source"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static TSource MaxOrDefault<TSource>(this IQueryable<TSource> source, TSource defaultValue) => source.DefaultIfEmpty(defaultValue).Max();

    /// <summary>
    /// 取最大值
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static TResult MaxOrDefault<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector, TResult defaultValue) => source.Select(selector).DefaultIfEmpty(defaultValue).Max();

    /// <summary>
    /// 取最大值
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <returns></returns>
    public static TResult MaxOrDefault<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector) => source.Select(selector).DefaultIfEmpty().Max();

    /// <summary>
    /// 取最大值
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static TSource MaxOrDefault<TSource>(this IEnumerable<TSource> source) => source.DefaultIfEmpty().Max();

    /// <summary>
    /// 取最大值
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="source"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static TSource MaxOrDefault<TSource>(this IEnumerable<TSource> source, TSource defaultValue) => source.DefaultIfEmpty(defaultValue).Max();

    /// <summary>
    /// 取最小值
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <returns></returns>
    public static TResult MinOrDefault<TSource, TResult>(this IQueryable<TSource> source, Expression<Func<TSource, TResult>> selector) => source.Select(selector).DefaultIfEmpty().Min();

    /// <summary>
    /// 取最小值
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static TResult MinOrDefault<TSource, TResult>(this IQueryable<TSource> source, Expression<Func<TSource, TResult>> selector, TResult defaultValue) => source.Select(selector).DefaultIfEmpty(defaultValue).Min();

    /// <summary>
    /// 取最小值
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static TSource MinOrDefault<TSource>(this IQueryable<TSource> source) => source.DefaultIfEmpty().Min();

    /// <summary>
    /// 取最小值
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="source"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static TSource MinOrDefault<TSource>(this IQueryable<TSource> source, TSource defaultValue) => source.DefaultIfEmpty(defaultValue).Min();

    /// <summary>
    /// 取最小值
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <returns></returns>
    public static TResult MinOrDefault<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector) => source.Select(selector).DefaultIfEmpty().Min();

    /// <summary>
    /// 取最小值
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static TResult MinOrDefault<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector, TResult defaultValue) => source.Select(selector).DefaultIfEmpty(defaultValue).Min();

    /// <summary>
    /// 取最小值
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static TSource MinOrDefault<TSource>(this IEnumerable<TSource> source) => source.DefaultIfEmpty().Min();

    /// <summary>
    /// 取最小值
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="source"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static TSource MinOrDefault<TSource>(this IEnumerable<TSource> source, TSource defaultValue) => source.DefaultIfEmpty(defaultValue).Min();

    /// <summary>
    /// 标准差
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="source"></param>
    /// <param name="selector"></param>
    /// <returns></returns>
    public static TResult StandardDeviation<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector) where TResult : IConvertible
    {
        return StandardDeviation(source.Select(t => selector(t).ConvertTo<double>())).ConvertTo<TResult>();
    }

    /// <summary>
    /// 标准差
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static T StandardDeviation<T>(this IEnumerable<T> source) where T : IConvertible
    {
        return StandardDeviation(source.Select(t => t.ConvertTo<double>())).ConvertTo<T>();
    }

    /// <summary>
    /// 标准差
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static double StandardDeviation(this IEnumerable<double> source)
    {
        double result = 0;
        var list = source as ICollection<double> ?? source.ToList();
        int count = list.Count;
        if (count > 1)
        {
            var avg = list.Average();
            var sum = list.Sum(d => (d - avg) * (d - avg));
            result = Math.Sqrt(sum / count);
        }

        return result;
    }

    /// <summary>
    /// 随机排序
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static IOrderedEnumerable<T> OrderByRandom<T>(this IEnumerable<T> source)
    {
        return source.OrderBy(_ => Guid.NewGuid());
    }

    /// <summary>
    /// 序列相等
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <param name="condition"></param>
    /// <returns></returns>
    public static bool SequenceEqual<T>(this IEnumerable<T> first, IEnumerable<T> second, Func<T, T, bool> condition)
    {
        if (first is ICollection<T> source1 && second is ICollection<T> source2)
        {
            if (source1.Count != source2.Count)
            {
                return false;
            }

            if (source1 is IList<T> list1 && source2 is IList<T> list2)
            {
                int count = source1.Count;
                for (int index = 0; index < count; ++index)
                {
                    if (!condition(list1[index], list2[index]))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        using IEnumerator<T> enumerator1 = first.GetEnumerator();
        using IEnumerator<T> enumerator2 = second.GetEnumerator();
        while (enumerator1.MoveNext())
        {
            if (!enumerator2.MoveNext() || !condition(enumerator1.Current, enumerator2.Current))
            {
                return false;
            }
        }

        return !enumerator2.MoveNext();
    }

    /// <summary>
    /// 序列相等
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <param name="condition"></param>
    /// <returns></returns>
    public static bool SequenceEqual<T1, T2>(this IEnumerable<T1> first, IEnumerable<T2> second, Func<T1, T2, bool> condition)
    {
        if (first is ICollection<T1> source1 && second is ICollection<T2> source2)
        {
            if (source1.Count != source2.Count)
            {
                return false;
            }

            if (source1 is IList<T1> list1 && source2 is IList<T2> list2)
            {
                int count = source1.Count;
                for (int index = 0; index < count; ++index)
                {
                    if (!condition(list1[index], list2[index]))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        using IEnumerator<T1> enumerator1 = first.GetEnumerator();
        using IEnumerator<T2> enumerator2 = second.GetEnumerator();
        while (enumerator1.MoveNext())
        {
            if (!enumerator2.MoveNext() || !condition(enumerator1.Current, enumerator2.Current))
            {
                return false;
            }
        }

        return !enumerator2.MoveNext();
    }

    /// <summary>
    /// 对比两个集合哪些是新增的、删除的、修改的
    /// </summary>
    /// <param name="first">新集合</param>
    /// <param name="second">旧集合</param>
    /// <returns></returns>
    public static (List<T> adds, List<T> remove, List<T> updates) CompareChanges<T>(this IEnumerable<T> first, IEnumerable<T> second)
    {
        first ??= new List<T>();
        second ??= new List<T>();
        var firstSource = first as ICollection<T> ?? first.ToList();
        var secondSource = second as ICollection<T> ?? second.ToList();
        var add = firstSource.Except(secondSource).ToList();
        var remove = secondSource.Except(firstSource).ToList();
        var update = firstSource.Intersect(secondSource).ToList();
        return (add, remove, update);
    }

    /// <summary>
    /// 对比两个集合哪些是新增的、删除的、修改的
    /// </summary>
    /// <typeparam name="TKey">对比因素</typeparam>
    /// <typeparam name="T"></typeparam>
    /// <param name="first">新集合</param>
    /// <param name="second">旧集合</param>
    /// <param name="keySelector">对比因素(可唯一确定元素的字段)</param>
    /// <returns></returns>
    public static (List<T> adds, List<T> remove, List<T> updates) CompareChanges<T, TKey>(this IEnumerable<T> first, IEnumerable<T> second, Func<T, TKey> keySelector)
    {
        return CompareChanges(first, second, keySelector, keySelector);
    }

    /// <summary>
    /// 对比两个集合哪些是新增的、删除的、修改的(大数据集时性能不佳，请考虑使用其他重载)
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="first">新集合</param>
    /// <param name="second">旧集合</param>
    /// <param name="condition">对比因素条件</param>
    /// <returns></returns>
    [Obsolete("大数据集时性能不佳，请考虑使用其他重载")]
    public static (List<T1> adds, List<T2> remove, List<T1> updates) CompareChanges<T1, T2>(this IEnumerable<T1> first, IEnumerable<T2> second, Func<T1, T2, bool> condition)
    {
        if (first == null) throw new ArgumentNullException(nameof(first));
        if (second == null) throw new ArgumentNullException(nameof(second));
        if (condition == null) throw new ArgumentNullException(nameof(condition));

        var adds = new List<T1>();
        var removes = new List<T2>();
        var updates = new List<T1>();
        var secondSet = new HashSet<T2>(second);
        foreach (var item in first)
        {
            var match = secondSet.FirstOrDefault(s => condition(item, s));
            if (match != null)
            {
                updates.Add(item);
                secondSet.Remove(match);
            }
            else
            {
                adds.Add(item);
            }
        }

        removes.AddRange(secondSet);
        return (adds, removes, updates);
    }

    /// <summary>
    /// 对比两个集合哪些是新增的、删除的、修改的
    /// </summary>
    /// <typeparam name="T1">集合1</typeparam>
    /// <typeparam name="T2">集合2</typeparam>
    /// <typeparam name="TKey">对比因素</typeparam>
    /// <param name="first">新集合</param>
    /// <param name="second">旧集合</param>
    /// <param name="firstKeySelector">集合1的对比因素(可唯一确定元素的字段)</param>
    /// <param name="secondKeySelector">集合2的对比因素(可唯一确定元素的字段)</param>
    /// <returns></returns>
    public static (List<T1> adds, List<T2> remove, List<T1> updates) CompareChanges<T1, T2, TKey>(this IEnumerable<T1> first, IEnumerable<T2> second, Func<T1, TKey> firstKeySelector, Func<T2, TKey> secondKeySelector)
    {
        first ??= new List<T1>();
        second ??= new List<T2>();

        var firstLookup = first.ToLookup(firstKeySelector);
        var secondLookup = second.ToLookup(secondKeySelector);
        var adds = new List<T1>();
        var removes = new List<T2>();
        var updates = new List<T1>();

        foreach (var kvp in firstLookup)
        {
            if (secondLookup.Contains(kvp.Key))
            {
                updates.AddRange(kvp);
            }
            else
            {
                adds.AddRange(kvp);
            }
        }

        foreach (var kvp in secondLookup.Where(kvp => !firstLookup.Contains(kvp.Key)))
        {
            removes.AddRange(kvp);
        }

        return (adds, removes, updates);
    }

    /// <summary>
    /// 对比两个集合哪些是新增的、删除的、修改的
    /// </summary>
    /// <param name="first">新集合</param>
    /// <param name="second">旧集合</param>
    /// <returns></returns>
    public static (List<T> adds, List<T> remove, List<(T first, T second)> updates) CompareChangesPlus<T>(this IEnumerable<T> first, IEnumerable<T> second)
    {
        first ??= new List<T>();
        second ??= new List<T>();
        var firstSource = first as ICollection<T> ?? first.ToList();
        var secondSource = second as ICollection<T> ?? second.ToList();
        var add = firstSource.Except(secondSource).ToList();
        var remove = secondSource.Except(firstSource).ToList();
        var updates = firstSource.Intersect(secondSource).Select(t1 => (t1, secondSource.FirstOrDefault(t2 => t1.Equals(t2)))).ToList();
        return (add, remove, updates);
    }

    /// <summary>
    /// 对比两个集合哪些是新增的、删除的、修改的
    /// </summary>
    /// <param name="first">新集合</param>
    /// <param name="second">旧集合</param>
    /// <param name="keySelector">对比因素</param>
    /// <returns></returns>
    public static (List<T> adds, List<T> remove, List<(T first, T second)> updates) CompareChangesPlus<T, TKey>(this IEnumerable<T> first, IEnumerable<T> second, Func<T, TKey> keySelector)
    {
        return CompareChangesPlus(first, second, keySelector, keySelector);
    }

    /// <summary>
    /// 对比两个集合哪些是新增的、删除的、修改的(大数据集时性能不佳，请考虑使用其他重载)
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="first">新集合</param>
    /// <param name="second">旧集合</param>
    /// <param name="condition">对比因素条件</param>
    /// <returns></returns>
    [Obsolete("大数据集时性能不佳，请考虑使用其他重载")]
    public static (List<T1> adds, List<T2> remove, List<(T1 first, T2 second)> updates) CompareChangesPlus<T1, T2>(
        this IEnumerable<T1> first,
        IEnumerable<T2> second,
        Func<T1, T2, bool> condition)
    {
        if (first == null) throw new ArgumentNullException(nameof(first));
        if (second == null) throw new ArgumentNullException(nameof(second));
        if (condition == null) throw new ArgumentNullException(nameof(condition));

        var adds = new List<T1>();
        var removes = new List<T2>();
        var updates = new List<(T1 first, T2 second)>();
        var secondSet = new HashSet<T2>(second);
        foreach (var item in first)
        {
            var match = secondSet.FirstOrDefault(s => condition(item, s));
            if (match != null)
            {
                updates.Add((item, match));
                secondSet.Remove(match);
            }
            else
            {
                adds.Add(item);
            }
        }

        removes.AddRange(secondSet);
        return (adds, removes, updates);
    }

    /// <summary>
    /// 对比两个集合哪些是新增的、删除的、修改的
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="first">新集合</param>
    /// <param name="second">旧集合</param>
    /// <param name="firstKeySelector">集合1的对比因素(可唯一确定元素的字段)</param>
    /// <param name="secondKeySelector">集合2的对比因素(可唯一确定元素的字段)</param>
    /// <returns></returns>
    public static (List<T1> adds, List<T2> remove, List<(T1 first, T2 second)> updates) CompareChangesPlus<T1, T2, TKey>(this IEnumerable<T1> first, IEnumerable<T2> second, Func<T1, TKey> firstKeySelector, Func<T2, TKey> secondKeySelector)
    {
        first ??= new List<T1>();
        second ??= new List<T2>();
        var lookup1 = first.ToLookup(firstKeySelector);
        var lookup2 = second.ToLookup(secondKeySelector);
        var add = new List<T1>();
        var remove = new List<T2>();
        var updates = new List<(T1 first, T2 second)>();

        foreach (var x in lookup1)
        {
            if (lookup2.Contains(x.Key))
            {
                updates.Add((x.First(), lookup2[x.Key].First()));
            }
            else
            {
                add.AddRange(x);
            }
        }

        foreach (var x in lookup2.Where(x => !lookup1.Contains(x.Key)))
        {
            remove.AddRange(x);
        }

        return (add, remove, updates);
    }

    /// <summary>
    /// 将集合声明为非null集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <returns></returns>
    public static List<T> AsNotNull<T>(this List<T> list)
    {
        return list ?? new List<T>();
    }

    /// <summary>
    /// 将集合声明为非null集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <returns></returns>
    public static IEnumerable<T> AsNotNull<T>(this IEnumerable<T> list)
    {
        return list ?? new List<T>();
    }

    /// <summary>
    /// 满足条件时执行筛选条件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="condition"></param>
    /// <param name="where"></param>
    /// <returns></returns>
    public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> source, bool condition, Func<T, bool> where)
    {
        return condition ? source.Where(where) : source;
    }

    /// <summary>
    /// 满足条件时执行筛选条件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="condition"></param>
    /// <param name="where"></param>
    /// <returns></returns>
    public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> source, Func<bool> condition, Func<T, bool> where)
    {
        return condition() ? source.Where(where) : source;
    }

    /// <summary>
    /// 满足条件时执行筛选条件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="condition"></param>
    /// <param name="where"></param>
    /// <returns></returns>
    public static IQueryable<T> WhereIf<T>(this IQueryable<T> source, bool condition, Expression<Func<T, bool>> where)
    {
        return condition ? source.Where(where) : source;
    }

    /// <summary>
    /// 满足条件时执行筛选条件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="condition"></param>
    /// <param name="where"></param>
    /// <returns></returns>
    public static IQueryable<T> WhereIf<T>(this IQueryable<T> source, Func<bool> condition, Expression<Func<T, bool>> where)
    {
        return condition() ? source.Where(where) : source;
    }

    /// <summary>
    /// 改变元素的索引位置
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list">集合</param>
    /// <param name="item">元素</param>
    /// <param name="index">索引值</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static IList<T> ChangeIndex<T>(this IList<T> list, T item, int index)
    {
        if (item is null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        ChangeIndexInternal(list, item, index);
        return list;
    }

    /// <summary>
    /// 改变元素的索引位置
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list">集合</param>
    /// <param name="condition">元素定位条件</param>
    /// <param name="index">索引值</param>
    public static IList<T> ChangeIndex<T>(this IList<T> list, Func<T, bool> condition, int index)
    {
        var item = list.FirstOrDefault(condition);
        if (item != null)
        {
            ChangeIndexInternal(list, item, index);
        }
        return list;
    }

    private static void ChangeIndexInternal<T>(IList<T> list, T item, int index)
    {
        index = Math.Max(0, index);
        index = Math.Min(list.Count - 1, index);
        list.Remove(item);
        list.Insert(index, item);
    }
}