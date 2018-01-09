using System;
using System.Collections.Generic;
using System.Net.Http;
using Masuit.Tools.Core.Config;
using Masuit.Tools.Logging;
using Masuit.Tools.Models;
using Newtonsoft.Json;

namespace Masuit.Tools.Core.Net
{
    /// <summary>
    /// Web操作扩展
    /// </summary>
    public static class WebExtension
    {
        #region 获取线程内唯一的EF上下文对象

        /// <summary>
        /// 获取线程内唯一的EF上下文对象，.NetCore中获取EF数据上下文对象，需要通过依赖注入的方式，请考虑在自己的项目中通过Masuit.Tools提供的CallContext对象实现获取线程内唯一的EF上下文对象，示例代码：
        /// <para>public static DataContext GetDbContext()</para>
        /// <para>{</para>
        /// <para>    DataContext db;</para>
        /// <para>    if (CallContext&lt;DataContext>.GetData("db") is null)</para>
        /// <para>    {</para>
        /// <para>        DbContextOptions&lt;DataContext> dbContextOption = new DbContextOptions&lt;DataContext>();</para>
        /// <para>        DbContextOptionsBuilder&lt;DataContext> dbContextOptionBuilder = new DbContextOptionsBuilder&lt;DataContext>(dbContextOption);</para>
        /// <para>        db = new DataContext(dbContextOptionBuilder.UseSqlServer(CommonHelper.ConnectionString).Options);</para>
        /// <para>        CallContext&lt;DataContext>.SetData("db", db);</para>
        /// <para>    }</para>
        /// <para>    db = CallContext&lt;DataContext>.GetData("db");</para>
        /// <para>    return db;</para>
        /// <para>}</para>
        /// </summary>
        /// <typeparam name="T">EF上下文容器对象</typeparam>
        /// <returns>EF上下文容器对象</returns>
        [Obsolete(@".NetCore中获取EF数据上下文对象，需要通过依赖注入的方式，请考虑在自己的项目中通过Masuit.Tools提供的CallContext对象实现获取线程内唯一的EF上下文对象，示例代码：
        public static DataContext GetDbContext()
        {
            DataContext db;
            if (CallContext<DataContext>.GetData(""db"") is null)
            {
                DbContextOptions<DataContext> dbContextOption = new DbContextOptions<DataContext>();
                DbContextOptionsBuilder<DataContext> dbContextOptionBuilder = new DbContextOptionsBuilder<DataContext>(dbContextOption);
                db = new DataContext(dbContextOptionBuilder.UseSqlServer(CommonHelper.ConnectionString).Options);
                CallContext<DataContext>.SetData(""db"", db);
            }
            db = CallContext<DataContext>.GetData(""db"");
            return db;
        }")]
        public static T GetDbContext<T>() where T : new()
        {
            throw new Exception(@".NetCore中获取EF数据上下文对象，需要通过依赖注入的方式，请考虑在自己的项目中通过Masuit.Tools提供的CallContext对象实现获取线程内唯一的EF上下文对象，示例代码：
                                        public static DataContext GetDbContext()
                                        {
                                            DataContext db;
                                            if (CallContext<DataContext>.GetData(""db"") is null)
                                            {
                                                DbContextOptions<DataContext> dbContextOption = new DbContextOptions<DataContext>();
                                                DbContextOptionsBuilder<DataContext> dbContextOptionBuilder = new DbContextOptionsBuilder<DataContext>(dbContextOption);
                                                db = new DataContext(dbContextOptionBuilder.UseSqlServer(CommonHelper.ConnectionString).Options);
                                                CallContext<DataContext>.SetData(""db"", db);
                                            }
                                            db = CallContext<DataContext>.GetData(""db"");
                                            return db;
                                        }
        ");
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
            ip.MatchInetAddress(out var isIpAddress);
            if (isIpAddress)
            {
                if (CoreConfig.Configuration is null)
                {
                    throw new Exception("未注入IConfiguration，请先在Startup.cs的构造函数中为Masuit.Tools.Core.Config.CoreConfig.Configuration赋值");
                }
                string ak = CoreConfig.Configuration["AppSettings:BaiduAK"];
                if (string.IsNullOrEmpty(ak))
                {
                    throw new Exception("未配置BaiduAK，请先在您的应用程序appsettings.json中的AppSettings节点下添加BaiduAK配置节(注意大小写)");
                }
                using (HttpClient client = new HttpClient() { BaseAddress = new Uri("http://api.map.baidu.com") })
                {
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
                            using (var client2 = new HttpClient { BaseAddress = new Uri("http://ip.taobao.com") })
                            {
                                var result = client2.GetStringAsync($"/service/getIpInfo.php?ip={ip}").Result;
                                TaobaoIP taobaoIp = JsonConvert.DeserializeObject<TaobaoIP>(result);
                                if (taobaoIp.Code == 0)
                                {
                                    return new Tuple<string, List<string>>(taobaoIp.IpData.Country + taobaoIp.IpData.Region + taobaoIp.IpData.City, new List<string>());
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        LogManager.Error(e);
                    }
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
            ip.MatchInetAddress(out var isIpAddress);
            if (isIpAddress)
            {
                if (CoreConfig.Configuration is null)
                {
                    throw new Exception("未注入IConfiguration，请先在Startup.cs的构造函数中为Masuit.Tools.Core.Config.CoreConfig.Configuration赋值");
                }
                string ak = CoreConfig.Configuration["AppSettings:BaiduAK"];
                if (string.IsNullOrEmpty(ak))
                {
                    throw new Exception("未配置BaiduAK，请先在您的应用程序appsettings.json中的AppSettings节点下添加BaiduAK配置节(注意大小写)");
                }
                using (HttpClient client = new HttpClient() { BaseAddress = new Uri("http://api.map.baidu.com") })
                {
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
                            using (var client2 = new HttpClient { BaseAddress = new Uri("http://ip.taobao.com") })
                            {
                                var result = client2.GetStringAsync($"/service/getIpInfo.php?ip={ip}").Result;
                                TaobaoIP taobaoIp = JsonConvert.DeserializeObject<TaobaoIP>(result);
                                if (taobaoIp.Code == 0)
                                {
                                    return new PhysicsAddress() { Status = 0, AddressResult = new AddressResult() { FormattedAddress = taobaoIp.IpData.Country + taobaoIp.IpData.Region + taobaoIp.IpData.City, AddressComponent = new AddressComponent() { Province = taobaoIp.IpData.Region }, Pois = new List<Pois>() } };
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        LogManager.Error(e);
                    }
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
                    try
                    {
                        var result = client.GetStringAsync($"/service/getIpInfo.php?ip={ip}").Result;
                        TaobaoIP taobaoIp = JsonConvert.DeserializeObject<TaobaoIP>(result);
                        if (taobaoIp.Code == 0)
                        {
                            return taobaoIp.IpData.Isp;
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }
                return $"未能找到{ip}的ISP信息";
            }
            return $"{ip}不是一个合法的IP";
        }

        #endregion
    }
}