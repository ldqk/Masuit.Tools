using System;
using System.Collections.Generic;
using System.Linq;

namespace Masuit.Tools.Maths
{
    /// <summary>
    /// 百分位元素选择器
    /// </summary>
    public static class PercentileSelector
    {
        private static T QuickSelect<T>(IList<T> arr, int left, int right, int k) where T : struct, IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T>
        {
            while (true)
            {
                if (left == right)
                {
                    return arr[left];
                }

                var pivotIndex = left + (right - left) / 2;
                pivotIndex = Partition(arr, left, right, pivotIndex);

                if (k == pivotIndex)
                {
                    return arr[k];
                }

                if (k < pivotIndex)
                {
                    right = pivotIndex - 1;
                    continue;
                }

                left = pivotIndex + 1;
            }
        }

        private static int Partition<T>(IList<T> arr, int left, int right, int pivotIndex) where T : struct, IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T>
        {
            var pivotValue = arr[pivotIndex];
            Swap(arr, pivotIndex, right);
            var storeIndex = left;

            for (int i = left; i < right; i++)
            {
                if (arr[i].CompareTo(pivotValue) == -1)
                {
                    Swap(arr, i, storeIndex);
                    storeIndex++;
                }
            }

            Swap(arr, storeIndex, right);
            return storeIndex;
        }

        private static void Swap<T>(IList<T> arr, int i, int j)
        {
            (arr[i], arr[j]) = (arr[j], arr[i]);
        }

        /// <summary>
        /// 查找第percentile百分位的元素
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="percentile"></param>
        public static T Percentile<T>(this IList<T> arr, double percentile) where T : struct, IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T>
        {
            if (arr.Count == 0)
            {
                return default;
            }

            var k = (int)Math.Ceiling(arr.Count * percentile * 0.01) - 1;
            return QuickSelect(arr, 0, arr.Count - 1, k);
        }

        /// <summary>
        /// 查找第percentile百分位的元素
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="percentile"></param>
        public static T Percentile<T>(this IEnumerable<T> arr, double percentile) where T : struct, IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T>
        {
            return Percentile(arr.ToList(), percentile);
        }
    }
}
