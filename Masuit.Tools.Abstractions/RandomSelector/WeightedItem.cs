namespace Masuit.Tools.RandomSelector;

public class WeightedItem<T>(T value, int weight)
{
    /// <summary>
    /// 权重
    /// </summary>
    public int Weight = weight;

    /// <summary>
    /// 元素
    /// </summary>
    public readonly T Value = value;

    /// <summary>
    /// 累计权重
    /// </summary>
    internal int CumulativeWeight = 0;
}
