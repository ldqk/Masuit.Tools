using System;
using System.Collections.Generic;
using System.Linq;

namespace Masuit.Tools.Core.Models
{
    /// <summary>
    /// 树形数据扩展
    /// </summary>
    public static class TreeExtensions
    {
        /// <summary>
        /// 过滤
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static IEnumerable<T> Filter<T>(this IEnumerable<T> items, Func<T, bool> func) where T : class, ITree<T>
        {
            var results = new List<T>();
            foreach (var item in items.Where(i => i != null))
            {
                item.Children = item.Children.Filter(func).ToList();
                if (item.Children.Any() || func(item))
                {
                    results.Add(item);
                }
            }
            return results;
        }

        /// <summary>
        /// 过滤
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static IEnumerable<T> Filter<T>(this T item, Func<T, bool> func) where T : class, ITree<T>
        {
            return (new[] { item }).Filter(func);
        }

        /// <summary>
        /// 平铺开
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> items) where T : ITree<T>
        {
            var result = new List<T>();
            foreach (var item in items)
            {
                result.Add(item);
                result.AddRange(item.Children.Flatten());
            }
            return result;
        }
    }
}