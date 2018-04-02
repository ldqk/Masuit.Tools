using System.Collections.Concurrent;
using System.Configuration;
using StackExchange.Redis;

namespace Masuit.Tools.NoSQL
{
    /// <summary>
    /// ConnectionMultiplexer对象管理帮助类
    /// </summary>
    public static class RedisConnectionManager2
    {
        /// <summary>
        /// Redis服务器连接字符串，在config配置文件中的ConnectionStrings节下配置，name固定为RedisHosts，值的格式：127.0.0.1:6379,allowadmin=true，如：<br/>
        /// &lt;connectionStrings&gt;<br/>
        ///      &lt;add name = "RedisHosts" connectionString="127.0.0.1:6379,allowadmin=true"/&gt;<br/>
        /// &lt;/connectionStrings&gt;
        /// </summary>
        public static readonly string RedisConnectionString = ConfigurationManager.ConnectionStrings["RedisHosts"]?.ConnectionString ?? "127.0.0.1:6379,allowadmin=true";

        private static readonly ConcurrentDictionary<string, ConnectionMultiplexer> ConnectionCache = new ConcurrentDictionary<string, ConnectionMultiplexer>();

        /// <summary>
        /// 对象池获取线程内唯一对象
        /// </summary>
        public static ConnectionMultiplexer Instance
        {
            get
            {
                var multiplexer = ConnectionCache.GetOrAdd(RedisConnectionString, GetManager(RedisConnectionString));
                return multiplexer;
            }
        }

        /// <summary>
        /// 缓存获取
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <returns>连接对象</returns>
        public static ConnectionMultiplexer GetConnectionMultiplexer(string connectionString)
        {
            var multiplexer = ConnectionCache.GetOrAdd(connectionString, GetManager(RedisConnectionString));
            return multiplexer;
        }

        private static ConnectionMultiplexer GetManager(string connectionString = null)
        {
            connectionString = connectionString ?? RedisConnectionString;
            var connect = ConnectionMultiplexer.Connect(ConfigurationOptions.Parse(connectionString, true));
            return connect;
        }

    }
}