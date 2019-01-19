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
        /// <typeparam name="TTarget">目标类型</typeparam>
        /// <param name="query">分类化的序列值</param>
        /// <param name="sortedPropertyDestName">目标属性的名称</param>
        /// <returns></returns>
        public static IOrderedQueryable<TSource> OrderBy<TSource, TTarget>(this IQueryable<TSource> query, string sortedPropertyDestName) where TSource : class where TTarget : class
        {
            // 没有使用MethodBase.GetCurrentMethod().Name，因为效率不高
            return CreateSortedMethodCall<TSource, TTarget, IOrderedQueryable<TSource>>(query, "OrderBy", sortedPropertyDestName);
        }

        /// <summary>
        /// 根据键按降序对序列的元素进行排序。
        /// </summary>
        /// <typeparam name="TSource">源类型</typeparam>
        /// <typeparam name="TTarget">目标类型</typeparam>
        /// <param name="query">分类化的序列值</param>
        /// <param name="sortedPropertyDestName">目标属性的名称</param>
        /// <returns></returns>
        public static IOrderedQueryable<TSource> OrderByDescending<TSource, TTarget>(this IQueryable<TSource> query, string sortedPropertyDestName) where TSource : class where TTarget : class
        {
            return CreateSortedMethodCall<TSource, TTarget, IOrderedQueryable<TSource>>(query, "OrderByDescending", sortedPropertyDestName);
        }

        /// <summary>
        ///  根据键按升序对序列的元素进行排序。
        /// </summary>
        /// <typeparam name="TSource">源类型</typeparam>
        /// <typeparam name="TTarget">目标类型</typeparam>
        /// <param name="query">分类化的序列值</param>
        /// <param name="sortedPropertyDestName">目标属性的名称</param>
        public static IOrderedQueryable<TSource> ThenBy<TSource, TTarget>(this IQueryable<TSource> query, string sortedPropertyDestName) where TSource : class where TTarget : class
        {
            return CreateSortedMethodCall<TSource, TTarget, IOrderedQueryable<TSource>>(query, "ThenBy", sortedPropertyDestName);
        }

        /// <summary>
        /// 根据键按降序对序列的元素进行排序。
        /// </summary>
        /// <typeparam name="TSource">源类型</typeparam>
        /// <typeparam name="TTarget">目标类型</typeparam>
        /// <param name="query">分类化的序列值</param>
        /// <param name="sortedPropertyDestName">目标属性的名称</param>
        public static IOrderedQueryable<TSource> ThenByDescending<TSource, TTarget>(this IQueryable<TSource> query, string sortedPropertyDestName) where TSource : class where TTarget : class
        {
            return CreateSortedMethodCall<TSource, TTarget, IOrderedQueryable<TSource>>(query, "ThenByDescending", sortedPropertyDestName);
        }

        /// <summary>
        /// 通过合并目标对象将序列的每个元素投影到新表单中。
        /// </summary>
        /// <typeparam name="TSource">源类型.</typeparam>
        /// <typeparam name="TTarget">目标类型.</typeparam>
        /// <param name="query">分类化的序列值</param>
        public static IQueryable<TTarget> Select<TSource, TTarget>(this IQueryable<TSource> query) where TSource : class where TTarget : class
        {
            return GetSelect<TSource, TTarget>(query, null);
        }

        /// <summary>
        /// 通过合并目标对象将序列的每个元素投影到新表单中。
        /// </summary>
        /// <typeparam name="TSource">源类型.</typeparam>
        /// <typeparam name="TTarget">目标类型.</typeparam>
        /// <param name="query">分类化的序列值</param>
        /// <param name="mapperName">mapper别名</param>
        /// <returns></returns>
        public static IQueryable<TTarget> Select<TSource, TTarget>(this IQueryable<TSource> query, string mapperName) where TSource : class where TTarget : class
        {
            return GetSelect<TSource, TTarget>(query, mapperName);
        }

        /// <summary>
        /// 根据谓词过滤一系列值。
        /// </summary>
        /// <typeparam name="TSource">源类型</typeparam>
        /// <typeparam name="TTarget">目标类型</typeparam>
        /// <param name="query">分类化的序列值</param>
        /// <param name="predicate">用于根据条件测试每个元素的功能。</param>
        /// <returns></returns>
        public static IQueryable<TTarget> Where<TSource, TTarget>(this IQueryable<TTarget> query, Expression<Func<TSource, bool>> predicate)
        {
            return Queryable.Where(query, predicate.ConvertTo<TSource, TTarget>());
        }

        private static TQueryable CreateSortedMethodCall<TSource, TTarget, TQueryable>(IQueryable<TSource> query, string methodName, string sortedPropertySourceName) where TSource : class where TTarget : class where TQueryable : class, IQueryable<TSource>
        {
            MapperConfiguration<TSource, TTarget> mapper = ExpressionMapper.GetMapper<TSource, TTarget>();
            var prop = mapper.GetLambdaDest(sortedPropertySourceName);
            var lambda = mapper.GetSortedExpression(sortedPropertySourceName);
            MethodCallExpression resultExp = Expression.Call(typeof(Queryable), methodName, new Type[]
            {
                typeof(TSource),
                prop.Type
            }, query.Expression, Expression.Quote(lambda));
            return query.Provider.CreateQuery<TSource>(resultExp) as TQueryable;
        }

        private static IQueryable<TTarget> GetSelect<TSource, TTarget>(IQueryable<TSource> query, string mapperName) where TSource : class where TTarget : class
        {
            // 不需要mapper
            if (typeof(TSource) == typeof(TTarget))
            {
                return (IQueryable<TTarget>)query;
            }
            return query.Select(ExpressionMapper.GetMapper<TSource, TTarget>(mapperName).GetLambdaExpression());
        }
    }
}