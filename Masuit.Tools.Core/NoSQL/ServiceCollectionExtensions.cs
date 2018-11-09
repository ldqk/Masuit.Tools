using Microsoft.Extensions.DependencyInjection;

namespace Masuit.Tools.Core.NoSQL
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLocalRedisHelper(this IServiceCollection services)
        {
            return AddRedisHelper(services, "local");
        }

        public static IServiceCollection AddDefaultRedisHelper(this IServiceCollection services, string redisHost)
        {
            return AddRedisHelper(services, "default", redisHost);
        }

        public static IServiceCollection AddRedisHelper(this IServiceCollection services, string name, string redisHost = null)
        {
            RedisHelperFactory.ConnectionCache[name] = redisHost;
            services.AddTransient<RedisHelperFactory>();
            return services;
        }
    }
}