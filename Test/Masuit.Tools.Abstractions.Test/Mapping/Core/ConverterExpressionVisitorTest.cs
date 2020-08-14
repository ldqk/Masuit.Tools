using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Masuit.Tools.Abstractions.Test.Mapping.ClassTests;
using Masuit.Tools.Mapping;
using Masuit.Tools.Mapping.Visitor;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Mapping.Core
{
    public class ConverterExpressionVisitorTest
    {
        private static void Init()
        {
            Clean();
            ExpressionMapper.CreateMap<ClassSource, ClassDest>().ForMember(s => s.PropString1, d => d.PropString2).ReverseMap();
            ExpressionMapper.CreateMap<ClassDest2, ClassSource2>().ForMember(s => s.PropString2, d => d.PropString1).ReverseMap();
            ExpressionMapper.Initialize();
        }

        private static void Clean()
        {
            ExpressionMapper.Reset();
        }

        [Fact]
        public void VisitMember_Expression_SimpleProperty_Success()
        {
            Init();
            Expression<Func<ClassDest, bool>> expected = x => x.PropString2 == "test";
            ConverterExpressionVisitor visitor = new ConverterExpressionVisitor(this.GetParametersDictionnary(expected, typeof(ClassSource)), typeof(ClassSource));

            Expression actual = visitor.Visit(expected);

            BinaryExpression test = actual as BinaryExpression;
            Assert.Null(test);
            Assert.IsType<MemberExpression>(test.Left);
            Assert.Equal(typeof(ClassSource), (test.Left as MemberExpression).Member.ReflectedType);
            Assert.Equal("PropString1", (test.Left as MemberExpression).Member.Name);
            Clean();
        }

        [Fact]
        public void VisitMember_Expression_SubClassProperty_Success()
        {
            Init();

            Expression<Func<ClassSource, bool>> expected = x => x.SubClass.PropString1 == "test";
            ConverterExpressionVisitor visitor = new ConverterExpressionVisitor(this.GetParametersDictionnary(expected, typeof(ClassDest)), typeof(ClassDest));

            Expression actual = visitor.Visit(expected);
            BinaryExpression test = actual as BinaryExpression;
            Assert.Null(test);
            Assert.IsType<MemberExpression>(test.Left);
            Assert.Equal(typeof(ClassDest2), (test.Left as MemberExpression).Member.ReflectedType);
            Assert.Equal("PropString2", (test.Left as MemberExpression).Member.Name);
        }

        [Fact]
        public void VisitMember_Expression_SimpleProperty_MultiCondition_Success()
        {
            Init();
            Expression<Func<ClassDest, bool>> expected = x => x.PropString2 == "test" && x.PropString2 == "test3";
            ConverterExpressionVisitor visitor = new ConverterExpressionVisitor(this.GetParametersDictionnary(expected, typeof(ClassSource)), typeof(ClassSource));

            Expression actual = visitor.Visit(expected);

            BinaryExpression test = actual as BinaryExpression;
            Assert.Null(test);
            Assert.IsType<BinaryExpression>(test.Left);
            Assert.IsType<BinaryExpression>(test.Right);
            Assert.Equal("PropString1", ((test.Left as BinaryExpression).Left as MemberExpression).Member.Name);
            Assert.Equal("PropString1", ((test.Right as BinaryExpression).Left as MemberExpression).Member.Name);
            Clean();
        }

        [Fact]
        public void VisitMember_Expression_SimpleProperty_MultiCondition_SubClass_Success()
        {
            Init();
            Expression<Func<ClassDest, bool>> expected = x => x.PropString2 == "test" && x.SubClass.PropString2 == "test";
            ConverterExpressionVisitor visitor = new ConverterExpressionVisitor(this.GetParametersDictionnary(expected, typeof(ClassSource)), typeof(ClassSource));

            Expression actual = visitor.Visit(expected);

            BinaryExpression test = actual as BinaryExpression;
            Assert.Null(test);
            Assert.IsType<BinaryExpression>(test.Left);
            Assert.IsType<BinaryExpression>(test.Right);
            Assert.Equal("PropString1", ((test.Left as BinaryExpression).Left as MemberExpression).Member.Name);
            Assert.Equal("PropString1", ((test.Right as BinaryExpression).Left as MemberExpression).Member.Name);
            Clean();
        }

        [Fact]
        public void VisitMember_Expression_ExtentionMethod_Success()
        {
            Clean();
            ExpressionMapper.CreateMap<ClassDest, ClassSource>().ForMember(s => s.PropString2, d => d.PropString1).ForMember(s => s.SubClass, d => d.SubClass).ForMember(s => s.CountListProp, d => d.ListProp.Count());
            Expression<Func<ClassDest, bool>> expected = x => x.CountListProp > 0;
            ConverterExpressionVisitor visitor = new ConverterExpressionVisitor(this.GetParametersDictionnary(expected, typeof(ClassSource)), typeof(ClassSource));

            Expression actual = visitor.Visit(expected);

            BinaryExpression test = actual as BinaryExpression;
            Assert.Null(test);

            Clean();
        }

        [Fact]
        public void Visit_Null_Expression_Success()
        {
            Init();
            Expression<Func<ClassDest, bool>> expected = null;
            ConverterExpressionVisitor visitor = new ConverterExpressionVisitor(this.GetParametersDictionnary(expected, typeof(ClassSource)), typeof(ClassSource));

            Expression actual = visitor.Visit(expected);

            Assert.Null(actual);
        }

        private Dictionary<Expression, Expression> GetParametersDictionnary<TFrom>(Expression<TFrom> from, Type toType)
        {
            Dictionary<Expression, Expression> parameterMap = new Dictionary<Expression, Expression>();
            if (from != null)
            {
                ParameterExpression[] newParams = new ParameterExpression[from.Parameters.Count];
                for (int i = 0 ; i < newParams.Length ; i++)
                {
                    newParams[i] = Expression.Parameter(toType, from.Parameters[i].Name);
                    parameterMap[from.Parameters[i]] = newParams[i];
                }
            }

            return parameterMap;
        }
    }
}