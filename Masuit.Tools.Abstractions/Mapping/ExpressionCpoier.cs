namespace Masuit.Tools.Mapping
{
    /// <summary>
    /// 表达式树复制对象
    /// </summary>
    public static class ExpressionCpoier
    {
        /// <summary>
        /// 复制一个新实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T Copy<T>(this T source)
        {
            return Copier<T>.Copy(source);
        }
    }
}