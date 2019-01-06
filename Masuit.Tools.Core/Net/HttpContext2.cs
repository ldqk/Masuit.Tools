using Microsoft.AspNetCore.Http;

namespace Masuit.Tools.Core.Net
{
    /// <summary>
    /// 实现类似于.NET Framework中的HttpContext静态对象，使用前需要在Startup中注入，ConfigureServices方法中：services.AddStaticHttpContext();，Configure方法中：app.UseStaticHttpContext();
    /// </summary>
    public static class HttpContext2
    {
        private static IHttpContextAccessor _accessor;

        /// <summary>
        /// 获取当前禽求上下文，使用前需要在Startup中注入，ConfigureServices方法中：services.AddStaticHttpContext();，Configure方法中：app.UseStaticHttpContext();
        /// </summary>
        public static HttpContext Current => _accessor.HttpContext;

        internal static void Configure(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }
    }
}