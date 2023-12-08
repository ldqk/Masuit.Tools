namespace Masuit.Tools.RandomSelector;

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
        return WeightedSelector.Items.Count == 0 ? default : BinarySelect(WeightedSelector.Items).Value;
    }
}
