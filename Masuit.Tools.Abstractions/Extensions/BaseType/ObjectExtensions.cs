using Newtonsoft.Json;
using System;
using System.Collections;

namespace Masuit.Tools
{
    public static class ObjectExtensions
    {
        public static bool IsPrimitive(this Type type)
        {
            if (type == typeof(string))
            {
                return true;
            }

            return type.IsValueType & type.IsPrimitive;
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
            IEnumerator? tempEnumerator = (value as IEnumerable)?.GetEnumerator();
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
        /// 严格比较两个对象是否是同一对象(判断引用)
        /// </summary>
        /// <param name="this">自己</param>
        /// <param name="o">需要比较的对象</param>
        /// <returns>是否同一对象</returns>
        public new static bool ReferenceEquals(this object @this, object o)
        {
            return object.ReferenceEquals(@this, o);
        }
    }
}