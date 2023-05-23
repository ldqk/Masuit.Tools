using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Masuit.Tools.AspNetCore.ResumeFileResults.ResumeFileResult
{
    /// <summary>
    /// 基于Stream的ResumeFileStreamResult
    /// </summary>
    public class ResumeFileStreamResult : FileStreamResult, IResumeFileResult
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="fileStream">文件流</param>
        /// <param name="contentType">Content-Type</param>
        /// <param name="etag">ETag</param>
        public ResumeFileStreamResult(FileStream fileStream, string contentType, string etag = null) : this(fileStream, MediaTypeHeaderValue.Parse(contentType), !string.IsNullOrEmpty(etag) ? EntityTagHeaderValue.Parse(etag) : null)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="fileStream">文件流</param>
        /// <param name="contentType">Content-Type</param>
        /// <param name="etag">ETag</param>
        public ResumeFileStreamResult(FileStream fileStream, MediaTypeHeaderValue contentType, EntityTagHeaderValue etag = null) : base(fileStream, contentType)
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

            var executor = context.HttpContext.RequestServices.GetRequiredService<IActionResultExecutor<ResumeFileStreamResult>>();
            return executor.ExecuteAsync(context, this);
        }
    }
}