using System;
using System.Linq.Expressions;

namespace Masuit.Tools.Linq
{
    /// <summary>
    /// linq扩展
    /// </summary>
    public static class LinqExtension
    {
        /// <summary>
        /// 与连接
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
        {
            var dateExpr = Expression.Parameter(typeof(T));
            var parameterReplacer = new ParameterReplacer(dateExpr);
            var leftwhere = parameterReplacer.Replace(left.Body);
            var rightwhere = parameterReplacer.Replace(right.Body);
            var body = Expression.And(leftwhere, rightwhere);
            return Expression.Lambda<Func<T, bool>>(body, dateExpr);
        }

        /// <summary>
        /// 或连接
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
        {
            var dateExpr = Expression.Parameter(typeof(T));
            var parameterReplacer = new ParameterReplacer(dateExpr);
            var leftwhere = parameterReplacer.Replace(left.Body);
            var rightwhere = parameterReplacer.Replace(right.Body);
            var body = Expression.Or(leftwhere, rightwhere);
            return Expression.Lambda<Func<T, bool>>(body, dateExpr);
        }
    }
}