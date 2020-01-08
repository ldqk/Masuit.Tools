namespace Masuit.Tools.RandomSelector
{
    public class SelectorOption
    {
        /// <summary>
        /// 多选时允许重复项
        /// </summary>
        public bool AllowDuplicate { get; set; }

        /// <summary>
        /// 是否移除权重0的元素
        /// </summary>
        public bool RemoveZeroWeightItems { get; set; }

        public SelectorOption()
        {
            AllowDuplicate = false;
            RemoveZeroWeightItems = true;
        }
    }
}
