using Masuit.Tools.AspNetCore.ResumeFileResults.Executor;
using Masuit.Tools.AspNetCore.ResumeFileResults.ResumeFileResult;
using Masuit.Tools.Core.Net;
using Masuit.Tools.Files;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Masuit.Tools.Core.AspNetCore
{
    /// <summary>
    /// 依赖注入ServiceCollection容器扩展方法
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 注入断点续传服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddResumeFileResult(this IServiceCollection services)
        {
            services.TryAddSingleton<IActionResultExecutor<ResumePhysicalFileResult>, ResumePhysicalFileResultExecutor>();
            services.TryAddSingleton<IActionResultExecutor<ResumeVirtualFileResult>, ResumeVirtualFileResultExecutor>();
            services.TryAddSingleton<IActionResultExecutor<ResumeFileStreamResult>, ResumeFileStreamResultExecutor>();
            services.TryAddSingleton<IActionResultExecutor<ResumeFileContentResult>, ResumeFileContentResultExecutor>();
            return services;
        }

        /// <summary>
        /// 注入7z压缩
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddSevenZipCompressor(this IServiceCollection services)
        {
            services.AddHttpClient();
            services.TryAddTransient<ISevenZipCompressor, SevenZipCompressor>();
            return services;
        }

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