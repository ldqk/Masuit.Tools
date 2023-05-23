using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using System;
using System.Threading.Tasks;

namespace Masuit.Tools.AspNetCore.ResumeFileResults.ResumeFileResult
{
    /// <summary>
    /// 基于本地物理路径的ResumePhysicalFileResult
    /// </summary>
    public class ResumePhysicalFileResult : PhysicalFileResult, IResumeFileResult
    {
        /// <summary>
        /// 基于本地物理路径的ResumePhysicalFileResult
        /// </summary>
        /// <param name="fileName">文件全路径</param>
        /// <param name="contentType">Content-Type</param>
        /// <param name="etag">ETag</param>
        public ResumePhysicalFileResult(string fileName, string contentType, string etag = null) : this(fileName, MediaTypeHeaderValue.Parse(contentType), !string.IsNullOrEmpty(etag) ? EntityTagHeaderValue.Parse(etag) : null)
        {
        }

        /// <summary>
        /// 基于本地物理路径的ResumePhysicalFileResult
        /// </summary>
        /// <param name="fileName">文件全路径</param>
        /// <param name="contentType">Content-Type</param>
        /// <param name="etag">ETag</param>
        public ResumePhysicalFileResult(string fileName, MediaTypeHeaderValue contentType, EntityTagHeaderValue etag = null) : base(fileName, contentType)
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

            var executor = context.HttpContext.RequestServices.GetRequiredService<IActionResultExecutor<ResumePhysicalFileResult>>();
            return executor.ExecuteAsync(context, this);
        }
    }
}