using Newtonsoft.Json;
using System;
using System.Collections;
using System.Threading.Tasks;

namespace Masuit.Tools
{
    public static partial class ObjectExtensions
    {
        #region Map

        /// <summary>
        /// 浅克隆
        /// </summary>
        /// <param name="source">源</param>
        /// <typeparam name="TDestination">目标类型</typeparam>
        /// <returns>目标类型</returns>
        public static TDestination Clone<TDestination>(this object source)
            where TDestination : new()
        {
            TDestination dest = new TDestination();
            dest.GetType().GetProperties().ForEach(p =>
            {
                p.SetValue(dest, source.GetType().GetProperty(p.Name)?.GetValue(source));
            });
            return dest;
        }

        /// <summary>
        /// 深克隆
        /// </summary>
        /// <param name="source">源</param>
        /// <typeparam name="TDestination">目标类型</typeparam>
        /// <returns>目标类型</returns>
        public static TDestination DeepClone<TDestination>(this object source)
            where TDestination : new()
        {
            return JsonConvert.DeserializeObject<TDestination>(JsonConvert.SerializeObject(source));
        }

        /// <summary>
        /// 浅克隆(异步)
        /// </summary>
        /// <param name="source">源</param>
        /// <typeparam name="TDestination">目标类型</typeparam>
        /// <returns>目标类型</returns>
        public static Task<TDestination> CloneAsync<TDestination>(this object source)
            where TDestination : new()
        {
            return Task.Run(() =>
           {
               return source.Clone<TDestination>();
           });
        }

        /// <summary>
        /// 深克隆(异步)
        /// </summary>
        /// <param name="source">源</param>
        /// <typeparam name="TDestination">目标类型</typeparam>
        /// <returns>目标类型</returns>
        public static async Task<TDestination> DeepCloneAsync<TDestination>(this object source)
            where TDestination : new()
        {
            return await Task.Run(() => JsonConvert.DeserializeObject<TDestination>(JsonConvert.SerializeObject(source)));
        }

        #endregion Map

        #region CheckNull

        /// <summary>
        /// 检查参数是否为null，为null时抛出异常
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">      要检查的对象</param>
        /// <param name="paramName">抛出异常时,显示的参数名</param>
        /// <exception cref="ArgumentNullException"><paramref name="obj" /> 为null时抛出</exception>
        public static void CheckNullWithException<T>(this T obj, string paramName)
        {
            if (obj == null) throw new ArgumentNullException(paramName);
        }

        /// <summary>
        /// 检查参数是否为null，为null时抛出异常
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">      要检查的对象</param>
        /// <param name="paramName">抛出异常时,显示的参数名</param>
        /// <param name="message">  抛出异常时,显示的错误信息</param>
        /// <exception cref="ArgumentNullException"><paramref name="obj" /> 为null时抛出</exception>
        public static void CheckNullWithException<T>(this T obj, string paramName, string message)
        {
            if (obj == null) throw new ArgumentNullException(paramName, message);
        }

        /// <summary>
        /// 检查参数是否为null或emtpy，为null或emtpy时抛出异常
        /// </summary>
        /// <param name="obj">      要检查的对象</param>
        /// <param name="paramName">抛出异常时,显示的参数名</param>
        /// <exception cref="ArgumentNullException"><paramref name="obj" /> 为null或emtpy时抛出</exception>
        public static void CheckNullOrEmptyWithException(this IEnumerable obj, string paramName)
        {
            if (obj.IsNullOrEmpty()) throw new ArgumentNullException(paramName);
        }

        /// <summary>
        /// 检查参数是否为null或emtpy，为null或emtpy时抛出异常
        /// </summary>
        /// <param name="obj">      要检查的对象</param>
        /// <param name="paramName">抛出异常时,显示的参数名</param>
        /// <param name="message">  抛出异常时,显示的错误信息</param>
        /// <exception cref="ArgumentNullException"><paramref name="obj" /> 为null或emtpy时抛出</exception>
        public static void CheckNullOrEmptyWithException(this IEnumerable obj, string paramName, string message)
        {
            if (obj.IsNullOrEmpty()) throw new ArgumentNullException(paramName, message);
        }

        #endregion CheckNull

        /// <summary>
        /// 判断是否为null，null或0长度都返回true
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty<T>(this T value)
          where T : class?
        {
            #region 1.对象级别

            //引用为null
            bool isObjectNull = value == null;
            if (isObjectNull == true) return true;

            //判断是否为集合
            IEnumerator? tempEnumerator = (value as IEnumerable)?.GetEnumerator();
            if (tempEnumerator == null) return false;//这里出去代表是对象 且 引用不为null.所以为false

            #endregion 1.对象级别

            #region 2.集合级别

            //到这里就代表是集合且引用不为空，判断长度
            //MoveNext方法返回tue代表集合中至少有一个数据,返回false就代表0长度
            bool isZeroLenth = tempEnumerator.MoveNext() == false;
            if (isZeroLenth == true) return true;

            return isZeroLenth;

            #endregion 2.集合级别
        }

        /// <summary>
        /// 转换成json字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToJsonString(this object obj, JsonSerializerSettings setting = null)
        {
            if (obj == null) return string.Empty;

            if (setting == null)
            {
                return JsonConvert.SerializeObject(obj);
            }

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