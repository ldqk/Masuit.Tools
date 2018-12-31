using System.Collections.Generic;

namespace Masuit.Tools
{
    public static class MapClass
    {
        /// <summary>
        /// 将对象TSource转换为TDest
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static TDest Map<TSource, TDest>(this TSource source) where TDest : class where TSource : class => ExpressionGenericMapper<TSource, TDest>.Map(source);

        /// <summary>
        /// 集合元素映射
        /// </summary>
        /// <param name="sources"></param>
        /// <returns></returns>
        public static List<TDest> MapList<TSource, TDest>(this IEnumerable<TSource> sources) where TDest : class where TSource : class => ExpressionGenericMapper<TSource, TDest>.MapList(sources);

        /// <summary>
        /// 将对象TSource的值赋给给TDest
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public static void MapTo<TSource, TDest>(this TSource source, TDest target) where TSource : class where TDest : class => ExpressionGenericMapper<TSource, TDest>.Map(source, target);

        /// <summary>
        /// 复制一个新对象
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static TSource Copy<TSource>(this TSource source) where TSource : class => ExpressionGenericMapper<TSource, TSource>.Map(source);

        /// <summary>
        /// 复制一个新集合
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static List<TSource> Copy<TSource>(this IEnumerable<TSource> source) where TSource : class => ExpressionGenericMapper<TSource, TSource>.MapList(source);
    }
}