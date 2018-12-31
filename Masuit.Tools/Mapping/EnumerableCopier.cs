using System;
using System.Collections.Generic;
using System.Reflection;

namespace Masuit.Tools
{
    /// <summary>
    /// 可遍历类型拷贝器
    /// </summary>
    internal class EnumerableCopier
    {
        private static readonly MethodInfo _copyArrayMethodInfo;

        private static readonly MethodInfo _copyICollectionMethodInfo;

        private static readonly Type _typeICollection = typeof(ICollection<>);

        static EnumerableCopier()
        {
            Type type = typeof(EnumerableCopier);
            _copyArrayMethodInfo = type.GetMethod(nameof(CopyArray));
            _copyICollectionMethodInfo = type.GetMethod(nameof(CopyICollection));
        }

        /// <summary>
        /// 根据IEnumerable的实现类类型选择合适的拷贝方法
        /// </summary>
        /// <param name="type">IEnumerable的实现类类型</param>
        /// <returns>拷贝方法信息</returns>
        public static MethodInfo GetMethondInfo(Type type)
        {
            if (type.IsArray)
            {
                return _copyArrayMethodInfo.MakeGenericMethod(type.GetElementType());
            }

            if (type.GetGenericArguments().Length > 0)
            {
                Type elementType = type.GetGenericArguments()[0];
                if (_typeICollection.MakeGenericType(elementType).IsAssignableFrom(type))
                {
                    return _copyICollectionMethodInfo.MakeGenericMethod(type, elementType);
                }
            }

            throw new UnsupportedTypeException(type);
        }

        /// <summary>
        /// 拷贝List
        /// </summary>
        /// <typeparam name="T">源ICollection实现类类型</typeparam>
        /// <typeparam name="TElement">源ICollection元素类型</typeparam>
        /// <param name="source">源ICollection对象</param>
        /// <returns>深拷贝完成的ICollection对象</returns>
        public static T CopyICollection<T, TElement>(T source) where T : ICollection<TElement> where TElement : class
        {
            T result = (T)MapperTools.CreateNewInstance(source.GetType());

            if (MapperTools.IsRefTypeExceptString(typeof(TElement)))
            {
                foreach (TElement item in source)
                {
                    result.Add(ExpressionGenericMapper<TElement, TElement>.Copy(item));
                }
            }
            else
            {
                foreach (TElement item in source)
                {
                    result.Add(item);
                }
            }

            return result;
        }

        /// <summary>
        /// 拷贝数组
        /// </summary>
        /// <typeparam name="TElement">源数组元素类型</typeparam>
        /// <param name="source">源List</param>
        /// <returns>深拷贝完成的数组</returns>
        public static TElement[] CopyArray<TElement>(TElement[] source) where TElement : class
        {
            TElement[] result = new TElement[source.Length];
            if (MapperTools.IsRefTypeExceptString(typeof(TElement)))
            {
                for (int i = 0; i < source.Length; i++)
                {
                    result[i] = ExpressionGenericMapper<TElement, TElement>.Copy(source[i]);
                }
            }
            else
            {
                for (int i = 0; i < source.Length; i++)
                {
                    result[i] = source[i];
                }
            }

            return result;
        }
    }
}