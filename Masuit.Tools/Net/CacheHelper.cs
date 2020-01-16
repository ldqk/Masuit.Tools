using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.Caching;

namespace Masuit.Tools.Net
{
    /// <summary>
    /// 全局统一的缓存类
    /// </summary>
    public static class CacheHelper
    {
        #region  获取数据缓存

        /// <summary>
        /// 获取数据缓存
        /// </summary>
        /// <typeparam name="T">返回的类型</typeparam>
        /// <param name="cache"></param>
        /// <param name="cacheKey">键</param>
        public static T GetCache<T>(this Cache cache, string cacheKey)
        {
            return (T)cache[cacheKey];
        }

        #endregion

        #region  设置数据缓存

        /// <summary>
        /// 设置数据缓存
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="cacheKey">键</param>
        /// <param name="objObject">值</param>
        public static void SetCache(this Cache cache, string cacheKey, object objObject)
        {
            cache.Insert(cacheKey, objObject);
        }

        /// <summary>
        /// 设置数据缓存
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="cacheKey">键</param>
        /// <param name="objObject">值</param>
        /// <param name="timeout">过期时间</param>
        /// <exception cref="ArgumentNullException"><paramref name="cacheKey"/>"/> is <c>null</c>.</exception>
        public static void SetCache(this Cache cache, string cacheKey, object objObject, TimeSpan timeout)
        {
            if (cacheKey == null)
            {
                throw new ArgumentNullException(nameof(cacheKey));
            }

            cache.Insert(cacheKey, objObject, null, DateTime.MaxValue, timeout, CacheItemPriority.NotRemovable, null);
        }

        /// <summary>
        /// 设置当前应用程序指定CacheKey的Cache值
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="cacheKey">键</param>
        /// <param name="objObject">值</param>
        /// <param name="absoluteExpiration">绝对过期时间</param>
        /// <param name="slidingExpiration">滑动过期时间</param>
        public static void SetCache(this Cache cache, string cacheKey, object objObject, DateTime absoluteExpiration, TimeSpan slidingExpiration)
        {
            cache.Insert(cacheKey, objObject, null, absoluteExpiration, slidingExpiration);
        }

        #endregion

        #region   移除缓存

        /// <summary>
        /// 移除指定数据缓存
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="cacheKey">键</param>
        public static void RemoveAllCache(this Cache cache, string cacheKey) => cache.Remove(cacheKey);

        /// <summary>
        /// 移除全部缓存
        /// </summary>
        public static void RemoveAllCache(this Cache cache)
        {
            IDictionaryEnumerator cacheEnum = cache.GetEnumerator();
            while (cacheEnum.MoveNext())
            {
                cache.Remove(cacheEnum.Key.ToString());
            }
        }

        #endregion

        private static SortedDictionary<string, object> dic = new SortedDictionary<string, object>();
        private static volatile Cache instance;
        private static readonly object LockHelper = new object();

        /// <summary>
        /// 添加缓存数据
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public static void Add(string key, object value)
        {
            dic.Add(key, value);
        }

        /// <summary>
        /// 缓存实例
        /// </summary>
        public static Cache Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (LockHelper)
                    {
                        if (instance == null)
                        {
                            instance = new Cache();
                        }
                    }
                }

                return instance;
            }
        }
    }
}