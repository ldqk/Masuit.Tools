using Masuit.Tools.Mvc.ActionResults;
using System.IO;
using System.Web.Mvc;

namespace Masuit.Tools.Mvc
{
    public static class ControllerExtension
    {
        /// <summary>
        /// 可断点续传和多线程下载的FileResult
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="virtualPath">服务端本地文件的虚拟路径</param>
        /// <returns></returns>
        public static ResumeFileResult ResumeFile(this ControllerBase controller, string virtualPath)
        {
            return ResumeFile(controller, virtualPath, null);
        }

        /// <summary>
        /// 可断点续传和多线程下载的FileResult
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="virtualPath">服务端本地文件的虚拟路径</param>
        /// <param name="fileDownloadName">下载的文件名</param>
        /// <returns></returns>
        public static ResumeFileResult ResumeFile(this ControllerBase controller, string virtualPath, string fileDownloadName)
        {
            return ResumeFile(controller, virtualPath, fileDownloadName, etag: null);
        }

        /// <summary>
        /// 可断点续传和多线程下载的FileResult
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="virtualPath">服务端本地文件的虚拟路径</param>
        /// <param name="fileDownloadName">下载的文件名</param>
        /// <param name="etag">ETag</param>
        /// <returns></returns>
        public static ResumeFileResult ResumeFile(this ControllerBase controller, string virtualPath, string fileDownloadName, string etag)
        {
            return new ResumeFileResult(controller.ControllerContext.HttpContext.Request.MapPath(virtualPath), controller.ControllerContext.HttpContext.Request)
            {
                FileDownloadName = fileDownloadName
            };
        }

        /// <summary>
        /// 可断点续传和多线程下载的FileResult
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="fileStream">文件流</param>
        /// <param name="fileDownloadName">下载的文件名</param>
        /// <returns></returns>
        public static ResumeActionResultBase ResumeFile(this ControllerBase controller, FileStream fileStream, string fileDownloadName)
        {
            return new ResumeFileStreamResult(fileStream, fileDownloadName);
        }

        /// <summary>
        /// 可断点续传和多线程下载的FileResult
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="fileStream">文件流</param>
        /// <param name="fileDownloadName">下载的文件名</param>
        /// <param name="etag">ETag</param>
        /// <returns></returns>
        public static ResumeActionResultBase ResumeFile(this ControllerBase controller, FileStream fileStream, string fileDownloadName, string etag)
        {
            return new ResumeFileStreamResult(fileStream, fileDownloadName, etag);
        }

        /// <summary>
        /// 可断点续传和多线程下载的FileResult
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="bytes">文件流</param>
        /// <param name="fileDownloadName">下载的文件名</param>
        /// <returns></returns>
        public static ResumeActionResultBase ResumeFile(this ControllerBase controller, byte[] bytes, string fileDownloadName)
        {
            return new ResumeFileContentResult(bytes, fileDownloadName);
        }

        /// <summary>
        /// 可断点续传和多线程下载的FileResult
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="bytes">文件流</param>
        /// <param name="fileDownloadName">下载的文件名</param>
        /// <param name="etag">ETag</param>
        /// <returns></returns>
        public static ResumeActionResultBase ResumeFile(this ControllerBase controller, byte[] bytes, string fileDownloadName, string etag)
        {
            return new ResumeFileContentResult(bytes, fileDownloadName, etag);
        }

        /// <summary>
        /// 可断点续传和多线程下载的FileResult
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="physicalPath">服务端本地文件的物理路径</param>
        /// <returns></returns>
        public static ResumeFileResult ResumePhysicalFile(this ControllerBase controller, string physicalPath)
        {
            return ResumePhysicalFile(controller, physicalPath, null, null);
        }

        /// <summary>
        /// 可断点续传和多线程下载的FileResult
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="physicalPath">服务端本地文件的物理路径</param>
        /// <param name="fileDownloadName">下载的文件名</param>
        /// <returns></returns>
        public static ResumeFileResult ResumePhysicalFile(this ControllerBase controller, string physicalPath, string fileDownloadName)
        {
            return ResumePhysicalFile(controller, physicalPath, fileDownloadName, etag: null);
        }

        /// <summary>
        /// 可断点续传和多线程下载的FileResult
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="physicalPath">服务端本地文件的物理路径</param>
        /// <param name="fileDownloadName">下载的文件名</param>
        /// <param name="etag">ETag</param>
        /// <returns></returns>
        public static ResumeFileResult ResumePhysicalFile(this ControllerBase controller, string physicalPath, string fileDownloadName, string etag)
        {
            return new ResumeFileResult(physicalPath, controller.ControllerContext.HttpContext.Request)
            {
                FileDownloadName = fileDownloadName
            };
        }
    }
}