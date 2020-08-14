using Masuit.Tools.Mapping.Visitor;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Masuit.Tools.Mapping.Extensions
{
    /// <summary>
    ///表达式树扩展
    /// </summary>
    public static class ExpressionExtentions
    {
        /// <summary>
        /// 转换
        /// </summary>
        /// <typeparam name="TFrom">源类型</typeparam>
        /// <param name="from">来源</param>
        /// <param name="toType">目标类型</param>
        /// <returns></returns>
        public static Expression ConvertTo<TFrom>(this Expression<Func<TFrom, object>> from, Type toType)
        {
            return ConvertImpl(from, toType);
        }

        /// <summary>
        /// 转换
        /// </summary>
        /// <typeparam name="TFrom">源类型</typeparam>
        /// <param name="from">来源</param>
        /// <param name="toType">目标类型</param>
        /// <returns></returns>
        public static Expression ConvertTo<TFrom>(this Expression<Func<TFrom, bool>> from, Type toType)
        {
            return ConvertImpl(from, toType);
        }

        /// <summary>
        /// 转换Lambda表达式树
        /// </summary>
        /// <typeparam name="TFrom">源类型</typeparam>
        /// <typeparam name="TTo">目标类型</typeparam>
        /// <param name="from">来源</param>
        /// <returns></returns>
        public static Expression ConvertTo<TFrom, TTo>(this Expression<Func<TFrom, object>> from)
        {
            return ConvertImpl(from, typeof(TTo));
        }

        /// <summary>
        /// 转换表达式树
        /// </summary>
        /// <typeparam name="TFrom">源类型的表达式树</typeparam>
        /// <typeparam name="TTo">目标类型的表达式树</typeparam>
        /// <param name="from">源类型的表达式树</param>
        /// <returns>表达式转换或如果没有找到映射原始表达式。</returns>
        public static Expression<Func<TTo, bool>> ConvertTo<TFrom, TTo>(this Expression<Func<TFrom, bool>> from)
        {
            return (Expression<Func<TTo, bool>>)ConvertImpl(from, typeof(TTo));
        }

        private static Expression ConvertImpl<TFrom>(Expression<TFrom> from, Type toType) where TFrom : class
        {
            //  重新映射不同类型的所有参数
            Dictionary<Expression, Expression> parameterMap = new Dictionary<Expression, Expression>();
            ParameterExpression[] newParams = new ParameterExpression[from.Parameters.Count];
            for (int i = 0; i < newParams.Length; i++)
            {
                newParams[i] = Expression.Parameter(toType, from.Parameters[i].Name);
                parameterMap[from.Parameters[i]] = newParams[i];
            }

            // 重新构建表达式树
            var body = new ConverterExpressionVisitor(parameterMap, toType).Visit(from.Body);
            return Expression.Lambda(body, newParams);
        }
    }
}