namespace Masuit.Tools.RandomSelector
{
    public class SelectorOptions
    {
        /// <summary>
        /// 多选时允许重复项
        /// </summary>
        public bool AllowDuplicates { get; set; }

        /// <summary>
        /// 是否移除权重0的元素
        /// </summary>
        public bool DropZeroWeightItems { get; set; }

        public SelectorOptions()
        {
            AllowDuplicates = false;
            DropZeroWeightItems = true;
        }
    }
}
