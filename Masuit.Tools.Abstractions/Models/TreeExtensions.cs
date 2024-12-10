using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Masuit.Tools.Systems;

namespace Masuit.Tools.Models;

/// <summary>
/// 树形数据扩展
/// </summary>
public static class TreeExtensions
{
    #region Filter 过滤

    /// <summary>
    /// 过滤
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static IEnumerable<T> Filter<T>(this IEnumerable<T> items, Func<T, bool> func) where T : class, ITreeChildren<T>
    {
        return Flatten(items).Where(func);
    }

    /// <summary>
    /// 过滤
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static IEnumerable<Tree<T>> Filter<T>(this IEnumerable<Tree<T>> items, Func<Tree<T>, bool> func) where T : class
    {
        return Flatten(items).Where(func);
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
        return Flatten(item).Where(func);
    }

    /// <summary>
    /// 过滤
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static IEnumerable<Tree<T>> Filter<T>(this Tree<T> item, Func<Tree<T>, bool> func) where T : class
    {
        return Flatten(item).Where(func);
    }

    #endregion Filter 过滤

    #region Flatten 平铺开任意树形结构数据

    /// <summary>
    /// 平铺开任意树形结构数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="selector"></param>
    /// <param name="optionAction">平铺时子级需要做的操作，参数1：子级对象，参数2：父级对象</param>
    /// <returns></returns>
    public static IEnumerable<T> Flatten<T>(this IEnumerable<T> items, Func<T, IEnumerable<T>> selector, Action<T, T> optionAction = null)
    {
        if(items == null)
        {
            yield break;
        }

        // 使用一个队列来存储待处理的节点
        Queue<T> queue = new();
        // 首先将所有项加入队列
        foreach(T item in items)
        {
            queue.Enqueue(item);
        }

        // 循环直到队列为空
        while(queue.Count > 0)
        {
            // 从队列中取出当前节点
            T currentItem = queue.Dequeue();

            // 将当前节点返回
            yield return currentItem;

            // 获取当前节点的所有子节点
            IEnumerable<T> children = selector(currentItem) ?? new List<T>();

            // 将所有子节点加入队列
            foreach(T child in children)
            {
                // 执行平铺时的操作（如果有的话）
                optionAction?.Invoke(child, currentItem);
                queue.Enqueue(child);
            }
        }
    }

    /// <summary>
    /// 平铺开任意树形结构数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="optionAction">平铺时子级需要做的操作，参数1：子级对象，参数2：父级对象</param>
    /// <returns></returns>
    public static IEnumerable<T> Flatten<T>(this IEnumerable<T> items, Action<T, T> optionAction = null) where T : class, ITreeChildren<T>
    {
        return TreeExtensions.Flatten<T>(
            items: items,
            selector: t => t.Children,
            optionAction: optionAction);
    }

    /// <summary>
    /// 平铺开任意树形结构数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="optionAction">平铺时子级需要做的操作，参数1：子级对象，参数2：父级对象</param>
    /// <returns></returns>
    public static IEnumerable<Tree<T>> Flatten<T>(this IEnumerable<Tree<T>> items, Action<Tree<T>, Tree<T>> optionAction = null) where T : class
    {
        return TreeExtensions.Flatten<Tree<T>>(
            items: items,
            selector: t => t.Children,
            optionAction: optionAction);
    }

    /// <summary>
    /// 平铺开任意树形结构数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="selector"></param>
    /// <param name="optionAction">平铺时子级需要做的操作，参数1：子级对象，参数2：父级对象</param>
    /// <returns></returns>
    public static IEnumerable<Tree<T>> Flatten<T>(this IEnumerable<Tree<T>> items, Func<Tree<T>, IEnumerable<Tree<T>>> selector, Action<Tree<T>, Tree<T>> optionAction = null)
    {
        return TreeExtensions.Flatten<Tree<T>>(items, selector, optionAction);
    }

    /// <summary>
    /// 平铺开任意树形结构数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="p"></param>
    /// <param name="selector"></param>
    /// <param name="optionAction">平铺时子级需要做的操作，参数1：子级对象，参数2：父级对象</param>
    /// <returns></returns>
    public static IEnumerable<T> Flatten<T>(this T p, Func<T, IEnumerable<T>> selector, Action<T, T> optionAction = null) where T : class, ITreeChildren<T>
    {
        if(p == null)
        {
            yield break;
        }

        // 使用一个队列来存储待处理的节点
        Queue<T> queue = new();
        queue.Enqueue(p);

        while(queue.Count > 0)
        {
            // 从队列中出队一个节点
            T currentItem = queue.Dequeue();

            // 将当前节点返回
            yield return currentItem;

            // 获取当前节点的子节点，如果子节点列表不存在，则初始化为空列表
            IEnumerable<T> children = selector(currentItem) ?? new List<T>();

            // 将所有子节点入队
            foreach(T child in children)
            {
                // 执行平铺时的操作（如果有的话）
                optionAction?.Invoke(child, currentItem);
                queue.Enqueue(child);
            }
        }
    }

    /// <summary>
    /// 平铺开任意树形结构数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="p"></param>
    ///  <param name="optionAction">平铺时子级需要做的操作，参数1：子级对象，参数2：父级对象</param>
    /// <returns></returns>
    public static IEnumerable<T> Flatten<T>(this T p, Action<T, T> optionAction = null) where T : class, ITreeChildren<T>
    {
        return TreeExtensions.Flatten<T>(
            p: p,
            selector: t => t.Children,
            optionAction: optionAction);
    }

    /// <summary>
    /// 平铺开任意树形结构数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="p"></param>
    /// <param name="optionAction">平铺时子级需要做的操作，参数1：子级对象，参数2：父级对象</param>
    /// <returns></returns>
    public static IEnumerable<Tree<T>> Flatten<T>(this Tree<T> p, Action<Tree<T>, Tree<T>> optionAction = null) where T : class
    {
        return TreeExtensions.Flatten<Tree<T>>(
            p: p,
            selector: t => t.Children,
            optionAction: optionAction);
    }

    /// <summary>
    /// 平铺开任意树形结构数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="p"></param>
    /// <param name="selector"></param>
    /// <param name="optionAction">平铺时子级需要做的操作，参数1：子级对象，参数2：父级对象</param>
    /// <returns></returns>
    public static IEnumerable<Tree<T>> Flatten<T>(this Tree<T> p, Func<Tree<T>, IEnumerable<Tree<T>>> selector, Action<Tree<T>, Tree<T>> optionAction = null)
    {
        return TreeExtensions.Flatten<Tree<T>>(
            p: p,
            selector: selector,
            optionAction: optionAction);
    }

    #endregion Flatten 平铺开任意树形结构数据

    #region ToTree 平行集合转换成树形结构

    /// <summary>
    /// 平行集合转换成树形结构
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="source"></param>
    /// <param name="topValue"></param>
    /// <returns></returns>
    public static List<T> ToTree<T, TKey>(
        this IEnumerable<T> source,
        TKey? topValue = null)
        where T : class, ITreeEntity<T, TKey>
        where TKey : struct, IComparable
    {
        if(source is IQueryable<T> queryable)
        {
            source = [.. queryable];
        }

        List<T> list = source.Where(t => t != null).ToList();
        return TreeExtensions.BuildTree(
            source: list,
            idSelector: t => t.Id,
            pidSelector: t => t.ParentId,
            topValue: topValue)
            .ToList();
    }

    /// <summary>
    /// 平行集合转换成树形结构
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static List<T> ToTree<T>(this IEnumerable<T> source) where T : class, ITreeEntity<T, int>
    {
        return TreeExtensions.ToTree<T, int>(source);
    }

    /// <summary>
    /// 平行集合转换成树形结构
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="topValue"></param>
    /// <returns></returns>
    public static List<T> ToTree<T>(this IEnumerable<T> source, string topValue) where T : class, ITreeEntity<T>
    {
        if(source is IQueryable<T> queryable)
        {
            source = queryable.ToList();
        }

        source = source.Where(t => t != null).ToList();

        return TreeExtensions.BuildTree(
            source: source,
            idSelector: t => t.Id,
            pidSelector: t => t.ParentId,
            topValue: topValue)
            .ToList();
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
    public static List<T> ToTree<T, TKey>(
        this IEnumerable<T> source,
        Expression<Func<T, TKey>> idSelector,
        Expression<Func<T, TKey>> pidSelector,
        TKey topValue = default)
        where T : ITreeParent<T>, ITreeChildren<T>
        where TKey : IComparable
    {
        if(source is IQueryable<T> queryable)
        {
            source = [.. queryable];
        }

        if(idSelector.Body.ToString() == pidSelector.Body.ToString())
        {
            throw new ArgumentException("idSelector和pidSelector不应该为同一字段！");
        }

        Func<T, TKey> pidFunc = pidSelector.Compile();
        Func<T, TKey> idFunc = idSelector.Compile();
        source = source.Where(t => t != null).ToList();
        return TreeExtensions.BuildTree(source, idFunc, pidFunc, topValue).ToList();
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
    public static List<T> ToTree<T, TKey>(
        this IEnumerable<T> source,
        Expression<Func<T, TKey>> idSelector,
        Expression<Func<T, TKey?>> pidSelector,
        TKey? topValue = default)
        where T : ITreeChildren<T>
        where TKey : struct
    {
        return TreeExtensions.ToTree<T, TKey>(source, idSelector, pidSelector, topValue);
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
    public static List<T> ToTree<T>(
        this IEnumerable<T> source,
        Expression<Func<T, string>> idSelector,
        Expression<Func<T, string>> pidSelector,
        string topValue = default)
        where T : ITreeParent<T>, ITreeChildren<T>
    {
        return TreeExtensions.ToTree<T, string>(source, idSelector, pidSelector, topValue);
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
    public static List<T> ToTree<T>(
        this IEnumerable<T> source,
        Expression<Func<T, int>> idSelector,
        Expression<Func<T, int>> pidSelector,
        int topValue = 0)
        where T : ITreeParent<T>, ITreeChildren<T>
    {
        return TreeExtensions.ToTree<T, int>(source, idSelector, pidSelector, topValue);
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
    public static List<T> ToTree<T>(
        this IEnumerable<T> source,
        Expression<Func<T, long>> idSelector,
        Expression<Func<T, long>> pidSelector,
        long topValue = 0)
        where T : ITreeParent<T>, ITreeChildren<T>
    {
        return TreeExtensions.ToTree<T, long>(source, idSelector, pidSelector, topValue);
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
    public static List<T> ToTree<T>(
        this IEnumerable<T> source,
        Expression<Func<T, Guid>> idSelector,
        Expression<Func<T, Guid>> pidSelector,
        Guid topValue = default)
        where T : ITreeParent<T>, ITreeChildren<T>
    {
        return TreeExtensions.ToTree<T, Guid>(source, idSelector, pidSelector, topValue);
    }

    internal static IEnumerable<T> BuildTree<T, TKey>(
        IEnumerable<T> source,
        Func<T, TKey> idSelector,
        Func<T, TKey> pidSelector,
        TKey topValue = default)
        where T : ITreeChildren<T>
        where TKey : IComparable
    {
        return TreeExtensions.BuildTreeBase<T, TKey>(
            dictCreator: () => new Dictionary<TKey, List<T>>(),
            pidSelector: pidSelector,
            source: source,
            idSelector: idSelector,
            topValue: topValue
            );
    }

    internal static IEnumerable<T> BuildTree<T, TKey>(
        IEnumerable<T> source,
        Func<T, TKey> idSelector,
        Func<T, TKey?> pidSelector,
        TKey? topValue = default)
        where T : ITreeChildren<T>
        where TKey : struct
    {
        Func<T, NullObject<TKey>> tempPidSelector = t => new NullObject<TKey>(pidSelector(t) ?? default);
        Func<T, NullObject<TKey>> tempIdSelector = t => new NullObject<TKey>(idSelector(t));
        NullObject<TKey> tempTopValue = new(topValue.HasValue ? topValue.Value : default);

        return TreeExtensions.BuildTreeBase<T, NullObject<TKey>>(
            dictCreator: () => new NullableDictionary<TKey, List<T>>(),
            pidSelector: tempPidSelector,
            source: source,
            idSelector: tempIdSelector,
            topValue: tempTopValue
            );
    }

    private static IEnumerable<T> BuildTreeBase<T, TKey>(
        Func<IDictionary<TKey, List<T>>> dictCreator,
        Func<T, TKey> pidSelector,

        IEnumerable<T> source,
        Func<T, TKey> idSelector,
        //Func<T, TKey> pidSelector,
        TKey topValue = default)
        where T : ITreeChildren<T>
    {
        // 创建一个字典，用于快速查找节点的子节点
        IDictionary<TKey, List<T>> childrenLookup = dictCreator?.Invoke() ?? throw new ArgumentNullException(nameof(dictCreator));
        ICollection<T> list = source as ICollection<T> ?? source.ToList();
        foreach(T item in list.Where(item => !childrenLookup.ContainsKey(idSelector(item))))
        {
            childrenLookup[idSelector(item)] = new List<T>();
        }

        // 构建树结构
        foreach(T item in list.Where(item => !Equals(pidSelector(item), default(TKey)) && childrenLookup.ContainsKey(pidSelector(item))))
        {
            childrenLookup[pidSelector(item)].Add(item);
        }

        // 找到根节点，即没有父节点的节点
        foreach(T root in list.Where(x => Equals(pidSelector(x), topValue)))
        {
            // 为根节点和所有子节点设置Children属性
            // 使用队列来模拟递归过程
            Queue<T> queue = new();
            queue.Enqueue(root);
            while(queue.Count > 0)
            {
                // 出队当前节点
                T current = queue.Dequeue();

                // 为当前节点设置子节点
                if(childrenLookup.TryGetValue(idSelector(current), out List<T> children))
                {
                    current.Children = children;
                    foreach(T child in children)
                    {
                        // 如果子节点实现了ITreeParent接口，则设置其Parent属性
                        if(child is ITreeParent<T> tree)
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
    public static List<Tree<T>> ToTreeGeneral<T, TKey>(
        this IEnumerable<T> source,
        Expression<Func<T, TKey>> idSelector,
        Expression<Func<T, TKey>> pidSelector,
        TKey topValue = default)
        where TKey : IComparable
    {
        if(idSelector.Body.ToString() == pidSelector.Body.ToString())
        {
            throw new ArgumentException("idSelector和pidSelector不应该为同一字段！");
        }

        Func<Tree<T>, TKey> pidFunc = t => pidSelector.Compile()(t.Value);
        Func<Tree<T>, TKey> idFunc = t => idSelector.Compile()(t.Value);
        List<Tree<T>> list = source.Where(t => t != null).Select(t => new Tree<T>(t)).ToList();

        return TreeExtensions.BuildTreeBase<Tree<T>, TKey>(
            dictCreator: () => new Dictionary<TKey, List<Tree<T>>>(),
            pidSelector: pidFunc,
            source: list,
            idSelector: idFunc,
            topValue: topValue
            )
            .ToList();
    }

    #endregion ToTree 平行集合转换成树形结构

    #region 获取子级

    /// <summary>
    /// 递归取出所有下级
    /// </summary>
    /// <param name="t"></param>
    /// <param name="selector"></param>
    /// <returns></returns>
    private static List<T> GetChildren<T>(T t, Func<T, IEnumerable<T>> selector)
    {
        if(t == null || selector == null)
        {
            return new List<T>(); // 如果t或selector为null，则返回空列表
        }

        List<T> list = new();
        Queue<T> queue = new();
        queue.Enqueue(t);
        while(queue.Count > 0)
        {
            T current = queue.Dequeue();
            list.Add(current); // 将当前节点添加到结果列表
            IEnumerable<T> children = selector(current) ?? new List<T>();
            foreach(T child in children)
            {
                // 只有当子节点还有自己的子节点时，才将其加入队列
                if(selector(child).Any())
                {
                    queue.Enqueue(child);
                }
            }
        }

        return list;
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

    #endregion 获取子级

    #region 获取父级

    /// <summary>
    /// 递归取出所有上级
    /// </summary>
    /// <param name="t"></param>
    /// <param name="selector"></param>
    /// <returns></returns>
    private static List<T> GetParents<T>(T t, Func<T, T> selector) where T : class
    {
        if(t == null || selector == null)
        {
            return new List<T>(); // 如果t或selector为null，则返回空列表
        }

        List<T> parents = new();
        T current = t;
        while(current != null)
        {
            // 添加当前节点到父节点列表
            parents.Add(current);

            // 通过selector函数获取当前节点的父节点
            current = selector(current);

            // 如果selector返回了已经遍历过的节点，则停止遍历以避免无限循环
            if(parents.Contains(current))
            {
                break;
            }
        }

        // 由于是逆序添加的，所以需要反转列表
        parents.Reverse();
        return parents;
    }

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

    #endregion 获取父级

    #region 是否是根节点/叶子节点

    /// <summary>
    /// 是否是根节点
    /// </summary>
    public static bool IsRoot<T>(this ITreeParent<T> tree) where T : ITreeParent<T> => tree.Parent == null;

    /// <summary>
    /// 是否是根节点
    /// </summary>
    public static bool IsRoot<T>(this Tree<T> tree) => tree.Parent == null;

    /// <summary>
    /// 是否是叶子节点
    /// </summary>
    public static bool IsLeaf<T>(this ITreeChildren<T> tree) where T : ITreeChildren<T> => tree.Children?.Count == 0;

    /// <summary>
    /// 是否是叶子节点
    /// </summary>
    public static bool IsLeaf<T>(this Tree<T> tree) => tree.Children?.Count == 0;

    #endregion 是否是根节点/叶子节点

    #region 层级/节点路径/获取根节点

    /// <summary>
    /// 深度层级
    /// </summary>
    public static int Level<T>(this T tree) where T : class, ITreeParent<T>
    {
        if(tree == null)
        {
            throw new ArgumentNullException(nameof(tree), "当前节点不能为null");
        }

        // 使用一个队列来存储待处理的节点
        Queue<T> queue = new();
        queue.Enqueue(tree);
        int level = 1;

        // 循环直到队列为空
        while(queue.Count > 0)
        {
            int nodeCount = queue.Count; // 当前层级的节点数

            for(int i = 0 ; i < nodeCount ; i++)
            {
                // 从队列中出队一个节点
                T currentNode = queue.Dequeue();

                // 如果当前节点是根节点，则返回当前层级
                if(currentNode.Parent == null)
                {
                    return level;
                }

                // 将当前节点的父节点入队
                if(currentNode.Parent != null)
                {
                    queue.Enqueue(currentNode.Parent);
                }
            }

            // 完成当前层级的遍历，准备进入下一层级
            level++;
        }

        // 如果执行到这里，说明没有找到根节点
        throw new InvalidOperationException("未找到根节点");
    }

    private static string GetFullPath<T>(T c, Func<T, string> selector, string separator = "/") where T : class, ITreeParent<T>
    {
        if(c == null || selector == null)
        {
            throw new ArgumentNullException(c == null ? nameof(c) : nameof(selector), "当前节点或选择器不能为null");
        }

        // 使用一个栈来存储节点，栈将逆序存储路径中的节点
        Stack<T> stack = new();
        T currentNode = c;
        while(currentNode != null)
        {
            stack.Push(currentNode);
            currentNode = currentNode.Parent;
        }

        // 构建路径字符串
        return stack.Select(selector).Join(separator);
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
    public static T GetRoot<T>(T c, Func<T, T> selector) where T : class, ITreeParent<T>
    {
        if(c == null || selector == null)
        {
            throw new ArgumentNullException(c == null ? nameof(c) : nameof(selector), "当前节点或父级选择器不能为null");
        }

        // 使用一个集合来存储已访问的节点，以避免无限循环
        HashSet<T> visited = new();
        T currentNode = c;

        // 向上遍历直到找到根节点
        while(currentNode != null)
        {
            // 如果当前节点已被访问，说明存在循环引用，抛出异常
            if(!visited.Add(currentNode))
            {
                throw new InvalidOperationException("节点存在循环引用");
            }

            T parent = selector(currentNode);
            if(parent == null)
            {
                // 找到了根节点
                return currentNode;
            }

            currentNode = parent;
        }

        // 如果currentNode为null，说明最初的节点c就是根节点
        return c;
    }

    /// <summary>
    /// 根节点
    /// </summary>
    public static T Root<T>(this T tree) where T : class, ITreeParent<T> => GetRoot(tree, t => t.Parent);

    #endregion 层级/节点路径/获取根节点
}

public static class TreeExtensionLong
{
    /// <summary>
    /// 平行集合转换成树形结构
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static List<T> ToTree<T>(this IEnumerable<T> source) where T : class, ITreeEntity<T, long>
    {
        return source.ToTree<T, long>();
    }
}

public static class TreeExtensionGuid
{
    /// <summary>
    /// 平行集合转换成树形结构
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static List<T> ToTree<T>(this IEnumerable<T> source) where T : class, ITreeEntity<T, Guid>
    {
        return source.ToTree<T, Guid>();
    }
}

public static class TreeExtensionString
{
    /// <summary>
    /// 平行集合转换成树形结构
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static List<T> ToTree<T>(this IEnumerable<T> source) where T : class, ITreeEntity<T>
    {
        if(source is IQueryable<T> queryable)
        {
            source = queryable.ToList();
        }

        source = source.Where(t => t != null).ToList();
        return source
            .Where(t => string.IsNullOrEmpty(t.ParentId))
            .SelectMany(parent => TreeExtensions.BuildTree<T, string>(source, t => t.Id, t => t.ParentId, parent.Id))
            .ToList();
    }
}