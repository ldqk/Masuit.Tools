using System;
using System.Net.Http;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;
using Masuit.Tools.Models;
using Masuit.Tools.NoSQL;
using Newtonsoft.Json;

namespace Masuit.Tools.Net
{
    /// <summary>
    /// Web操作扩展
    /// </summary>
    public static class WebExtension
    {
        #region 获取线程内唯一的EF上下文对象

        /// <summary>
        /// 获取线程内唯一的EF上下文对象
        /// </summary>
        /// <typeparam name="T">EF上下文容器对象</typeparam>
        /// <returns>EF上下文容器对象</returns>
        public static T GetDbContext<T>() where T : new()
        {
            T db;
            if (CallContext.GetData("db") == null) //由于CallContext比HttpContext先存在，所以首选CallContext为线程内唯一对象
            {
                db = new T();
                CallContext.SetData("db", db);
            }
            db = (T)CallContext.GetData("db");
            return db;
        }

        #endregion

        #region 写Session

        /// <summary>
        /// 写Session
        /// </summary>
        /// <param name="session"></param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public static void Set(this HttpSessionStateBase session, string key, dynamic value) => session.Add(key, value);

        /// <summary>
        /// 写Session
        /// </summary>
        /// <param name="session"></param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public static void Set(this HttpSessionState session, string key, dynamic value) => session.Add(key, value);

        /// <summary>
        /// 将Session存到Redis，需要先在config中配置链接字符串，连接字符串在config配置文件中的ConnectionStrings节下配置，name固定为RedisHosts，值的格式：127.0.0.1:6379,allowadmin=true，若未正确配置，将按默认值“127.0.0.1:6379,allowadmin=true”进行操作，如：<br/>
        /// &lt;connectionStrings&gt;<br/>
        ///      &lt;add name = "RedisHosts" connectionString="127.0.0.1:6379,allowadmin=true"/&gt;<br/>
        /// &lt;/connectionStrings&gt;
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_"></param>
        /// <param name="obj">需要存的对象</param>
        /// <param name="cookieName">Cookie键，默认为sessionId</param>
        /// <returns></returns>
        public static bool SetByRedis<T>(this HttpSessionState _, T obj, string cookieName = "sessionId")
        {
            var sessionId = Guid.NewGuid().ToString();
            if (!string.IsNullOrEmpty(cookieName))
            {
                //如果cookieName为null，则不调用Cookie存储
                HttpCookie cookie = new HttpCookie(cookieName, sessionId);
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
            RedisHelper helper = new RedisHelper(1);
            return helper.SetString(sessionId, obj, TimeSpan.FromMinutes(20)); //存储数据到缓存服务器，这里将字符串"my value"缓存，key 是"test"
        }

        /// <summary>
        /// 将Session存到Redis，需要先在config中配置链接字符串，连接字符串在config配置文件中的ConnectionStrings节下配置，name固定为RedisHosts，值的格式：127.0.0.1:6379,allowadmin=true，若未正确配置，将按默认值“127.0.0.1:6379,allowadmin=true”进行操作，如：<br/>
        /// &lt;connectionStrings&gt;<br/>
        ///      &lt;add name = "RedisHosts" connectionString="127.0.0.1:6379,allowadmin=true"/&gt;<br/>
        /// &lt;/connectionStrings&gt;
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_"></param>
        /// <param name="obj">需要存的对象</param>
        /// <param name="cookieName">Cookie键，默认为sessionId</param>
        /// <returns></returns>
        public static bool SetByRedis<T>(this HttpSessionStateBase _, T obj, string cookieName = "sessionId")
        {
            var sessionId = Guid.NewGuid().ToString();
            if (!string.IsNullOrEmpty(cookieName))
            {
                //如果cookieName为null，则不调用Cookie存储
                HttpCookie cookie = new HttpCookie(cookieName, sessionId);
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
            RedisHelper helper = new RedisHelper(1);
            return helper.SetString(sessionId, obj, TimeSpan.FromMinutes(20)); //存储数据到缓存服务器，这里将字符串"my value"缓存，key 是"test"
        }
        #endregion

        #region 获取Session

        /// <summary>
        /// 获取Session
        /// </summary>
        /// <typeparam name="T">对象</typeparam>
        /// <param name="session"></param>
        /// <param name="key">键</param>
        /// <returns>对象</returns>
        public static T Get<T>(this HttpSessionStateBase session, string key) => (T)session[key];

        /// <summary>
        /// 获取Session
        /// </summary>
        /// <typeparam name="T">对象</typeparam>
        /// <param name="session"></param>
        /// <param name="key">键</param>
        /// <returns>对象</returns>
        public static T Get<T>(this HttpSessionState session, string key) => (T)session[key];

        /// <summary>
        /// 从Redis取Session
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_"></param>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public static T GetByRedis<T>(this HttpSessionState _, string sessionId)
        {
            RedisHelper helper = new RedisHelper(1);
            if (helper.KeyExists(sessionId))
            {
                helper.Expire(sessionId, TimeSpan.FromMinutes(20));
                return helper.GetString<T>(sessionId);
            }
            return default(T);
        }

        /// <summary>
        /// 从Redis取Session
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_"></param>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public static T GetByRedis<T>(this HttpSessionStateBase _, string sessionId)
        {
            RedisHelper helper = new RedisHelper(1);
            if (helper.KeyExists(sessionId))
            {
                helper.Expire(sessionId, TimeSpan.FromMinutes(20));
                return helper.GetString<T>(sessionId);
            }
            return default(T);
        }

        /// <summary>
        /// 从Redis根据Cookie读取到的键取Session值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_"></param>
        /// <param name="cookieName">用于存SessionId的Cookie键</param>
        /// <returns></returns>
        public static T GetByCookieRedis<T>(this HttpSessionState _, string cookieName = "sessionId")
        {
            RedisHelper helper = new RedisHelper(1);
            var key = HttpContext.Current.Request.Cookies[cookieName]?.Value;
            if (helper.KeyExists(key))
            {
                helper.Expire(key, TimeSpan.FromMinutes(20));
                return helper.GetString<T>(key);
            }
            return default(T);
        }

        /// <summary>
        /// 从Redis根据Cookie读取到的键取Session值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_"></param>
        /// <param name="cookieName">用于存SessionId的Cookie键</param>
        /// <returns></returns>
        public static T GetByCookieRedis<T>(this HttpSessionStateBase _, string cookieName = "sessionId")
        {
            RedisHelper helper = new RedisHelper(1);
            var key = HttpContext.Current.Request.Cookies[cookieName]?.Value;
            if (helper.KeyExists(key))
            {
                helper.Expire(key, TimeSpan.FromMinutes(20));
                return helper.GetString<T>(key);
            }
            return default(T);
        }

        /// <summary>
        /// 从Redis根据Cookie读取到的键移除Session
        /// </summary>
        /// <param name="_"></param>
        /// <param name="cookieName"></param>
        /// <returns></returns>
        public static bool RemoveByCookieRedis(this HttpSessionStateBase _, string cookieName = "sessionId")
        {
            RedisHelper helper = new RedisHelper(1);
            var key = HttpContext.Current.Request.Cookies[cookieName]?.Value;
            HttpContext.Current.Request.Cookies[cookieName].Expires = DateTime.Now.AddDays(-1);
            return helper.DeleteKey(key);
        }

        /// <summary>
        /// 从Redis移除对应键的Session
        /// </summary>
        /// <param name="_"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool RemoveByRedis(this HttpSessionStateBase _, string key = "sessionId")
        {
            RedisHelper helper = new RedisHelper(1);
            return helper.DeleteKey(key);
        }

        #endregion

        #region 获取客户端IP地址信息

        /// <summary>
        /// 获取客户端IP地址信息
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <returns>详细的地理位置、运营商信息</returns>
        public static async Task<IPData> GetIPAddressInfoAsync(this string ip)
        {
            var client = new HttpClient();
            bool isIpAddress;
            Match match = ip.MatchInetAddress(out isIpAddress); //IP地址正则
            if (isIpAddress)
            {
                try
                {
                    string ipData = await client.GetStringAsync($"http://ip.taobao.com/service/getIpInfo.php?ip={ip}"); //根据API地址获取
                    var ipAddress = JsonConvert.DeserializeObject<INetAddress>(ipData); //IP地址对象变量声明
                    return ipAddress?.data ?? new INetAddress { data = { region = "未能获取到IP地址信息" } }.data; //如果发生异常，则构造一个空对象;
                }
                catch (Exception)
                {
                    return new INetAddress { data = { region = "未能获取到IP地址信息" } }.data; //如果发生异常，则构造一个空对象
                }
            }

            return new INetAddress { data = { region = "IP地址格式不正确" } }.data; //如果发生异常，则构造一个空对象
        }

        #endregion
    }
}