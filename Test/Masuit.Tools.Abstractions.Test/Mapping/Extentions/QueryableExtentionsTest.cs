using System.Linq;
using System.Linq.Expressions;
using Masuit.Tools.Abstractions.Test.Mapping.ClassTests;
using Masuit.Tools.Mapping;
using Masuit.Tools.Mapping.Extensions;
using Xunit;

namespace Masuit.Tools.UnitTest.Mapping.Extentions
{
    public class QueryableExtentionsTest
    {
        private static void Init()
        {
            Clean();
            ExpressionMapper.CreateMap<ClassSource, ClassDest>().ForMember(s => s.PropString1, d => d.PropString2).ReverseMap();
            ExpressionMapper.Initialize();
        }

        private static void Clean()
        {
            ExpressionMapper.Reset();
        }

        [Fact(DisplayName = "Extentions")]
        public void OrderBy_Success()
        {
            Init();

            QueryableImplTest<ClassSource> expected = new QueryableImplTest<ClassSource>();

            IQueryable<ClassSource> actual = expected.OrderBy<ClassSource, ClassDest>("PropInt1");
            Assert.True(this.CheckExpressionMethod(actual.Expression, nameof(QueryableExtentions.OrderBy)));
        }

        [Fact(DisplayName = "Extentions")]
        public void OrderByDescending_Success()
        {
            QueryableImplTest<ClassSource> expected = new QueryableImplTest<ClassSource>();

            IQueryable<ClassSource> actual = expected.OrderByDescending<ClassSource, ClassDest>("PropInt1");
            Assert.True(this.CheckExpressionMethod(actual.Expression, nameof(QueryableExtentions.OrderByDescending)));
        }

        [Fact(DisplayName = "Extentions")]
        public void ThenBy_Success()
        {
            ExpressionMapper.CreateMap<ClassSource, ClassDest>().ForMember(s => s.PropString1, d => d.PropString2);
            ExpressionMapper.Initialize();

            QueryableImplTest<ClassSource> expected = new QueryableImplTest<ClassSource>();

            IQueryable<ClassSource> actual = expected.OrderByDescending<ClassSource, ClassDest>("PropInt1").ThenBy<ClassSource, ClassDest>("PropInt1");
            Assert.True(this.CheckExpressionMethod(actual.Expression, nameof(QueryableExtentions.ThenBy)));
        }

        [Fact(DisplayName = "Extentions")]
        public void ThenByDescending_Success()
        {
            Init();

            QueryableImplTest<ClassSource> expected = new QueryableImplTest<ClassSource>();

            IQueryable<ClassSource> actual = expected.OrderByDescending<ClassSource, ClassDest>("PropInt1").ThenByDescending<ClassSource, ClassDest>("PropInt1");
            Assert.True(this.CheckExpressionMethod(actual.Expression, nameof(QueryableExtentions.ThenByDescending)));
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