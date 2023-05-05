using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Masuit.Tools.Dynamics;
using Newtonsoft.Json.Linq;

namespace Masuit.Tools
{
    public static class ObjectExtensions
    {
        private static readonly MethodInfo CloneMethod = typeof(object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool IsPrimitive(this Type type)
        {
            if (type == typeof(string))
            {
                return true;
            }

            return type.IsValueType & type.IsPrimitive;
        }

        public static object DeepClone(this object originalObject)
        {
            return InternalCopy(originalObject, new Dictionary<object, object>(new ReferenceEqualityComparer()));
        }

        public static T DeepClone<T>(this T original)
        {
            return (T)DeepClone((object)original);
        }

        private static object InternalCopy(object originalObject, IDictionary<object, object> visited)
        {
            if (originalObject == null)
            {
                return null;
            }

            var typeToReflect = originalObject.GetType();
            if (IsPrimitive(typeToReflect))
            {
                return originalObject;
            }

            if (visited.ContainsKey(originalObject))
            {
                return visited[originalObject];
            }

            if (typeof(Delegate).IsAssignableFrom(typeToReflect))
            {
                return null;
            }

            var cloneObject = CloneMethod.Invoke(originalObject, null);
            if (typeToReflect.IsArray)
            {
                var arrayType = typeToReflect.GetElementType();
                if (!IsPrimitive(arrayType))
                {
                    Array clonedArray = (Array)cloneObject;
                    clonedArray.ForEach((array, indices) => array.SetValue(InternalCopy(clonedArray.GetValue(indices), visited), indices));
                }
            }

            visited.Add(originalObject, cloneObject);
            CopyFields(originalObject, visited, cloneObject, typeToReflect);
            RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect);
            return cloneObject;
        }

        private static void RecursiveCopyBaseTypePrivateFields(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect)
        {
            if (typeToReflect.BaseType != null)
            {
                RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect.BaseType);
                CopyFields(originalObject, visited, cloneObject, typeToReflect.BaseType, BindingFlags.Instance | BindingFlags.NonPublic, info => info.IsPrivate);
            }
        }

        private static void CopyFields(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy, Func<FieldInfo, bool> filter = null)
        {
            foreach (FieldInfo fieldInfo in typeToReflect.GetFields(bindingFlags))
            {
                if (filter != null && !filter(fieldInfo))
                {
                    continue;
                }

                if (IsPrimitive(fieldInfo.FieldType) || fieldInfo.IsInitOnly)
                {
                    continue;
                }

                var originalFieldValue = fieldInfo.GetValue(originalObject);
                var clonedFieldValue = InternalCopy(originalFieldValue, visited);
                fieldInfo.SetValue(cloneObject, clonedFieldValue);
            }
        }

        /// <summary>
        /// 判断是否为null，null或0长度都返回true
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty<T>(this T value)
          where T : class
        {
            #region 1.对象级别

            //引用为null
            bool isObjectNull = value is null;
            if (isObjectNull)
            {
                return true;
            }

            //判断是否为集合
            var tempEnumerator = (value as IEnumerable)?.GetEnumerator();
            if (tempEnumerator == null) return false;//这里出去代表是对象 且 引用不为null.所以为false

            #endregion 1.对象级别

            #region 2.集合级别

            //到这里就代表是集合且引用不为空，判断长度
            //MoveNext方法返回tue代表集合中至少有一个数据,返回false就代表0长度
            bool isZeroLenth = tempEnumerator.MoveNext() == false;
            if (isZeroLenth) return true;

            return isZeroLenth;

            #endregion 2.集合级别
        }

        /// <summary>
        /// 转成非null
        /// </summary>
        /// <param name="s"></param>
        /// <param name="value">为空时的替换值</param>
        /// <returns></returns>
        public static T IfNull<T>(this T s, T value)
        {
            return s ?? value;
        }

        /// <summary>
        /// 转换成json字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="setting"></param>
        /// <returns></returns>
        public static string ToJsonString(this object obj, JsonSerializerSettings setting = null)
        {
            if (obj == null) return string.Empty;
            return JsonConvert.SerializeObject(obj, setting);
        }

        /// <summary>
        /// 链式操作
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="source"></param>
        /// <param name="action"></param>
        public static T2 Next<T1, T2>(this T1 source, Func<T1, T2> action)
        {
            return action(source);
        }

        /// <summary>
        /// 将对象转换成字典
        /// </summary>
        /// <param name="value"></param>
        public static Dictionary<string, string> ToDictionary(this object value)
        {
            var dictionary = new Dictionary<string, string>();
            if (value != null)
            {
                foreach (var property in value.GetType().GetProperties())
                {
                    var obj = property.GetValue(value, null) ?? string.Empty;
                    dictionary.Add(property.Name, obj + string.Empty);
                }
            }

            return dictionary;
        }

        /// <summary>
        /// 将对象转换成字典
        /// </summary>
        /// <param name="value"></param>
        public static Dictionary<string, string> ToDictionary(this JObject value)
        {
            var dictionary = new Dictionary<string, string>();
            if (value != null)
            {
                var enumerator = value.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var obj = enumerator.Current.Value ?? string.Empty;
                    dictionary.Add(enumerator.Current.Key, obj + string.Empty);
                }
            }

            return dictionary;
        }

        public static dynamic ToDynamic(this object obj)
        {
            return DynamicFactory.WithObject(obj);
        }
    }

    internal class ReferenceEqualityComparer : EqualityComparer<object>
    {
        public override bool Equals(object x, object y)
        {
            return ReferenceEquals(x, y);
        }

        public override int GetHashCode(object obj)
        {
            if (obj is null) return 0;
            return obj.GetHashCode();
        }
    }

    internal static class ArrayExtensions
    {
        public static void ForEach(this Array array, Action<Array, int[]> action)
        {
            if (array.LongLength == 0)
            {
                return;
            }

            ArrayTraverse walker = new ArrayTraverse(array);
            do action(array, walker.Position);
            while (walker.Step());
        }

        internal class ArrayTraverse
        {
            public int[] Position;
            private readonly int[] _maxLengths;

            public ArrayTraverse(Array array)
            {
                _maxLengths = new int[array.Rank];
                for (int i = 0; i < array.Rank; ++i)
                {
                    _maxLengths[i] = array.GetLength(i) - 1;
                }
                Position = new int[array.Rank];
            }

            public bool Step()
            {
                for (int i = 0; i < Position.Length; ++i)
                {
                    if (Position[i] < _maxLengths[i])
                    {
                        Position[i]++;
                        for (int j = 0; j < i; j++)
                        {
                            Position[j] = 0;
                        }
                        return true;
                    }
                }
                return false;
            }
        }
    }
}
