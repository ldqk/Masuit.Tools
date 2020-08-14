using System;
using System.Linq.Expressions;
using Masuit.Tools.Abstractions.Test.Mapping.ClassTests;
using Masuit.Tools.Mapping.Visitor;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Mapping.Core
{
    public class MapperExpressionVisitorTest
    {
        [Fact(DisplayName = "Visit")]
        public void Visit_ExpressionIsNull_ReturnNull()
        {
            MapperExpressionVisitor expected = new MapperExpressionVisitor(Expression.Parameter(typeof(ClassSource), "s"));
            Expression exp = null;

            Expression actual = expected.Visit(exp);

            Assert.Null(actual);
        }

        [Fact(DisplayName = "Visit")]
        public void Visit_ExpressionLambdaConstantSimple_ReturnExpression()
        {
            MapperExpressionVisitor expected = new MapperExpressionVisitor(Expression.Parameter(typeof(ClassSource), "s"));
            Expression<Func<ClassSource, object>> exp = x => x.PropInt1;

            Expression actual = expected.Visit(exp, true);

            Assert.Null(actual);
        }

        [Fact(DisplayName = "Visit")]
        public void Visit_ExpressionConstantSimple_ReturnExpression()
        {
            MapperExpressionVisitor expected = new MapperExpressionVisitor(Expression.Parameter(typeof(ClassSource), "s"));
            Expression<Func<ClassSource, object>> exp = x => x.PropInt1;

            Expression actual = expected.Visit(exp.Body, true);

            Assert.Null(actual);
        }

        [Fact(DisplayName = "Visit")]
        public void Visit_ExpressionMemberSimple_ReturnExpression()
        {
            MapperExpressionVisitor expected = new MapperExpressionVisitor(Expression.Parameter(typeof(ClassSource), "s"));
            Expression<Func<ClassSource, object>> exp = x => x.PropString1;

            Expression actual = expected.Visit(exp.Body, true);

            Assert.Null(actual);
        }

        [Fact(DisplayName = "Visit")]
        public void Visit_ExpressionSubClassCheckIfNull_ReturnExpression()
        {
            MapperExpressionVisitor expected = new MapperExpressionVisitor(Expression.Parameter(typeof(ClassSource), "s"));
            Expression<Func<ClassSource, object>> exp = x => x.SubClass.PropString1;

            Expression actual = expected.Visit(exp, true);

            Assert.True(actual.NodeType == ExpressionType.MemberAccess);
        }

        [Fact(DisplayName = "Visit")]
        public void Visit_ExpressionSubClassCheckIfNull_DefaultValueConstant_ReturnExpression()
        {
            MapperExpressionVisitor expected = new MapperExpressionVisitor(Expression.Parameter(typeof(ClassSource), "s"));
            Expression<Func<ClassSource, object>> exp = x => x.SubClass.PropInt1;

            Expression actual = expected.Visit(exp, true);

            Assert.True(actual.NodeType == ExpressionType.MemberAccess);
        }

        [Fact(DisplayName = "Visit")]
        public void Visit_ParameterExpression_CheckIfNull_IsTrue_Expression()
        {
            MapperExpressionVisitor expected = new MapperExpressionVisitor(Expression.Parameter(typeof(ClassSource), "s"));
            ParameterExpression exp = Expression.Parameter(typeof(ClassSource2), "x");

            Expression actual = expected.Visit(exp, true) as ParameterExpression;

            Assert.True(actual.NodeType == ExpressionType.Parameter);
            Assert.NotEqual(exp.Type, actual.Type);
        }
    }
}