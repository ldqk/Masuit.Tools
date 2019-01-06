using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Masuit.Tools.Core.Net
{
    /// <summary>
    /// 实现类似于.NET Framework的HttpContext静态对象的中间件对象
    /// </summary>
    public static class StaticHttpContextExtensions
    {
        /// <summary>
        /// 注入HttpContext静态对象，方便在任意地方获取HttpContext，services.AddHttpContextAccessor();
        /// </summary>
        /// <param name="services"></param>
        public static void AddStaticHttpContext(this IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        /// <summary>
        /// 注入HttpContext静态对象，方便在任意地方获取HttpContext，app.UseStaticHttpContext();
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseStaticHttpContext(this IApplicationBuilder app)
        {
            var httpContextAccessor = app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
            HttpContext2.Configure(httpContextAccessor);
            return app;
        }
    }
}