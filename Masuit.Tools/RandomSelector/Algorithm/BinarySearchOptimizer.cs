using System.Collections.Generic;

namespace Masuit.Tools.RandomSelector.Algorithm
{
    public class BinarySearchOptimizer
    {
        /// <summary>
        /// 计算累计权重
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        public static int[] GetCumulativeWeights<T>(List<WeightedItem<T>> items)
        {
            int totalWeight = 0;
            int index = 0;
            var resultArray = new int[items.Count + 1];

            foreach (var item in items)
            {
                totalWeight += item.Weight;
                resultArray[index] = totalWeight;
                index++;
            }

            return resultArray;
        }
    }
}
