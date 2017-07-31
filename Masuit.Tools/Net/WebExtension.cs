using System;
using System.Net.Http;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.SessionState;
using Masuit.Tools.Models;
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
        public static void SetSession(this HttpSessionState session, string key, dynamic value) => session.Add(key, value);

        #endregion

        #region 获取Session

        /// <summary>
        /// 获取Session
        /// </summary>
        /// <typeparam name="T">对象</typeparam>
        /// <param name="session"></param>
        /// <param name="key">键</param>
        /// <returns>对象</returns>
        public static T GetSession<T>(this HttpSessionState session, string key) => (T)session[key];

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