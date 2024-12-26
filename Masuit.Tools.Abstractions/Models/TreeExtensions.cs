#if NETFRAMEWORK
using System;
using System.Collections.Generic;
using System.Linq;
#endif

using System.Linq.Expressions;
using Masuit.Tools.Systems;

namespace Masuit.Tools.Models;

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
    public static IEnumerable<T> Filter<T>(this IEnumerable<T> items, Func<T, bool> func) where T : class, ITreeChildren<T> => Flatten(items).Where(func);

    /// <summary>
    /// 过滤
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static IEnumerable<Tree<T>> Filter<T>(this IEnumerable<Tree<T>> items, Func<Tree<T>, bool> func) where T : class => Flatten(items).Where(func);

    /// <summary>
    /// 过滤
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static IEnumerable<T> Filter<T>(this T item, Func<T, bool> func) where T : class, ITreeChildren<T> => Flatten(item).Where(func);

    /// <summary>
    /// 过滤
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static IEnumerable<Tree<T>> Filter<T>(this Tree<T> item, Func<Tree<T>, bool> func) where T : class => Flatten(item).Where(func);

    /// <summary>
    /// 平铺开
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="optionAction">平铺时子级需要做的操作，参数1：子级对象，参数2：父级对象</param>
    /// <returns></returns>
    public static IEnumerable<T> Flatten<T>(this IEnumerable<T> items, Action<T, T> optionAction = null) where T : class, ITreeChildren<T> => items.Flatten(i => i.Children ?? [], optionAction);

    /// <summary>
    /// 平铺开
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="p"></param>
    /// <param name="optionAction">平铺时子级需要做的操作，参数1：子级对象，参数2：父级对象</param>
    /// <returns></returns>
    public static IEnumerable<T> Flatten<T>(this T p, Action<T, T> optionAction = null) where T : class, ITreeChildren<T> => p != null ? Flatten([p], t => t.Children, optionAction) : [];

    /// <summary>
    /// 平铺开任意树形结构数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="selector">子节点获取方法</param>
    /// <param name="optionAction">平铺时子级需要做的操作，参数1：子级对象，参数2：父级对象</param>
    /// <returns></returns>
    public static IEnumerable<T> Flatten<T>(this IEnumerable<T> items, Func<T, IEnumerable<T>> selector, Action<T, T> optionAction = null)
    {
        if (items == null)
            yield break;

        // 使用一个队列来存储待处理的节点
        var queue = new Queue<T>();
        // 首先将所有项加入队列
        foreach (var item in items)
            queue.Enqueue(item);

        // 循环直到队列为空
        while (queue.Count > 0)
        {
            // 从队列中取出当前节点
            var currentItem = queue.Dequeue();

            // 将当前节点返回
            yield return currentItem;

            // 获取当前节点的所有子节点
            var children = selector(currentItem) ?? [];

            // 将所有子节点加入队列
            foreach (var child in children)
            {
                // 执行平铺时的操作（如果有的话）
                optionAction?.Invoke(child, currentItem);
                queue.Enqueue(child);
            }
        }
    }

    /// <summary>
    /// 平铺开
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="optionAction">平铺时子级需要做的操作，参数1：子级对象，参数2：父级对象</param>
    /// <returns></returns>
    public static IEnumerable<Tree<T>> Flatten<T>(this IEnumerable<Tree<T>> items, Action<Tree<T>, Tree<T>> optionAction = null) where T : class => items != null ? Flatten(items, t => t.Children, optionAction) : [];

    /// <summary>
    /// 平铺开
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="p"></param>
    /// <param name="optionAction">平铺时子级需要做的操作，参数1：子级对象，参数2：父级对象</param>
    /// <returns></returns>
    public static IEnumerable<Tree<T>> Flatten<T>(this Tree<T> p, Action<Tree<T>, Tree<T>> optionAction = null) where T : class
    {
        if (p == null)
        {
            return [];
        }

        return Flatten([p], t => t.Children, optionAction);
    }

    /// <summary>
    /// 平行集合转换成树形结构
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="idSelector"></param>
    /// <param name="pidSelector"></param>
    /// <param name="topValue">根对象parentId的值</param>
    /// <returns></returns>
    public static List<T> ToTree<T>(this IEnumerable<T> source, Expression<Func<T, string>> idSelector, Expression<Func<T, string>> pidSelector, string topValue = null) where T : ITreeParent<T>, ITreeChildren<T> => ToTree<T, string>(source, idSelector, pidSelector, topValue);

    /// <summary>
    /// 平行集合转换成树形结构
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="idSelector"></param>
    /// <param name="pidSelector"></param>
    /// <param name="topValue">根对象parentId的值</param>
    /// <returns></returns>
    public static List<T> ToTree<T>(this IEnumerable<T> source, Expression<Func<T, int>> idSelector, Expression<Func<T, int>> pidSelector, int topValue = 0) where T : ITreeParent<T>, ITreeChildren<T> => ToTree<T, int>(source, idSelector, pidSelector, topValue);

    /// <summary>
    /// 平行集合转换成树形结构
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="idSelector"></param>
    /// <param name="pidSelector"></param>
    /// <param name="topValue">根对象parentId的值</param>
    /// <returns></returns>
    public static List<T> ToTree<T>(this IEnumerable<T> source, Expression<Func<T, long>> idSelector, Expression<Func<T, long>> pidSelector, long topValue = 0) where T : ITreeParent<T>, ITreeChildren<T> => ToTree<T, long>(source, idSelector, pidSelector, topValue);

    /// <summary>
    /// 平行集合转换成树形结构
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="idSelector"></param>
    /// <param name="pidSelector"></param>
    /// <param name="topValue">根对象parentId的值</param>
    /// <returns></returns>
    public static List<T> ToTree<T>(this IEnumerable<T> source, Expression<Func<T, Guid>> idSelector, Expression<Func<T, Guid>> pidSelector, Guid topValue = default) where T : ITreeParent<T>, ITreeChildren<T> => ToTree<T, Guid>(source, idSelector, pidSelector, topValue);

    /// <summary>
    /// 平行集合转换成树形结构
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="source"></param>
    /// <param name="idSelector"></param>
    /// <param name="pidSelector"></param>
    /// <param name="topValue">根对象parentId的值</param>
    /// <returns></returns>
    public static List<T> ToTree<T, TKey>(this IEnumerable<T> source, Expression<Func<T, TKey>> idSelector, Expression<Func<T, TKey>> pidSelector, TKey topValue = default) where T : ITreeParent<T>, ITreeChildren<T> where TKey : IComparable => idSelector.Body.ToString() != pidSelector.Body.ToString() ? BuildTree(source.Enumerable2NonNullList(), idSelector.Compile(), pidSelector.Compile(), topValue).ToList() : throw new ArgumentException("idSelector和pidSelector不应该为同一字段！");

    /// <summary>
    /// 平行集合转换成树形结构
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static List<T> ToTree<T>(this IEnumerable<T> source) where T : class, ITreeEntity<T, int> => ToTree<T, int>(source);

    /// <summary>
    /// 平行集合转换成树形结构
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="source"></param>
    /// <param name="topValue"></param>
    /// <returns></returns>
    public static List<T> ToTree<T, TKey>(this IEnumerable<T> source, TKey? topValue = null) where T : class, ITreeEntity<T, TKey> where TKey : struct, IComparable => BuildTree(source.Enumerable2NonNullList(), topValue).ToList();

    /// <summary>
    /// 平行集合转换成树形结构
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="source"></param>
    /// <param name="idSelector"></param>
    /// <param name="pidSelector"></param>
    /// <param name="topValue">根对象parentId的值</param>
    /// <returns></returns>
    public static List<T> ToTree<T, TKey>(this IEnumerable<T> source, Expression<Func<T, TKey>> idSelector, Expression<Func<T, TKey?>> pidSelector, TKey? topValue = null) where T : ITreeChildren<T> where TKey : struct => idSelector.Body.ToString() != pidSelector.Body.ToString() ? BuildTree(source.Enumerable2NonNullList(), idSelector.Compile(), pidSelector.Compile(), topValue).ToList() : throw new ArgumentException("idSelector和pidSelector不应该为同一字段！");

    private static IEnumerable<T> BuildTree<T, TKey>(IEnumerable<T> source, Func<T, TKey> idSelector, Func<T, TKey> pidSelector, TKey topValue = default) where T : ITreeChildren<T> where TKey : IComparable
    {
        // 创建一个字典，用于快速查找节点的子节点
        var childrenLookup = new Dictionary<TKey, List<T>>();
        var list = source as ICollection<T> ?? source.ToList();
        foreach (var item in list.Where(item => !childrenLookup.ContainsKey(idSelector(item))))
        {
            childrenLookup[idSelector(item)] = [];
        }

        // 构建树结构
        foreach (var item in list.Where(item => !Equals(pidSelector(item), default(TKey)) && childrenLookup.ContainsKey(pidSelector(item))))
        {
            childrenLookup[pidSelector(item)].Add(item);
        }

        // 找到根节点，即没有父节点的节点
        foreach (var root in list.Where(x => Equals(pidSelector(x), topValue)))
        {
            // 为根节点和所有子节点设置Children属性
            // 使用队列来模拟递归过程
            var queue = new Queue<T>();
            queue.Enqueue(root);
            while (queue.Count > 0)
            {
                // 出队当前节点
                var current = queue.Dequeue();

                // 为当前节点设置子节点
                if (childrenLookup.TryGetValue(idSelector(current), out var children))
                {
                    current.Children = children;
                    foreach (var child in children)
                    {
                        // 如果子节点实现了ITreeParent接口，则设置其Parent属性
                        if (child is ITreeParent<T> tree)
                        {
                            tree.Parent = current;
                        }

                        // 将子节点入队以继续处理
                        queue.Enqueue(child);
                    }
                }
            }

            yield return root;
        }
    }

    internal static IEnumerable<T> BuildTree<T, TKey>(IEnumerable<T> source, TKey? topValue = null) where T : ITreeEntity<T, TKey> where TKey : struct, IComparable => BuildTree(source, t => t.Id, t => t.ParentId, topValue);

    internal static IEnumerable<T> BuildTree<T>(IEnumerable<T> source, string topValue = null) where T : ITreeEntity<T> => BuildTree(source, t => t.Id, t => t.ParentId, topValue);

    internal static IEnumerable<T> BuildTree<T>(IEnumerable<T> source, T parent) where T : ITreeEntity<T>
    {
        // 创建一个字典，用于快速查找节点的子节点
        var childrenLookup = new NullableDictionary<string, List<T>>();
        var list = source as ICollection<T> ?? source.ToList();
        foreach (var item in list.Where(item => !childrenLookup.ContainsKey(item.Id)))
            childrenLookup[item.Id] = [];

        // 构建树结构
        foreach (var item in list.Where(item => !string.IsNullOrEmpty(item.ParentId) && childrenLookup.ContainsKey(item.ParentId)))
            childrenLookup[item.ParentId].Add(item);

        // 找到根节点，即没有父节点的节点
        foreach (var root in list.Where(x => x.Id == parent.Id))
        {
            // 为根节点和所有子节点设置Children属性
            // 使用队列来模拟递归过程
            var queue = new Queue<T>();
            queue.Enqueue(root);
            while (queue.Count > 0)
            {
                // 出队当前节点
                var current = queue.Dequeue();

                // 为当前节点设置子节点
                if (childrenLookup.TryGetValue(current.Id, out var children))
                {
                    current.Children = children;
                    foreach (var child in children)
                    {
                        // 如果子节点实现了ITreeParent接口，则设置其Parent属性
                        if (child is ITreeParent<T> tree)
                            tree.Parent = current;

                        // 将子节点入队以继续处理
                        queue.Enqueue(child);
                    }
                }
            }

            yield return root;
        }
    }

    private static IEnumerable<T> BuildTree<T, TKey>(IEnumerable<T> source, Func<T, TKey> idSelector, Func<T, TKey?> pidSelector, TKey? topValue = null) where T : ITreeChildren<T> where TKey : struct
    {
        // 创建一个字典，用于快速查找节点的子节点
        var childrenLookup = new NullableDictionary<TKey, List<T>>();
        var list = source as ICollection<T> ?? source.ToList();
        foreach (var item in list.Where(item => !childrenLookup.ContainsKey(idSelector(item))))
        {
            childrenLookup[idSelector(item)] = [];
        }

        // 构建树结构
        foreach (var item in list.Where(item => !Equals(pidSelector(item), default(TKey)) && childrenLookup.ContainsKey(pidSelector(item) ?? default)))
        {
            childrenLookup[pidSelector(item) ?? default].Add(item);
        }

        // 找到根节点，即没有父节点的节点
        foreach (var root in list.Where(x => Equals(pidSelector(x), topValue)))
        {
            // 为根节点和所有子节点设置Children属性
            // 使用队列来模拟递归过程
            var queue = new Queue<T>();
            queue.Enqueue(root);
            while (queue.Count > 0)
            {
                // 出队当前节点
                var current = queue.Dequeue();

                // 为当前节点设置子节点
                if (childrenLookup.TryGetValue(idSelector(current), out var children))
                {
                    current.Children = children;
                    foreach (var child in children)
                    {
                        // 如果子节点实现了ITreeParent接口，则设置其Parent属性
                        if (child is ITreeParent<T> tree)
                        {
                            tree.Parent = current;
                        }

                        // 将子节点入队以继续处理
                        queue.Enqueue(child);
                    }
                }
            }

            yield return root;
        }
    }

    /// <summary>
    /// 平行集合转换成树形结构
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="source"></param>
    /// <param name="idSelector"></param>
    /// <param name="pidSelector"></param>
    /// <param name="topValue">根对象parentId的值</param>
    /// <returns></returns>
    public static List<Tree<T>> ToTreeGeneral<T, TKey>(this IEnumerable<T> source, Expression<Func<T, TKey>> idSelector, Expression<Func<T, TKey>> pidSelector, TKey topValue = default) where TKey : IComparable => idSelector.Body.ToString() != pidSelector.Body.ToString() ? BuildTreeGeneral(source.Where(t => t != null).ToList(), idSelector.Compile(), pidSelector.Compile(), topValue).ToList() : throw new ArgumentException("idSelector和pidSelector不应该为同一字段！");

    private static IEnumerable<Tree<T>> BuildTreeGeneral<T, TKey>(List<T> list, Func<T, TKey> idSelector, Func<T, TKey> pidSelector, TKey parent) where TKey : IComparable
    {
        // 创建一个字典，用于快速查找节点的子节点
        var lookup = new Dictionary<TKey, List<Tree<T>>>();
        foreach (var item in list.Where(item => !lookup.ContainsKey(idSelector(item))))
        {
            lookup[idSelector(item)] = [];
        }

        // 构建树结构
        foreach (var item in list.Where(item => !Equals(pidSelector(item), default(TKey)) && lookup.ContainsKey(pidSelector(item))))
        {
            lookup[pidSelector(item)].Add(new Tree<T>(item));
        }

        // 找到根节点，即没有父节点的节点
        foreach (var root in list.Where(x => Equals(pidSelector(x), parent)))
        {
            // 为根节点和所有子节点设置Children属性
            var queue = new Queue<Tree<T>>();
            var item = new Tree<T>(root);
            queue.Enqueue(item);
            while (queue.Count > 0)
            {
                // 出队当前节点
                var current = queue.Dequeue();

                // 为当前节点设置子节点
                if (lookup.TryGetValue(idSelector(current.Value), out var children))
                {
                    current.Children = children;
                    foreach (var child in children)
                    {
                        child.Parent = current.Parent;
                        // 将子节点入队以继续处理
                        queue.Enqueue(child);
                    }
                }
            }

            yield return item;
        }
    }

    /// <summary>
    /// 所有子级
    /// </summary>
    public static ICollection<T> AllChildren<T>(this T tree) where T : ITreeChildren<T> => GetChildren(tree, c => c.Children);

    /// <summary>
    /// 所有子级
    /// </summary>
    public static ICollection<T> AllChildren<T>(this T tree, Func<T, IEnumerable<T>> selector) where T : ITreeChildren<T> => GetChildren(tree, selector);

    /// <summary>
    /// 所有子级
    /// </summary>
    public static ICollection<Tree<T>> AllChildren<T>(this Tree<T> tree) => GetChildren(tree, c => c.Children);

    /// <summary>
    /// 所有子级
    /// </summary>
    public static ICollection<Tree<T>> AllChildren<T>(this Tree<T> tree, Func<Tree<T>, IEnumerable<Tree<T>>> selector) => GetChildren(tree, selector);

    /// <summary>
    /// 所有父级
    /// </summary>
    public static List<T> AllParent<T>(this T tree) where T : class, ITreeParent<T> => GetParents(tree, c => c.Parent);

    /// <summary>
    /// 所有父级
    /// </summary>
    public static List<T> AllParent<T>(this T tree, Func<T, T> selector) where T : class, ITreeParent<T> => GetParents(tree, selector);

    /// <summary>
    /// 所有父级
    /// </summary>
    public static List<Tree<T>> AllParent<T>(this Tree<T> tree, Func<Tree<T>, Tree<T>> selector) => GetParents(tree, selector);

    /// <summary>
    /// 是否是根节点
    /// </summary>
    public static bool IsRoot<T>(this ITreeParent<T> tree) where T : ITreeParent<T> => tree.Parent == null;

    /// <summary>
    /// 是否是叶子节点
    /// </summary>
    public static bool IsLeaf<T>(this ITreeChildren<T> tree) where T : ITreeChildren<T> => tree.Children?.Count == 0;

    /// <summary>
    /// 是否是根节点
    /// </summary>
    public static bool IsRoot<T>(this Tree<T> tree) => tree.Parent == null;

    /// <summary>
    /// 是否是叶子节点
    /// </summary>
    public static bool IsLeaf<T>(this Tree<T> tree) => tree.Children?.Count == 0;

    /// <summary>
    /// 深度层级
    /// </summary>
    public static int Level<T>(this T tree) where T : class, ITreeParent<T>
    {
        if (tree == null)
            throw new ArgumentNullException(nameof(tree), "当前节点不能为null");

        // 使用一个队列来存储待处理的节点
        var queue = new Queue<T>();
        queue.Enqueue(tree);
        int level = 1;

        // 循环直到队列为空
        while (queue.Count > 0)
        {
            int nodeCount = queue.Count; // 当前层级的节点数

            for (int i = 0; i < nodeCount; i++)
            {
                // 从队列中出队一个节点
                T currentNode = queue.Dequeue();

                // 如果当前节点是根节点，则返回当前层级
                if (currentNode.Parent == null)
                    return level;

                // 将当前节点的父节点入队
                if (currentNode.Parent != null)
                    queue.Enqueue(currentNode.Parent);
            }

            // 完成当前层级的遍历，准备进入下一层级
            level++;
        }

        // 如果执行到这里，说明没有找到根节点
        throw new InvalidOperationException("未找到根节点");
    }

    /// <summary>
    /// 节点路径（UNIX路径格式，以“/”分隔）
    /// </summary>
    public static string Path<T>(this T tree) where T : class, ITree<T> => GetFullPath(tree, t => t.Name);

    /// <summary>
    /// 节点路径（UNIX路径格式，以“/”分隔）
    /// </summary>
    public static string Path<T>(this T tree, Func<T, string> selector) where T : class, ITreeParent<T> => GetFullPath(tree, selector);

    /// <summary>
    /// 节点路径
    /// </summary>
    /// <param name="tree"></param>
    /// <param name="separator">分隔符</param>
    public static string Path<T>(this T tree, string separator) where T : class, ITree<T> => GetFullPath(tree, t => t.Name, separator);

    /// <summary>
    /// 节点路径
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tree"></param>
    /// <param name="selector">选择字段</param>
    /// <param name="separator">分隔符</param>
    /// <returns></returns>
    public static string Path<T>(this T tree, Func<T, string> selector, string separator) where T : class, ITreeParent<T> => GetFullPath(tree, selector, separator);

    /// <summary>
    /// 根节点
    /// </summary>
    public static T Root<T>(this T tree) where T : class, ITreeParent<T> => GetRoot(tree, t => t.Parent);

    private static string GetFullPath<T>(T c, Func<T, string> selector, string separator = "/") where T : class, ITreeParent<T>
    {
        if (c == null || selector == null)
        {
            throw new ArgumentNullException(c == null ? nameof(c) : nameof(selector), "当前节点或选择器不能为null");
        }

        // 使用一个栈来存储节点，栈将逆序存储路径中的节点
        var stack = new Stack<T>();
        var currentNode = c;
        while (currentNode != null)
        {
            stack.Push(currentNode);
            currentNode = currentNode.Parent;
        }

        // 构建路径字符串
        return stack.Select(selector).Join(separator);
    }

    /// <summary>
    /// 根节点
    /// </summary>
    public static T GetRoot<T>(T c, Func<T, T> selector) where T : class, ITreeParent<T>
    {
        if (c == null || selector == null)
        {
            throw new ArgumentNullException(c == null ? nameof(c) : nameof(selector), "当前节点或父级选择器不能为null");
        }

        // 使用一个集合来存储已访问的节点，以避免无限循环
        var visited = new HashSet<T>();
        var currentNode = c;

        // 向上遍历直到找到根节点
        while (true)
        {
            // 如果当前节点已被访问，说明存在循环引用，抛出异常
            if (!visited.Add(currentNode))
                throw new InvalidOperationException("节点存在循环引用");

            var parent = selector(currentNode);
            if (parent == null)
            {
                return currentNode; // 找到了根节点
            }

            currentNode = parent;
        }
    }

    /// <summary>
    /// 递归取出所有下级
    /// </summary>
    /// <param name="t"></param>
    /// <param name="selector"></param>
    /// <returns></returns>
    private static List<T> GetChildren<T>(T t, Func<T, IEnumerable<T>> selector)
    {
        if (t == null || selector == null)
            return []; // 如果t或selector为null，则返回空列表

        var list = new List<T>();
        var queue = new Queue<T>();
        queue.Enqueue(t);
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            list.Add(current); // 将当前节点添加到结果列表
            var children = selector(current) ?? new List<T>();
            foreach (var child in children.Where(child => selector(child).Any()))
            {
                queue.Enqueue(child);
            }
        }

        return list;
    }

    /// <summary>
    /// 递归取出所有上级
    /// </summary>
    /// <param name="t"></param>
    /// <param name="selector"></param>
    /// <returns></returns>
    private static List<T> GetParents<T>(T t, Func<T, T> selector) where T : class
    {
        if (t == null || selector == null)
            return []; // 如果t或selector为null，则返回空列表

        var parents = new List<T>();
        var current = t;
        while (current != null)
        {
            // 添加当前节点到父节点列表
            parents.Add(current);

            // 通过selector函数获取当前节点的父节点
            current = selector(current);

            // 如果selector返回了已经遍历过的节点，则停止遍历以避免无限循环
            if (parents.Contains(current))
            {
                break;
            }
        }

        // 由于是逆序添加的，所以需要反转列表
        parents.Reverse();
        return parents;
    }
}

/// <summary>
/// 树形数据扩展 long
/// </summary>
public static class TreeExtensionLong
{
    /// <summary>
    /// 平行集合转换成树形结构
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static List<T> ToTree<T>(this IEnumerable<T> source) where T : class, ITreeEntity<T, long> => source.ToTree<T, long>();
}

/// <summary>
/// 树形数据扩展 guid
/// </summary>
public static class TreeExtensionGuid
{
    /// <summary>
    /// 平行集合转换成树形结构
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static List<T> ToTree<T>(this IEnumerable<T> source) where T : class, ITreeEntity<T, Guid> => source.ToTree<T, Guid>();
}

/// <summary>
/// 树形数据扩展 string
/// </summary>
public static class TreeExtensionString
{
    /// <summary>
    /// 平行集合转换成树形结构
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="topValue"></param>
    /// <returns></returns>
    public static List<T> ToTree<T>(this IEnumerable<T> source, string topValue) where T : class, ITreeEntity<T> => TreeExtensions.BuildTree(source.Enumerable2NonNullList(), topValue).ToList();

    /// <summary>
    /// 平行集合转换成树形结构
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static List<T> ToTree<T>(this IEnumerable<T> source) where T : class, ITreeEntity<T> => source.Enumerable2NonNullList().Where(t => string.IsNullOrEmpty(t.ParentId)).SelectMany(parent => TreeExtensions.BuildTree(source, parent)).ToList();
}

internal static class TreeExtensionCommon
{
    /// <summary>
    /// 将IEnumerable转换成不含null的List,若T为IQueryable，则将数据读取到内存
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static List<T> Enumerable2NonNullList<T>(this IEnumerable<T> source)
    {
        source = source is IQueryable<T> queryable ? queryable.AsEnumerable() : source;
        return (typeof(T).IsValueType ? source : source.Where(t => t != null)).ToList();
    }
}