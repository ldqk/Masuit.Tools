using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Masuit.Tools
{
    public static class IDictionaryExtensions
    {
        /// <summary>
        /// 添加或更新键值对
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="this"></param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public static TValue AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, TValue value)
        {
            if (!@this.ContainsKey(key))
            {
                @this.Add(new KeyValuePair<TKey, TValue>(key, value));
            }
            else
            {
                @this[key] = value;
            }

            return @this[key];
        }

        /// <summary>
        /// 添加或更新键值对
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="this"></param>
        /// <param name="that">另一个字典集</param>
        /// <returns></returns>
        public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> @this, IDictionary<TKey, TValue> that)
        {
            foreach (var item in that)
            {
                AddOrUpdate(@this, item.Key, item.Value);
            }
        }

        /// <summary>
        /// 添加或更新键值对
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="this"></param>
        /// <param name="that">另一个字典集</param>
        /// <returns></returns>
        public static void AddOrUpdateTo<TKey, TValue>(this IDictionary<TKey, TValue> @this, IDictionary<TKey, TValue> that)
        {
            foreach (var item in @this)
            {
                AddOrUpdate(that, item.Key, item.Value);
            }
        }

        /// <summary>
        /// 添加或更新键值对
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="this"></param>
        /// <param name="key">键</param>
        /// <param name="addValue">添加时的值</param>
        /// <param name="updateValueFactory">更新时的操作</param>
        /// <returns></returns>
        public static TValue AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
        {
            if (!@this.ContainsKey(key))
            {
                @this.Add(new KeyValuePair<TKey, TValue>(key, addValue));
            }
            else
            {
                @this[key] = updateValueFactory(key, @this[key]);
            }

            return @this[key];
        }

        /// <summary>
        /// 添加或更新键值对
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="this"></param>
        /// <param name="that">另一个字典集</param>
        /// <param name="updateValueFactory">更新时的操作</param>
        /// <returns></returns>
        public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> @this, IDictionary<TKey, TValue> that, Func<TKey, TValue, TValue> updateValueFactory)
        {
            foreach (var item in that)
            {
                AddOrUpdate(@this, item.Key, item.Value, updateValueFactory);
            }
        }

        /// <summary>
        /// 添加或更新键值对
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="this"></param>
        /// <param name="that">另一个字典集</param>
        /// <param name="updateValueFactory">更新时的操作</param>
        /// <returns></returns>
        public static void AddOrUpdateTo<TKey, TValue>(this IDictionary<TKey, TValue> @this, IDictionary<TKey, TValue> that, Func<TKey, TValue, TValue> updateValueFactory)
        {
            foreach (var item in @this)
            {
                AddOrUpdate(that, item.Key, item.Value, updateValueFactory);
            }
        }

        /// <summary>
        /// 添加或更新键值对
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="this"></param>
        /// <param name="key">键</param>
        /// <param name="addValueFactory">添加时的操作</param>
        /// <param name="updateValueFactory">更新时的操作</param>
        /// <returns></returns>
        public static TValue AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
        {
            if (!@this.ContainsKey(key))
            {
                @this.Add(new KeyValuePair<TKey, TValue>(key, addValueFactory(key)));
            }
            else
            {
                @this[key] = updateValueFactory(key, @this[key]);
            }

            return @this[key];
        }

        /// <summary>
        /// 遍历IEnumerable
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="action">回调方法</param>
        public static void ForEach<TKey, TValue>(this IDictionary<TKey, TValue> dic, Action<TKey, TValue> action)
        {
            foreach (var item in dic)
            {
                action(item.Key, item.Value);
            }
        }

        /// <summary>
        /// 安全的转换成字典集
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TElement"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector">键选择器</param>
        /// <param name="elementSelector">值选择器</param>
        /// <returns></returns>
        public static Dictionary<TKey, TElement> ToDictionarySafety<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            var dic = new Dictionary<TKey, TElement>();
            foreach (var item in source)
            {
                AddOrUpdate(dic, keySelector(item), elementSelector(item));
            }

            return dic;
        }

        /// <summary>
        /// 安全的转换成字典集
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TElement"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector">键选择器</param>
        /// <param name="elementSelector">值选择器</param>
        /// <returns></returns>
        public static ConcurrentDictionary<TKey, TElement> ToConcurrentDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            var dic = new ConcurrentDictionary<TKey, TElement>();
            foreach (var item in source)
            {
                AddOrUpdate(dic, keySelector(item), elementSelector(item));
            }

            return dic;
        }

        /// <summary>
        /// 转换成并发字典集合
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dic"></param>
        /// <returns></returns>
        public static ConcurrentDictionary<TKey, TValue> AsConcurrentDictionary<TKey, TValue>(this Dictionary<TKey, TValue> dic)
        {
            var cd = new ConcurrentDictionary<TKey, TValue>();
            cd.AddOrUpdate(dic);
            return cd;
        }

        /// <summary>
        /// 转换成普通字典集合
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dic"></param>
        /// <returns></returns>
        public static Dictionary<TKey, TValue> AsDictionary<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dic)
        {
            var cd = new Dictionary<TKey, TValue>();
            cd.AddOrUpdate(dic);
            return cd;
        }
    }
}