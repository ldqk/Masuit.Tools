using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Masuit.Tools.Mapping
{
    /// <summary>
    /// 表达式树复制对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class Copier<T>
    {
        private static readonly ParameterExpression ParameterExpression = Expression.Parameter(typeof(T), "p");
        private static Func<T, T> _func;
        private static readonly Dictionary<string, Expression> DictRule = new Dictionary<string, Expression>();

        /// <summary>
        /// 深拷贝
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T Copy(T source)
        {
            if (_func == null)
            {
                List<MemberBinding> memberBindings = new List<MemberBinding>();
                foreach (var item in typeof(T).GetProperties())
                {
                    if (DictRule.ContainsKey(item.Name))
                    {
                        MemberBinding memberBinding = Expression.Bind(item, DictRule[item.Name]);
                        memberBindings.Add(memberBinding);
                    }
                    else
                    {
                        var tInProperty = typeof(T).GetProperty(item.Name);
                        var tInField = typeof(T).GetField(item.Name);
                        if (tInProperty != null || tInField != null)
                        {
                            MemberExpression property = Expression.PropertyOrField(ParameterExpression, item.Name);
                            MemberBinding memberBinding = Expression.Bind(item, property);
                            memberBindings.Add(memberBinding);
                        }
                    }
                }

                foreach (var item in typeof(T).GetFields())
                {
                    if (DictRule.ContainsKey(item.Name))
                    {
                        MemberBinding memberBinding = Expression.Bind(item, DictRule[item.Name]);
                        memberBindings.Add(memberBinding);
                    }
                    else
                    {
                        var tInProperty = typeof(T).GetProperty(item.Name);
                        var tInField = typeof(T).GetField(item.Name);
                        if (tInProperty != null || tInField != null)
                        {
                            MemberExpression property = Expression.PropertyOrField(ParameterExpression, item.Name);
                            MemberBinding memberBinding = Expression.Bind(item, property);
                            memberBindings.Add(memberBinding);
                        }
                    }
                }

                MemberInitExpression memberInitExpression = Expression.MemberInit(Expression.New(typeof(T)), memberBindings.ToArray());
                Expression<Func<T, T>> lambda = Expression.Lambda<Func<T, T>>(memberInitExpression, ParameterExpression);
                _func = lambda.Compile();
            }
            return _func.Invoke(source);
        }
    }
}