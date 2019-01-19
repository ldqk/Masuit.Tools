using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Masuit.Tools.Mapping.Core
{
    /// <summary>
    /// 单例存储映射器。
    /// </summary>
    /// <remarks>不需要单例，因为适用于所有线程。</remarks>
    public class MapperConfigurationCollectionContainer : IEnumerable<MapperConfigurationBase>
    {
        private readonly HashSet<MapperConfigurationBase> _items;
        private static MapperConfigurationCollectionContainer currentInstance;

        private MapperConfigurationCollectionContainer()
        {
            _items = new HashSet<MapperConfigurationBase>();
        }

        /// <summary>
        /// mapper映射容器
        /// </summary>
        public static MapperConfigurationCollectionContainer Instance => currentInstance ?? (currentInstance = new MapperConfigurationCollectionContainer());

        /// <summary>
        /// count
        /// </summary>
        public int Count => _items.Count;

        /// <summary>
        /// 索引器
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal MapperConfigurationBase this[int index]
        {
            get
            {
                if (index > _items.Count)
                    throw new IndexOutOfRangeException();
                var enumerator = GetEnumerator();
                int i = 0;
                while (enumerator.MoveNext())
                {
                    if (i == index)
                    {
                        return enumerator.Current;
                    }
                    i++;
                }

                return null;
            }
        }

        /// <summary>
        /// 查找指定的源。
        /// </summary>
        /// <param name="source">源类型</param>
        /// <param name="target">目标对象</param>
        /// <param name="name">别名</param>
        internal MapperConfigurationBase Find(Type source, Type target, string name = null)
        {
            foreach (var current in this)
            {
                string nameMapper = string.IsNullOrEmpty(name) ? current.paramClassSource.Name : name;
                if (current.SourceType == source && current.TargetType == target && current.Name == nameMapper)
                {
                    return current;
                }
            }
            return null;
        }

        /// <summary>
        /// 是否存在谓词的映射。
        /// </summary>
        /// <param name="match">条件表达式</param>
        public bool Exists(Func<MapperConfigurationBase, bool> match)
        {
            return this.Any(match);
        }

        /// <summary>
        /// 移除指定的元素
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            MapperConfigurationBase itemToDelete = this[index];
            if (itemToDelete != null)
            {
                _items.Remove(itemToDelete);
            }
        }

        /// <summary>
        /// 清除
        /// </summary>
        public void Clear()
        {
            _items.Clear();
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="value"></param>
        public void Add(MapperConfigurationBase value)
        {
            _items.Add(value);
        }

        /// <summary>
        /// 迭代器
        /// </summary>
        /// <returns></returns>
        public IEnumerator<MapperConfigurationBase> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        /// <summary>
        /// 迭代器
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}