namespace Masuit.Tools.RandomSelector
{
    public class WeightedItem<T>
    {
        public int Weight;
        public readonly T Value;

        /// <summary>
        /// 累计权重
        /// </summary>
        internal int CumulativeWeight;

        public WeightedItem(T value, int weight)
        {
            Value = value;
            Weight = weight;
            CumulativeWeight = 0;
        }
    }
}
