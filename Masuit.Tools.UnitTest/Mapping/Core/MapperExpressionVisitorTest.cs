using Masuit.Tools.Mapping.Visitor;
using Masuit.Tools.UnitTest.Mapping.ClassTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq.Expressions;

namespace Masuit.Tools.UnitTest.Mapping.Core
{
    [TestClass]
    public class MapperExpressionVisitorTest
    {
        [TestMethod, TestCategory("Visit")]
        public void Visit_ExpressionIsNull_ReturnNull()
        {
            MapperExpressionVisitor expected = new MapperExpressionVisitor(Expression.Parameter(typeof(ClassSource), "s"));
            Expression exp = null;

            var actual = expected.Visit(exp);

            Assert.IsNull(actual);
        }
        [TestMethod, TestCategory("Visit")]
        public void Visit_ExpressionLambdaConstantSimple_ReturnExpression()
        {
            MapperExpressionVisitor expected = new MapperExpressionVisitor(Expression.Parameter(typeof(ClassSource), "s"));
            Expression<Func<ClassSource, object>> exp = x => x.PropInt1;

            var actual = expected.Visit(exp, true);

            Assert.IsNotNull(actual);
        }
        [TestMethod, TestCategory("Visit")]
        public void Visit_ExpressionConstantSimple_ReturnExpression()
        {
            MapperExpressionVisitor expected = new MapperExpressionVisitor(Expression.Parameter(typeof(ClassSource), "s"));
            Expression<Func<ClassSource, object>> exp = x => x.PropInt1;

            var actual = expected.Visit(exp.Body, true);

            Assert.IsNotNull(actual);
        }
        [TestMethod, TestCategory("Visit")]
        public void Visit_ExpressionMemberSimple_ReturnExpression()
        {
            MapperExpressionVisitor expected = new MapperExpressionVisitor(Expression.Parameter(typeof(ClassSource), "s"));
            Expression<Func<ClassSource, object>> exp = x => x.PropString1;

            var actual = expected.Visit(exp.Body, true);

            Assert.IsNotNull(actual);
        }

        [TestMethod, TestCategory("Visit")]
        public void Visit_ExpressionSubClassCheckIfNull_ReturnExpression()
        {
            MapperExpressionVisitor expected = new MapperExpressionVisitor(Expression.Parameter(typeof(ClassSource), "s"));
            Expression<Func<ClassSource, object>> exp = x => x.SubClass.PropString1;

            var actual = expected.Visit(exp, true);

            Assert.IsTrue(actual.NodeType == ExpressionType.MemberAccess);
        }
        [TestMethod, TestCategory("Visit")]
        public void Visit_ExpressionSubClassCheckIfNull_DefaultValueConstant_ReturnExpression()
        {
            MapperExpressionVisitor expected = new MapperExpressionVisitor(Expression.Parameter(typeof(ClassSource), "s"));
            Expression<Func<ClassSource, object>> exp = x => x.SubClass.PropInt1;

            var actual = expected.Visit(exp, true);

            Assert.IsTrue(actual.NodeType == ExpressionType.MemberAccess);
        }
        [TestMethod, TestCategory("Visit")]
        public void Visit_ParameterExpression_CheckIfNull_IsTrue_Expression()
        {
            MapperExpressionVisitor expected = new MapperExpressionVisitor(Expression.Parameter(typeof(ClassSource), "s"));
            ParameterExpression exp = Expression.Parameter(typeof(ClassSource2), "x");

            Expression actual = expected.Visit(exp, true) as ParameterExpression;

            Assert.IsTrue(actual.NodeType == ExpressionType.Parameter);
            Assert.AreNotEqual(actual.Type, exp.Type);
        }
    }
}
