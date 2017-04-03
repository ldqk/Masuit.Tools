namespace Masuit.Tools.Objects
{
    /// <summary>
    /// Object扩展类
    /// </summary>
    public static class ObjectExt
    {
        /// <summary>
        /// 严格比较两个对象是否是同一对象
        /// </summary>
        /// <param name="_this">自己</param>
        /// <param name="o">需要比较的对象</param>
        /// <returns>是否同一对象</returns>
        public new static bool ReferenceEquals(this object _this, object o)
        {
            return object.ReferenceEquals(_this, o);
        }
    }
}