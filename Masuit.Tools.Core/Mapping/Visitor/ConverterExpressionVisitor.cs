using Masuit.Tools.Mapping.Core;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Masuit.Tools.Mapping.Visitor
{
    /// <summary>
    /// 将表达式从源转换为目标的访问者表达式
    /// </summary>
    public class ConverterExpressionVisitor : ExpressionVisitor
    {
        private readonly Dictionary<Expression, Expression> _parameterMap;
        private readonly Type _destinationType;
        private MapperConfigurationBase _mapper;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="parameterMap"></param>
        /// <param name="typeDestination"></param>
        public ConverterExpressionVisitor(Dictionary<Expression, Expression> parameterMap, Type typeDestination)
        {
            _parameterMap = parameterMap;
            _destinationType = typeDestination;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node">访问表达式树</param>
        /// <returns>
        /// 改变表达式，如果它或它的任何子表达式被修改; 否则，返回原始表达式。
        /// </returns>
        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (!_parameterMap.TryGetValue(node, out var found))
            {
                found = Expression.Parameter(_destinationType, "dest");
            }

            return found;
        }

        /// <summary>
        /// 将表达式分布在此类中更专业的访问方法之一。
        /// </summary>
        /// <param name="node">访问表达式树</param>
        /// <returns>
        /// 改变表达式，如果它或它的任何子表达式被修改; 否则，返回原始表达式。
        /// </returns>
        public override Expression Visit(Expression node)
        {
            if (node != null)
            {
                Expression expression;
                switch (node.NodeType)
                {
                    case ExpressionType.Lambda:
                        expression = base.Visit((node as LambdaExpression).Body);
                        break;
                    default:
                        expression = base.Visit(node);
                        break;
                }
                return expression;
            }
            return node;
        }

        /// <summary>
        /// 访问子表达式树
        /// </summary>
        /// <param name="node">访问表达式树</param>
        /// <returns>
        /// 改变表达式，如果它或它的任何子表达式被修改; 否则，返回原始表达式。
        /// </returns>
        protected override Expression VisitMember(MemberExpression node)
        {
            var expr = Visit(node.Expression);
            if (expr != null && expr.Type != node.Type)
            {
                if (_mapper == null)
                {
                    _mapper = ExpressionMapper.GetMapper(node.Member.DeclaringType, _destinationType);
                }
                Expression expDest;
                // 认为原始类是简单属性（不是子对象）。
                if (!expr.Type.IsValueType && expr.Type != typeof(string) && expr.NodeType != ExpressionType.Parameter && expr.NodeType != ExpressionType.Constant)
                {
                    var subExp = ExpressionMapper.GetMapper(node.Member.DeclaringType, expr.Type);
                    expDest = subExp.GetLambdaDest(node.Member.Name);
                    return AnalyseDestExpression(expr, expDest);
                }
                expDest = _mapper.GetLambdaDest(node.Member.Name);
                if (expDest != null)
                {
                    return AnalyseDestExpression(expr, expDest);
                }
            }
            return base.VisitMember(node);
        }

        private Expression AnalyseDestExpression(Expression expr, Expression expDest)
        {
            if (expDest.NodeType == ExpressionType.MemberAccess)
            {
                return Expression.MakeMemberAccess(expr, (expDest as MemberExpression).Member);
            }
            return base.Visit(expDest);
        }
    }
}