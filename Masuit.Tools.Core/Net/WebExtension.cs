using Masuit.Tools.Core.Config;
using Masuit.Tools.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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
            ip.MatchInetAddress(out var isIpAddress);
            if (isIpAddress)
            {
                var address = await GetPhysicsAddressInfo(ip);
                if (address.Status == 0)
                {
                    string detail = $"{address.AddressResult.FormattedAddress} {address.AddressResult.AddressComponent.Direction}{address.AddressResult.AddressComponent.Distance ?? "0"}米";
                    List<string> pois = address.AddressResult.Pois.Select(p => $"{p.AddressDetail}{p.Name} {p.Direction}{p.Distance ?? "0"}米").ToList();
                    return new Tuple<string, List<string>>(detail, pois);
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
        public static async Task<PhysicsAddress> GetPhysicsAddressInfo(this string ip)
        {
            ip.MatchInetAddress(out var isIpAddress);
            if (isIpAddress)
            {
                string ak = CoreConfig.Configuration["BaiduAK"];
                if (string.IsNullOrEmpty(ak))
                {
                    throw new Exception("未配置BaiduAK，请先在您的应用程序appsettings.json中下添加BaiduAK配置节(注意大小写)");
                }
                using (HttpClient client = new HttpClient() { BaseAddress = new Uri("http://api.map.baidu.com") })
                {
                    client.DefaultRequestHeaders.Referrer = new Uri("http://lbsyun.baidu.com/jsdemo.htm");
                    var task = client.GetAsync($"/location/ip?ak={ak}&ip={ip}&coor=bd09ll").ContinueWith(async t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            return null;
                        }
                        var res = await t;
                        if (res.IsSuccessStatusCode)
                        {
                            var ipAddress = JsonConvert.DeserializeObject<BaiduIP>(await res.Content.ReadAsStringAsync());
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
                                using (var client2 = new HttpClient { BaseAddress = new Uri("http://ip.taobao.com") })
                                {
                                    return await await client2.GetAsync($"/service/getIpInfo.php?ip={ip}").ContinueWith(async tt =>
                                    {
                                        if (tt.IsFaulted || tt.IsCanceled)
                                        {
                                            return null;
                                        }
                                        var result = await tt;
                                        if (result.IsSuccessStatusCode)
                                        {
                                            TaobaoIP taobaoIp = JsonConvert.DeserializeObject<TaobaoIP>(await result.Content.ReadAsStringAsync());
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
                                        }
                                        return null;
                                    });
                                }
                            }
                        }
                        return null;
                    });
                    return await await task;
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
            if (ip.MatchInetAddress())
            {
                using (var client = new HttpClient { BaseAddress = new Uri("http://ip.taobao.com") })
                {
                    var task = client.GetAsync($"/service/getIpInfo.php?ip={ip}").ContinueWith(async t =>
                    {
                        if (t.IsFaulted)
                        {
                            return $"未能找到{ip}的ISP信息";
                        }
                        var result = await t;
                        if (result.IsSuccessStatusCode)
                        {
                            TaobaoIP taobaoIp = JsonConvert.DeserializeObject<TaobaoIP>(await result.Content.ReadAsStringAsync());
                            if (taobaoIp.Code == 0)
                            {
                                return taobaoIp.IpData.Isp;
                            }
                        }
                        return $"未能找到{ip}的ISP信息";
                    });
                    return task.Result.Result;
                }
            }
            return $"{ip}不是一个合法的IP";
        }

        #endregion

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
                return default;
            }
            return JsonConvert.DeserializeObject<T>(value);
        }
    }
}