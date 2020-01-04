using Masuit.Tools.Mapping.Core;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Masuit.Tools.Mapping.Extensions
{
    /// <summary>
    /// IQueryable的扩展
    /// </summary>
    public static class QueryableExtentions
    {
        /// <summary>
        /// 根据键按升序对序列的元素进行排序。
        /// </summary>
        /// <typeparam name="TSource">源类型</typeparam>
        /// <typeparam name="TDest">目标类型</typeparam>
        /// <param name="query">分类化的序列值</param>
        /// <param name="sortedPropertyDestName">目标属性的名称</param>
        /// <returns></returns>
        public static IOrderedQueryable<TSource> OrderBy<TSource, TDest>(this IQueryable<TSource> query, string sortedPropertyDestName) where TSource : class where TDest : class
        {
            // 没有使用MethodBase.GetCurrentMethod().Name，因为效率不高
            return CreateSortedMethodCall<TSource, TDest, IOrderedQueryable<TSource>>(query, "OrderBy", sortedPropertyDestName);
        }

        /// <summary>
        /// 根据键按降序对序列的元素进行排序。
        /// </summary>
        /// <typeparam name="TSource">源类型</typeparam>
        /// <typeparam name="TDest">目标类型</typeparam>
        /// <param name="query">分类化的序列值</param>
        /// <param name="sortedPropertyDestName">目标属性的名称</param>
        /// <returns></returns>
        public static IOrderedQueryable<TSource> OrderByDescending<TSource, TDest>(this IQueryable<TSource> query, string sortedPropertyDestName) where TSource : class where TDest : class
        {
            return CreateSortedMethodCall<TSource, TDest, IOrderedQueryable<TSource>>(query, "OrderByDescending", sortedPropertyDestName);
        }

        /// <summary>
        ///  根据键按升序对序列的元素进行排序。
        /// </summary>
        /// <typeparam name="TSource">源类型</typeparam>
        /// <typeparam name="TDest">目标类型</typeparam>
        /// <param name="query">分类化的序列值</param>
        /// <param name="sortedPropertyDestName">目标属性的名称</param>
        public static IOrderedQueryable<TSource> ThenBy<TSource, TDest>(this IQueryable<TSource> query, string sortedPropertyDestName) where TSource : class where TDest : class
        {
            return CreateSortedMethodCall<TSource, TDest, IOrderedQueryable<TSource>>(query, "ThenBy", sortedPropertyDestName);
        }

        /// <summary>
        /// 根据键按降序对序列的元素进行排序。
        /// </summary>
        /// <typeparam name="TSource">源类型</typeparam>
        /// <typeparam name="TDest">目标类型</typeparam>
        /// <param name="query">分类化的序列值</param>
        /// <param name="sortedPropertyDestName">目标属性的名称</param>
        public static IOrderedQueryable<TSource> ThenByDescending<TSource, TDest>(this IQueryable<TSource> query, string sortedPropertyDestName) where TSource : class where TDest : class
        {
            return CreateSortedMethodCall<TSource, TDest, IOrderedQueryable<TSource>>(query, "ThenByDescending", sortedPropertyDestName);
        }

        /// <summary>
        /// 通过合并目标对象将序列的每个元素投影到新表单中。
        /// </summary>
        /// <typeparam name="TSource">源类型.</typeparam>
        /// <typeparam name="TDest">目标类型.</typeparam>
        /// <param name="query">分类化的序列值</param>
        public static IQueryable<TDest> ProjectTo<TSource, TDest>(this IQueryable<TSource> query) where TSource : class where TDest : class
        {
            return GetSelect<TSource, TDest>(query, null);
        }

        /// <summary>
        /// 通过合并目标对象将序列的每个元素投影到新表单中。
        /// </summary>
        /// <typeparam name="TSource">源类型.</typeparam>
        /// <typeparam name="TDest">目标类型.</typeparam>
        /// <param name="query">分类化的序列值</param>
        /// <param name="mapperName">mapper别名</param>
        /// <returns></returns>
        public static IQueryable<TDest> ProjectTo<TSource, TDest>(this IQueryable<TSource> query, string mapperName) where TSource : class where TDest : class
        {
            return GetSelect<TSource, TDest>(query, mapperName);
        }

        /// <summary>
        /// 根据谓词过滤一系列值。
        /// </summary>
        /// <typeparam name="TSource">源类型</typeparam>
        /// <typeparam name="TDest">目标类型</typeparam>
        /// <param name="query">分类化的序列值</param>
        /// <param name="predicate">用于根据条件测试每个元素的功能。</param>
        /// <returns></returns>
        public static IQueryable<TDest> WhereTo<TSource, TDest>(this IQueryable<TDest> query, Expression<Func<TSource, bool>> predicate)
        {
            return query.Where(predicate.ConvertTo<TSource, TDest>());
        }

        private static TQueryable CreateSortedMethodCall<TSource, TDest, TQueryable>(IQueryable<TSource> query, string methodName, string sortedPropertySourceName) where TSource : class where TDest : class where TQueryable : class, IQueryable<TSource>
        {
            MapperConfiguration<TSource, TDest> mapper = ExpressionMapper.GetMapper<TSource, TDest>();
            var prop = mapper.GetLambdaDest(sortedPropertySourceName);
            var lambda = mapper.GetSortedExpression(sortedPropertySourceName);
            MethodCallExpression resultExp = Expression.Call(typeof(Queryable), methodName, new Type[]
            {
                typeof(TSource),
                prop.Type
            }, query.Expression, Expression.Quote(lambda));
            return query.Provider.CreateQuery<TSource>(resultExp) as TQueryable;
        }

        private static IQueryable<TDest> GetSelect<TSource, TDest>(IQueryable<TSource> query, string mapperName) where TSource : class where TDest : class
        {
            // 不需要mapper
            if (typeof(TSource) == typeof(TDest))
            {
                return (IQueryable<TDest>)query;
            }
            return query.Select(ExpressionMapper.GetMapper<TSource, TDest>(mapperName).GetLambdaExpression());
        }
    }
}