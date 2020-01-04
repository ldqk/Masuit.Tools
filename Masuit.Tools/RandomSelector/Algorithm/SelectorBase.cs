using System;
using System.Collections.Generic;
using System.Linq;

namespace Masuit.Tools.RandomSelector.Algorithm
{
    internal abstract class SelectorBase<T>
    {
        protected readonly WeightedSelector<T> WeightedSelector;

        internal SelectorBase(WeightedSelector<T> weightedSelector)
        {
            WeightedSelector = weightedSelector;
        }

        protected int GetSeed(List<WeightedItem<T>> items)
        {
            var sum = items.Sum(i => i.Weight) + 1;
            return new Random().Next(1, sum);
        }

        /// <summary>
        /// 执行二进制筛选
        /// </summary>
        protected WeightedItem<T> Select(List<WeightedItem<T>> items)
        {
            if (items.Count == 0)
            {
                throw new InvalidOperationException("没有元素可以筛选");
            }

            var seed = GetSeed(items);
            return BinarySearch(items, seed);
        }

        /// <summary>
        /// 线性搜索算法
        /// </summary>
        protected WeightedItem<T> SelectWithLinearSearch(List<WeightedItem<T>> items)
        {
            // 这只对具有允许重复项的多选功能有用，它会随着时间从列表中删除项目。 在这些条件下没有消耗更多性能让二进制搜索起作用。
            if (items.Count == 0)
            {
                throw new InvalidOperationException("没有元素可以筛选");
            }

            var seed = GetSeed(items);
            return LinearSearch(items, seed);
        }

        /// <summary>
        /// 线性筛选
        /// </summary>
        /// <param name="items"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        private WeightedItem<T> LinearSearch(IEnumerable<WeightedItem<T>> items, int seed)
        {
            var count = 0;
            foreach (var item in items)
            {
                count += item.Weight;
                if (seed <= count)
                {
                    return item;
                }
            }

            throw new InvalidOperationException("没有任何的筛选结果");
        }

        /// <summary>
        /// 二进制筛选
        /// </summary>
        /// <param name="items"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        private WeightedItem<T> BinarySearch(List<WeightedItem<T>> items, int seed)
        {
            int index = Array.BinarySearch(WeightedSelector.CumulativeWeights, seed);

            //如果存在接近的匹配项，二进制搜索返回的负数会比搜索的第1个索引少1。
            if (index < 0)
            {
                index = -index - 1;
            }

            return items[index];
        }
    }
}
