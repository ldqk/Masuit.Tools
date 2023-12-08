using System;
using System.Collections.Generic;

namespace Masuit.Tools.RandomSelector;

/// <summary>
/// 多选器
/// </summary>
/// <typeparam name="T"></typeparam>
internal class MultipleSelector<T> : SelectorBase<T>
{
    internal MultipleSelector(WeightedSelector<T> weightedSelector) : base(weightedSelector)
    {
    }

    internal IEnumerable<T> Select(int count)
    {
        Validate(ref count);
        var items = new List<WeightedItem<T>>(WeightedSelector.Items);
        int result = 0;
        do
        {
            var item = WeightedSelector.Option.AllowDuplicate ? BinarySelect(items) : LinearSelect(items);
            yield return item.Value;
            result++;
            if (!WeightedSelector.Option.AllowDuplicate)
            {
                items.Remove(item);
            }
        } while (result < count);
    }

    private void Validate(ref int count)
    {
        if (count <= 0)
        {
            throw new InvalidOperationException("筛选个数必须大于0");
        }

        var items = WeightedSelector.Items;

        if (items.Count == 0)
        {
            count = 0;
            return;
        }

        if (!WeightedSelector.Option.AllowDuplicate && items.Count < count)
        {
            count = items.Count;
        }
    }
}
