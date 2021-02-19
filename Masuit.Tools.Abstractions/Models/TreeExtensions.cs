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
        public static IEnumerable<T> Filter<T>(this IEnumerable<T> items, Func<T, bool> func) where T : ITreeChildren<T>
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
        public static IEnumerable<T> Filter<T>(this T item, Func<T, bool> func) where T : ITreeChildren<T>
        {
            return new[] { item }.Filter(func);
        }

        /// <summary>
        /// 平铺开
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> items) where T : ITreeChildren<T>
        {
            var result = new List<T>();
            foreach (var item in items)
            {
                result.Add(item);
                result.AddRange(item.Children.Flatten());
            }
            return result;
        }

        /// <summary>
        /// 所有子级
        /// </summary>
        public static ICollection<T> AllChildren<T>(this ITreeChildren<T> tree) where T : ITreeChildren<T> => GetChildren(tree);

        /// <summary>
        /// 所有父级
        /// </summary>
        public static ICollection<T> AllParent<T>(this ITreeParent<T> tree) where T : ITreeParent<T> => GetParents(tree);

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
        public static string Path<T>(this ITree<T> tree) where T : ITree<T> => GetFullPath(tree);

        private static string GetFullPath<T>(ITree<T> c) where T : ITree<T> => c.Parent != null ? GetFullPath(c.Parent) + "/" + c.Name : c.Name;

        /// <summary>
        /// 递归取出所有下级
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private static List<T> GetChildren<T>(ITreeChildren<T> t) where T : ITreeChildren<T>
        {
            return t.Children.Union(t.Children.Where(c => c.Children.Any()).SelectMany(tree => GetChildren(tree))).ToList();
        }

        /// <summary>
        /// 递归取出所有上级
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private static List<T> GetParents<T>(ITreeParent<T> t) where T : ITreeParent<T>
        {
            var list = new List<T>() { t.Parent };
            return t.Parent != null ? list.Union(GetParents(t.Parent)).ToList() : list;
        }
    }
}