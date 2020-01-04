using System;
using System.Collections.Generic;

namespace Masuit.Tools.RandomSelector.Algorithm
{
    /// <summary>
    /// 多选器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class MultipleSelector<T> : SelectorBase<T>
    {
        internal MultipleSelector(WeightedSelector<T> weightedSelector) : base(weightedSelector)
        {
        }

        internal List<T> Select(int count)
        {
            Validate(ref count);
            var Items = new List<WeightedItem<T>>(WeightedSelector.Items);
            var ResultList = new List<T>();

            do
            {
                var Item = WeightedSelector.Options.AllowDuplicates ? Select(Items) : SelectWithLinearSearch(Items);
                ResultList.Add(Item.Value);
                if (!WeightedSelector.Options.AllowDuplicates)
                {
                    Items.Remove(Item);
                }
            } while (ResultList.Count < count);
            return ResultList;
        }

        private void Validate(ref int count)
        {
            if (count <= 0)
            {
                throw new InvalidOperationException("筛选个数必须大于0");
            }

            var Items = WeightedSelector.Items;

            if (Items.Count == 0)
            {
                throw new InvalidOperationException("没有元素可以被筛选");
            }

            if (!WeightedSelector.Options.AllowDuplicates && Items.Count < count)
            {
                count = Items.Count;
            }
        }
    }
}
