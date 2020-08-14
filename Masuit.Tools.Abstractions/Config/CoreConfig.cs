#if NET461

using System.Configuration;

namespace Masuit.Tools.Core.Config
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

using System;
using Microsoft.Extensions.Configuration;

namespace Masuit.Tools.Core.Config
{
    /// <summary>
    /// .net core的配置导入
    /// </summary>
    public static class ConfigHelper
    {
        /// <summary>
        /// 配置对象
        /// </summary>
        public static IConfiguration Configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", true, true)
            .Build();

        public static string GetConfigOrDefault(string key, string defaultValue = "")
        {
            string config = ConfigHelper.Configuration[key];
            if (config.IsNullOrEmpty())
            {
                return defaultValue;
            }
            else
            {
                return config;
            }
        }
    }
}

#endif