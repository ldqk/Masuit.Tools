using Masuit.Tools.AspNetCore.ResumeFileResults.Executor;
using Masuit.Tools.AspNetCore.ResumeFileResults.ResumeFileResult;
using Masuit.Tools.Files;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace Masuit.Tools.Core.AspNetCore;

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
        services.AddHttpClient<ISevenZipCompressor, SevenZipCompressor>();
        return services;
    }

    /// <summary>
    /// 自动扫描需要注册的服务，被ServiceInject标记的class可自动注入
    /// </summary>
    /// <param name="services"></param>
    public static void AutoRegisterServices(this IServiceCollection services)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        services.RegisterServiceByAttribute(assemblies);
        services.RegisterBackgroundService(assemblies);
    }

    /// <summary>
    /// 通过 ServiceAttribute 批量注册服务
    /// </summary>
    /// <param name="services"></param>
    private static void RegisterServiceByAttribute(this IServiceCollection services, IEnumerable<Assembly> assemblies)
    {
        var types = assemblies.SelectMany(t => t.GetTypes()).Where(t => t.GetCustomAttributes(typeof(ServiceInjectAttribute), false).Length > 0 && t.IsClass && !t.IsAbstract).ToList();

        foreach (var type in types)
        {
            var typeInterface = type.GetInterfaces().FirstOrDefault();
            if (typeInterface == null)
            {
                //服务非继承自接口的直接注入
                switch (type.GetCustomAttribute<ServiceInjectAttribute>().Lifetime)
                {
                    case ServiceLifetime.Singleton: services.AddSingleton(type); break;
                    case ServiceLifetime.Scoped: services.AddScoped(type); break;
                    case ServiceLifetime.Transient: services.AddTransient(type); break;
                }
            }
            else
            {
                //服务继承自接口的和接口一起注入
                switch (type.GetCustomAttribute<ServiceInjectAttribute>().Lifetime)
                {
                    case ServiceLifetime.Singleton: services.AddSingleton(typeInterface, type); break;
                    case ServiceLifetime.Scoped: services.AddScoped(typeInterface, type); break;
                    case ServiceLifetime.Transient: services.AddTransient(typeInterface, type); break;
                }
            }
        }
    }

    /// <summary>
    /// 注册后台服务
    /// </summary>
    /// <param name="services"></param>
    private static void RegisterBackgroundService(this IServiceCollection services, IEnumerable<Assembly> assemblies)
    {
        var types = assemblies.SelectMany(t => t.GetTypes()).Where(t => typeof(BackgroundService).IsAssignableFrom(t) && !t.IsAbstract);
        foreach (var type in types)
        {
            services.AddSingleton(typeof(IHostedService), type);
        }
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class ServiceInjectAttribute : Attribute
{
    public ServiceInjectAttribute()
    {
    }

    public ServiceInjectAttribute(ServiceLifetime lifetime)
    {
        Lifetime = lifetime;
    }

    public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Transient;
}
