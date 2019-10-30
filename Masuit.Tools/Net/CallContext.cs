using System.Collections.Concurrent;
using System.Threading;

namespace Masuit.Tools.Net
{
    /// <summary>
    /// 取线程内唯一对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal static class CallContext<T>
    {
        private static readonly ConcurrentDictionary<string, AsyncLocal<T>> State = new ConcurrentDictionary<string, AsyncLocal<T>>();

        /// <summary>
        /// 设置值
        /// </summary>
        /// <param name="name"></param>
        /// <param name="data"></param>
        public static void SetData(string name, T data) => State.GetOrAdd(name, _ => new AsyncLocal<T>()).Value = data;

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T GetData(string name) => State.TryGetValue(name, out AsyncLocal<T> data) ? data.Value : default(T);

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T GetOrAdd(string name, T value) => State.GetOrAdd(name, new AsyncLocal<T> { Value = value }).Value;
    }
}