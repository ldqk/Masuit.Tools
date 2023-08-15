using System;

namespace Masuit.Tools.RandomSelector
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
			if (WeightedSelector.Items.Count == 0)
			{
				return default(T);
			}

			return BinarySelect(WeightedSelector.Items).Value;
		}
	}
}
