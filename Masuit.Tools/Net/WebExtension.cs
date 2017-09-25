using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Runtime.Remoting.Messaging;
using System.Web;
using System.Web.SessionState;
using Masuit.Tools.Logging;
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
        public static void Set(this HttpSessionStateBase session, string key, dynamic value) => session[key] = value;

        /// <summary>
        /// 写Session
        /// </summary>
        /// <param name="session"></param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public static void Set(this HttpSessionState session, string key, dynamic value) => session[key] = value;

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
        /// <param name="expire">过期时间，默认20分钟</param>
        /// <returns></returns>
        public static bool SetByRedis<T>(this HttpSessionState _, T obj, string cookieName = "sessionId", int expire = 20)
        {
            var sessionId = Guid.NewGuid().ToString();
            if (!string.IsNullOrEmpty(cookieName))
            {
                //如果cookieName为null，则不调用Cookie存储
                HttpCookie cookie = new HttpCookie(cookieName, sessionId);
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
            RedisHelper helper = new RedisHelper(1);
            return helper.SetString(sessionId, obj, TimeSpan.FromMinutes(expire)); //存储数据到缓存服务器，这里将字符串"my value"缓存，key 是"test"
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
        /// <param name="expire">过期时间，默认20分钟</param>
        /// <returns></returns> 
        public static bool SetByRedis<T>(this HttpSessionStateBase _, T obj, string cookieName = "sessionId", int expire = 20)
        {
            var sessionId = Guid.NewGuid().ToString();
            if (!string.IsNullOrEmpty(cookieName))
            {
                //如果cookieName为null，则不调用Cookie存储
                HttpCookie cookie = new HttpCookie(cookieName, sessionId);
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
            RedisHelper helper = new RedisHelper(1);
            return helper.SetString(sessionId, obj, TimeSpan.FromMinutes(expire)); //存储数据到缓存服务器，这里将字符串"my value"缓存，key 是"test"
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
        /// <param name="key">键</param>
        /// <param name="expire">过期时间，默认20分钟</param>
        /// <returns></returns> 
        public static T GetByRedis<T>(this HttpSessionState _, string key, int expire = 20)
        {
            RedisHelper helper = new RedisHelper(1);
            if (helper.KeyExists(key))
            {
                helper.Expire(key, TimeSpan.FromMinutes(expire));
                return helper.GetString<T>(key);
            }
            return default(T);
        }

        /// <summary>
        /// 从Redis取Session
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_"></param>
        /// <param name="key">键</param>
        /// <param name="expire">过期时间，默认20分钟</param>
        /// <returns></returns>
        public static T GetByRedis<T>(this HttpSessionStateBase _, string key, int expire = 20)
        {
            RedisHelper helper = new RedisHelper(1);
            if (helper.KeyExists(key))
            {
                helper.Expire(key, TimeSpan.FromMinutes(expire));
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
        /// <param name="expire">过期时间，默认20分钟</param>
        /// <returns></returns> 
        public static T GetByCookieRedis<T>(this HttpSessionState _, string cookieName = "sessionId", int expire = 20)
        {
            RedisHelper helper = new RedisHelper(1);
            var key = HttpContext.Current.Request.Cookies[cookieName]?.Value;
            if (helper.KeyExists(key))
            {
                helper.Expire(key, TimeSpan.FromMinutes(expire));
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
        /// <param name="expire">过期时间，默认20分钟</param>
        /// <returns></returns> 
        public static T GetByCookieRedis<T>(this HttpSessionStateBase _, string cookieName = "sessionId", int expire = 20)
        {
            RedisHelper helper = new RedisHelper(1);
            var key = HttpContext.Current.Request.Cookies[cookieName]?.Value;
            if (helper.KeyExists(key))
            {
                helper.Expire(key, TimeSpan.FromMinutes(expire));
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

        /// <summary>
        /// 从Redis根据Cookie读取到的键移除Session
        /// </summary>
        /// <param name="_"></param>
        /// <param name="cookieName"></param>
        /// <returns></returns>
        public static bool RemoveByCookieRedis(this HttpSessionState _, string cookieName = "sessionId")
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
        public static bool RemoveByRedis(this HttpSessionState _, string key = "sessionId")
        {
            RedisHelper helper = new RedisHelper(1);
            return helper.DeleteKey(key);
        }

        #endregion

        #region 获取客户端IP地址信息

        /// <summary>
        /// 根据IP地址获取详细地理信息
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static Tuple<string, List<string>> GetIPAddressInfo(this string ip)
        {
            bool isIpAddress;
            ip.MatchInetAddress(out isIpAddress);
            if (isIpAddress)
            {
                string ak = ConfigurationManager.AppSettings["BaiduAK"];
                if (string.IsNullOrEmpty(ak))
                {
                    throw new Exception("未配置BaiduAK，请先在您的应用程序web.config或者App.config中的AppSettings节点下添加BaiduAK配置节(注意大小写)");
                }
                HttpClient client = new HttpClient() { BaseAddress = new Uri("http://api.map.baidu.com") };
                try
                {
                    string ipJson = client.GetStringAsync($"/location/ip?ak={ak}&ip={ip}&coor=bd09ll").Result;
                    var ipAddress = JsonConvert.DeserializeObject<BaiduIP>(ipJson);
                    if (ipAddress.Status == 0)
                    {
                        LatiLongitude point = ipAddress.AddressInfo.LatiLongitude;
                        string result = client.GetStringAsync($"/geocoder/v2/?location={point.Y},{point.X}&output=json&pois=1&radius=1000&latest_admin=1&coordtype=bd09ll&ak={ak}").Result;
                        PhysicsAddress address = JsonConvert.DeserializeObject<PhysicsAddress>(result);
                        if (address.Status == 0)
                        {
                            string detail = $"{address.AddressResult.FormattedAddress} {address.AddressResult.AddressComponent.Direction}{address.AddressResult.AddressComponent.Distance ?? "0"}米";
                            List<string> pois = new List<string>();
                            address.AddressResult.Pois.ForEach(p => { pois.Add($"{p.AddressDetail}{p.Name} {p.Direction}{p.Distance ?? "0"}米"); });
                            return new Tuple<string, List<string>>(detail, pois);
                        }
                    }
                    else
                    {
                        client = new HttpClient { BaseAddress = new Uri("http://ip.taobao.com") };
                        string result = client.GetStringAsync($"/service/getIpInfo.php?ip={ip}").Result;
                        TaobaoIP taobaoIp = JsonConvert.DeserializeObject<TaobaoIP>(result);
                        if (taobaoIp.Code == 0)
                        {
                            return new Tuple<string, List<string>>(taobaoIp.IpData.Country + taobaoIp.IpData.Region + taobaoIp.IpData.City, new List<string>());
                        }
                    }
                }
                catch (Exception e)
                {
                    LogManager.Error(e);
                }
                return new Tuple<string, List<string>>("IP地址不正确", new List<string>());
            }
            return new Tuple<string, List<string>>($"{ip}不是一个合法的IP地址", new List<string>());
        }

        /// <summary>
        /// 根据IP地址获取详细地理信息对象
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static PhysicsAddress GetPhysicsAddressInfo(this string ip)
        {
            bool isIpAddress;
            ip.MatchInetAddress(out isIpAddress);
            if (isIpAddress)
            {
                string ak = ConfigurationManager.AppSettings["BaiduAK"];
                if (string.IsNullOrEmpty(ak))
                {
                    throw new Exception("未配置BaiduAK，请先在您的应用程序web.config或者App.config中的AppSettings节点下添加BaiduAK配置节(注意大小写)");
                }
                HttpClient client = new HttpClient() { BaseAddress = new Uri("http://api.map.baidu.com") };
                try
                {
                    string ipJson = client.GetStringAsync($"/location/ip?ak={ak}&ip={ip}&coor=bd09ll").Result;
                    var ipAddress = JsonConvert.DeserializeObject<BaiduIP>(ipJson);
                    if (ipAddress.Status == 0)
                    {
                        LatiLongitude point = ipAddress.AddressInfo.LatiLongitude;
                        string result = client.GetStringAsync($"/geocoder/v2/?location={point.Y},{point.X}&output=json&pois=1&radius=1000&latest_admin=1&coordtype=bd09ll&ak={ak}").Result;
                        PhysicsAddress address = JsonConvert.DeserializeObject<PhysicsAddress>(result);
                        if (address.Status == 0)
                        {
                            return address;
                        }
                    }
                    else
                    {
                        client = new HttpClient { BaseAddress = new Uri("http://ip.taobao.com") };
                        var result = client.GetStringAsync($"/service/getIpInfo.php?ip={ip}").Result;
                        TaobaoIP taobaoIp = JsonConvert.DeserializeObject<TaobaoIP>(result);
                        if (taobaoIp.Code == 0)
                        {
                            return new PhysicsAddress() { Status = 0, AddressResult = new AddressResult() { FormattedAddress = taobaoIp.IpData.Country + taobaoIp.IpData.Region + taobaoIp.IpData.City, AddressComponent = new AddressComponent() { Province = taobaoIp.IpData.Region }, Pois = new List<Pois>() } };
                        }
                    }
                }
                catch (Exception e)
                {
                    LogManager.Error(e);
                }
            }
            return null;
        }

        /// <summary>
        /// 根据IP地址获取ISP
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static string GetISP(this string ip)
        {
            HttpClient client = new HttpClient() { BaseAddress = new Uri($"https://api.asilu.com") };
            string result = client.GetStringAsync($"/ip/?ip={ip}").Result;
            try
            {
                return JsonConvert.DeserializeObject<IspInfo>(result).ISPName;
            }
            catch (Exception)
            {
                client = new HttpClient { BaseAddress = new Uri("http://ip.taobao.com") };
                try
                {
                    result = client.GetStringAsync($"/service/getIpInfo.php?ip={ip}").Result;
                    TaobaoIP taobaoIp = JsonConvert.DeserializeObject<TaobaoIP>(result);
                    if (taobaoIp.Code == 0)
                    {
                        return taobaoIp.IpData.Isp;
                    }
                }
                catch (Exception e)
                {
                    LogManager.Error(e);
                }
                return $"未能找到{ip}的ISP信息";
            }
        }

        #endregion
    }
}