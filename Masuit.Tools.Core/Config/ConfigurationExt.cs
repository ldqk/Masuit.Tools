using Microsoft.Extensions.Configuration;

namespace Masuit.Tools.Core.Config
{
    public static class ConfigurationExt
    {
        /// <summary>
        /// 将配置添加到Masuit.Tools，若未调用，将自动加载默认的appsettings.json
        /// </summary>
        /// <param name="config"></param>
        public static void AddToMasuitTools(this IConfiguration config)
        {
            CoreConfig.Configuration = config;
        }
    }
}