using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Masuit.Tools.Core.Net;
using Masuit.Tools.Core.Systems;
using StackExchange.Redis;

namespace Masuit.Tools.Core.NoSQL
{
    /// <summary>
    /// ConnectionMultiplexer对象管理帮助类
    /// </summary>
    public static class RedisConnectionManager
    {
        /// <summary>
        /// Redis服务器连接字符串，默认为：127.0.0.1:6379,allowadmin=true<br/>
        /// </summary>
        public static string RedisConnectionString
        {
            get => "127.0.0.1:6379,allowadmin=true";
            set { }
        }

        //private static readonly object Locker = new object();
        //private static ConnectionMultiplexer _instance;
        private static readonly ConcurrentDictionary<string, ConcurrentLimitedQueue<ConnectionMultiplexer>> ConnectionCache = new ConcurrentDictionary<string, ConcurrentLimitedQueue<ConnectionMultiplexer>>();

        /// <summary>
        /// 单例获取
        /// </summary>
        public static ConnectionMultiplexer Instance
        {
            get
            {
                var queue = ConnectionCache.GetOrAdd(RedisConnectionString, new ConcurrentLimitedQueue<ConnectionMultiplexer>(32));
                if (queue.IsEmpty)
                {
                    Parallel.For(0, queue.Limit, i =>
                    {
                        queue.Enqueue(GetManager(RedisConnectionString));
                    });
                }
                ConnectionMultiplexer multiplexer;
                if (CallContext<ConnectionMultiplexer>.GetData(RedisConnectionString) == null)
                {
                    queue.TryDequeue(out multiplexer);
                    CallContext<ConnectionMultiplexer>.SetData(RedisConnectionString, multiplexer);
                }
                multiplexer = CallContext<ConnectionMultiplexer>.GetData(RedisConnectionString);
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
            var queue = ConnectionCache.GetOrAdd(connectionString, new ConcurrentLimitedQueue<ConnectionMultiplexer>(32));
            if (queue.IsEmpty)
            {
                Parallel.For(0, queue.Limit, i =>
                {
                    queue.Enqueue(GetManager(connectionString));
                });
            }
            ConnectionMultiplexer multiplexer;
            if (CallContext<ConnectionMultiplexer>.GetData(connectionString) == null)
            {
                queue.TryDequeue(out multiplexer);
                CallContext<ConnectionMultiplexer>.SetData(connectionString, multiplexer);
            }
            multiplexer = CallContext<ConnectionMultiplexer>.GetData(connectionString);
            return multiplexer;
        }

        private static ConnectionMultiplexer GetManager(string connectionString = null)
        {
            connectionString = connectionString ?? RedisConnectionString;
            var connect = ConnectionMultiplexer.Connect(ConfigurationOptions.Parse(connectionString, true));
            //注册如下事件
            connect.ConnectionFailed += MuxerConnectionFailed;
            connect.ConnectionRestored += MuxerConnectionRestored;
            connect.ErrorMessage += MuxerErrorMessage;
            connect.ConfigurationChanged += MuxerConfigurationChanged;
            connect.HashSlotMoved += MuxerHashSlotMoved;
            connect.InternalError += MuxerInternalError;

            return connect;
        }

        #region 事件

        /// <summary>
        /// 配置更改时
        /// </summary>
        /// <param name="sender">触发者</param>
        /// <param name="e">事件参数</param>
        private static void MuxerConfigurationChanged(object sender, EndPointEventArgs e)
        {
            Console.WriteLine("Configuration changed: " + e.EndPoint);
        }

        /// <summary>
        /// 发生错误时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerErrorMessage(object sender, RedisErrorEventArgs e)
        {
            Console.WriteLine("ErrorMessage: " + e.Message);
        }

        /// <summary>
        /// 重新建立连接之前的错误
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerConnectionRestored(object sender, ConnectionFailedEventArgs e)
        {
            Console.WriteLine("ConnectionRestored: " + e.EndPoint);
        }

        /// <summary>
        /// 连接失败 ， 如果重新连接成功你将不会收到这个通知
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerConnectionFailed(object sender, ConnectionFailedEventArgs e)
        {
            Console.WriteLine("重新连接：Endpoint failed: " + e.EndPoint + ", " + e.FailureType + (e.Exception == null ? "" : (", " + e.Exception.Message)));
        }

        /// <summary>
        /// 更改集群
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerHashSlotMoved(object sender, HashSlotMovedEventArgs e)
        {
            Console.WriteLine("HashSlotMoved:NewEndPoint" + e.NewEndPoint + ", OldEndPoint" + e.OldEndPoint);
        }

        /// <summary>
        /// redis类库错误
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerInternalError(object sender, InternalErrorEventArgs e)
        {
            Console.WriteLine("InternalError:Message" + e.Exception.Message);
        }

        #endregion 事件
    }
}