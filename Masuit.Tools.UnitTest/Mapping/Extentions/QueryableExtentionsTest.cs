using Masuit.Tools.Mapping;
using Masuit.Tools.Mapping.Extensions;
using Masuit.Tools.UnitTest.Mapping.ClassTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Masuit.Tools.UnitTest.Mapping.Extentions
{
    [TestClass]
    public class QueryableExtentionsTest
    {
        [ClassInitialize]
        public static void Init(TestContext context)
        {
            Clean();
            ExpressionMapper.CreateMap<ClassSource, ClassDest>().ForMember(s => s.PropString1, d => d.PropString2).ReverseMap();
            ExpressionMapper.Initialize();
        }

        [ClassCleanup]
        public static void Clean()
        {
            ExpressionMapper.Reset();
        }

        [TestMethod, TestCategory("Extentions")]
        public void OrderBy_Success()
        {
            Init(null);

            QueryableImplTest<ClassSource> expected = new QueryableImplTest<ClassSource>();

            IQueryable<ClassSource> actual = expected.OrderBy<ClassSource, ClassDest>("PropInt1");
            Assert.IsTrue(CheckExpressionMethod(actual.Expression, nameof(QueryableExtentions.OrderBy)));
        }

        [TestMethod, TestCategory("Extentions")]
        public void OrderByDescending_Success()
        {
            QueryableImplTest<ClassSource> expected = new QueryableImplTest<ClassSource>();

            IQueryable<ClassSource> actual = expected.OrderByDescending<ClassSource, ClassDest>("PropInt1");
            Assert.IsTrue(CheckExpressionMethod(actual.Expression, nameof(QueryableExtentions.OrderByDescending)));
        }

        [TestMethod, TestCategory("Extentions")]
        public void ThenBy_Success()
        {
            ExpressionMapper.CreateMap<ClassSource, ClassDest>().ForMember(s => s.PropString1, d => d.PropString2);
            ExpressionMapper.Initialize();

            QueryableImplTest<ClassSource> expected = new QueryableImplTest<ClassSource>();

            IQueryable<ClassSource> actual = expected.OrderByDescending<ClassSource, ClassDest>("PropInt1").ThenBy<ClassSource, ClassDest>("PropInt1");
            Assert.IsTrue(CheckExpressionMethod(actual.Expression, nameof(QueryableExtentions.ThenBy)));
        }

        [TestMethod, TestCategory("Extentions")]
        public void ThenByDescending_Success()
        {
            Init(null);

            QueryableImplTest<ClassSource> expected = new QueryableImplTest<ClassSource>();

            IQueryable<ClassSource> actual = expected.OrderByDescending<ClassSource, ClassDest>("PropInt1").ThenByDescending<ClassSource, ClassDest>("PropInt1");
            Assert.IsTrue(CheckExpressionMethod(actual.Expression, nameof(QueryableExtentions.ThenByDescending)));
        }

        [TestMethod, TestCategory("Extentions")]
        public void Select_Success()
        {
            Init(null);

            QueryableImplTest<ClassSource> expected = new QueryableImplTest<ClassSource>();

            var actual = expected.Select<ClassSource, ClassDest>();
            Assert.IsTrue(CheckExpressionMethod(actual.Expression, nameof(QueryableExtentions.Select)));
        }

        [TestMethod, TestCategory("Extentions")]
        public void Where_Success()
        {
            QueryableImplTest<ClassDest> expected = new QueryableImplTest<ClassDest>();
            Expression<Func<ClassSource, bool>> criterias = x => x.PropInt1 == 1;
            var actual = expected.Where(criterias);
            Assert.IsTrue(CheckExpressionMethod(actual.Expression, nameof(QueryableExtentions.Where)));
        }

        private bool CheckExpressionMethod(Expression expression, string methodeName)
        {
            if (expression.NodeType == ExpressionType.Call)
            {
                return (expression as MethodCallExpression).Method.Name == methodeName;
            }

            return false;
        }
    }
}