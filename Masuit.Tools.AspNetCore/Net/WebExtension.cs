using FastExpressionCompiler;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Linq.Expressions;

namespace Masuit.Tools.Core.Net
{
    /// <summary>
    /// Web操作扩展
    /// </summary>
    public static class WebExtension
    {
        /// <summary>
        /// 写Session
        /// </summary>
        /// <param name="session"></param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public static void Set(this ISession session, string key, object value)
        {
            session.SetString(key, value.ToJsonString());
        }

        /// <summary>
        /// 获取Session
        /// </summary>
        /// <typeparam name="T">对象</typeparam>
        /// <param name="session"></param>
        /// <param name="key">键</param>
        /// <returns>对象</returns>
        public static T Get<T>(this ISession session, string key)
        {
            string value = session.GetString(key);
            if (string.IsNullOrEmpty(value))
            {
                return typeof(T).Namespace switch
                {
                    "System.Collections.Generic" => (T)(Expression.Lambda(Expression.New(typeof(T))).CompileFast().DynamicInvoke()),
                    _ => default
                };
            }
            return JsonConvert.DeserializeObject<T>(value);
        }
    }
}