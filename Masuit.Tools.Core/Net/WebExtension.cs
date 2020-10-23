using Masuit.Tools.Core.Config;
using Masuit.Tools.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading.Tasks;

namespace Masuit.Tools.Core.Net
{
    /// <summary>
    /// Web操作扩展
    /// </summary>
    public static class WebExtension
    {
        #region 获取客户端IP地址信息

        /// <summary>
        /// 根据IP地址获取详细地理信息
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static async Task<Tuple<string, List<string>>> GetIPAddressInfo(this string ip)
        {
            var address = await GetPhysicsAddressInfo(ip);
            if (address?.Status == 0)
            {
                string detail = $"{address.AddressResult.FormattedAddress} {address.AddressResult.AddressComponent.Direction}{address.AddressResult.AddressComponent.Distance ?? "0"}米";
                var pois = address.AddressResult.Pois.Select(p => $"{p.AddressDetail}{p.Name} {p.Direction}{p.Distance ?? "0"}米").ToList();
                return new Tuple<string, List<string>>(detail, pois);
            }

            return new Tuple<string, List<string>>("IP地址不正确", new List<string>());
        }

        private static readonly HttpClient HttpClient = new HttpClient();

        /// <summary>
        /// 根据IP地址获取详细地理信息对象
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static async Task<PhysicsAddress> GetPhysicsAddressInfo(this string ip)
        {
            if (!ip.MatchInetAddress())
            {
                return null;
            }

            var ak = ConfigHelper.GetConfigOrDefault("BaiduAK", null) ?? throw new Exception("未配置BaiduAK，请先在您的应用程序appsettings.json中下添加BaiduAK配置节(注意大小写)；或手动在程序入口处调用IConfiguration的AddToMasuitTools方法");
            HttpClient.DefaultRequestHeaders.Referrer = new Uri("http://lbsyun.baidu.com/jsdemo.htm");
            var task = HttpClient.GetAsync($"http://api.map.baidu.com/location/ip?ak={ak}&ip={ip}&coor=bd09ll").ContinueWith(async t =>
            {
                if (t.IsFaulted || t.IsCanceled)
                {
                    return null;
                }
                var res = await t;
                if (!res.IsSuccessStatusCode)
                {
                    return null;
                }

                var ipAddress = JsonConvert.DeserializeObject<BaiduIP>(await res.Content.ReadAsStringAsync());
                if (ipAddress.Status == 0)
                {
                    var point = ipAddress.AddressInfo.LatiLongitude;
                    var result = HttpClient.GetStringAsync($"http://api.map.baidu.com/geocoder/v2/?location={point.Y},{point.X}&output=json&pois=1&radius=1000&latest_admin=1&coordtype=bd09ll&ak={ak}").Result;
                    var address = JsonConvert.DeserializeObject<PhysicsAddress>(result);
                    if (address.Status == 0)
                    {
                        return address;
                    }
                }
                else
                {
                    return await HttpClient.GetAsync($"http://ip.taobao.com/service/getIpInfo.php?ip={ip}").ContinueWith(tt =>
                    {
                        if (tt.IsFaulted || tt.IsCanceled)
                        {
                            return null;
                        }
                        var result = tt.Result;
                        if (!result.IsSuccessStatusCode)
                        {
                            return null;
                        }

                        var taobaoIp = JsonConvert.DeserializeObject<TaobaoIP>(result.Content.ReadAsStringAsync().Result);
                        if (taobaoIp.Code == 0)
                        {
                            return new PhysicsAddress()
                            {
                                Status = 0,
                                AddressResult = new AddressResult()
                                {
                                    FormattedAddress = taobaoIp.IpData.Country + taobaoIp.IpData.Region + taobaoIp.IpData.City,
                                    AddressComponent = new AddressComponent()
                                    {
                                        Province = taobaoIp.IpData.Region
                                    },
                                    Pois = new List<Pois>()
                                }
                            };
                        }
                        return null;
                    });
                }
                return null;
            });
            return await await task;
        }

        /// <summary>
        /// 根据IP地址获取ISP
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static string GetISP(this string ip)
        {
            if (!ip.MatchInetAddress())
            {
                return $"{ip}不是一个合法的IP";
            }

            var task = HttpClient.GetAsync($"http://ip.taobao.com/service/getIpInfo.php?ip={ip}").ContinueWith(async t =>
            {
                if (t.IsFaulted)
                {
                    return $"未能找到{ip}的ISP信息";
                }
                var result = await t;
                if (result.IsSuccessStatusCode)
                {
                    var taobaoIp = JsonConvert.DeserializeObject<TaobaoIP>(await result.Content.ReadAsStringAsync());
                    if (taobaoIp.Code == 0)
                    {
                        return taobaoIp.IpData.Isp;
                    }
                }
                return $"未能找到{ip}的ISP信息";
            });
            return task.Result.Result;
        }

        #endregion 获取客户端IP地址信息

        /// <summary>
        /// 写Session
        /// </summary>
        /// <param name="session"></param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public static void Set(this ISession session, string key, object value)
        {
            session.SetString(key, value.ToJsonString());
        }

        /// <summary>
        /// 获取Session
        /// </summary>
        /// <typeparam name="T">对象</typeparam>
        /// <param name="session"></param>
        /// <param name="key">键</param>
        /// <returns>对象</returns>
        public static T Get<T>(this ISession session, string key)
        {
            string value = session.GetString(key);
            if (string.IsNullOrEmpty(value))
            {
                return typeof(T).Namespace switch
                {
                    "System.Collections.Generic" => (T)(Expression.Lambda(Expression.New(typeof(T))).Compile().DynamicInvoke()),
                    _ => default
                };
            }
            return JsonConvert.DeserializeObject<T>(value);
        }
    }
}