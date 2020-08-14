using Masuit.Tools.RandomSelector;
using System.Collections.Generic;
using System.Linq;

namespace Masuit.Tools
{
    public static partial class Extensions
    {
        public static int TotalWeight<T>(this WeightedSelector<T> selector)
        {
            return selector.Items.Count == 0 ? 0 : selector.Items.Sum(t => t.Weight);
        }

        public static List<WeightedItem<T>> OrderByWeightDescending<T>(this WeightedSelector<T> selector)
        {
            return selector.Items.OrderByDescending(item => item.Weight).ToList();
        }

        public static List<WeightedItem<T>> OrderByWeightAscending<T>(this WeightedSelector<T> selector)
        {
            return selector.Items.OrderBy(item => item.Weight).ToList();
        }

        public static T WeightedItem<T>(this IEnumerable<WeightedItem<T>> list)
        {
            return new WeightedSelector<T>(list).Select();
        }

        public static List<T> WeightedItems<T>(this IEnumerable<WeightedItem<T>> list, int count)
        {
            return new WeightedSelector<T>(list).SelectMultiple(count);
        }
    }
}
