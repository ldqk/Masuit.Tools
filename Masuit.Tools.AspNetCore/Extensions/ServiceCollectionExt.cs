using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Masuit.Tools.AspNetCore.Extensions;

public static class ServiceCollectionExt
{
    public static IServiceCollection AddMultipartRequestService(this IServiceCollection services)
    {
        services.TryAddScoped<IMultipartRequestService, MultipartRequestService>();
        return services;
    }
}
