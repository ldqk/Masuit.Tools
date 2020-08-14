using Masuit.Tools.Linq;
using System;
using System.Linq.Expressions;
using Xunit;

namespace Masuit.Tools.Test
{
    public class LinqExtensionTest
    {
        [Fact]
        public void And_TwoBoolExpression()
        {
            //arrange
            Expression<Func<string, bool>> where1 = s => s.StartsWith("a");
            Expression<Func<string, bool>> where2 = s => s.Length > 10;
            Func<string, bool> func = where1.And(where2).Compile();

            //act assert
            Assert.False(func("abc"));
            Assert.True(func("abcd12345678"));
        }
        [Fact]
        public void Or_TwoBoolExpression()
        {
            //arrange
            Expression<Func<string, bool>> where1 = s => s.StartsWith("a");
            Expression<Func<string, bool>> where2 = s => s.Length > 10;
            Func<string, bool> func = where1.Or(where2).Compile();

            //act assert
            Assert.True(func("abc"));
            Assert.True(func("abcd12345678"));
            Assert.False(func("cbcd12348"));
        }
    }
}