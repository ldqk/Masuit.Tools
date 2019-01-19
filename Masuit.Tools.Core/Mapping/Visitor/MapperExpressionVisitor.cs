using Masuit.Tools.Mapping.Helper;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Masuit.Tools.Mapping.Visitor
{
    /// <summary>
    /// mapper表达式树访问器
    /// </summary>
    public class MapperExpressionVisitor : ExpressionVisitor
    {
        private bool _checkNull;
        readonly Stack<MemberExpression> _membersToCheck;

        internal Expression Parameter { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="paramClassSource"></param>
        public MapperExpressionVisitor(Expression paramClassSource)
        {
            Parameter = paramClassSource;
            _membersToCheck = new Stack<MemberExpression>();
        }

        /// <summary>
        /// 将表达式分布在此类中更专一的访问方法之一。
        /// </summary>
        /// <param name="node">访问表达式树</param>
        /// <param name="checkIfNullity">检查null值</param>
        /// <returns>
        /// 改变表达式，如果它或它的任何子表达式被修改; 否则，返回原始表达式。
        /// </returns>
        public Expression Visit(Expression node, bool checkIfNullity = false)
        {
            _checkNull = checkIfNullity;
            if (node == null)
            {
                return node;
            }

            if (_checkNull)
            {
                Expression result;
                switch (node.NodeType)
                {
                    case ExpressionType.MemberAccess:
                        result = VisitMember(node as MemberExpression);
                        break;
                    case ExpressionType.Parameter:
                        result = VisitParameter(node as ParameterExpression);
                        break;
                    case ExpressionType.Convert:
                        result = VisitMember((node as UnaryExpression).Operand as MemberExpression);
                        break;
                    case ExpressionType.Lambda:
                        LambdaExpression lambda = ((LambdaExpression)node);
                        // 子表达式树
                        if (lambda.Body.NodeType != ExpressionType.Lambda)
                        {
                            result = Visit(lambda.Body);
                        }
                        else
                        {
                            return lambda;
                        }

                        break;
                    default:
                        result = base.Visit(node);
                        break;
                }

                bool isFirst = true;
                Expression previousExpression = null;
                if (_membersToCheck.Count > 1)
                {
                    // 在分配值之前测试所有子对象。例如：source.SubClass.SubClass2.MyProperty。是哪个会给：
                    // source.SubClass!= null? source.SubClass.SubClass2!= null? source.SubClass.SubClass2.MyProperty:DefaultValueOfProperty:DefaultValueOfProperty
                    foreach (MemberExpression item in _membersToCheck)
                    {
                        if (!isFirst) // 不要测试该属性的值。
                        {
                            object defaultValue = MapperHelper.GetDefaultValue(item.Type);

                            // 创建默认值的验证。
                            Expression notDefaultValue = Expression.NotEqual(item, Expression.Constant(defaultValue, item.Type));
                            Expression conditional = null;
                            // 它创建了包含上述条件的条件。
                            if (previousExpression != null)
                            {
                                object defaultPreviousValue = MapperHelper.GetDefaultValue(previousExpression.Type);
                                conditional = Expression.Condition(notDefaultValue, previousExpression, Expression.Constant(defaultPreviousValue, previousExpression.Type));
                            }

                            // 它会影响新创建的条件，这些条件将成为之前的条件。
                            previousExpression = conditional;
                        }
                        else // 属性
                        {
                            previousExpression = item;
                            isFirst = false;
                        }
                    }

                    return previousExpression;
                }

                // 不需要递归的元素。
                if (_membersToCheck.Count == 1)
                {
                    var item = _membersToCheck.Peek();
                    object defaultValue = MapperHelper.GetDefaultValue(item.Type);
                    // 创建默认值的验证。
                    Expression notDefaultValue = Expression.NotEqual(item, Expression.Constant(defaultValue, item.Type));
                    Expression conditional = Expression.Condition(notDefaultValue, item, Expression.Constant(defaultValue, item.Type));

                    return conditional;
                }

                return result;
            }

            // 默认返回（更改参数），删除lambda表达式的验证。
            if ((node.NodeType == ExpressionType.Lambda))
            {
                LambdaExpression lambda = ((LambdaExpression)node);
                // 子表达式树
                if (lambda.Body.NodeType != ExpressionType.Call)
                {
                    return base.Visit(lambda.Body);
                }

                return lambda;
            }

            return base.Visit(node);
        }

        /// <summary>
        /// 访问表达式树
        /// </summary>
        /// <param name="node">表达式树节点</param>
        /// <returns>
        /// 改变表达式，如果它或它的任何子表达式被修改; 否则，返回原始表达式。
        /// </returns>
        protected override Expression VisitParameter(ParameterExpression node)
        {
            return Parameter;
        }

        /// <summary>
        /// 访问子表达式树
        /// </summary>
        /// <param name="node">表达式树节点</param>
        /// <returns>
        /// 改变表达式，如果它或它的任何子表达式被修改; 否则，返回原始表达式。
        /// </returns>
        protected override Expression VisitMember(MemberExpression node)
        {
            if (node == null)
            {
                return node;
            }

            if (node.Member.ReflectedType != Parameter.Type)
            {
                var exp = Visit(node.Expression);
                return Expression.MakeMemberAccess(exp, node.Member);
            }

            MemberExpression memberAccessExpression = (MemberExpression)base.VisitMember(node);

            // 稍后处理
            if (memberAccessExpression != null && _checkNull)
            {
                // 如果最后一个成员是第一次访问，那么每次都得回去，当前的插入成员位于列表的第一行以更改顺序。
                _membersToCheck.Push(memberAccessExpression);
            }

            return memberAccessExpression;
        }

        /// <summary>
        ///  访问子表达式树
        /// </summary>
        /// <param name="node">表达式树节点</param>
        /// <returns>
        /// 改变表达式，如果它或它的任何子表达式被修改; 否则，返回原始表达式。
        /// </returns>
        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node != null)
            {
                if (node.Operand.NodeType == ExpressionType.MemberAccess)
                {
                    return VisitMember(node.Operand as MemberExpression);
                }

                if (node.NodeType == ExpressionType.Convert)
                {
                    return Visit(node.Operand);
                }
            }

            return node;
        }
    }
}