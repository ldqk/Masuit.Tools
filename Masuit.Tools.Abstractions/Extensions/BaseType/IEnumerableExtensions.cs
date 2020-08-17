using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Masuit.Tools
{
    public static partial class IEnumerableExtensions
    {
        #region SyncForEach

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

        #endregion SyncForEach

        #region AsyncForEach

        /// <summary>
        /// 遍历IEnumerable
        /// </summary>
        /// <param name="objs"></param>
        /// <param name="action">回调方法</param>
        /// <typeparam name="T"></typeparam>
        public static async void ForEachAsync<T>(this IEnumerable<T> objs, Action<T> action)
        {
            await Task.Run(() =>
            {
                Parallel.ForEach(objs, action);
            });
        }

        #endregion AsyncForEach

        #region To方法

        /// <summary>
        /// 【内部方法】集合接口转具体实现
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TArray"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <param name="converter"></param>
        /// <param name="defaultValueFunc">当<paramref name="source"/>为<see cref="null"/>时，会调用此委托生成默认值</param>
        /// <returns></returns>
        private static TArray IEnumerableBaseTo<TSource, TResult, TArray>(
          IEnumerable<TSource> source,
          Func<TSource, TResult> selector,
          Func<IEnumerable<TResult>, TArray> converter,
          Func<TArray> defaultValueFunc)
        {
            selector.CheckNullWithException(nameof(selector));

            return source == null ? defaultValueFunc() : converter(source.Select(selector));
        }

        /// <summary>
        /// Select+ToList的组合版本
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <param name="defaultValue">当<paramref name="source"/>为<see cref="null"/>时，返回的值.默认值为:0长度的<see cref="List{T}"/></param>
        /// <returns></returns>
        public static List<TResult> ToList<TSource, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, TResult> selector,
            List<TResult>? defaultValue = null)
        {
            return IEnumerableBaseTo(source, selector, Enumerable.ToList, () => defaultValue ?? new List<TResult>());
        }

        /// <summary>
        /// Select+ToList的组合版本(异步)
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <param name="defaultValue">当<paramref name="source"/>为<see cref="null"/>时，返回的值.默认值为:0长度的<see cref="List{T}"/></param>
        /// <returns></returns>
        public static Task<List<TResult>> ToListAsync<TSource, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, TResult> selector,
            List<TResult>? defaultValue = null)
        {
            return Task.Run(() => ToList(source, selector, defaultValue));
        }

        #endregion To方法

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
            var hash = new HashSet<TKey>();
            return source.Where(p => hash.Add(keySelector(p)));
        }

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
        /// 添加符合条件的多个元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="predicate"></param>
        /// <param name="values"></param>
        public static void AddRangeIf<T>(
            this ICollection<T> @this,
            Func<T, bool> predicate,
            params T[] values)
        {
            foreach (var obj in values)
            {
                if (predicate(obj))
                {
                    @this.Add(obj);
                }
            }
        }

        /// <summary>
        /// 添加不重复的元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="values"></param>
        public static void AddRangeIfNotContains<T>(this ICollection<T> @this, params T[] values)
        {
            foreach (T obj in values)
            {
                if (!@this.Contains(obj))
                {
                    @this.Add(obj);
                }
            }
        }

        /// <summary>
        /// 移除符合条件的元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="where"></param>
        public static void RemoveWhere<T>(this ICollection<T> @this, Func<T, bool> @where)
        {
            foreach (var obj in @this.Where(where).ToList())
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
            foreach (var item in list.Select((o, i) => new { Value = o, Index = i }).Where(p => condition(p.Value)).OrderByDescending(p => p.Index))
            {
                if (item.Index + 1 == list.Count)
                {
                    list.Add(value);
                }
                else
                {
                    list.Insert(item.Index + 1, value);
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
            foreach (var item in list.Select((o, i) => new { Value = o, Index = i }).Where(p => p.Index == index).OrderByDescending(p => p.Index))
            {
                if (item.Index + 1 == list.Count)
                {
                    list.Add(value);
                }
                else
                {
                    list.Insert(item.Index + 1, value);
                }
            }
        }
    }
}