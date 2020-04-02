using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

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
        public string Name { get; set; }

        /// <summary>
        /// 父节点
        /// </summary>
        public T Parent { get; set; }

        /// <summary>
        /// 子级
        /// </summary>
        public ICollection<T> Children { get; set; }

        /// <summary>
        /// 所有子级
        /// </summary>
        [NotMapped]
        public ICollection<T> AllChildren => GetChildren(this);

        /// <summary>
        /// 所有父级
        /// </summary>
        [NotMapped]
        public ICollection<T> AllParent => GetParents(this);

        /// <summary>
        /// 是否是根节点
        /// </summary>
        [NotMapped]
        public bool IsRoot => Parent == null;

        /// <summary>
        /// 是否是叶子节点
        /// </summary>
        [NotMapped]
        public bool IsLeaf => Children.Count == 0;

        /// <summary>
        /// 深度
        /// </summary>
        [NotMapped]
        public int Level => IsRoot ? 0 : Parent.Level + 1;

        /// <summary>
        /// 节点路径（UNIX路径格式，以“/”分隔）
        /// </summary>
        [NotMapped]
        public string Path => GetFullPath(this);

        private string GetFullPath(ITree<T> c) => c.Parent != null ? GetFullPath(c.Parent) + "/" + c.Name : c.Name;

        /// <summary>
        /// 递归取出所有下级
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private List<T> GetChildren(ITree<T> t)
        {
            return t.Children.Union(t.Children.Where(c => c.Children.Any()).SelectMany(tree => GetChildren(tree))).ToList();
        }

        /// <summary>
        /// 递归取出所有上级
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private List<T> GetParents(ITree<T> t)
        {
            var list = new List<T>() { t.Parent };
            return t.Parent != null ? list.Union(GetParents(t.Parent)).ToList() : list;
        }
    }
}