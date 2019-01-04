using Masuit.Tools.AspNetCore.ResumeFileResults.ResumeFileResult;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Masuit.Tools.AspNetCore.ResumeFileResults.Extensions
{
    /// <summary>
    /// ResumeFileHelper
    /// </summary>
    public static class ActionContextExtension
    {
        /// <summary>
        /// 设置响应头ContentDispositionHeader
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        public static void SetContentDispositionHeaderInline(this ActionContext context, IResumeFileResult result)
        {
            context.HttpContext.Response.Headers[HeaderNames.AccessControlExposeHeaders] = HeaderNames.ContentDisposition;
            if (string.IsNullOrEmpty(result.FileDownloadName))
            {
                var contentDisposition = new ContentDispositionHeaderValue("inline");

                if (!string.IsNullOrWhiteSpace(result.FileInlineName))
                {
                    contentDisposition.SetHttpFileName(result.FileInlineName);
                }

                context.HttpContext.Response.Headers[HeaderNames.ContentDisposition] = contentDisposition.ToString();
            }
        }
    }
}