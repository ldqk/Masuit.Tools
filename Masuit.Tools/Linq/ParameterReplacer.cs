using System.Linq.Expressions;

namespace Masuit.Tools.Core.Linq
{
    /// <summary>
    /// linq参数替换器
    /// </summary>
    public class ParameterReplacer : ExpressionVisitor
    {
        /// <summary>
        /// linq参数替换器
        /// </summary>
        /// <param name="paramExpr"></param>
        public ParameterReplacer(ParameterExpression paramExpr)
        {
            this.ParameterExpression = paramExpr;
        }

        /// <summary>
        /// 参数表达式
        /// </summary>
        public ParameterExpression ParameterExpression { get; private set; }

        /// <summary>
        /// 表达式替换
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        public Expression Replace(Expression expr)
        {
            return this.Visit(expr);
        }
        /// <summary>
        /// 表达式参数访问
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        protected override Expression VisitParameter(ParameterExpression p)
        {
            return this.ParameterExpression;
        }
    }
}