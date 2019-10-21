using Masuit.Tools.AspNetCore.ResumeFileResults.Extensions;
using Masuit.Tools.AspNetCore.ResumeFileResults.ResumeFileResult;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;

namespace Masuit.Tools.AspNetCore.ResumeFileResults.WebTest.Controllers
{
    /// <summary>
    /// 测试控制器
    /// </summary>
    [Route("file")]
    public class TestController : Controller
    {
        private const string EntityTag = "\"TestFile\"";

        private readonly IWebHostEnvironment _hostingEnvironment;

        private readonly DateTimeOffset _lastModified = new DateTimeOffset(2016, 1, 1, 0, 0, 0, TimeSpan.Zero);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hostingEnvironment"></param>
        public TestController(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet("content/{fileName}/{etag}")]
        public IActionResult FileContent(bool fileName, bool etag)
        {
            string webRoot = _hostingEnvironment.WebRootPath;
            var content = System.IO.File.ReadAllBytes(Path.Combine(webRoot, "TestFile.txt"));
            ResumeFileContentResult result = this.ResumeFile(content, "text/plain", fileName ? "TestFile.txt" : null, etag ? EntityTag : null);
            result.LastModified = _lastModified;
            return result;
        }

        [HttpGet("content/{fileName}")]
        public IActionResult FileContent(bool fileName)
        {
            string webRoot = _hostingEnvironment.WebRootPath;
            var content = System.IO.File.ReadAllBytes(Path.Combine(webRoot, "TestFile.txt"));
            var result = new ResumeFileContentResult(content, "text/plain")
            {
                FileInlineName = "TestFile.txt",
                LastModified = _lastModified
            };
            return result;
        }

        [HttpHead("file")]
        public IActionResult FileHead()
        {
            ResumeVirtualFileResult result = this.ResumeFile("TestFile.txt", "text/plain", "TestFile.txt", EntityTag);
            result.LastModified = _lastModified;
            return result;
        }

        [HttpPut("file")]
        public IActionResult FilePut()
        {
            ResumeVirtualFileResult result = this.ResumeFile("TestFile.txt", "text/plain", "TestFile.txt", EntityTag);
            result.LastModified = _lastModified;
            return result;
        }

        [HttpGet("stream/{fileName}/{etag}")]
        public IActionResult FileStream(bool fileName, bool etag)
        {
            string webRoot = _hostingEnvironment.WebRootPath;
            FileStream stream = System.IO.File.OpenRead(Path.Combine(webRoot, "TestFile.txt"));

            ResumeFileStreamResult result = this.ResumeFile(stream, "text/plain", fileName ? "TestFile.txt" : null, etag ? EntityTag : null);
            result.LastModified = _lastModified;
            return result;
        }

        [HttpGet("stream/{fileName}")]
        public IActionResult FileStream(bool fileName)
        {
            string webRoot = _hostingEnvironment.WebRootPath;
            FileStream stream = System.IO.File.OpenRead(Path.Combine(webRoot, "TestFile.txt"));

            var result = new ResumeFileStreamResult(stream, "text/plain")
            {
                FileInlineName = "TestFile.txt",
                LastModified = _lastModified
            };

            return result;
        }

        [HttpGet("physical/{fileName}/{etag}")]
        public IActionResult PhysicalFile(bool fileName, bool etag)
        {
            string webRoot = _hostingEnvironment.WebRootPath;

            ResumePhysicalFileResult result = this.ResumePhysicalFile(Path.Combine(webRoot, "TestFile.txt"), "text/plain", fileName ? "TestFile.txt" : null, etag ? EntityTag : null);
            result.LastModified = _lastModified;
            return result;
        }

        [HttpGet("physical/{fileName}")]
        public IActionResult PhysicalFile(bool fileName)
        {
            string webRoot = _hostingEnvironment.WebRootPath;

            var result = new ResumePhysicalFileResult(Path.Combine(webRoot, "TestFile.txt"), "text/plain")
            {
                FileInlineName = "TestFile.txt",
                LastModified = _lastModified
            };

            return result;
        }

        [HttpGet("virtual/{fileName}/{etag}")]
        public IActionResult VirtualFile(bool fileName, bool etag)
        {
            ResumeVirtualFileResult result = this.ResumeFile("TestFile.txt", "text/plain", fileName ? "TestFile.txt" : null, etag ? EntityTag : null);
            result.LastModified = _lastModified;
            return result;
        }

        [HttpGet("virtual/{fileName}")]
        public IActionResult VirtualFile(bool fileName)
        {
            var result = new ResumeVirtualFileResult("TestFile.txt", "text/plain")
            {
                FileInlineName = "TestFile.txt",
                LastModified = _lastModified
            };
            return result;
        }
    }
}