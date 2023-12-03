using Microsoft.AspNetCore.Builder;

namespace Masuit.Tools.AspNetCore.ModelBinder;

public static class MiddlewareExtensions
{
    /// <summary>
    /// 使用自动参数绑定中间件
    /// </summary>
    /// <param name="appBuilder"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseBodyOrDefaultModelBinder(this IApplicationBuilder appBuilder)
    {
        return appBuilder.UseMiddleware<BodyOrDefaultBinderMiddleware>();
    }
}
