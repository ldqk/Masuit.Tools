using Microsoft.Extensions.DependencyInjection;

namespace Masuit.Tools.Core.NoSQL
{
    /// <summary>
    /// asp.net core依赖注入容器扩展
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 注入一个本地化的RedisHelper
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddLocalRedisHelper(this IServiceCollection services)
        {
            return AddRedisHelper(services, "local");
        }

        /// <summary>
        /// 注入一个默认的RedisHelper实例
        /// </summary>
        /// <param name="services"></param>
        /// <param name="redisHost"></param>
        /// <returns></returns>
        public static IServiceCollection AddDefaultRedisHelper(this IServiceCollection services, string redisHost)
        {
            return AddRedisHelper(services, "default", redisHost);
        }

        /// <summary>
        /// 注入RedisHelper
        /// </summary>
        /// <param name="services"></param>
        /// <param name="name"></param>
        /// <param name="redisHost"></param>
        /// <returns></returns>
        public static IServiceCollection AddRedisHelper(this IServiceCollection services, string name, string redisHost = null)
        {
            RedisHelperFactory.ConnectionCache[name] = redisHost;
            services.AddTransient<RedisHelperFactory>();
            return services;
        }
    }
}