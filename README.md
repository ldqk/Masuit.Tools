# Masuit.Tools
包含一些常用的操作类，大都是静态类，加密解密，反射操作，硬件信息，字符串扩展方法，日期时间扩展操作，大文件拷贝，图像裁剪，验证码等常用封装。
[官网教程](http://masuit.com/55)

# 特色功能示例代码
1.检验字符串是否是Email
```csharp
bool isEmail="3444764617@qq.com".MatchEmail();
```
2.获取CPU核心数
```csharp
int core = SystemInfo.GetCpuCount();
```
3.大文件操作
```csharp
        FileStream fs = new FileStream(@"D:\boot.vmdk", FileMode.OpenOrCreate, FileAccess.ReadWrite);
        {
                //fs.CopyToFile(@"D:\1.bak");//同步复制大文件
                fs.CopyToFileAsync(@"D:\1.bak");//异步复制大文件
                string md5 = fs.GetFileMD5Async().Result;//异步获取文件的MD5
        }
```

4.html的防XSS处理：
```csharp
string html = @"<link href='/Content/font-awesome/css' rel='stylesheet'/>
        <!--[if IE 7]>
        <link href='/Content/font-awesome-ie7.min.css' rel='stylesheet'/>
        <![endif]-->
        <script src='/Scripts/modernizr'></script>
        <div id='searchBox' role='search'>
        <form action='/packages' method='get'>
        <span class='user-actions'><a href='/users/account/LogOff'>退出</a></span>
        <input name='q' id='searchBoxInput'/>
        <input id='searchBoxSubmit' type='submit' value='Submit' />
        </form>
        </div>";
string s = html.HtmlSantinizerStandard();//清理后：<div><span><a href="/users/account/LogOff">退出</a></span></div>
```
5.整理操作系统的内存：
```csharp
Windows.ClearMemorySilent();
```
6.任意进制转换
```csharp
NumberFormater nf = new NumberFormater(36);//内置2-62进制的转换
//NumberFormater nf = new NumberFormater("0123456789abcdefghijklmnopqrstuvwxyz");// 自定义进制字符，可用于生成验证码
string s36 = nf.ToString(12345678);
long num = nf.FromString("7clzi");
Console.WriteLine("12345678的36进制是：" + s36); // 7clzi
Console.WriteLine("36进制的7clzi是：" + num); // 12345678
```
```csharp
var bin=12345678.ToBinary(36);//7clzi
```
7.纳秒级计时器
```csharp
HiPerfTimer timer = HiPerfTimer.StartNew();
for (int i = 0; i < 100000; i++)
{
    //todo
}
timer.Stop();
Console.WriteLine("执行for循环100000次耗时"+timer.Duration+"s");
```
```csharp
double time = HiPerfTimer.Execute(() =>
{
    for (int i = 0; i < 100000; i++)
    {
        //todo
    }
});
Console.WriteLine("执行for循环100000次耗时"+time+"s");
```
8.单机产生唯一有序的短id
```csharp
var token=Stopwatch.GetTimestamp().ToBinary(36);
```
```csharp
var set = new HashSet<string>();
double time = HiPerfTimer.Execute(() =>
{
    for (int i = 0; i < 1000000; i++)
    {
        set.Add(Stopwatch.GetTimestamp().ToBinary(36));
    }
});
Console.WriteLine(set.Count==1000000);//True
Console.WriteLine("产生100w个id耗时"+time+"s");//1.6639039s
```
9.产生分布式唯一有序短id
```csharp
var sf = SnowFlake.GetInstance();
string token = sf.GetUniqueId();// rcofqodori0w
string shortId = sf.GetUniqueShortId(8);// qodw9728
```
```csharp
var set = new HashSet<string>();
double time = HiPerfTimer.Execute(() =>
{
    for (int i = 0; i < 1000000; i++)
    {
        set.Add(SnowFlake.GetInstance().GetUniqueId());
    }
});
Console.WriteLine(set.Count == 1000000); //True
Console.WriteLine("产生100w个id耗时" + time + "s"); //2.6891495s
```
10.农历转换
```csharp
ChineseCalendar.CustomHolidays.Add(DateTime.Parse("2018-12-31"),"元旦节");//自定义节假日
ChineseCalendar today = new ChineseCalendar(DateTime.Parse("2018-12-31"));
Console.WriteLine(today.ChineseDateString);// 二零一八年十一月廿五
Console.WriteLine(today.AnimalString);// 生肖：狗
Console.WriteLine(today.GanZhiDateString);// 干支：戊戌年甲子月丁酉日
Console.WriteLine(today.DateHoliday);// 获取按公历计算的节假日
...
```
11.Linq表达式树扩展
```csharp
Expression<Func<string, bool>> where1 = s => s.StartsWith("a");
Expression<Func<string, bool>> where2 = s => s.Length > 10;
Func<string, bool> func = where1.And(where2).Compile();
bool b=func("abcd12345678");//true
```
```csharp
Expression<Func<string, bool>> where1 = s => s.StartsWith("a");
Expression<Func<string, bool>> where2 = s => s.Length > 10;
Func<string, bool> func = where1.Or(where2).Compile();
bool b=func("abc");// true
```
12.模版引擎
```csharp
var tmp = new Template("{{name}}，你好！");
tmp.Set("name", "万金油");
string s = tmp.Render();//万金油，你好！
```
```csharp
var tmp = new Template("{{one}},{{two}},{{three}}");
string s = tmp.Set("one", "1").Set("two", "2").Set("three", "3").Render();// 1,2,3
```
```csharp
var tmp = new Template("{{name}}，{{greet}}！");
tmp.Set("name", "万金油");
string s = tmp.Render();// throw 模版变量{{greet}}未被使用
```
# Asp.Net MVC和Asp.Net Core的支持断点续传和多线程下载的ResumeFileResult

允许你在ASP.NET Core中通过MVC/WebAPI应用程序传输文件数据时使用断点续传以及多线程下载。

它允许提供`ETag`标题以及`Last-Modified`标题。 它还支持以下前置条件标题：`If-Match`，`If-None-Match`，`If-Modified-Since`，`If-Unmodified-Since`，`If-Range`。
## 支持 ASP.NET Core 2.0
从.NET Core2.0开始，ASP.NET Core内部支持断点续传。 因此只是对FileResult做了一些扩展。 只留下了“Content-Disposition” Inline的一部分。 所有代码都依赖于基础.NET类。

## 如何使用 
### .NET Framework
在你的控制器中，你可以像在`FileResult`一样的方式使用它。
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
        using Masuit.Tools.AspNetCore.ResumeFileResults.Extensions;
```

```csharp
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddResumeFileResult();
        }
```

然后在你的控制器中，你可以像在`FileResult`一样的方式使用它。

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
