using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;

namespace Masuit.Tools
{
    /// <summary>
    /// 工具类
    /// </summary>
    internal static class MapperTools
    {
        private static readonly Type _typeString = typeof(string);

        private static readonly Type _typeIEnumerable = typeof(IEnumerable);

        private static readonly ConcurrentDictionary<Type, Func<object>> _ctors = new ConcurrentDictionary<Type, Func<object>>();

        /// <summary>
        /// 判断是否是string以外的引用类型
        /// </summary>
        /// <returns>True：是string以外的引用类型，False：不是string以外的引用类型</returns>
        public static bool IsRefTypeExceptString(Type type) => !type.IsValueType && type != _typeString;

        /// <summary>
        /// 判断是否是string以外的可遍历类型
        /// </summary>
        /// <returns>True：是string以外的可遍历类型，False：不是string以外的可遍历类型</returns>
        public static bool IsIEnumerableExceptString(Type type) => _typeIEnumerable.IsAssignableFrom(type) && type != _typeString;

        /// <summary>
        /// 创建指定类型实例
        /// </summary>
        /// <param name="type">类型信息</param>
        /// <returns>指定类型的实例</returns>
        public static object CreateNewInstance(Type type) => _ctors.GetOrAdd(type, t => Expression.Lambda<Func<object>>(Expression.New(t)).Compile())();

        public static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static bool IsEnumerable(Type type)
        {
            return type.IsArray || type.GetInterfaces().Any(x => x == typeof(ICollection) || x == typeof(IEnumerable));
        }

    }
}