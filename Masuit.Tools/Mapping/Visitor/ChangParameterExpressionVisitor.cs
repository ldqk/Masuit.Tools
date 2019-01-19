using System.Linq;
using System.Linq.Expressions;

namespace Masuit.Tools.Mapping.Visitor
{
    internal class ChangParameterExpressionVisitor : ExpressionVisitor
    {
        private readonly Expression[] _parameter;
        internal ChangParameterExpressionVisitor(params Expression[] parameter)
        {
            _parameter = parameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (node != null)
            {
                Expression returnParameter = _parameter.FirstOrDefault(x => x.Type == node.Type);
                if (returnParameter != null)
                    return returnParameter;
            }
            return node;
        }

    }
}
