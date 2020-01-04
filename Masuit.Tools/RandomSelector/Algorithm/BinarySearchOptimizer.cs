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
            int TotalWeight = 0;
            int Index = 0;
            var ResultArray = new int[items.Count + 1];

            foreach (var Item in items)
            {
                TotalWeight += Item.Weight;
                ResultArray[Index] = TotalWeight;
                Index++;
            }

            return ResultArray;
        }
    }
}
