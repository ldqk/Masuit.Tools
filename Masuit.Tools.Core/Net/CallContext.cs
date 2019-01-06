using System.Collections.Concurrent;
using System.Threading;

namespace Masuit.Tools.Core.Net
{
    /// <summary>
    /// 取线程内唯一对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class CallContext<T>
    {
        static ConcurrentDictionary<string, AsyncLocal<T>> state = new ConcurrentDictionary<string, AsyncLocal<T>>();

        /// <summary>
        /// 设置值
        /// </summary>
        /// <param name="name"></param>
        /// <param name="data"></param>
        public static void SetData(string name, T data) => state.GetOrAdd(name, _ => new AsyncLocal<T>()).Value = data;

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T GetData(string name) => state.TryGetValue(name, out AsyncLocal<T> data) ? data.Value : default(T);
    }
}