using System.Linq.Expressions;

namespace Masuit.Tools.Linq
{
    /// <summary>
    /// 表达式树参数替换
    /// </summary>
    public class ParameterReplacer : ExpressionVisitor
    {
        public ParameterReplacer(ParameterExpression paramExpr)
        {
            this.ParameterExpression = paramExpr;
        }

        /// <summary>
        /// 参数表达式树
        /// </summary>
        public ParameterExpression ParameterExpression { get; private set; }

        /// <summary>
        /// 替换表达式树
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        public Expression Replace(Expression expr)
        {
            return this.Visit(expr);
        }

        protected override Expression VisitParameter(ParameterExpression p)
        {
            return this.ParameterExpression;
        }
    }
}