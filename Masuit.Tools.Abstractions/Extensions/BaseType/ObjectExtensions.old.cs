using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Masuit.Tools
{
    public static partial class ObjectExtensions
    {
        #region Map

        /// <summary>
        /// 映射到目标类型(浅克隆)
        /// </summary>
        /// <param name="source">源</param>
        /// <typeparam name="TDestination">目标类型</typeparam>
        /// <returns>目标类型</returns>
        [Obsolete("请使用Clone")]
        public static TDestination MapTo<TDestination>(this object source)
            where TDestination : new()
        {
            return source.Clone<TDestination>();
        }

        /// <summary>
        /// 映射到目标类型(深克隆)
        /// </summary>
        /// <param name="source">源</param>
        /// <typeparam name="TDestination">目标类型</typeparam>
        /// <returns>目标类型</returns>
        [Obsolete("请使用DeepClone")]
        public static TDestination Map<TDestination>(this object source)
            where TDestination : new()
        {
            return source.DeepClone<TDestination>();
        }

        /// <summary>
        /// 映射到目标类型(浅克隆)
        /// </summary>
        /// <param name="source">源</param>
        /// <typeparam name="TDestination">目标类型</typeparam>
        /// <returns>目标类型</returns>
        [Obsolete("请使用CloneAsync")]
        public static Task<TDestination> MapToAsync<TDestination>(this object source)
            where TDestination : new()
        {
            return Task.Run(() =>
            {
                return source.Clone<TDestination>();
            });
        }

        /// <summary>
        /// 映射到目标类型(深克隆)
        /// </summary>
        /// <param name="source">源</param>
        /// <typeparam name="TDestination">目标类型</typeparam>
        /// <returns>目标类型</returns>
        [Obsolete("请使用DeepCloneAsync")]
        public static Task<TDestination> MapAsync<TDestination>(this object source) where TDestination : new()
        {
            return source.DeepCloneAsync<TDestination>();
        }

        #endregion Map

        private static readonly JsonSerializerSettings defaultJsonSettings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        /// <summary>
        /// 转换成json字符串
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [Obsolete("请改用ToJsonExt")]
        public static string ToJsonString(this object source)
        {
            return source.ToJsonExt(ObjectExtensions.defaultJsonSettings);
        }
    }
}