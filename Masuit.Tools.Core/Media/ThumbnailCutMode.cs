namespace Masuit.Tools.Media
{
    /// <summary>
    /// 图像裁剪模式
    /// </summary>
    public enum ThumbnailCutMode
    {
        /// <summary>
        /// 锁定高度
        /// </summary>
        LockHeight,

        /// <summary>
        /// 锁定宽度
        /// </summary>
        LockWidth,

        /// <summary>
        /// 固定宽高
        /// </summary>
        Fixed,

        /// <summary>
        /// 裁剪
        /// </summary>
        Cut
    }
}