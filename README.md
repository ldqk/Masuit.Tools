# Masuit.Tools
包含一些常用的操作类，大都是静态类，加密解密，反射操作，硬件信息，字符串扩展方法，日期时间扩展操作，大文件拷贝，图像裁剪，验证码等常用封装。
[官网教程](http://masuit.com/55)

# Asp.Net MVC和Asp.Net Core的支持断点续传和多线程下载的ResumeFileResult

允许你在ASP.NET Core中通过MVC/WebAPI应用程序传输文件数据时使用断点续传以及多线程下载。

它允许提供`ETag`标题以及`Last-Modified`标题。 它还支持以下前置条件标题：`If-Match`，`If-None-Match`，`If-Modified-Since`，`If-Unmodified-Since`，`If-Range`。
## 支持 ASP.NET Core 2.0
从.NET Core2.0开始，ASP.NET Core内部支持断点续传。 因此删除了与断点续传相关的所有代码。 只留下了“Content-Disposition” Inline的一部分。 现在所有代码都依赖于基础.NET类。
还删除了对多部分请求的支持。 为了支持我将不得不复制很多原始代码，因为目前没有办法简单地覆盖基类的某些部分。

## 如何使用 
### .NET Framework
在你的控制器中，你可以像在`FileResult`帮助器中构建一样使用它。
```csharp
        using Masuit.Tools.Mvc;
        using Masuit.Tools.Mvc.ResumeFileResult;
```

```csharp

        private readonly MimeMapper mimeMapper=new MimeMapper(); // 推荐使用依赖注入

        public ActionResult ResumeFileResult()
        {
            var path = Server.MapPath("~/Content/test.mp4");
            return new ResumeFileResult(path, mimeMapper.GetMimeFromPath(path), Request);
        }

        public ActionResult ResumeFile()
        {
            return this.ResumeFile("~/Content/test.mp4", mimeMapper.GetMimeFromPath(path), "test.mp4");
        }

        public ActionResult ResumePhysicalFile()
        {
            return this.ResumePhysicalFile(@"D:/test.mp4", mimeMapper.GetMimeFromPath(@"D:/test.mp4"), "test.mp4");
        }
```

### Asp.Net Core
要使用ResumeFileResults，必须在`Startup.cs`的`ConfigureServices`方法调用中配置服务：

```csharp
        using Masuit.Tools.AspNetCore.ResumeFileResults.DependencyInjection;
```

```csharp
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddResumeFileResult();
        }
```

然后在你的控制器中，你可以像在`FileResult`帮助器中构建一样使用它。

```csharp
        using Masuit.Tools.AspNetCore.ResumeFileResults.Extensions;
```

```csharp
        private const string EntityTag = "\"TestFile\"";

        private readonly IHostingEnvironment _hostingEnvironment;

        private readonly DateTimeOffset _lastModified = new DateTimeOffset(2016, 1, 1, 0, 0, 0, TimeSpan.Zero);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hostingEnvironment"></param>
        public TestController(IHostingEnvironment hostingEnvironment)
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
```

以上示例将为您的数据提供“Content-Disposition：attachment”。 当没有提供fileName时，数据将作为“Content-Disposition：inline”提供。
另外，它可以提供`ETag`和`LastModified`标题。

```csharp
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
```
