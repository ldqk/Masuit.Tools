using Masuit.Tools.Systems;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace Masuit.Tools.NoSQL
{
    /// <summary>
    /// Redis操作
    /// </summary>
    public class RedisHelper : Disposable
    {
        private int DbNum { get; }
        private ConnectionMultiplexer _conn;

        /// <summary>
        /// 自定义键
        /// </summary>
        public string CustomKey;

        public static string DefaultConnectionString { get; set; } = ConfigurationManager.ConnectionStrings["RedisHosts"]?.ConnectionString ?? "127.0.0.1:6379,allowadmin=true,abortConnect=false,connectTimeout=1000,connectRetry=1,syncTimeout=20000";

        /// <summary>
        /// 连接失败 ， 如果重新连接成功你将不会收到这个通知
        /// </summary>
        public event EventHandler<ConnectionFailedEventArgs> ConnectionFailed;

        /// <summary>
        /// 重新建立连接之前的错误
        /// </summary>
        public event EventHandler<ConnectionFailedEventArgs> ConnectionRestored;

        /// <summary>
        /// 发生错误时
        /// </summary>
        public event EventHandler<RedisErrorEventArgs> ErrorMessage;

        /// <summary>
        /// 配置更改时
        /// </summary>
        public event EventHandler<EndPointEventArgs> ConfigurationChanged;

        /// <summary>
        /// 更改集群时
        /// </summary>
        public event EventHandler<HashSlotMovedEventArgs> HashSlotMoved;

        /// <summary>
        /// redis类库错误时
        /// </summary>
        public event EventHandler<InternalErrorEventArgs> InternalError;

        /// <summary>
        /// 静态连接池
        /// </summary>
        public static ConcurrentDictionary<string, ConnectionMultiplexer> ConnectionCache { get; set; } = new ConcurrentDictionary<string, ConnectionMultiplexer>();

        #region 构造函数

        /// <summary>
        /// 构造函数，使用该构造函数需要先在config中配置链接字符串，连接字符串在config配置文件中的ConnectionStrings节下配置，name固定为RedisHosts，值的格式：127.0.0.1:6379,allowadmin=true，若未正确配置，将按默认值“127.0.0.1:6379,allowadmin=true,abortConnect=false”进行操作，如：<br/>
        /// &lt;connectionStrings&gt;<br/>
        ///      &lt;add name = "RedisHosts" connectionString="127.0.0.1:6379,allowadmin=true,abortConnect=false"/&gt;<br/>
        /// &lt;/connectionStrings&gt;
        /// </summary>
        /// <param name="dbNum">数据库编号</param>
        public RedisHelper(int dbNum = 0) : this(null, dbNum)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="readWriteHosts">Redis服务器连接字符串，格式：127.0.0.1:6379,allowadmin=true,abortConnect=false</param>
        /// <param name="dbNum">数据库的编号</param>
        public RedisHelper(string readWriteHosts, int dbNum = 0)
        {
            DbNum = dbNum;
            _conn = string.IsNullOrWhiteSpace(readWriteHosts) ? ConnectionMultiplexer.Connect(ConfigurationOptions.Parse(DefaultConnectionString)) : ConnectionMultiplexer.Connect(ConfigurationOptions.Parse(readWriteHosts));
            //_conn.ConfigurationChanged += MuxerConfigurationChanged;
            _conn.ConfigurationChanged += ConfigurationChanged;
            //_conn.ConnectionFailed += MuxerConnectionFailed;
            _conn.ConnectionFailed += ConnectionFailed;
            //_conn.ConnectionRestored += MuxerConnectionRestored;
            _conn.ConnectionRestored += ConnectionRestored;
            //_conn.ErrorMessage += MuxerErrorMessage;
            _conn.ErrorMessage += ErrorMessage;
            //_conn.HashSlotMoved += MuxerHashSlotMoved;
            _conn.HashSlotMoved += HashSlotMoved;
            //_conn.InternalError += MuxerInternalError;
            _conn.InternalError += InternalError;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="readWriteHosts">Redis服务器连接字符串，格式：127.0.0.1:6379,allowadmin=true,abortConnect=false</param>
        /// <param name="dbNum">数据库的编号</param>
        /// <param name="_"></param>
        private RedisHelper(string readWriteHosts, int dbNum, int _)
        {
            DbNum = dbNum;
            readWriteHosts = string.IsNullOrWhiteSpace(readWriteHosts) ? DefaultConnectionString : readWriteHosts;
            _conn = ConnectionCache.GetOrAdd(readWriteHosts, ConnectionMultiplexer.Connect(ConfigurationOptions.Parse(readWriteHosts)));
            ////_conn.ConfigurationChanged += MuxerConfigurationChanged;
            //_conn.ConfigurationChanged += ConfigurationChanged;
            ////_conn.ConnectionFailed += MuxerConnectionFailed;
            //_conn.ConnectionFailed += ConnectionFailed;
            ////_conn.ConnectionRestored += MuxerConnectionRestored;
            //_conn.ConnectionRestored += ConnectionRestored;
            ////_conn.ErrorMessage += MuxerErrorMessage;
            //_conn.ErrorMessage += ErrorMessage;
            ////_conn.HashSlotMoved += MuxerHashSlotMoved;
            //_conn.HashSlotMoved += HashSlotMoved;
            ////_conn.InternalError += MuxerInternalError;
            //_conn.InternalError += InternalError;
        }

        /// <summary>
        /// 获取新实例
        /// </summary>
        /// <param name="db">数据库的编号</param>
        /// <returns></returns>
        public static RedisHelper GetInstance(int db = 0)
        {
            return new RedisHelper(db);
        }

        /// <summary>
        /// 获取单例
        /// </summary>
        /// <param name="db">数据库的编号</param>
        /// <returns></returns>
        public static RedisHelper GetSingleInstance(int db = 0)
        {
            return new RedisHelper(null, db, 0);
        }

        /// <summary>
        /// 从对象池获取默认实例
        /// </summary>
        /// <param name="conn">Redis服务器连接字符串，格式：127.0.0.1:6379,allowadmin=true,abortConnect=false</param>
        /// <param name="db">数据库的编号</param>
        /// <returns></returns>
        public static RedisHelper GetInstance(string conn, int db = 0)
        {
            return new RedisHelper(conn, db);
        }

        /// <summary>
        /// 获取单例
        /// </summary>
        /// <param name="conn">Redis服务器连接字符串，格式：127.0.0.1:6379,allowadmin=true,abortConnect=false</param>
        /// <param name="db">数据库的编号</param>
        /// <returns></returns>
        public static RedisHelper GetSingleInstance(string conn, int db = 0)
        {
            return new RedisHelper(conn, db, 0);
        }

        #endregion 构造函数

        #region String

        #region 同步方法

        /// <summary>
        /// 保存单个key value
        /// </summary>
        /// <param name="key">Redis Key</param>
        /// <param name="value">保存的值</param>
        /// <param name="expiry">过期时间</param>
        /// <returns>是否保存成功</returns>
        public bool SetString(string key, string value, TimeSpan? expiry = default(TimeSpan?))
        {
            key = AddSysCustomKey(key);
            return Do(db => db.StringSet(key, value, expiry));
        }

        /// <summary>
        /// 保存多个key value
        /// </summary>
        /// <param name="keyValues">键值对</param>
        /// <returns>是否保存成功</returns>
        public bool SetString(List<KeyValuePair<RedisKey, RedisValue>> keyValues)
        {
            List<KeyValuePair<RedisKey, RedisValue>> newkeyValues = keyValues.Select(p => new KeyValuePair<RedisKey, RedisValue>(AddSysCustomKey(p.Key), p.Value)).ToList();
            return Do(db => db.StringSet(newkeyValues.ToArray()));
        }

        /// <summary>
        /// 保存一个对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="obj">值</param>
        /// <param name="expiry">过期时间</param>
        /// <returns>是否保存成功</returns>
        public bool SetString<T>(string key, T obj, TimeSpan? expiry = default(TimeSpan?))
        {
            key = AddSysCustomKey(key);
            string json = ConvertJson(obj);
            return Do(db => db.StringSet(key, json, expiry));
        }

        /// <summary>
        /// 获取单个key的值
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>值</returns>
        public string GetString(string key)
        {
            if (KeyExists(key))
            {
                key = AddSysCustomKey(key);
                return Do(db => db.StringGet(key));
            }

            return string.Empty;
        }

        /// <summary>
        /// 获取多个Key
        /// </summary>
        /// <param name="listKey">键集合</param>
        /// <returns>值集合</returns>
        public RedisValue[] GetString(List<string> listKey)
        {
            List<string> newKeys = listKey.Select(AddSysCustomKey).ToList();
            return Do(db => db.StringGet(ConvertRedisKeys(newKeys)));
        }

        /// <summary>
        /// 获取一个key的对象
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">键</param>
        /// <returns>实例对象</returns>
        public T GetString<T>(string key)
        {
            if (KeyExists(key))
            {
                key = AddSysCustomKey(key);
                return Do(db => ConvertObj<T>(db.StringGet(key)));
            }

            return default(T);
        }

        /// <summary>
        /// 为数字增长val
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="val">可以为负</param>
        /// <returns>增长后的值</returns>
        public double StringIncrement(string key, double val = 1)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.StringIncrement(key, val));
        }

        /// <summary>
        /// 为数字减少val
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="val">可以为负</param>
        /// <returns>减少后的值</returns>
        public double StringDecrement(string key, double val = 1)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.StringDecrement(key, val));
        }

        #endregion 同步方法

        #region 异步方法

        /// <summary>
        /// 保存单个key value
        /// </summary>
        /// <param name="key">Redis Key</param>
        /// <param name="value">保存的值</param>
        /// <param name="expiry">过期时间</param>
        /// <returns>是否保存成功</returns>
        public async Task<bool> SetStringAsync(string key, string value, TimeSpan? expiry = default(TimeSpan?))
        {
            key = AddSysCustomKey(key);
            return await Do(async db => await db.StringSetAsync(key, value, expiry));
        }

        /// <summary>
        /// 保存多个key value
        /// </summary>
        /// <param name="keyValues">键值对</param>
        /// <returns>是否保存成功</returns>
        public async Task<bool> SetStringAsync(List<KeyValuePair<RedisKey, RedisValue>> keyValues)
        {
            var newkeyValues = keyValues.Select(p => new KeyValuePair<RedisKey, RedisValue>(AddSysCustomKey(p.Key), p.Value)).ToList();
            return await Do(async db => await db.StringSetAsync(newkeyValues.ToArray()));
        }

        /// <summary>
        /// 保存一个对象
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="obj">需要被缓存的对象</param>
        /// <param name="expiry">过期时间</param>
        /// <returns>是否保存成功</returns>
        public async Task<bool> SetStringAsync<T>(string key, T obj, TimeSpan? expiry = default(TimeSpan?))
        {
            key = AddSysCustomKey(key);
            string json = ConvertJson(obj);
            return await Do(async db => await db.StringSetAsync(key, json, expiry));
        }

        /// <summary>
        /// 获取单个key的值
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>值</returns>
        public async Task<string> GetStringAsync(string key)
        {
            if (KeyExists(key))
            {
                key = AddSysCustomKey(key);
                return await Do(async db => await db.StringGetAsync(key));
            }

            return string.Empty;
        }

        /// <summary>
        /// 获取多个Key
        /// </summary>
        /// <param name="listKey">键集合</param>
        /// <returns>值集合</returns>
        public async Task<RedisValue[]> GetStringAsync(List<string> listKey)
        {
            List<string> newKeys = listKey.Select(AddSysCustomKey).ToList();
            return await Do(async db => await db.StringGetAsync(ConvertRedisKeys(newKeys)));
        }

        /// <summary>
        /// 获取一个key的对象
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">键</param>
        /// <returns>实例对象</returns>
        public async Task<T> GetStringAsync<T>(string key)
        {
            if (KeyExists(key))
            {
                key = AddSysCustomKey(key);
                string result = await Do(async db => await db.StringGetAsync(key));
                return ConvertObj<T>(result);
            }

            return default(T);
        }

        /// <summary>
        /// 为数字增长val
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="val">可以为负</param>
        /// <returns>增长后的值</returns>
        public async Task<double> IncrementStringAsync(string key, double val = 1)
        {
            key = AddSysCustomKey(key);
            return await Do(async db => await db.StringIncrementAsync(key, val));
        }

        /// <summary>
        /// 为数字减少val
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="val">可以为负</param>
        /// <returns>减少后的值</returns>
        public async Task<double> DecrementStringAsync(string key, double val = 1)
        {
            key = AddSysCustomKey(key);
            return await Do(async db => await db.StringDecrementAsync(key, val));
        }

        #endregion 异步方法

        #endregion String

        #region Hash

        #region 同步方法

        /// <summary>
        /// 判断某个数据是否已经被缓存
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="dataKey">对象的字段</param>
        /// <returns>是否缓存成功</returns>
        public bool HashExists(string key, string dataKey)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.HashExists(key, dataKey));
        }

        /// <summary>
        /// 存储数据到hash表
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="dataKey">对象的字段</param>
        /// <param name="t">对象实例</param>
        /// <returns>是否存储成功</returns>
        public bool SetHash<T>(string key, string dataKey, T t)
        {
            key = AddSysCustomKey(key);
            return Do(db =>
            {
                string json = ConvertJson(t);
                return db.HashSet(key, dataKey, json);
            });
        }

        /// <summary>
        /// 存储数据到hash表
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="dataKey">对象的字段</param>
        /// <param name="t">对象实例</param>
        /// <param name="expire">过期时间</param>
        /// <returns>是否存储成功</returns>
        public bool SetHash<T>(string key, string dataKey, T t, TimeSpan expire)
        {
            bool b = SetHash(key, dataKey, t);
            Expire(key, expire);
            return b;
        }

        /// <summary>
        /// 移除hash中的某值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="dataKey">对象的字段</param>
        /// <returns>是否移除成功</returns>
        public bool DeleteHash(string key, string dataKey)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.HashDelete(key, dataKey));
        }

        /// <summary>
        /// 移除hash中的多个值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="dataKeys">对象的字段集合</param>
        /// <returns>数量</returns>
        public long DeleteHash(string key, List<RedisValue> dataKeys)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.HashDelete(key, dataKeys.ToArray()));
        }

        /// <summary>
        /// 从hash表获取数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="dataKey">对象的字段</param>
        /// <returns>对象实例</returns>
        public T GetHash<T>(string key, string dataKey)
        {
            if (KeyExists(key))
            {
                key = AddSysCustomKey(key);
                return Do(db =>
                {
                    string value = db.HashGet(key, dataKey);
                    return ConvertObj<T>(value);
                });
            }

            return default(T);
        }

        /// <summary>
        /// 为数字增长val
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="dataKey">对象的字段</param>
        /// <param name="val">可以为负</param>
        /// <returns>增长后的值</returns>
        public double IncrementHash(string key, string dataKey, double val = 1)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.HashIncrement(key, dataKey, val));
        }

        /// <summary>
        /// 为数字减少val
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="dataKey">对象的字段</param>
        /// <param name="val">可以为负</param>
        /// <returns>减少后的值</returns>
        public double DecrementHash(string key, string dataKey, double val = 1)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.HashDecrement(key, dataKey, val));
        }

        /// <summary>
        /// 获取hashkey所有Redis key
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">键</param>
        /// <returns>数据集合</returns>
        public List<T> HashKeys<T>(string key)
        {
            key = AddSysCustomKey(key);
            return Do(db =>
            {
                RedisValue[] values = db.HashKeys(key);
                return ConvetList<T>(values);
            });
        }

        #endregion 同步方法

        #region 异步方法

        /// <summary>
        /// 判断某个数据是否已经被缓存
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="dataKey">对象的字段</param>
        /// <returns>是否缓存成功</returns>
        public async Task<bool> ExistsHashAsync(string key, string dataKey)
        {
            key = AddSysCustomKey(key);
            return await Do(async db => await db.HashExistsAsync(key, dataKey));
        }

        /// <summary>
        /// 存储数据到hash表
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="dataKey">对象的字段</param>
        /// <param name="t">对象实例</param>
        /// <returns>是否存储成功</returns>
        public async Task<bool> SetHashAsync<T>(string key, string dataKey, T t)
        {
            key = AddSysCustomKey(key);
            return await Do(async db =>
            {
                string json = ConvertJson(t);
                return await db.HashSetAsync(key, dataKey, json);
            });
        }

        /// <summary>
        /// 存储数据到hash表
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="dataKey">对象的字段</param>
        /// <param name="t">对象实例</param>
        /// <param name="expire"></param>
        /// <returns>是否存储成功</returns>
        public async Task<bool> SetHashAsync<T>(string key, string dataKey, T t, TimeSpan expire)
        {
            var b = await SetHashAsync(key, dataKey, t);
            await ExpireAsync(key, expire);
            return b;
        }

        /// <summary>
        /// 移除hash中的某值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="dataKey">对象的字段</param>
        /// <returns>是否移除成功</returns>
        public async Task<bool> DeleteHashAsync(string key, string dataKey)
        {
            key = AddSysCustomKey(key);
            return await Do(async db => await db.HashDeleteAsync(key, dataKey));
        }

        /// <summary>
        /// 移除hash中的多个值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="dataKeys">对象的字段集合</param>
        /// <returns>数量</returns>
        public async Task<long> DeleteHashAsync(string key, List<RedisValue> dataKeys)
        {
            key = AddSysCustomKey(key);
            return await Do(async db => await db.HashDeleteAsync(key, dataKeys.ToArray()));
        }

        /// <summary>
        /// 从hash表获取数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="dataKey">对象的字段</param>
        /// <returns>对象实例</returns>
        public async Task<T> GetHashAsync<T>(string key, string dataKey)
        {
            if (KeyExists(key))
            {
                key = AddSysCustomKey(key);
                string value = await Do(async db => await db.HashGetAsync(key, dataKey));
                return ConvertObj<T>(value);
            }

            return default(T);
        }

        /// <summary>
        /// 为数字增长val
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="dataKey">对象的字段</param>
        /// <param name="val">可以为负</param>
        /// <returns>增长后的值</returns>
        public async Task<double> IncrementHashAsync(string key, string dataKey, double val = 1)
        {
            key = AddSysCustomKey(key);
            return await Do(async db => await db.HashIncrementAsync(key, dataKey, val));
        }

        /// <summary>
        /// 为数字减少val
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="dataKey">对象的字段</param>
        /// <param name="val">可以为负</param>
        /// <returns>减少后的值</returns>
        public async Task<double> DecrementHashAsync(string key, string dataKey, double val = 1)
        {
            key = AddSysCustomKey(key);
            return await Do(async db => await db.HashDecrementAsync(key, dataKey, val));
        }

        /// <summary>
        /// 获取hashkey所有Redis key
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">键</param>
        /// <returns>数据集合</returns>
        public async Task<List<T>> HashKeysAsync<T>(string key)
        {
            key = AddSysCustomKey(key);
            RedisValue[] values = await Do(async db => await db.HashKeysAsync(key));
            return ConvetList<T>(values);
        }

        #endregion 异步方法

        #endregion Hash

        #region List

        #region 同步方法

        /// <summary>
        /// 移除指定ListId的内部List的值
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void RemoveList<T>(string key, T value)
        {
            key = AddSysCustomKey(key);
            Do(db => db.ListRemove(key, ConvertJson(value)));
        }

        /// <summary>
        /// 获取指定key的List
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">键</param>
        /// <returns>数据集</returns>
        public List<T> ListRange<T>(string key)
        {
            if (KeyExists(key))
            {
                key = AddSysCustomKey(key);
                return Do(redis =>
                {
                    var values = redis.ListRange(key);
                    return ConvetList<T>(values);
                });
            }

            return new List<T>();
        }

        /// <summary>
        /// 入队
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void ListRightPush<T>(string key, T value)
        {
            key = AddSysCustomKey(key);
            Do(db => db.ListRightPush(key, ConvertJson(value)));
        }

        /// <summary>
        /// 出队
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">键</param>
        /// <returns>值</returns>
        public T ListRightPop<T>(string key)
        {
            if (KeyExists(key))
            {
                key = AddSysCustomKey(key);
                return Do(db =>
                {
                    var value = db.ListRightPop(key);
                    return ConvertObj<T>(value);
                });
            }

            return default(T);
        }

        /// <summary>
        /// 入栈
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void ListLeftPush<T>(string key, T value)
        {
            key = AddSysCustomKey(key);
            Do(db => db.ListLeftPush(key, ConvertJson(value)));
        }

        /// <summary>
        /// 出栈
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">键</param>
        /// <returns>对象实例</returns>
        public T ListLeftPop<T>(string key)
        {
            if (KeyExists(key))
            {
                key = AddSysCustomKey(key);
                return Do(db =>
                {
                    var value = db.ListLeftPop(key);
                    return ConvertObj<T>(value);
                });
            }

            return default(T);
        }

        /// <summary>
        /// 获取集合中的数量
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>数量</returns>
        public long ListLength(string key)
        {
            key = AddSysCustomKey(key);
            return Do(redis => redis.ListLength(key));
        }

        #endregion 同步方法

        #region 异步方法

        /// <summary>
        /// 移除指定ListId的内部List的值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public async Task<long> RemoveListAsync<T>(string key, T value)
        {
            key = AddSysCustomKey(key);
            return await Do(async db => await db.ListRemoveAsync(key, ConvertJson(value)));
        }

        /// <summary>
        /// 获取指定key的List
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">键</param>
        /// <returns>数据集合</returns>
        public async Task<List<T>> ListRangeAsync<T>(string key)
        {
            if (KeyExists(key))
            {
                key = AddSysCustomKey(key);
                var values = await Do(async redis => await redis.ListRangeAsync(key));
                return ConvetList<T>(values);
            }

            return new List<T>();
        }

        /// <summary>
        /// 入队
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public async Task<long> ListRightPushAsync<T>(string key, T value)
        {
            key = AddSysCustomKey(key);
            return await Do(async db => await db.ListRightPushAsync(key, ConvertJson(value)));
        }

        /// <summary>
        /// 出队
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">键</param>
        /// <returns>对象实例</returns>
        public async Task<T> ListRightPopAsync<T>(string key)
        {
            if (KeyExists(key))
            {
                key = AddSysCustomKey(key);
                var value = await Do(async db => await db.ListRightPopAsync(key));
                return ConvertObj<T>(value);
            }

            return default(T);
        }

        /// <summary>
        /// 入栈
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public async Task<long> ListLeftPushAsync<T>(string key, T value)
        {
            key = AddSysCustomKey(key);
            return await Do(async db => await db.ListLeftPushAsync(key, ConvertJson(value)));
        }

        /// <summary>
        /// 出栈
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">键</param>
        /// <returns>实例对象</returns>
        public async Task<T> ListLeftPopAsync<T>(string key)
        {
            if (KeyExists(key))
            {
                key = AddSysCustomKey(key);
                var value = await Do(async db => await db.ListLeftPopAsync(key));
                return ConvertObj<T>(value);
            }

            return default(T);
        }

        /// <summary>
        /// 获取集合中的数量
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>数量</returns>
        public async Task<long> ListLengthAsync(string key)
        {
            key = AddSysCustomKey(key);
            return await Do(async redis => await redis.ListLengthAsync(key));
        }

        #endregion 异步方法

        #endregion List

        #region SortedSet 有序集合

        #region 同步方法

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="score">排序号</param>
        public bool AddSortedSet<T>(string key, T value, double score)
        {
            key = AddSysCustomKey(key);
            return Do(redis => redis.SortedSetAdd(key, ConvertJson(value), score));
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public bool RemoveSortedSet<T>(string key, T value)
        {
            key = AddSysCustomKey(key);
            return Do(redis => redis.SortedSetRemove(key, ConvertJson(value)));
        }

        /// <summary>
        /// 获取全部
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>数据集合</returns>
        public List<T> SetRangeSortedByRank<T>(string key)
        {
            if (KeyExists(key))
            {
                key = AddSysCustomKey(key);
                return Do(redis =>
                {
                    var values = redis.SortedSetRangeByRank(key);
                    return ConvetList<T>(values);
                });
            }

            return new List<T>();
        }

        /// <summary>
        /// 获取集合中的数量
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>数量</returns>
        public long SetSortedLength(string key)
        {
            key = AddSysCustomKey(key);
            return Do(redis => redis.SortedSetLength(key));
        }

        #endregion 同步方法

        #region 异步方法

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="score">排序号</param>
        public async Task<bool> SortedSetAddAsync<T>(string key, T value, double score)
        {
            key = AddSysCustomKey(key);
            return await Do(async redis => await redis.SortedSetAddAsync(key, ConvertJson(value), score));
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public async Task<bool> SortedSetRemoveAsync<T>(string key, T value)
        {
            key = AddSysCustomKey(key);
            return await Do(async redis => await redis.SortedSetRemoveAsync(key, ConvertJson(value)));
        }

        /// <summary>
        /// 获取全部
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>数据集合</returns>
        public async Task<List<T>> SortedSetRangeByRankAsync<T>(string key)
        {
            if (KeyExists(key))
            {
                key = AddSysCustomKey(key);
                var values = await Do(async redis => await redis.SortedSetRangeByRankAsync(key));
                return ConvetList<T>(values);
            }

            return new List<T>();
        }

        /// <summary>
        /// 获取集合中的数量
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>数量</returns>
        public async Task<long> SortedSetLengthAsync(string key)
        {
            key = AddSysCustomKey(key);
            return await Do(async redis => await redis.SortedSetLengthAsync(key));
        }

        #endregion 异步方法

        #endregion SortedSet 有序集合

        #region key

        /// <summary>
        /// 删除单个key
        /// </summary>
        /// <param name="key">redis key</param>
        /// <returns>是否删除成功</returns>
        public bool DeleteKey(string key)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.KeyDelete(key));
        }

        /// <summary>
        /// 删除单个key
        /// </summary>
        /// <param name="key">redis key</param>
        /// <returns>是否删除成功</returns>
        public async Task<bool> DeleteKeyAsync(string key)
        {
            key = AddSysCustomKey(key);
            return await Do(async db => await db.KeyDeleteAsync(key));
        }

        /// <summary>
        /// 删除多个key
        /// </summary>
        /// <param name="keys">rediskey</param>
        /// <returns>成功删除的个数</returns>
        public long DeleteKey(List<string> keys)
        {
            List<string> newKeys = keys.Select(AddSysCustomKey).ToList();
            return Do(db => db.KeyDelete(ConvertRedisKeys(newKeys)));
        }

        /// <summary>
        /// 判断key是否存储
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>是否存储成功</returns>
        public bool KeyExists(string key)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.KeyExists(key));
        }

        /// <summary>
        /// 重新命名key
        /// </summary>
        /// <param name="key">旧的键</param>
        /// <param name="newKey">新的键</param>
        /// <returns>处理结果</returns>
        public bool RenameKey(string key, string newKey)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.KeyRename(key, newKey));
        }

        /// <summary>
        /// 设置Key的过期时间
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="expiry">过期时间</param>
        /// <returns>处理结果</returns>
        public bool Expire(string key, TimeSpan? expiry = default(TimeSpan?))
        {
            key = AddSysCustomKey(key);
            return Do(db => db.KeyExpire(key, expiry));
        }

        /// <summary>
        /// 设置Key的过期时间
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="expiry">过期时间</param>
        /// <returns>处理结果</returns>
        public async Task<bool> ExpireAsync(string key, TimeSpan? expiry = default(TimeSpan?))
        {
            key = AddSysCustomKey(key);
            return await Do(async db => await db.KeyExpireAsync(key, expiry));
        }

        #endregion key

        #region 发布订阅

        /// <summary>
        /// Redis发布订阅  订阅
        /// </summary>
        /// <param name="subChannel">订阅频道</param>
        /// <param name="handler">处理过程</param>
        public void Subscribe(string subChannel, Action<RedisChannel, RedisValue> handler = null)
        {
            ISubscriber sub = _conn.GetSubscriber();
            sub.Subscribe(subChannel, (channel, message) =>
            {
                if (handler == null)
                {
                    Console.WriteLine(subChannel + " 订阅收到消息：" + message);
                }
                else
                {
                    handler(channel, message);
                }
            });
        }

        /// <summary>
        /// Redis发布订阅  发布
        /// </summary>
        /// <typeparam name="T">消息对象</typeparam>
        /// <param name="channel">发布频道</param>
        /// <param name="msg">消息</param>
        /// <returns>消息的数量</returns>
        public long Publish<T>(string channel, T msg)
        {
            ISubscriber sub = _conn.GetSubscriber();
            return sub.Publish(channel, ConvertJson(msg));
        }

        /// <summary>
        /// Redis发布订阅  取消订阅
        /// </summary>
        /// <param name="channel">频道</param>
        public void Unsubscribe(string channel)
        {
            ISubscriber sub = _conn.GetSubscriber();
            sub.Unsubscribe(channel);
        }

        /// <summary>
        /// Redis发布订阅  取消全部订阅
        /// </summary>
        public void UnsubscribeAll()
        {
            ISubscriber sub = _conn.GetSubscriber();
            sub.UnsubscribeAll();
        }

        #endregion 发布订阅

        #region 其他

        /// <summary>
        /// 创建一个事务
        /// </summary>
        /// <returns>事务对象</returns>
        public ITransaction CreateTransaction()
        {
            return GetDatabase().CreateTransaction();
        }

        /// <summary>
        /// 获得一个数据库实例
        /// </summary>
        /// <returns>数据库实例</returns>
        public IDatabase GetDatabase()
        {
            return _conn.GetDatabase(DbNum);
        }

        /// <summary>
        /// 获得一个服务器实例
        /// </summary>
        /// <param name="hostAndPort">服务器地址</param>
        /// <returns>服务器实例</returns>
        public IServer GetServer(string hostAndPort = null)
        {
            hostAndPort = string.IsNullOrEmpty(hostAndPort) ? _conn.Configuration.Split(',')[0] : hostAndPort;
            return _conn.GetServer(hostAndPort);
        }

        /// <summary>
        /// 设置前缀
        /// </summary>
        /// <param name="customKey">前缀</param>
        public void SetSysCustomKey(string customKey)
        {
            CustomKey = customKey;
        }

        #endregion 其他

        #region 辅助方法

        private string AddSysCustomKey(string oldKey)
        {
            var prefixKey = CustomKey ?? string.Empty;
            return prefixKey + oldKey;
        }

        private T Do<T>(Func<IDatabase, T> func)
        {
            var database = _conn.GetDatabase(DbNum);
            return func(database);
        }

        private string ConvertJson<T>(T value)
        {
            return JsonConvert.SerializeObject(value);
        }

        private T ConvertObj<T>(RedisValue value)
        {
            return JsonConvert.DeserializeObject<T>(value);
        }

        private List<T> ConvetList<T>(RedisValue[] values)
        {
            return values.Select(ConvertObj<T>).ToList();
        }

        private RedisKey[] ConvertRedisKeys(List<string> redisKeys)
        {
            return redisKeys.Select(redisKey => (RedisKey)redisKey).ToArray();
        }

        #endregion 辅助方法

        #region 事件

        ///// <summary>
        ///// 配置更改时
        ///// </summary>
        ///// <param name="sender">触发者</param>
        ///// <param name="e">事件参数</param>
        //private static void MuxerConfigurationChanged(object sender, EndPointEventArgs e)
        //{
        //    Console.WriteLine("Configuration changed: " + e.EndPoint);
        //}

        ///// <summary>
        ///// 发生错误时
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private static void MuxerErrorMessage(object sender, RedisErrorEventArgs e)
        //{
        //    Console.WriteLine("ErrorMessage: " + e.Message);
        //}

        ///// <summary>
        ///// 重新建立连接之前的错误
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private static void MuxerConnectionRestored(object sender, ConnectionFailedEventArgs e)
        //{
        //    Console.WriteLine("ConnectionRestored: " + e.EndPoint);
        //}

        ///// <summary>
        ///// 连接失败 ， 如果重新连接成功你将不会收到这个通知
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private static void MuxerConnectionFailed(object sender, ConnectionFailedEventArgs e)
        //{
        //    Console.WriteLine("重新连接：Endpoint failed: " + e.EndPoint + ", " + e.FailureType + (e.Exception == null ? "" : (", " + e.Exception.Message)));
        //}

        ///// <summary>
        ///// 更改集群
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private static void MuxerHashSlotMoved(object sender, HashSlotMovedEventArgs e)
        //{
        //    Console.WriteLine("HashSlotMoved:NewEndPoint" + e.NewEndPoint + ", OldEndPoint" + e.OldEndPoint);
        //}

        ///// <summary>
        ///// redis类库错误
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private static void MuxerInternalError(object sender, InternalErrorEventArgs e)
        //{
        //    Console.WriteLine("InternalError:Message" + e.Exception.Message);
        //}

        #endregion 事件

        public override void Dispose(bool disposing)
        {
            _conn?.Dispose();
            _conn = null;
        }
    }
}