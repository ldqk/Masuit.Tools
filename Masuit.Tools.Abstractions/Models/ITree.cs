using System.Collections.Generic;

namespace Masuit.Tools.Models
{
    /// <summary>
    /// 树形实体接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ITree<T> : ITreeParent<T>, ITreeChildren<T>
    {
        /// <summary>
        /// 名字
        /// </summary>
        string Name { get; set; }
    }

    /// <summary>
    /// 带子级的树形实体接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ITreeChildren<T>
    {
        /// <summary>
        /// 子级
        /// </summary>
        ICollection<T> Children { get; set; }
    }

    /// <summary>
    /// 带父节点的树形实体接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ITreeParent<T>
    {
        /// <summary>
        /// 父节点
        /// </summary>
        T Parent { get; set; }
    }
}