using Microsoft.Extensions.Configuration;

namespace Masuit.Tools.Core.Config
{
    /// <summary>
    /// .net core的配置导入
    /// </summary>
    public class CoreConfig
    {
        /// <summary>
        /// 配置对象
        /// </summary>
        public static IConfiguration Configuration { get; set; }
    }
}