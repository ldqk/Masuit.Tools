using Microsoft.Extensions.Configuration;
using System;

namespace Masuit.Tools.Core.Config
{
    /// <summary>
    /// .net core的配置导入
    /// </summary>
    internal class CoreConfig
    {
        /// <summary>
        /// 配置对象
        /// </summary>
        internal static IConfiguration Configuration => new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory).AddJsonFile("appsettings.json", true, true).AddInMemoryCollection().Build();
    }
}
