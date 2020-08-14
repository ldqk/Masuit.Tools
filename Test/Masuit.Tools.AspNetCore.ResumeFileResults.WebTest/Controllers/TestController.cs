using Masuit.Tools.AspNetCore.ResumeFileResults.Extensions;
using Masuit.Tools.AspNetCore.ResumeFileResults.ResumeFileResult;
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

        private readonly DateTimeOffset _lastModified = new DateTimeOffset(2016, 1, 1, 0, 0, 0, TimeSpan.Zero);

        [HttpGet("content/{fileName}/{etag}")]
        public IActionResult FileContent(bool fileName, bool etag)
        {
            var result = this.ResumeFile(System.IO.File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory + "wwwroot", "TestFile.txt")), "text/plain", fileName ? "TestFile.txt" : null, etag ? EntityTag : null);
            result.LastModified = _lastModified;
            return result;
        }

        [HttpGet("content/{fileName}")]
        public IActionResult FileContent(bool fileName)
        {
            return new ResumeFileContentResult(System.IO.File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory + "wwwroot", "TestFile.txt")), "text/plain")
            {
                FileInlineName = "TestFile.txt",
                LastModified = _lastModified
            };
        }

        [HttpHead("file")]
        public IActionResult FileHead()
        {
            var result = this.ResumeFile("TestFile.txt", "text/plain", "TestFile.txt", EntityTag);
            result.LastModified = _lastModified;
            return result;
        }

        [HttpPut("file")]
        public IActionResult FilePut()
        {
            var result = this.ResumeFile("TestFile.txt", "text/plain", "TestFile.txt", EntityTag);
            result.LastModified = _lastModified;
            return result;
        }

        [HttpGet("stream/{fileName}/{etag}")]
        public IActionResult FileStream(bool fileName, bool etag)
        {
            var stream = System.IO.File.OpenRead(Path.Combine(AppContext.BaseDirectory + "wwwroot", "TestFile.txt"));
            var result = this.ResumeFile(stream, "text/plain", fileName ? "TestFile.txt" : null, etag ? EntityTag : null);
            result.LastModified = _lastModified;
            return result;
        }

        [HttpGet("stream/{fileName}")]
        public IActionResult FileStream(bool fileName)
        {
            var stream = System.IO.File.OpenRead(Path.Combine(AppContext.BaseDirectory + "wwwroot", "TestFile.txt"));
            return new ResumeFileStreamResult(stream, "text/plain")
            {
                FileInlineName = "TestFile.txt",
                LastModified = _lastModified
            };
        }

        [HttpGet("physical/{fileName}/{etag}")]
        public IActionResult PhysicalFile(bool fileName, bool etag)
        {
            var result = this.ResumePhysicalFile(Path.Combine(AppContext.BaseDirectory + "wwwroot", "TestFile.txt"), "text/plain", fileName ? "TestFile.txt" : null, etag ? EntityTag : null);
            result.LastModified = _lastModified;
            return result;
        }

        [HttpGet("physical/{fileName}")]
        public IActionResult PhysicalFile(bool fileName)
        {
            return new ResumePhysicalFileResult(Path.Combine(AppContext.BaseDirectory + "wwwroot", "TestFile.txt"), "text/plain")
            {
                FileInlineName = "TestFile.txt",
                LastModified = _lastModified
            };
        }

        [HttpGet("virtual/{fileName}/{etag}")]
        public IActionResult VirtualFile(bool fileName, bool etag)
        {
            var result = this.ResumeFile("TestFile.txt", "text/plain", fileName ? "TestFile.txt" : null, etag ? EntityTag : null);
            result.LastModified = _lastModified;
            return result;
        }

        [HttpGet("virtual/{fileName}")]
        public IActionResult VirtualFile(bool fileName)
        {
            return new ResumeVirtualFileResult("TestFile.txt", "text/plain")
            {
                FileInlineName = "TestFile.txt",
                LastModified = _lastModified
            };
        }
    }
}