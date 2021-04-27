#if NET461
using System.Configuration;

namespace Masuit.Tools.Config
{
    public static class ConfigHelper
    {
        public static string GetConfigOrDefault(string key, string defaultValue = "")
        {
            return ConfigurationManager.AppSettings.Get(key) ?? defaultValue;
        }
    }
}

#else

using Microsoft.Extensions.Configuration;
using System;

namespace Masuit.Tools.Config
{
    /// <summary>
    /// .net core的配置导入
    /// </summary>
    public static class ConfigHelper
    {
        /// <summary>
        /// 配置对象
        /// </summary>
        public static IConfiguration Configuration { get; private set; } = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory).AddJsonFile("appsettings.json", true, true).Build();

        public static string GetConfigOrDefault(string key, string defaultValue = "")
        {
            string config = Configuration[key];
            return config.IsNullOrEmpty() ? defaultValue : config;
        }

        /// <summary>
        /// 将配置添加到Masuit.Tools，若未调用，将自动加载默认的appsettings.json
        /// </summary>
        /// <param name="config"></param>
        public static void AddToMasuitTools(this IConfiguration config)
        {
            Configuration = config;
        }
    }
}

#endif