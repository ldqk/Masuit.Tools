using System;

namespace Masuit.Tools.RandomSelector.Algorithm
{
    /// <summary>
    /// 单选器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class SingleSelector<T> : SelectorBase<T>
    {
        internal SingleSelector(WeightedSelector<T> weightedSelector) : base(weightedSelector)
        {
        }

        internal T Select()
        {
            var Items = WeightedSelector.Items;
            if (Items.Count == 0)
            {
                throw new InvalidOperationException("没有元素可以筛选");
            }

            return Select(Items).Value;
        }
    }
}
