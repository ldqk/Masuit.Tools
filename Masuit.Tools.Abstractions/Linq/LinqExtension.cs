using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Masuit.Tools.Linq
{
    /// <summary>
    /// LINQ扩展方法
    /// </summary>
    public static class LinqExtension
    {
        /// <summary>
        /// 与连接
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="left">左条件</param>
        /// <param name="right">右条件</param>
        /// <returns>新表达式</returns>
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
        {
            return CombineLambdas(left, right, ExpressionType.AndAlso);
        }

        /// <summary>
        /// 与连接
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="left">左条件</param>
        /// <param name="condition">符合什么条件</param>
        /// <param name="right">右条件</param>
        /// <returns>新表达式</returns>
        public static Expression<Func<T, bool>> AndIf<T>(this Expression<Func<T, bool>> left, bool condition, Expression<Func<T, bool>> right)
        {
            return condition ? CombineLambdas(left, right, ExpressionType.AndAlso) : left;
        }

        /// <summary>
        /// 与连接
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="left">左条件</param>
        /// <param name="condition">符合什么条件</param>
        /// <param name="right">右条件</param>
        /// <returns>新表达式</returns>
        public static Expression<Func<T, bool>> AndIf<T>(this Expression<Func<T, bool>> left, Func<bool> condition, Expression<Func<T, bool>> right)
        {
            return condition() ? CombineLambdas(left, right, ExpressionType.AndAlso) : left;
        }

        /// <summary>
        /// 或连接
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="left">左条件</param>
        /// <param name="right">右条件</param>
        /// <returns>新表达式</returns>
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
        {
            return CombineLambdas(left, right, ExpressionType.OrElse);
        }

        /// <summary>
        /// 或连接
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="left">左条件</param>
        /// <param name="condition">符合什么条件</param>
        /// <param name="right">右条件</param>
        /// <returns>新表达式</returns>
        public static Expression<Func<T, bool>> OrIf<T>(this Expression<Func<T, bool>> left, bool condition, Expression<Func<T, bool>> right)
        {
            return condition ? CombineLambdas(left, right, ExpressionType.OrElse) : left;
        }

        /// <summary>
        /// 或连接
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="left">左条件</param>
        /// <param name="condition">符合什么条件</param>
        /// <param name="right">右条件</param>
        /// <returns>新表达式</returns>
        public static Expression<Func<T, bool>> OrIf<T>(this Expression<Func<T, bool>> left, Func<bool> condition, Expression<Func<T, bool>> right)
        {
            return condition() ? CombineLambdas(left, right, ExpressionType.OrElse) : left;
        }

        private static Expression<Func<T, bool>> CombineLambdas<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right, ExpressionType expressionType)
        {
            var visitor = new SubstituteParameterVisitor
            {
                Sub =
                {
                    [right.Parameters[0]] = left.Parameters[0]
                }
            };

            Expression body = Expression.MakeBinary(expressionType, left.Body, visitor.Visit(right.Body));
            return Expression.Lambda<Func<T, bool>>(body, left.Parameters[0]);
        }
    }

    internal class SubstituteParameterVisitor : ExpressionVisitor
    {
        public Dictionary<Expression, Expression> Sub = new Dictionary<Expression, Expression>();

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return Sub.TryGetValue(node, out var newValue) ? newValue : node;
        }
    }
}
