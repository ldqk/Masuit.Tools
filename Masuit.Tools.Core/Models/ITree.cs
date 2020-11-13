using System.Collections.Generic;

namespace Masuit.Tools.Core.Models
{
    /// <summary>
    /// 树形实体接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ITree<T> where T : ITree<T>
    {
        /// <summary>
        /// 名字
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// 父节点
        /// </summary>
        T Parent { get; set; }

        /// <summary>
        /// 子级
        /// </summary>
        ICollection<T> Children { get; set; }
    }
}