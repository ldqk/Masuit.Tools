using Masuit.Tools.AspNetCore.ResumeFileResults.Executor;
using Masuit.Tools.AspNetCore.ResumeFileResults.ResumeFileResult;
using Masuit.Tools.Files;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Masuit.Tools.AspNetCore.ResumeFileResults.Extensions
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
        /// <param name="enableCache">是否启用缓存</param>
        /// <returns></returns>
        public static IServiceCollection AddSevenZipCompressor(this IServiceCollection services, bool enableCache = true)
        {
            services.AddHttpClient();
            services.TryAddTransient<ISevenZipCompressor, SevenZipCompressor>();
            SevenZipCompressor.EnableCache = enableCache;
            return services;
        }
    }
}