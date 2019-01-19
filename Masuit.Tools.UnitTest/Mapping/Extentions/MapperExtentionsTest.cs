using Masuit.Tools.Mapping;
using Masuit.Tools.Mapping.Extensions;
using Masuit.Tools.UnitTest.Mapping.ClassTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq.Expressions;

namespace Masuit.Tools.UnitTest.Mapping.Extentions
{
    [TestClass]
    public class MapperExtentionsTest
    {
        [ClassInitialize]
        public static void Init(TestContext context)
        {
            Clean();
            ExpressionMapper.CreateMap<ClassSource, ClassDest>().ForMember(s => s.PropString1, d => d.PropString2).ReverseMap();
            ExpressionMapper.CreateMap<ClassDest2, ClassSource2>().ForMember(s => s.PropString2, d => d.PropString1).ReverseMap();
            ExpressionMapper.Initialize();
        }

        [ClassCleanup]
        public static void Clean()
        {
            ExpressionMapper.Reset();
        }

        [TestMethod]
        public void ConvertTo_SimpleExpression_Success()
        {
            Expression<Func<ClassDest, bool>> expected = x => x.PropString2 == "test";

            var actual = expected.ConvertTo<ClassDest, ClassSource>() as LambdaExpression;
            var test = actual.Body as BinaryExpression;
            Assert.IsNotNull(actual);
            Assert.IsInstanceOfType(test.Left, typeof(MemberExpression));
            Assert.AreEqual((test.Left as MemberExpression).Member.ReflectedType, typeof(ClassSource));
            Assert.AreEqual((test.Left as MemberExpression).Member.Name, "PropString1");
        }

        [TestMethod]
        public void ConvertTo_WithType()
        {
            Clean();
            ExpressionMapper.CreateMap<ClassSource, ClassDest>().ForMember(s => s.PropString1, d => d.PropString2).ReverseMap();
            Expression<Func<ClassDest, bool>> expected = x => x.PropString2 == "test";

            var actual = expected.ConvertTo(typeof(ClassSource)) as LambdaExpression;
            var test = actual.Body as BinaryExpression;

            Assert.IsNotNull(actual);
            Assert.IsInstanceOfType(test.Left, typeof(MemberExpression));
            Assert.AreEqual((test.Left as MemberExpression).Member.ReflectedType, typeof(ClassSource));
            Assert.AreEqual((test.Left as MemberExpression).Member.Name, "PropString1");
        }
    }
}