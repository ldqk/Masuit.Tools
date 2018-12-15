using Masuit.Tools.NoSQL;
using System.Collections.Generic;

namespace Masuit.Tools.Core.NoSQL
{
    /// <summary>
    /// RedisHelper工厂类
    /// </summary>
    public class RedisHelperFactory
    {
        internal static Dictionary<string, string> ConnectionCache { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// 创建一个Redis实例
        /// </summary>
        /// <param name="name"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public RedisHelper Create(string name, int db = 0)
        {
            return RedisHelper.GetInstance(ConnectionCache[name], db);
        }

        /// <summary>
        /// 创建一个默认实例
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public RedisHelper CreateDefault(int db = 0)
        {
            return RedisHelper.GetInstance(ConnectionCache["default"], db);
        }

        /// <summary>
        /// 创建一个本地连接实例
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public RedisHelper CreateLocal(int db = 0)
        {
            return RedisHelper.GetInstance(ConnectionCache["local"], db);
        }
    }
}