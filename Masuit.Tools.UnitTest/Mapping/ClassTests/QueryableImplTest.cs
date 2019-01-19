using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Masuit.Tools.UnitTest.Mapping.ClassTests
{
    public class QueryableImplTest<T> : IOrderedQueryable<T>
    {
        public QueryableImplTest()
        {
            Provider = new QueryProviderImplTest();
            Expression = Expression.Constant(this);
        }

        public QueryableImplTest(Expression expression)
        : this()
        {
            Expression = expression;
        }
        public Type ElementType => typeof(T);

        public Expression Expression { get; }

        public IQueryProvider Provider { get; }

        public IEnumerator<T> GetEnumerator()
        {
            return Provider.CreateQuery<T>(Expression).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Provider.CreateQuery<T>(Expression).GetEnumerator();
        }
    }
    public class QueryProviderImplTest : IQueryProvider
    {
        public IQueryable CreateQuery(Expression expression)
        {
            return null;
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new QueryableImplTest<TElement>(expression);
        }

        public object Execute(Expression expression)
        {
            return null;
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return default(TResult);
        }
    }
}
