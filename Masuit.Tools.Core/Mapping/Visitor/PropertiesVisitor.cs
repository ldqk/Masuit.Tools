using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Masuit.Tools.Mapping.Visitor
{
    internal class PropertiesVisitor : ExpressionVisitor
    {
        private readonly List<PropertyInfo> _propertiesOfExpression;

        private readonly Type _typeReference;

        internal PropertiesVisitor(Type typeReference)
        {
            _typeReference = typeReference;
            _propertiesOfExpression = new List<PropertyInfo>();
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (_typeReference == node.Member.DeclaringType)
            {
                _propertiesOfExpression.Add(node.Member as PropertyInfo);
            }
            return base.VisitMember(node);
        }

        internal List<PropertyInfo> GetProperties(Expression expression)
        {
            Visit(expression);
            return _propertiesOfExpression;
        }
    }
}