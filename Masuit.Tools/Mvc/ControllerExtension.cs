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
        /// <param name="contentType">Content-Type</param>
        /// <returns></returns>
        public static ResumeFileResult ResumeFile(this ControllerBase controller, string virtualPath, string contentType)
        {
            return ResumeFile(controller, virtualPath, contentType, fileDownloadName: null);
        }

        /// <summary>
        /// 可断点续传和多线程下载的FileResult
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="virtualPath">服务端本地文件的虚拟路径</param>
        /// <param name="contentType">Content-Type</param>
        /// <param name="fileDownloadName">下载的文件名</param>
        /// <returns></returns>
        public static ResumeFileResult ResumeFile(this ControllerBase controller, string virtualPath, string contentType, string fileDownloadName)
        {
            return ResumeFile(controller, virtualPath, contentType, fileDownloadName, etag: null);
        }

        /// <summary>
        /// 可断点续传和多线程下载的FileResult
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="virtualPath">服务端本地文件的虚拟路径</param>
        /// <param name="contentType">Content-Type</param>
        /// <param name="fileDownloadName">下载的文件名</param>
        /// <param name="etag">ETag</param>
        /// <returns></returns>
        public static ResumeFileResult ResumeFile(this ControllerBase controller, string virtualPath, string contentType, string fileDownloadName, string etag)
        {
            string physicalPath = controller.ControllerContext.HttpContext.Request.MapPath(virtualPath);
            return new ResumeFileResult(physicalPath, contentType, controller.ControllerContext.HttpContext.Request)
            {
                FileDownloadName = fileDownloadName
            };
        }

        /// <summary>
        /// 可断点续传和多线程下载的FileResult
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="physicalPath">服务端本地文件的物理路径</param>
        /// <param name="contentType">Content-Type</param>
        /// <returns></returns>
        public static ResumeFileResult ResumePhysicalFile(this ControllerBase controller, string physicalPath, string contentType)
        {
            return ResumePhysicalFile(controller, physicalPath, contentType, fileDownloadName: null, etag: null);
        }

        /// <summary>
        /// 可断点续传和多线程下载的FileResult
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="physicalPath">服务端本地文件的物理路径</param>
        /// <param name="contentType">Content-Type</param>
        /// <param name="fileDownloadName">下载的文件名</param>
        /// <returns></returns>
        public static ResumeFileResult ResumePhysicalFile(this ControllerBase controller, string physicalPath, string contentType, string fileDownloadName)
        {
            return ResumePhysicalFile(controller, physicalPath, contentType, fileDownloadName, etag: null);
        }

        /// <summary>
        /// 可断点续传和多线程下载的FileResult
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="physicalPath">服务端本地文件的物理路径</param>
        /// <param name="contentType">Content-Type</param>
        /// <param name="fileDownloadName">下载的文件名</param>
        /// <param name="etag">ETag</param>
        /// <returns></returns>
        public static ResumeFileResult ResumePhysicalFile(this ControllerBase controller, string physicalPath, string contentType, string fileDownloadName, string etag)
        {
            return new ResumeFileResult(physicalPath, contentType, controller.ControllerContext.HttpContext.Request)
            {
                FileDownloadName = fileDownloadName
            };
        }
    }
}