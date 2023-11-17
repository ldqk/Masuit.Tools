using System;
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

    /// <summary>
    /// 树形实体（值类型主键）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public interface ITreeEntity<T, TKey> : ITreeChildren<T> where TKey : struct, IComparable
    {
        /// <summary>
        /// 主键id
        /// </summary>
        public TKey Id { get; set; }

        /// <summary>
        /// 父级id
        /// </summary>
        public TKey? ParentId { get; set; }
    }

    /// <summary>
    /// 树形实体(字符串主键)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ITreeEntity<T> : ITreeChildren<T>
    {
        /// <summary>
        /// 主键id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 父级id
        /// </summary>
        public string ParentId { get; set; }
    }
}
