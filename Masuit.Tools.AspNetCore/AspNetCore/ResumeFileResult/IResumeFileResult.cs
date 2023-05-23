namespace Masuit.Tools.AspNetCore.ResumeFileResults.ResumeFileResult
{
    /// <summary>
    /// 可断点续传的FileResult
    /// </summary>
    public interface IResumeFileResult
    {
        /// <summary>
        /// 文件下载名
        /// </summary>
        string FileDownloadName { get; set; }

        /// <summary>
        /// 给响应头的文件名
        /// </summary>
        string FileInlineName { get; set; }
    }
}