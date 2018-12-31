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
13.List转Datatable
```csharp
var list = new List<MyClass>()
{
    new MyClass()
    {
        Name = "张三",
        Age = 22
    },
    new MyClass()
    {
        Name = "李四",
        Age = 21
    },
    new MyClass()
    {
        Name = "王五",
        Age = 28
    }
};
var table = list.Select(c => new{姓名=c.Name,年龄=c.Age}).ToList().ToDataTable();// 将自动填充列姓名和年龄
```
14.文件压缩解压
```csharp
SharpZip.PackFiles("D:\\1.zip","D:\\test");
SharpZip.UnpackFiles("D:\\1.zip","D:\\test");
```
```csharp
ClassZip.Zip("D:\\1.txt","D:\\1.zip");
ClassZip.UnZip("D:\\1.zip","D:\\1");
byte[] bytes = ClassZip.ZipStream(new List<string>()
{
    "D:\\1.txt",
    "E:\\2.txt",
    "D:\\test\\3.txt"
});
```
15.日志组件
```csharp
LogManager.LogDirectory=AppDomain.CurrentDomain.BaseDirectory+"/logs";
LogManager.Event+=info =>
{
    //todo:注册一些事件操作
};
LogManager.Info("记录一次消息");
LogManager.Error(new Exception("异常消息"));
```
16.FTP客户端
```csharp
FtpClient ftpClient = FtpClient.GetAnonymousClient("192.168.2.2");//创建一个匿名访问的客户端
//FtpClient ftpClient = FtpClient.GetClient("192.168.2.3","admin","123456");// 创建一个带用户名密码的客户端
ftpClient.Delete("/1.txt");// 删除文件
ftpClient.Download("/test/2.txt","D:\\test\\2.txt");// 下载文件
ftpClient.UploadFile("/test/22.txt","D:\\test\\22.txt",(sum, progress) =>
{
    Console.WriteLine("已上传："+progress*1.0/sum);
});//上传文件并检测进度
List<string> files = ftpClient.GetFiles("/");//列出ftp服务端文件列表
...
```
17.多线程后台下载
```csharp
var mtd = new MultiThreadDownloader("https://attachments-cdn.shimo.im/yXwC4kphjVQu06rH/KeyShot_Pro_7.3.37.7z",Environment.GetEnvironmentVariable("temp"),"E:\\Downloads\\KeyShot_Pro_7.3.37.7z",8);
mtd.TotalProgressChanged+=(sender, e) =>
{
    var downloader = sender as MultiThreadDownloader;
    Console.WriteLine("下载进度："+downloader.TotalProgress+"%");
    Console.WriteLine("下载速度："+downloader.TotalSpeedInBytes/1024/1024+"MBps");
};
mtd.FileMergeProgressChanged+=(sender, e) =>
{
    Console.WriteLine("下载完成");
};
mtd.Start();//开始下载
//mtd.Pause(); // 暂停下载
//mtd.Resume(); // 继续下载
```
18.Socket客户端操作类
```csharp
var tcpClient = new TcpClient(AddressFamily.InterNetwork);
Socket socket = tcpClient.ConnectSocket(IPAddress.Any,5000);
socket.SendFile("D:\\test\\1.txt",false,i =>
{
    Console.WriteLine("已发送"+i+"%");
});
```
19.RedisHelper
.Net Framework:
```csharp
RedisHelper redisHelper = RedisHelper.GetInstance();// 获取新实例并指定连接第0个数据库
//RedisHelper redisHelper = RedisHelper.GetInstance(2);// 获取新实例并指定连接第2个数据库
//RedisHelper redisHelper = RedisHelper.GetInstance("192.168.3.150:6379");// 获取新实例并指定连接第0个数据库
//RedisHelper redisHelper = RedisHelper.GetInstance("192.168.3.150:6379",2);// 获取新实例并指定连接第2个数据库
//RedisHelper redisHelper = RedisHelper.GetSingleInstance();// 获取单例实例并指定连接第2个数据库
//RedisHelper redisHelper = RedisHelper.GetSingleInstance(2);// 获取单例实例并指定连接第2个数据库
//RedisHelper redisHelper = RedisHelper.GetSingleInstance("192.168.3.150:6379");// 获取单例实例并指定连接第0个数据库
//RedisHelper redisHelper = RedisHelper.GetSingleInstance("192.168.3.150:6379",2);// 获取单例实例并指定连接第2个数据库
redisHelper.SetString("key","value");
string value = redisHelper.GetString("key");
redisHelper.ListLeftPush("list","value");
List<string> list = redisHelper.ListRange<string>("list");
```
Asp.Net Core依赖注入方式:
Startup.cs:
```csharp
services.AddDefaultRedisHelper("192.168.16.145:6379,password=xilife2018,connectTimeout=1000,connectRetry=1,syncTimeout=1000");//注入一个默认实例
services.AddLocalRedisHelper();// 注入本地实例
services.AddRedisHelper("aa", "192.168.16.145:6379,password=xilife2018,connectTimeout=1000,connectRetry=1,syncTimeout=1000");// 通用注入
```
Controller:
```csharp
public RedisHelper RedisHelper { get; set; }
public HomeController(RedisHelperFactory redisHelperFactory)
{
    RedisHelper=redisHelperFactory.Create("aa",0);// 创建命名为aa的RedisHelper，指定数据库0
    RedisHelper=redisHelperFactory.CreateDefault(0); // 创建默认的RedisHelper，指定数据库0
    RedisHelper=redisHelperFactory.CreateLocal(0); // 创建连接本机的RedisHelper，指定数据库0
}
```
方法调用方式和.NET Framework方式相同
20.加密解密
```csharp
var enc="123456".MDString();// MD5加密
var enc="123456".MDString("abc");// MD5加盐加密
var enc="123456".MDString2();// MD5两次加密
var enc="123456".MDString2("abc");// MD5两次加盐加密
var enc="123456".MDString3();// MD5三次加密
var enc="123456".MDString3("abc");// MD5三次加盐加密

string aes = "123456".AESEncrypt();// AES加密为密文
string s = aes.AESDecrypt(); //AES解密为明文
string aes = "123456".AESEncrypt("abc");// AES密钥加密为密文
string s = aes.AESDecrypt("abc"); //AES密钥解密为明文

string aes = "123456".DesEncrypt();// DES加密为密文
string s = aes.DesDecrypt(); //DES解密为明文
string aes = "123456".DesEncrypt("abcdefgh");// DES密钥加密为密文
string s = aes.DesDecrypt("abcdefgh"); //DES密钥加密为密文

RsaKey rsaKey = RsaCrypt.GenerateRsaKeys();// 生成RSA密钥对
string encrypt = "123456".RSAEncrypt(rsaKey.PublicKey);// 公钥加密
string s = encrypt.RSADecrypt(rsaKey.PrivateKey);// 私钥解密
```
21.Redis分布式锁
```csharp
using (RedisLock redisLock = new RedisLock("127.0.0.1:6379"))
{
    if (redisLock.TryLock("lock", TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10), out var lockObject))// 加锁
    {
        //todo:需要执行的原子操作
    }

    var redisResult = redisLock.UnLock(lockObject);// 释放锁
}
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
