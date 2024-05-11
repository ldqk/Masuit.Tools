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

    /// <summary>
    /// 平铺开
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="optionAction">平铺时子级需要做的操作，参数1：子级对象，参数2：父级对象</param>
    /// <returns></returns>
    public static IEnumerable<T> Flatten<T>(this IEnumerable<T> items, Action<T, T> optionAction = null) where T : class, ITreeChildren<T>
    {
        if (items == null)
        {
            yield break;
        }

        // 使用一个栈来存储待处理的节点
        var stack = new Stack<T>();
        // 首先将所有项压入栈中
        foreach (var item in items)
        {
            stack.Push(item);
        }

        // 循环直到栈为空
        while (stack.Count > 0)
        {
            // 弹出栈顶的节点
            var currentItem = stack.Pop();
            yield return currentItem;

            // 为当前节点设置子节点，如果optionAction不为空，则对每个子节点执行操作
            var children = currentItem.Children ?? new List<T>();
            foreach (var child in children)
            {
                optionAction?.Invoke(child, currentItem);
                stack.Push(child); // 将子节点压入栈中
            }
        }
    }

    /// <summary>
    /// 平铺开
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="p"></param>
    /// <param name="optionAction">平铺时子级需要做的操作，参数1：子级对象，参数2：父级对象</param>
    /// <returns></returns>
    public static IEnumerable<T> Flatten<T>(this T p, Action<T, T> optionAction = null) where T : class, ITreeChildren<T>
    {
        if (p == null)
        {
            yield break;
        }

        // 使用一个队列来存储待处理的节点
        var queue = new Queue<T>();
        queue.Enqueue(p);

        while (queue.Count > 0)
        {
            // 从队列中出队一个节点
            var currentItem = queue.Dequeue();

            // 将当前节点返回
            yield return currentItem;

            // 获取当前节点的子节点，如果子节点列表不存在，则初始化为空列表
            var children = currentItem.Children ?? new List<T>();

            // 将所有子节点入队
            foreach (var child in children)
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
    /// <param name="selector"></param>
    /// <param name="optionAction">平铺时子级需要做的操作，参数1：子级对象，参数2：父级对象</param>
    /// <returns></returns>
    public static IEnumerable<T> Flatten<T>(this IEnumerable<T> items, Func<T, IEnumerable<T>> selector, Action<T, T> optionAction = null)
    {
        if (items == null)
        {
            yield break;
        }

        // 使用一个队列来存储待处理的节点
        var queue = new Queue<T>();
        // 首先将所有项加入队列
        foreach (var item in items)
        {
            queue.Enqueue(item);
        }

        // 循环直到队列为空
        while (queue.Count > 0)
        {
            // 从队列中取出当前节点
            var currentItem = queue.Dequeue();

            // 将当前节点返回
            yield return currentItem;

            // 获取当前节点的所有子节点
            var children = selector(currentItem) ?? new List<T>();

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
    public static IEnumerable<Tree<T>> Flatten<T>(this IEnumerable<Tree<T>> items, Action<Tree<T>, Tree<T>> optionAction = null) where T : class
    {
        if (items == null)
        {
            yield break;
        }

        // 使用一个队列来存储待处理的节点
        var queue = new Queue<Tree<T>>();
        // 首先将所有项加入队列
        foreach (var item in items)
        {
            queue.Enqueue(item);
        }

        // 循环直到队列为空
        while (queue.Count > 0)
        {
            // 从队列中取出当前节点
            var currentItem = queue.Dequeue();

            // 将当前节点返回
            yield return currentItem;

            // 获取当前节点的所有子节点，如果子节点列表不存在，则初始化为空列表
            var children = currentItem.Children ?? new List<Tree<T>>();

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
    /// <param name="p"></param>
    /// <param name="optionAction">平铺时子级需要做的操作，参数1：子级对象，参数2：父级对象</param>
    /// <returns></returns>
    public static IEnumerable<Tree<T>> Flatten<T>(this Tree<T> p, Action<Tree<T>, Tree<T>> optionAction = null) where T : class
    {
        if (p == null)
        {
            yield break;
        }

        // 使用一个队列来存储待处理的节点
        var queue = new Queue<Tree<T>>();
        queue.Enqueue(p);

        // 遍历队列直到它为空
        while (queue.Count > 0)
        {
            // 从队列中取出当前节点
            var currentItem = queue.Dequeue();

            // 将当前节点返回
            yield return currentItem;

            // 获取当前节点的子节点列表，如果为空则初始化为空列表
            var children = currentItem.Children ?? new List<Tree<T>>();

            // 将所有子节点添加到队列中
            foreach (var child in children)
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
    /// <param name="selector"></param>
    /// <param name="optionAction">平铺时子级需要做的操作，参数1：子级对象，参数2：父级对象</param>
    /// <returns></returns>
    public static IEnumerable<Tree<T>> Flatten<T>(this IEnumerable<Tree<T>> items, Func<Tree<T>, IEnumerable<Tree<T>>> selector, Action<Tree<T>, Tree<T>> optionAction = null)
    {
        if (items == null)
        {
            yield break;
        }

        // 使用一个队列来存储待处理的节点
        var queue = new Queue<Tree<T>>();
        // 首先将所有项加入队列
        foreach (var item in items)
        {
            queue.Enqueue(item);
        }

        // 循环直到队列为空
        while (queue.Count > 0)
        {
            // 从队列中取出当前节点
            var currentItem = queue.Dequeue();

            // 将当前节点返回
            yield return currentItem;

            // 获取当前节点的所有子节点
            var children = selector(currentItem) ?? new List<Tree<T>>();

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
    /// 平行集合转换成树形结构
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="idSelector"></param>
    /// <param name="pidSelector"></param>
    /// <param name="topValue">根对象parentId的值</param>
    /// <returns></returns>
    public static List<T> ToTree<T>(this IEnumerable<T> source, Expression<Func<T, string>> idSelector, Expression<Func<T, string>> pidSelector, string topValue = default) where T : ITreeParent<T>, ITreeChildren<T>
    {
        return ToTree<T, string>(source, idSelector, pidSelector, topValue);
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
    public static List<T> ToTree<T>(this IEnumerable<T> source, Expression<Func<T, int>> idSelector, Expression<Func<T, int>> pidSelector, int topValue = 0) where T : ITreeParent<T>, ITreeChildren<T>
    {
        return ToTree<T, int>(source, idSelector, pidSelector, topValue);
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
    public static List<T> ToTree<T>(this IEnumerable<T> source, Expression<Func<T, long>> idSelector, Expression<Func<T, long>> pidSelector, long topValue = 0) where T : ITreeParent<T>, ITreeChildren<T>
    {
        return ToTree<T, long>(source, idSelector, pidSelector, topValue);
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
    public static List<T> ToTree<T>(this IEnumerable<T> source, Expression<Func<T, Guid>> idSelector, Expression<Func<T, Guid>> pidSelector, Guid topValue = default) where T : ITreeParent<T>, ITreeChildren<T>
    {
        return ToTree<T, Guid>(source, idSelector, pidSelector, topValue);
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
    public static List<T> ToTree<T, TKey>(this IEnumerable<T> source, Expression<Func<T, TKey>> idSelector, Expression<Func<T, TKey>> pidSelector, TKey topValue = default) where T : ITreeParent<T>, ITreeChildren<T> where TKey : IComparable
    {
        if (source is IQueryable<T> queryable)
        {
            source = queryable.ToList();
        }

        if (idSelector.Body.ToString() == pidSelector.Body.ToString())
        {
            throw new ArgumentException("idSelector和pidSelector不应该为同一字段！");
        }

        var pidFunc = pidSelector.Compile();
        var idFunc = idSelector.Compile();
        return BuildTree(source.Where(t => t != null), idFunc, pidFunc, topValue).ToList();
    }

    /// <summary>
    /// 平行集合转换成树形结构
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static List<T> ToTree<T>(this IEnumerable<T> source) where T : class, ITreeEntity<T, int>
    {
        return ToTree<T, int>(source);
    }

    /// <summary>
    /// 平行集合转换成树形结构
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="source"></param>
    /// <param name="topValue"></param>
    /// <returns></returns>
    public static List<T> ToTree<T, TKey>(this IEnumerable<T> source, TKey? topValue = null) where T : class, ITreeEntity<T, TKey> where TKey : struct, IComparable
    {
        if (source is IQueryable<T> queryable)
        {
            source = queryable.ToList();
        }

        var list = source.Where(t => t != null).ToList();
        return BuildTree(list, topValue).ToList();
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
    public static List<T> ToTree<T, TKey>(this IEnumerable<T> source, Expression<Func<T, TKey>> idSelector, Expression<Func<T, TKey?>> pidSelector, TKey? topValue = default) where T : ITreeChildren<T> where TKey : struct
    {
        if (source is IQueryable<T> queryable)
        {
            source = queryable.ToList();
        }

        if (idSelector.Body.ToString() == pidSelector.Body.ToString())
        {
            throw new ArgumentException("idSelector和pidSelector不应该为同一字段！");
        }

        var pidFunc = pidSelector.Compile();
        var idFunc = idSelector.Compile();
        source = source.Where(t => t != null).ToList();
        return BuildTree(source, idFunc, pidFunc, topValue).ToList();
    }

    private static IEnumerable<T> BuildTree<T, TKey>(IEnumerable<T> source, Func<T, TKey> idSelector, Func<T, TKey> pidSelector, TKey topValue = default) where T : ITreeChildren<T> where TKey : IComparable
    {
        // 创建一个字典，用于快速查找节点的子节点
        var childrenLookup = new Dictionary<TKey, List<T>>();
        var list = source as ICollection<T> ?? source.ToList();
        foreach (var item in list.Where(item => !childrenLookup.ContainsKey(idSelector(item))))
        {
            childrenLookup[idSelector(item)] = new List<T>();
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

    internal static IEnumerable<T> BuildTree<T, TKey>(IEnumerable<T> source, TKey? topValue = default) where T : ITreeEntity<T, TKey> where TKey : struct, IComparable
    {
        // 创建一个字典，用于快速查找节点的子节点
        var childrenLookup = new NullableDictionary<TKey, List<T>>();
        var list = source as ICollection<T> ?? source.ToList();
        foreach (var item in list.Where(item => !childrenLookup.ContainsKey(item.Id)))
        {
            childrenLookup[item.Id] = new List<T>();
        }

        // 构建树结构
        foreach (var item in list.Where(item => (item.ParentId != null || !Equals(item.ParentId, default(TKey))) && childrenLookup.ContainsKey(item.ParentId ?? default)))
        {
            childrenLookup[item.ParentId ?? default].Add(item);
        }

        // 找到根节点，即没有父节点的节点
        foreach (var root in list.Where(x => Equals(x.ParentId, topValue)))
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

    internal static IEnumerable<T> BuildTree<T>(IEnumerable<T> source, string topValue = null) where T : ITreeEntity<T>
    {
        // 创建一个字典，用于快速查找节点的子节点
        var childrenLookup = new NullableDictionary<string, List<T>>();
        var list = source as ICollection<T> ?? source.ToList();
        foreach (var item in list.Where(item => !childrenLookup.ContainsKey(item.Id)))
        {
            childrenLookup[item.Id] = new List<T>();
        }

        // 构建树结构
        foreach (var item in list.Where(item => !string.IsNullOrEmpty(item.ParentId) && childrenLookup.ContainsKey(item.ParentId)))
        {
            childrenLookup[item.ParentId].Add(item);
        }

        // 找到根节点，即没有父节点的节点
        foreach (var root in list.Where(x => Equals(x.ParentId, topValue)))
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

    internal static IEnumerable<T> BuildTree<T>(IEnumerable<T> source, T parent) where T : ITreeEntity<T>
    {
        // 创建一个字典，用于快速查找节点的子节点
        var childrenLookup = new NullableDictionary<string, List<T>>();
        var list = source as ICollection<T> ?? source.ToList();
        foreach (var item in list.Where(item => !childrenLookup.ContainsKey(item.Id)))
        {
            childrenLookup[item.Id] = new List<T>();
        }

        // 构建树结构
        foreach (var item in list.Where(item => !string.IsNullOrEmpty(item.ParentId) && childrenLookup.ContainsKey(item.ParentId)))
        {
            childrenLookup[item.ParentId].Add(item);
        }

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

    private static IEnumerable<T> BuildTree<T, TKey>(IEnumerable<T> source, Func<T, TKey> idSelector, Func<T, TKey?> pidSelector, TKey? topValue = default) where T : ITreeChildren<T> where TKey : struct
    {
        // 创建一个字典，用于快速查找节点的子节点
        var childrenLookup = new NullableDictionary<TKey, List<T>>();
        var list = source as ICollection<T> ?? source.ToList();
        foreach (var item in list.Where(item => !childrenLookup.ContainsKey(idSelector(item))))
        {
            childrenLookup[idSelector(item)] = new List<T>();
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
    public static List<Tree<T>> ToTreeGeneral<T, TKey>(this IEnumerable<T> source, Expression<Func<T, TKey>> idSelector, Expression<Func<T, TKey>> pidSelector, TKey topValue = default) where TKey : IComparable
    {
        if (idSelector.Body.ToString() == pidSelector.Body.ToString())
        {
            throw new ArgumentException("idSelector和pidSelector不应该为同一字段！");
        }

        var pidFunc = pidSelector.Compile();
        var idFunc = idSelector.Compile();
        var list = source.Where(t => t != null).ToList();
        var temp = new List<Tree<T>>();
        foreach (var item in list.Where(item => pidFunc(item) is null || pidFunc(item).Equals(topValue)))
        {
            var parent = new Tree<T>(item);
            temp.AddRange(BuildTree(list, parent, idFunc, pidFunc));
        }

        return temp;
    }

    private static IEnumerable<Tree<T>> BuildTree<T, TKey>(List<T> list, Tree<T> parent, Func<T, TKey> idSelector, Func<T, TKey> pidSelector) where TKey : IComparable
    {
        // 创建一个字典，用于快速查找节点的子节点
        var childrenLookup = new Dictionary<TKey, List<Tree<T>>>();
        foreach (var item in list.Where(item => !childrenLookup.ContainsKey(idSelector(item))))
        {
            childrenLookup[idSelector(item)] = new List<Tree<T>>();
        }

        // 构建树结构
        foreach (var item in list.Where(item => !Equals(pidSelector(item), default(TKey)) && childrenLookup.ContainsKey(pidSelector(item))))
        {
            childrenLookup[pidSelector(item)].Add(new Tree<T>(item));
        }

        // 找到根节点，即没有父节点的节点
        foreach (var root in list.Where(x => Equals(pidSelector(x), idSelector(parent.Value))))
        {
            // 为根节点和所有子节点设置Children属性
            // 使用队列来模拟递归过程
            var queue = new Queue<Tree<T>>();
            queue.Enqueue(new Tree<T>(root));

            while (queue.Count > 0)
            {
                // 出队当前节点
                var current = queue.Dequeue();

                // 为当前节点设置子节点
                if (childrenLookup.TryGetValue(idSelector(current.Value), out var children))
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

            yield return new Tree<T>(root);
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
        {
            throw new ArgumentNullException(nameof(tree), "当前节点不能为null");
        }

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
                {
                    return level;
                }

                // 将当前节点的父节点入队
                if (currentNode.Parent != null)
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
        T currentNode = c;

        // 向上遍历直到找到根节点
        while (currentNode != null)
        {
            // 如果当前节点已被访问，说明存在循环引用，抛出异常
            if (!visited.Add(currentNode))
            {
                throw new InvalidOperationException("节点存在循环引用");
            }

            var parent = selector(currentNode);
            if (parent == null)
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
    /// 递归取出所有下级
    /// </summary>
    /// <param name="t"></param>
    /// <param name="selector"></param>
    /// <returns></returns>
    private static List<T> GetChildren<T>(T t, Func<T, IEnumerable<T>> selector)
    {
        if (t == null || selector == null)
        {
            return new List<T>(); // 如果t或selector为null，则返回空列表
        }

        var list = new List<T>();
        var queue = new Queue<T>();
        queue.Enqueue(t);
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            list.Add(current); // 将当前节点添加到结果列表
            var children = selector(current) ?? new List<T>();
            foreach (var child in children)
            {
                // 只有当子节点还有自己的子节点时，才将其加入队列
                if (selector(child).Any())
                {
                    queue.Enqueue(child);
                }
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
        {
            return new List<T>(); // 如果t或selector为null，则返回空列表
        }

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
    /// <param name="topValue"></param>
    /// <returns></returns>
    public static List<T> ToTree<T>(this IEnumerable<T> source, string topValue) where T : class, ITreeEntity<T>
    {
        if (source is IQueryable<T> queryable)
        {
            source = queryable.ToList();
        }

        source = source.Where(t => t != null).ToList();
        return TreeExtensions.BuildTree(source, topValue).ToList();
    }

    /// <summary>
    /// 平行集合转换成树形结构
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static List<T> ToTree<T>(this IEnumerable<T> source) where T : class, ITreeEntity<T>
    {
        if (source is IQueryable<T> queryable)
        {
            source = queryable.ToList();
        }

        source = source.Where(t => t != null).ToList();
        return source.Where(t => string.IsNullOrEmpty(t.ParentId)).SelectMany(parent => TreeExtensions.BuildTree(source, parent)).ToList();
    }
}