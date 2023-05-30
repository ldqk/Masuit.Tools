using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Net.Http.Headers;

namespace Masuit.Tools.AspNetCore.ResumeFileResults.ResumeFileResult;

/// <summary>
/// 基于Stream的ResumeFileStreamResult
/// </summary>
public class ResumeFileStreamResult : FileStreamResult, IResumeFileResult
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="stream">文件流</param>
    /// <param name="contentType">Content-Type</param>
    /// <param name="etag">ETag</param>
    public ResumeFileStreamResult(Stream stream, string contentType, string etag = null) : this(stream, MediaTypeHeaderValue.Parse(contentType), !string.IsNullOrEmpty(etag) ? EntityTagHeaderValue.Parse(etag) : null)
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="stream">文件流</param>
    /// <param name="contentType">Content-Type</param>
    /// <param name="etag">ETag</param>
    public ResumeFileStreamResult(Stream stream, MediaTypeHeaderValue contentType, EntityTagHeaderValue etag = null) : base(stream, contentType)
    {
        if (stream.CanSeek)
        {
            stream.Seek(0, SeekOrigin.Begin);
        }

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
