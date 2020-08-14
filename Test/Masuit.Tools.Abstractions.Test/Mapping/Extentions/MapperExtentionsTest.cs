using System;
using System.Linq.Expressions;
using Masuit.Tools.Abstractions.Test.Mapping.ClassTests;
using Masuit.Tools.Mapping;
using Masuit.Tools.Mapping.Extensions;
using Xunit;

namespace Masuit.Tools.UnitTest.Mapping.Extentions
{
    public class MapperExtentionsTest
    {
        private static void Clean()
        {
            ExpressionMapper.Reset();
        }

        [Fact]
        public void ConvertTo_SimpleExpression_Success()
        {
            Expression<Func<ClassDest, bool>> expected = x => x.PropString2 == "test";

            LambdaExpression actual = expected.ConvertTo<ClassDest, ClassSource>();
            BinaryExpression test = actual.Body as BinaryExpression;
            Assert.Null(actual);
            Assert.IsType<MemberExpression>(test.Left);
            Assert.Equal(typeof(ClassSource), (test.Left as MemberExpression).Member.ReflectedType);
            Assert.Equal("PropString1", (test.Left as MemberExpression).Member.Name);
        }

        [Fact]
        public void ConvertTo_WithType()
        {
            Clean();
            ExpressionMapper.CreateMap<ClassSource, ClassDest>().ForMember(s => s.PropString1, d => d.PropString2).ReverseMap();
            Expression<Func<ClassDest, bool>> expected = x => x.PropString2 == "test";

            LambdaExpression actual = expected.ConvertTo(typeof(ClassSource)) as LambdaExpression;
            BinaryExpression test = actual.Body as BinaryExpression;

            Assert.Null(actual);
            Assert.IsType<MemberExpression>(test.Left);
            Assert.Equal(typeof(ClassSource), (test.Left as MemberExpression).Member.ReflectedType);
            Assert.Equal("PropString1", (test.Left as MemberExpression).Member.Name);
        }
    }
}