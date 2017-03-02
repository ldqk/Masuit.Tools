using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Masuit.Tools.Net
{
    /// <summary>
    /// Redis帮助类
    /// </summary>
    public static class RedisHelper
    {
        private static readonly string Coonstr = ConfigurationManager.ConnectionStrings["RedisExchangeHosts"].ConnectionString;

        private static readonly object _locker = new object();
        private static ConnectionMultiplexer _instance;

        static RedisHelper()
        {
        }

        /// <summary>
        /// 返回已连接的实例， 
        /// </summary>
        public static ConnectionMultiplexer Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_locker)
                    {
                        if ((_instance == null) || !_instance.IsConnected)
                            _instance = ConnectionMultiplexer.Connect(Coonstr);
                    }
                }
                //注册如下事件
                _instance.ConnectionFailed += MuxerConnectionFailed;
                _instance.ConnectionRestored += MuxerConnectionRestored;
                _instance.ErrorMessage += MuxerErrorMessage;
                _instance.ConfigurationChanged += MuxerConfigurationChanged;
                _instance.HashSlotMoved += MuxerHashSlotMoved;
                _instance.InternalError += MuxerInternalError;
                return _instance;
            }
        }

        /// <summary>
        /// 获取一个连接实例
        /// </summary>
        /// <returns></returns>
        public static IDatabase GetDatabase()
        {
            return Instance.GetDatabase();
        }

        /// <summary>
        /// 过期时间
        /// </summary>
        /// <param name="Min">分钟</param>
        /// <returns></returns>
        private static TimeSpan ExpireTimeSpan(double Min)
        {
            bool bl = bool.Parse(ConfigurationManager.AppSettings["UseRedis"]);

            if (bl)
                return TimeSpan.FromMinutes(Min);
            return TimeSpan.FromMilliseconds(1);
        }

        /// <summary>
        /// 清除 包含特定字符的所有缓存
        /// </summary>
        /// <param name="keyStr">键</param>
        public static void RemoveSpeStr(string keyStr)
        {
            List<string> listKeys = GetAllKeys();
            foreach (string k in listKeys)
            {
                if (k.Contains(keyStr))
                    Remove(k);
            }
        }

        /// <summary>
        /// 判断在缓存中是否存在该key的缓存数据
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool Exists(string key)
        {
            return GetDatabase().KeyExists(key); //可直接调用
        }

        /// <summary>
        /// 移除指定key的缓存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool Remove(string key)
        {
            if (Exists(key))
                return GetDatabase().KeyDelete(key);
            return false;
        }

        /// <summary>
        ///  Set
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="t">值</param>
        /// <param name="minOut">多少分钟后过期，默认值180分钟</param>
        /// <returns></returns>
        public static bool Set<T>(string key, T t, double minOut = 60 * 3)
        {
            return GetDatabase().StringSet(key, Serialize(t), ExpireTimeSpan(minOut));
        }

        /// <summary>
        /// Get
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T Get<T>(string key)
        {
            return Deserialize<T>(GetDatabase().StringGet(key));
        }

        /// <summary>
        /// DataSet 缓存
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="ds">内存表集合</param>
        /// <param name="minOut">过期时间，默认180分钟</param> 
        public static bool SetData(string key, DataSet ds, double minOut = 60 * 3)
        {
            return GetDatabase().StringSet(key, Serialize(ds), ExpireTimeSpan(minOut));
        }

        /// <summary>
        /// 获取 DataSet 
        /// </summary>
        /// <param name="key">键</param> 
        public static DataSet GetDataSet(string key)
        {
            return Deserialize<DataSet>(GetDatabase().StringGet(key));
        }

        /// <summary>
        /// 刷新缓存
        /// </summary>
        public static void FlushAll()
        {
            var endpoints = Instance.GetEndPoints();
            var server = Instance.GetServer(endpoints[0]);

            server.FlushDatabase(); // to wipe a single database, 0 by default
        }

        /// <summary>
        /// 得到所有缓存键值
        /// </summary>
        /// <returns></returns>
        public static List<string> GetAllKeys()
        {
            List<string> lstKey = new List<string>();
            var endpoints = Instance.GetEndPoints();
            var server = Instance.GetServer(endpoints[0]);
            var keys = server.Keys();
            foreach (var key in keys)
                lstKey.Add(key);
            return lstKey;
        }

        //----------------------------------------------------------------------------------------------------------
        private static string JsonSerialize(object o)
        {
            if (o == null)
                return null;
            return JsonConvert.SerializeObject(o);
        }

        private static T JsonDeserialize<T>(string json)
        {
            if (json == null)
                return default(T);
            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// 序列化对象
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        private static byte[] Serialize(object o)
        {
            if (o == null)
                return null;
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, o);
                byte[] objectDataAsStream = memoryStream.ToArray();
                return objectDataAsStream;
            }
        }

        /// <summary>
        /// 反序列化对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        private static T Deserialize<T>(byte[] stream)
        {
            if (stream == null)
                return default(T);
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (MemoryStream memoryStream = new MemoryStream(stream))
            {
                T result = (T)binaryFormatter.Deserialize(memoryStream);
                return result;
            }
        }

        /// <summary>
        /// 配置更改时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="Exception"></exception>
        private static void MuxerConfigurationChanged(object sender, EndPointEventArgs e)
        {
            //LogHelper.LogExceRun("Configuration changed: " + e.EndPoint, new Exception());
            throw new Exception("配置更改时: " + e.EndPoint);
        }

        /// <summary>
        /// 发生错误时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="Exception"></exception>
        private static void MuxerErrorMessage(object sender, RedisErrorEventArgs e)
        {
            //LogHelper.LogExceRun("ErrorMessage: " + e.Message, new Exception());
            throw new Exception("发生错误时:" + e.Message);
        }

        /// <summary>
        /// 重新建立连接之前的错误
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="Exception"></exception>
        private static void MuxerConnectionRestored(object sender, ConnectionFailedEventArgs e)
        {
            //LogHelper.LogExceRun("ConnectionRestored: " + e.EndPoint, new Exception());
            throw new Exception("重新建立连接之前的错误: " + e.EndPoint);
        }

        /// <summary>
        /// 连接失败 ， 如果重新连接成功你将不会收到这个通知
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="Exception"></exception>
        private static void MuxerConnectionFailed(object sender, ConnectionFailedEventArgs e)
        {
            //LogHelper.LogExceRun("重新连接：Endpoint failed: " + e.EndPoint + ", " + e.FailureType + (e.Exception == null ? "" : (", " + e.Exception.Message)), new Exception());
            throw new Exception("重新连接：Endpoint failed: " + e.EndPoint + ", " + e.FailureType + (e.Exception == null ? "" : ", " + e.Exception.Message));
        }

        /// <summary>
        /// 更改集群
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="Exception"></exception>
        private static void MuxerHashSlotMoved(object sender, HashSlotMovedEventArgs e)
        {
            //LogHelper.LogExceRun("HashSlotMoved:NewEndPoint" + e.NewEndPoint + ", OldEndPoint" + e.OldEndPoint, new Exception());
            throw new Exception("更改集群HashSlotMoved:NewEndPoint" + e.NewEndPoint + ", OldEndPoint" + e.OldEndPoint);
        }

        /// <summary>
        /// redis类库错误
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="Exception"></exception>
        private static void MuxerInternalError(object sender, InternalErrorEventArgs e)
        {
            //LogHelper.LogExceRun("redis类库错误InternalError:Message" + e.Exception.Message, new Exception());
            throw new Exception("redis类库错误InternalError:Message" + e.Exception.Message);
        }

        /// <summary>
        /// GetServer方法会接收一个EndPoint类或者一个唯一标识一台服务器的键值对
        /// 有时候需要为单个服务器指定特定的命令
        /// 使用IServer可以使用所有的shell命令，比如：
        /// DateTime lastSave = server.LastSave();
        /// ClientInfo[] clients = server.ClientList();
        /// 如果报错在连接字符串后加 ,allowAdmin=true;
        /// </summary>
        /// <param name="host">主机地址</param>
        /// <param name="port">端口号</param>
        /// <returns></returns>
        public static IServer GetServer(string host, int port)
        {
            IServer server = Instance.GetServer(host, port);
            return server;
        }

        /// <summary>
        /// 获取全部终结点
        /// </summary>
        /// <returns></returns>
        public static EndPoint[] GetEndPoints()
        {
            EndPoint[] endpoints = Instance.GetEndPoints();
            return endpoints;
        }
    }
}