using System.Collections.Generic;

namespace Masuit.Tools.Models;

/// <summary>
/// 代表树形结构的类
/// </summary>
/// <typeparam name="T"></typeparam>
public class Tree<T> : ITreeChildren<Tree<T>>
{
    public Tree(T value)
    {
        this.Value = value;
    }

    /// <summary>
    /// 代表当前节点的值
    /// </summary>
    public T Value { get; set; }

    /// <summary>
    /// 父节点
    /// </summary>
    public virtual T Parent { get; set; }

    /// <summary>
    /// 子级
    /// </summary>
    public virtual ICollection<Tree<T>> Children { get; set; }
}