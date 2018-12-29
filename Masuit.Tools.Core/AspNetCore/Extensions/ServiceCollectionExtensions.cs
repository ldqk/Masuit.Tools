using Masuit.Tools.AspNetCore.ResumeFileResults.Executor;
using Masuit.Tools.AspNetCore.ResumeFileResults.ResumeFileResult;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

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
        /// <exception cref="ArgumentNullException"></exception>
        public static IServiceCollection AddResumeFileResult(this IServiceCollection services)
        {
            services.TryAddSingleton<IActionResultExecutor<ResumePhysicalFileResult>, ResumePhysicalFileResultExecutor>();
            services.TryAddSingleton<IActionResultExecutor<ResumeVirtualFileResult>, ResumeVirtualFileResultExecutor>();
            services.TryAddSingleton<IActionResultExecutor<ResumeFileStreamResult>, ResumeFileStreamResultExecutor>();
            services.TryAddSingleton<IActionResultExecutor<ResumeFileContentResult>, ResumeFileContentResultExecutor>();
            return services;
        }
    }
}