using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using System;
using System.Threading.Tasks;

namespace Masuit.Tools.AspNetCore.ResumeFileResults.ResumeFileResult
{
    /// <summary>
    /// 基于Stream的ResumeFileContentResult
    /// </summary>
    public class ResumeFileContentResult : FileContentResult, IResumeFileResult
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="fileContents">文件二进制流</param>
        /// <param name="contentType">Content-Type</param>
        /// <param name="etag">ETag</param>
        public ResumeFileContentResult(byte[] fileContents, string contentType, string etag = null) : this(fileContents, MediaTypeHeaderValue.Parse(contentType), !string.IsNullOrEmpty(etag) ? EntityTagHeaderValue.Parse(etag) : null)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="fileContents">文件二进制流</param>
        /// <param name="contentType">Content-Type</param>
        /// <param name="etag">ETag</param>
        public ResumeFileContentResult(byte[] fileContents, MediaTypeHeaderValue contentType, EntityTagHeaderValue etag = null) : base(fileContents, contentType)
        {
            EntityTag = etag;
            EnableRangeProcessing = true;
        }

        /// <inheritdoc/>
        public string FileInlineName { get; set; }

        /// <inheritdoc/>
        public override Task ExecuteResultAsync(ActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var executor = context.HttpContext.RequestServices.GetRequiredService<IActionResultExecutor<ResumeFileContentResult>>();
            return executor.ExecuteAsync(context, this);
        }
    }
}