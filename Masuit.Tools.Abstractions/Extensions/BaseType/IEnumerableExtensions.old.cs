using System;
using System.Collections.Generic;

using System.Text;
using System.Threading.Tasks;

namespace Masuit.Tools
{
    public static partial class IEnumerableExtensions
    {
        /// <summary>
        /// 映射到目标类型的集合
        /// </summary>
        /// <param name="source">源</param>
        /// <typeparam name="TDestination">目标类型</typeparam>
        /// <returns>目标类型集合</returns>
        [Obsolete("请改用新ToList方法，自行传递clone方法")]
        public static IEnumerable<TDestination> ToList<TSource, TDestination>(this IEnumerable<TSource> source)
            where TDestination : new()
        {
            return IEnumerableExtensions.ToList<TSource, TDestination>(
                source: source,
                selector: t => t.Clone<TDestination>(),
                defaultValue: null);
        }

        /// <summary>
        /// 映射到目标类型的集合
        /// </summary>
        /// <param name="source">源</param>
        /// <typeparam name="TDestination">目标类型</typeparam>
        /// <returns>目标类型集合</returns>
        [Obsolete("请改用新ToListAsync方法，自行传递clone方法")]
        public static Task<IEnumerable<TDestination>> ToListAsync<TSource, TDestination>(this IEnumerable<TSource> source)
            where TDestination : new()
        {
            return Task.Run(() =>
            {
                return source.ToList<TSource, TDestination>();
            });
        }
    }
}