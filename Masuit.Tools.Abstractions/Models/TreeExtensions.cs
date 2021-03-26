using System;
using System.Collections.Generic;
using System.Linq;

namespace Masuit.Tools.Models
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
        public static IEnumerable<T> Filter<T>(this IEnumerable<T> items, Func<T, bool> func) where T : class, ITreeChildren<T>
        {
            var results = new List<T>();
            foreach (var item in items.Where(i => i != null))
            {
                item.Children ??= new List<T>();
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
        public static IEnumerable<T> Filter<T>(this T item, Func<T, bool> func) where T : class, ITreeChildren<T>
        {
            return new[] { item }.Filter(func);
        }

        /// <summary>
        /// 平铺开
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> items) where T : class, ITreeChildren<T>
        {
            var result = new List<T>();
            foreach (var item in items)
            {
                result.Add(item);
                item.Children ??= new List<T>();
                result.AddRange(item.Children.Flatten());
            }

            return result;
        }

        /// <summary>
        /// 平铺开
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> Flatten<T>(this T p) where T : class, ITreeChildren<T>
        {
            var result = new List<T>()
            {
                p
            };
            foreach (var item in p.Children)
            {
                result.Add(item);
                item.Children ??= new List<T>();
                result.AddRange(item.Children.Flatten());
            }

            return result;
        }

        /// <summary>
        /// 平铺开任意树形结构数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> items, Func<T, IEnumerable<T>> selector)
        {
            var result = new List<T>();
            foreach (var item in items)
            {
                result.Add(item);
                result.AddRange(selector(item).Flatten(selector));
            }

            return result;
        }

        /// <summary>
        /// 所有子级
        /// </summary>
        public static ICollection<T> AllChildren<T>(this T tree) where T : ITreeChildren<T> => GetChildren(tree, c => c.Children);

        /// <summary>
        /// 所有子级
        /// </summary>
        public static ICollection<T> AllChildren<T>(this T tree, Func<T, IEnumerable<T>> selector) => GetChildren(tree, selector);

        /// <summary>
        /// 所有父级
        /// </summary>
        public static ICollection<T> AllParent<T>(this T tree) where T : ITreeParent<T> => GetParents(tree, c => c.Parent);

        /// <summary>
        /// 所有父级
        /// </summary>
        public static ICollection<T> AllParent<T>(this T tree, Func<T, T> selector) => GetParents(tree, selector);

        /// <summary>
        /// 是否是根节点
        /// </summary>
        public static bool IsRoot<T>(this ITreeParent<T> tree) where T : ITreeParent<T> => tree.Parent == null;

        /// <summary>
        /// 是否是叶子节点
        /// </summary>
        public static bool IsLeaf<T>(this ITreeChildren<T> tree) where T : ITreeChildren<T> => tree.Children?.Count == 0;

        /// <summary>
        /// 深度层级
        /// </summary>
        public static int Level<T>(this ITreeParent<T> tree) where T : ITreeParent<T> => IsRoot(tree) ? 1 : Level(tree.Parent) + 1;

        /// <summary>
        /// 节点路径（UNIX路径格式，以“/”分隔）
        /// </summary>
        public static string Path<T>(this T tree) where T : ITree<T> => GetFullPath(tree, t => t.Name);

        /// <summary>
        /// 节点路径（UNIX路径格式，以“/”分隔）
        /// </summary>
        public static string Path<T>(this T tree, Func<T, string> selector) where T : ITreeParent<T> => GetFullPath(tree, selector);

        /// <summary>
        /// 根节点
        /// </summary>
        public static T Root<T>(this T tree) where T : ITreeParent<T> => GetRoot(tree, t => t.Parent);

        private static string GetFullPath<T>(T c, Func<T, string> selector) where T : ITreeParent<T> => c.Parent != null ? GetFullPath(c.Parent, selector) + "/" + selector(c) : selector(c);

        /// <summary>
        /// 根节点
        /// </summary>
        public static T GetRoot<T>(T c, Func<T, T> selector) where T : ITreeParent<T> => c.Parent != null ? GetRoot(c.Parent, selector) : c;

        /// <summary>
        /// 递归取出所有下级
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private static List<T> GetChildren<T>(T t, Func<T, IEnumerable<T>> selector)
        {
            return selector(t).Union(selector(t).Where(c => selector(c).Any()).SelectMany(c => GetChildren(c, selector))).ToList();
        }

        /// <summary>
        /// 递归取出所有上级
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private static List<T> GetParents<T>(T t, Func<T, T> selector)
        {
            var list = new List<T>() { selector(t) };
            if (selector(t) != null)
            {
                return list.Union(GetParents(selector(t), selector)).Where(x => x != null).ToList();
            }

            list.RemoveAll(x => x == null);
            return list;
        }
    }
}