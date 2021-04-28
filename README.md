# Masuit.Tools
[![LICENSE](https://img.shields.io/badge/license-Anti%20996-blue.svg)](https://github.com/996icu/996.ICU/blob/master/LICENSE) [![nuget](https://img.shields.io/nuget/v/Masuit.Tools.Core.svg)](https://www.nuget.org/packages/Masuit.Tools.Core) [![nuget](https://img.shields.io/nuget/dt/Masuit.Tools.Core.svg)](https://www.nuget.org/packages/Masuit.Tools.Core) <a href="https://gitee.com/masuit/Masuit.Tools"><img src="https://gitee.com/static/images/logo-black.svg" height="24"></a> <a href="https://github.com/ldqk/Masuit.Tools"><img src="https://p.pstatp.com/origin/13841000102b8e2ba20b2" height="24"></a>  
包含一些常用的操作类，大都是静态类，加密解密，反射操作，动态编译，权重随机筛选算法，简繁转换，分布式短id，表达式树，linq扩展，文件压缩，多线程下载和FTP客户端，硬件信息，字符串扩展方法，日期时间扩展操作，中国农历，大文件拷贝，图像裁剪，验证码，断点续传，实体映射、集合扩展等常用封装。  
[官网教程](https://masuit.com/55)  

⭐⭐⭐喜欢这个项目的话就Star、Fork、Follow素质三连关♂注一下吧⭐⭐⭐  
项目开发模式：日常代码积累+网络搜集  
## 本项目已得到[JetBrains](https://www.jetbrains.com/shop/eform/opensource)的支持！  
<img src="https://www.jetbrains.com/shop/static/images/jetbrains-logo-inv.svg" height="100">     

## Star趋势
<img src="https://starchart.cc/ldqk/Masuit.Tools.svg">    

## 请注意：
一旦使用本开源项目以及引用了本项目或包含本项目代码的公司因为违反劳动法（包括但不限定非法裁员、超时用工、雇佣童工等）在任何法律诉讼中败诉的，项目作者有权利追讨本项目的使用费，或者直接不允许使用任何包含本项目的源代码！任何性质的`外包公司`或`996公司`需要使用本类库，请联系作者进行商业授权！其他企业或个人可随意使用不受限。

## 建议开发环境
操作系统：Windows 10 1903及以上版本  
开发工具：VisualStudio2019 v16.5及以上版本  
SDK：.Net Core 3.1.0及以上版本

## 安装程序包
.NET Framework 4.5  
`.NET Framework 4.5专用版本，相比4.6.1及.NET Core的版本，阉割了HTML、文件压缩、ASP.NET扩展、硬件监测、Session扩展等功能。`
```shell
PM> Install-Package Masuit.Tools.Net45
```
.NET Framework ≥4.6.1
```shell
PM> Install-Package Masuit.Tools.Net
```
.NET Core 2.1以上或.NET5
```shell
PM> Install-Package Masuit.Tools.Core
```
## 为工具库注册配置
工具库需要用到外部配置节：  
1. EmailDomainWhiteList，邮箱校验需要用到的白名单域名，英文逗号分隔，每个元素支持正则表达式，若未配置，则不启用邮箱校验白名单
2. EmailDomainBlockList，邮箱校验需要用到的黑名单域名，英文逗号分隔，每个元素支持正则表达式，且黑名单优先级高于白名单，若未配置，则不启用邮箱校验黑白名单
3. BaiduAK，获取IP/地理位置相关百度云APIKey，若未配置，则无法调用GetIPLocation以及GetPhysicalAddress相关方法
```csharp
public Startup(IConfiguration configuration)
{
    configuration.AddToMasuitTools(); // 若未调用，则默认自动尝试加载appsettings.json
}
```
## 特色功能示例代码
### 1.检验字符串是否是Email、手机号、URL、IP地址、身份证号等
```csharp
bool isEmail="3444764617@qq.com".MatchEmail(); // 可在appsetting.json中添加EmailDomainWhiteList和EmailDomainBlockList配置邮箱域名黑白名单，逗号分隔，如"EmailDomainBlockList": "^\\w{1,5}@qq.com,^\\w{1,5}@163.com,^\\w{1,5}@gmail.com,^\\w{1,5}@outlook.com",
bool isInetAddress = "114.114.114.114".MatchInetAddress();
bool isUrl = "http://masuit.com".MatchUrl();
bool isPhoneNumber = "15205201520".MatchPhoneNumber();
bool isIdentifyCard = "312000199502230660".MatchIdentifyCard();// 校验中国大陆身份证号
bool isCNPatentNumber = "200410018477.9".MatchCNPatentNumber(); // 校验中国专利申请号或专利号，是否带校验位，校验位前是否带“.”，都可以校验，待校验的号码前不要带CN、ZL字样的前缀
```

### 2.硬件监测(仅支持Windows)
```csharp
float load = SystemInfo.CpuLoad;// 获取CPU占用率
long physicalMemory = SystemInfo.PhysicalMemory;// 获取物理内存总数
long memoryAvailable = SystemInfo.MemoryAvailable;// 获取物理内存可用率
double freePhysicalMemory = SystemInfo.GetFreePhysicalMemory();// 获取可用物理内存
Dictionary<string, string> diskFree = SystemInfo.DiskFree();// 获取磁盘每个分区可用空间
Dictionary<string, string> diskTotalSpace = SystemInfo.DiskTotalSpace();// 获取磁盘每个分区总大小
Dictionary<string, double> diskUsage = SystemInfo.DiskUsage();// 获取磁盘每个分区使用率
double temperature = SystemInfo.GetCPUTemperature();// 获取CPU温度
int cpuCount = SystemInfo.GetCpuCount();// 获取CPU核心数
IList<string> ipAddress = SystemInfo.GetIPAddress();// 获取本机所有IP地址
string localUsedIp = SystemInfo.GetLocalUsedIP();// 获取本机当前正在使用的IP地址
IList<string> macAddress = SystemInfo.GetMacAddress();// 获取本机所有网卡mac地址
string osVersion = SystemInfo.GetOsVersion();// 获取操作系统版本
RamInfo ramInfo = SystemInfo.GetRamInfo();// 获取内存信息
var cpuSN=SystemInfo.GetCpuInfo()[0].SerialNumber; // CPU序列号
var driveSN=SystemInfo.GetDiskInfo()[0].SerialNumber; // 硬盘序列号
```
### 3.大文件操作
```csharp
FileStream fs = new FileStream(@"D:\boot.vmdk", FileMode.OpenOrCreate, FileAccess.ReadWrite);
{
        //fs.CopyToFile(@"D:\1.bak");//同步复制大文件
        fs.CopyToFileAsync(@"D:\1.bak");//异步复制大文件
        string md5 = fs.GetFileMD5Async().Result;//异步获取文件的MD5
        string sha1 = fs.GetFileSha1();//异步获取文件的SHA1
}
memoryStream.SaveFile("filename"); // 将内存流转储成文件
```
### 4.html的防XSS处理：
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
### 5.整理操作系统的内存：
```csharp
Windows.ClearMemorySilent();
```
### 6.任意进制转换
可用于生成短id，短hash等操作，纯数学运算。
```csharp
NumberFormater nf = new NumberFormater(36);//内置2-62进制的转换
//NumberFormater nf = new NumberFormater("0123456789abcdefghijklmnopqrstuvwxyz");// 自定义进制字符，可用于生成验证码
string s36 = nf.ToString(12345678);
long num = nf.FromString("7clzi");
Console.WriteLine("12345678的36进制是：" + s36); // 7clzi
Console.WriteLine("36进制的7clzi是：" + num); // 12345678
```
```csharp
//扩展方法形式调用
var bin=12345678.ToBinary(36);//7clzi
var num="7clzi".FromBinary(36);//12345678
```
```csharp
//超大数字的进制转换
var num = "E6186159D38CD50E0463A55E596336BD".FromBinaryBig(16);
Console.WriteLine(num); // 十进制：305849028665645097422198928560410015421
Console.WriteLine(num.ToBinary(64)); // 64进制：3C665pQUPl3whzFlVpoPqZ，22位长度
Console.WriteLine(num.ToBinary(36)); // 36进制：dmed4dkd5bhcg4qdktklun0zh，25位长度
```
### 7.纳秒级性能计时器
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
### 8.单机产生唯一有序的短id
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
### 9.产生分布式唯一有序短id
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
### 10.农历转换
```csharp
ChineseCalendar.CustomHolidays.Add(DateTime.Parse("2018-12-31"),"元旦节");//自定义节假日
ChineseCalendar today = new ChineseCalendar(DateTime.Parse("2018-12-31"));
Console.WriteLine(today.ChineseDateString);// 二零一八年十一月廿五
Console.WriteLine(today.AnimalString);// 生肖：狗
Console.WriteLine(today.GanZhiDateString);// 干支：戊戌年甲子月丁酉日
Console.WriteLine(today.DateHoliday);// 获取按公历计算的节假日
...
```
### 11.Linq表达式树扩展
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
### 12.模版引擎
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
### 13.List转Datatable
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
var table = list.Select(c => new{姓名=c.Name,年龄=c.Age}).ToDataTable();// 将自动填充列姓名和年龄
```
### 14.文件压缩解压
.NET Framework
```csharp
MemoryStream ms = SevenZipCompressor.ZipStream(new List<string>()
{
    @"D:\1.txt",
    "http://ww3.sinaimg.cn/large/87c01ec7gy1fsq6rywto2j20je0d3td0.jpg",
});//压缩成内存流
```
```csharp
SevenZipCompressor.Zip(new List<string>()
{
    @"D:\1.txt",
    "http://ww3.sinaimg.cn/large/87c01ec7gy1fsq6rywto2j20je0d3td0.jpg",
}, zip);//压缩成zip
SevenZipCompressor.UnRar(@"D:\Download\test.rar", @"D:\Download\");//解压rar
SevenZipCompressor.Decompress(@"D:\Download\test.tar", @"D:\Download\");//自动识别解压压缩包
SevenZipCompressor.Decompress(@"D:\Download\test.7z", @"D:\Download\");
```
ASP.NET Core

Startup.cs
```csharp
services.AddSevenZipCompressor();
```
构造函数注入ISevenZipCompressor
```csharp
private readonly ISevenZipCompressor _sevenZipCompressor;
public Test(ISevenZipCompressor sevenZipCompressor)
{
    _sevenZipCompressor = sevenZipCompressor;
}
```
使用方式同.NET Framework版本
### 15.日志组件
```csharp
LogManager.LogDirectory=AppDomain.CurrentDomain.BaseDirectory+"/logs";
LogManager.Event+=info =>
{
    //todo:注册一些事件操作
};
LogManager.Info("记录一次消息");
LogManager.Error(new Exception("异常消息"));
```
### 16.FTP客户端
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
### 17.多线程后台下载
```csharp
var mtd = new MultiThreadDownloader("https://attachments-cdn.shimo.im/yXwC4kphjVQu06rH/KeyShot_Pro_7.3.37.7z",Environment.GetEnvironmentVariable("temp"),"E:\\Downloads\\KeyShot_Pro_7.3.37.7z",8);
mtd.Configure(req =>
 {
     req.Referer = "https://masuit.com";
     req.Headers.Add("Origin", "https://baidu.com");
});
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
mtd.FileMergedComplete+=(sender,e)=>{
    Console.WriteLine("文件合并完成");
};
mtd.Start();//开始下载
//mtd.Pause(); // 暂停下载
//mtd.Resume(); // 继续下载
```
### 18.Socket客户端操作类
```csharp
var tcpClient = new TcpClient(AddressFamily.InterNetwork);
Socket socket = tcpClient.ConnectSocket(IPAddress.Any,5000);
socket.SendFile("D:\\test\\1.txt",false,i =>
{
    Console.WriteLine("已发送"+i+"%");
});
```
### 19.加密解密
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

string enc = "123456".DesEncrypt();// DES加密为密文
string s = enc.DesDecrypt(); //DES解密为明文
string enc = "123456".DesEncrypt("abcdefgh");// DES密钥加密为密文
string s = enc.DesDecrypt("abcdefgh"); //DES密钥解密为明文

RsaKey rsaKey = RsaCrypt.GenerateRsaKeys();// 生成RSA密钥对
string encrypt = "123456".RSAEncrypt(rsaKey.PublicKey);// 公钥加密
string s = encrypt.RSADecrypt(rsaKey.PrivateKey);// 私钥解密

string s = "123".Crc32();// 生成crc32摘要
string s = "123".Crc64();// 生成crc64摘要
```
### 20.实体校验
```csharp
public class MyClass
{
    [IsEmail] //可在appsetting.json中添加EmailDomainWhiteList配置邮箱域名白名单，逗号分隔
    public string Email { get; set; }

    [IsPhone]
    public string PhoneNumber { get; set; }

    [IsIPAddress]
    public string IP { get; set; }

    [MinValue(0, ErrorMessage = "年龄最小为0岁"), MaxValue(100, ErrorMessage = "年龄最大100岁")]
    public int Age { get; set; }

    [ComplexPassword]//密码复杂度校验
    public string Password { get; set; }
}
```
### 21.HTML操作
```csharp
List<string> srcs = "html".MatchImgSrcs().ToList();// 获取html字符串里所有的img标签的src属性
var imgTags = "html".MatchImgTags();//获取html字符串里的所有的img标签
var str="html".RemoveHtmlTag(); // 去除html标签
...
```
### 22.DateTime扩展
```csharp
double milliseconds = DateTime.Now.GetTotalMilliseconds();// 获取毫秒级时间戳
double microseconds = DateTime.Now.GetTotalMicroseconds();// 获取微秒级时间戳
double nanoseconds = DateTime.Now.GetTotalNanoseconds();// 获取纳秒级时间戳
double seconds = DateTime.Now.GetTotalSeconds();// 获取秒级时间戳
double minutes = DateTime.Now.GetTotalMinutes();// 获取分钟级时间戳
...
```
### 23.IP地址和URL
```csharp
bool inRange = "192.168.2.2".IpAddressInRange("192.168.1.1","192.168.3.255");// 判断IP地址是否在这个地址段里
bool isPrivateIp = "172.16.23.25".IsPrivateIP();// 判断是否是私有地址
bool isExternalAddress = "http://baidu.com".IsExternalAddress();// 判断是否是外网的URL

//以下需要配置baiduAK
string isp = "114.114.114.114".GetISP(); // 获取ISP运营商信息
PhysicsAddress physicsAddress = "114.114.114.114".GetPhysicsAddressInfo().Result;// 获取详细地理信息对象
Tuple<string, List<string>> ipAddressInfo = "114.114.114.114".GetIPAddressInfo().Result;// 获取详细地理信息集合
```
### 24.元素去重
```csharp
var list = new List<MyClass>()
{
    new MyClass()
    {
        Email = "1@1.cn"
    },
    new MyClass()
    {
        Email = "1@1.cn"
    },
    new MyClass()
    {
        Email = "1@1.cn"
    }
};
List<MyClass> classes = list.DistinctBy(c => c.Email).ToList();
Console.WriteLine(classes.Count==1);//True
```

### 25.枚举扩展
```csharp
public enum MyEnum
{
    [Display(Name = "读")]
    [Description("读")]
    Read,
    
    [Display(Name = "写")]
    [Description("写")]
    Write
}
```
```csharp
Dictionary<int, string> dic1 = typeof(MyEnum).GetDictionary();// 获取枚举值和字符串表示的字典映射
var dic2 = typeof(MyEnum).GetDescriptionAndValue();// 获取字符串表示和枚举值的字典映射
string desc = MyEnum.Read.GetDescription();// 获取Description标签
string display = MyEnum.Read.GetDisplay();// 获取Display标签的Name属性
var value = typeof(MyEnum).GetValue("Read");//获取字符串表示值对应的枚举值
string enumString = 0.ToEnumString(typeof(MyEnum));// 获取枚举值对应的字符串表示
```
### 26.定长队列实现
```csharp
LimitedQueue<string> queue = new LimitedQueue<string>(32);// 声明一个容量为32个元素的定长队列
ConcurrentLimitedQueue<string> queue = new ConcurrentLimitedQueue<string>(32);// 声明一个容量为32个元素的线程安全的定长队列
```
### 27.反射操作
```csharp
MyClass myClass = new MyClass();
PropertyInfo[] properties = myClass.GetProperties();// 获取属性列表
myClass.SetProperty("Email","1@1.cn");//给对象设置值
myClass.DeepClone(); // 对象深拷贝，带嵌套层级的
```
### 28.获取线程内唯一对象
```csharp
CallContext<T>.SetData("db",dbContext);//设置线程内唯一对象
CallContext<T>.GetData("db");//获取线程内唯一对象
```
### 29.asp.net core 获取静态的HttpContext对象
Startup.cs
```csharp
public void ConfigureServices(IServiceCollection services)
{
    // ...
    services.AddStaticHttpContext();
    // ...
}

public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    // ...
    app.UseStaticHttpContext();
    // ...
}
```

```csharp
public async Task<IActionResult> Index()
{
    HttpContext context = HttpContext2.Current;
}
```
### 30.邮件发送
```csharp
new Email()
{
    SmtpServer = "smtp.masuit.com",// SMTP服务器
    SmtpPort = 25, // SMTP服务器端口
    EnableSsl = true,//使用SSL
    Username = "admin@masuit.com",// 邮箱用户名
    Password = "123456",// 邮箱密码
    Tos = "10000@qq.com,10001@qq.com", //收件人
    Subject = "测试邮件",//邮件标题
    Body = "你好啊",//邮件内容
}.SendAsync(s =>
{
    Console.WriteLine(s);// 发送成功后的回调
});// 异步发送邮件
```
### 31.图像的简单处理
```csharp
ImageUtilities.CompressImage(@"F:\src\1.jpg", @"F:\dest\2.jpg");//无损压缩图片

"base64".SaveDataUriAsImageFile();// 将Base64编码转换成图片

Image image = Image.FromFile(@"D:\1.jpg");
image.MakeThumbnail(@"D:\2.jpg", 120, 80, ThumbnailCutMode.LockWidth);//生成缩略图

Bitmap bmp = new Bitmap(@"D:\1.jpg");
Bitmap newBmp = bmp.BWPic(bmp.Width, bmp.Height);//转换成黑白
Bitmap newBmp = bmp.CutAndResize(new Rectangle(0, 0, 1600, 900), 160, 90);//裁剪并缩放
bmp.RevPicLR(bmp.Width, bmp.Height);//左右镜像
bmp.RevPicUD(bmp.Width, bmp.Height);//上下镜像

var marker=ImageWatermarker(stream);
stream=maker.AddWatermark("水印文字",color,水印位置,边距,字体大小,字体,抗锯齿); // 给图片添加水印
```
### 32.随机数
```csharp
Random rnd = new Random();
int num = rnd.StrictNext();//产生真随机数
double gauss = rnd.NextGauss(20,5);//产生正态高斯分布的随机数
```
### 33.权重筛选功能
```csharp
var data=new List<WeightedItem<string>>()
{
     new WeightedItem<string>("A", 1),
     new WeightedItem<string>("B", 3),
     new WeightedItem<string>("C", 4),
     new WeightedItem<string>("D", 4),
};
var item=data.WeightedItem();//按权重选出1个元素
var list=data.WeightedItems(2);//按权重选出2个元素
```
```csharp
var selector = new WeightedSelector<string>(new List<WeightedItem<string>>()
{
    new WeightedItem<string>("A", 1),
    new WeightedItem<string>("B", 3),
    new WeightedItem<string>("C", 4),
    new WeightedItem<string>("D", 4),
});
var item = selector.Select();//按权重选出1个元素
var list = selector.SelectMultiple(3);//按权重选出3个元素
```
### 34.EF Core支持AddOrUpdate方法
```csharp
/// <summary>
/// 按Id添加或更新文章实体
/// </summary>
public override Post SavePost(Post t)
{
    DataContext.Set<Post>().AddOrUpdate(t => t.Id, t);
    return t;
}
```
### 35.敏感信息掩码
```csharp
"13123456789".Mask(); // 131****5678
"admin@masuit.com".MaskEmail(); // a****n@masuit.com
```
### 36.集合扩展
```csharp
var list = new List<string>()
{
    "1","3","3","3"
};
list.AddRangeIf(s => s.Length > 1, "1", "11"); // 将被添加元素中的长度大于1的元素添加到list
list.AddRangeIfNotContains("1", "11"); // 将被添加元素中不包含的元素添加到list
list.RemoveWhere(s => s.Length<1); // 将集合中长度小于1的元素移除
list.InsertAfter(0, "2"); // 在第一个元素之后插入
list.InsertAfter(s => s == "1", "2"); // 在元素"1"后插入
var dic = list.ToDictionarySafety(s => s); // 安全的转换成字典类型，当键重复时只添加一个键
var dic = list.ToConcurrentDictionary(s => s); // 转换成并发字典类型，当键重复时只添加一个键
var dic = list.ToDictionarySafety(s => s, s => s.GetHashCode()); // 安全的转换成字典类型，当键重复时只添加一个键
dic.AddOrUpdate("4", 4); // 添加或更新键值对
dic.AddOrUpdate(new Dictionary<string, int>()
{
    ["5"] = 5,["55"]=555
}); // 批量添加或更新键值对
dic.AddOrUpdate("5", 6, (s, i) => 66); // 如果是添加，则值为6，若更新则值为66
dic.AddOrUpdate("5", 6, 666); // 如果是添加，则值为6，若更新则值为666
dic.GetOrAdd("7",77); // 字典获取或添加元素
dic.GetOrAdd("7",()=>77); // 字典获取或添加元素
dic.AsConcurrentDictionary(); // 普通字典转换成并发字典集合
var table=list.ToDataTable(); // 转换成DataTable类型
table.AddIdentityColumn(); //给DataTable增加一个自增列
table.HasRows(); // 检查DataTable 是否有数据行
table.ToList<T>(); // datatable转List
var set = list.ToHashSet(s=>s.Name);// 转HashSet
var cts = new CancellationTokenSource(100); //取消口令
await list.ForeachAsync(async i=>{
    await Task.Delay(100);
    Console.WriteLine(i);
},cts.Token); // 异步foreach

await list.ForAsync(async (item,index)=>{
    await Task.Delay(100);
    Console.WriteLine(item+"_"+index);
},cts.Token); // 异步for，带索引编号
await list.SelectAsync(async i=>{
    await Task.Delay(100);
    return i*10;
}); // 异步Select
await list.SelectAsync(async (item,index)=>{
    await Task.Delay(100);
    return item*10;
}); // 异步Select，带索引编号
string s=list.Join(",");//将字符串集合连接成逗号分隔的单字符串
var max=list.MaxOrDefault(); // 取最大值，当集合为空的时候不会报错
var max=list.MaxOrDefault(selector); // 取最大值，当集合为空的时候不会报错
var max=list.MaxOrDefault(selector,default); // 取最大值，当集合为空的时候不会报错
var max=list.MinOrDefault(); // 取最小值，当集合为空的时候不会报错
var max=list.MinOrDefault(selector); // 取最小值，当集合为空的时候不会报错
var max=list.MinOrDefault(selector,default); // 取最小值，当集合为空的时候不会报错
var stdDev=list.Select(s=>s.ConvertTo<int>()).StandardDeviation(); // 求标准差

var pages=queryable.ToPagedList(1,10); // 分页查询
var pages=await queryable.ToPagedListAsync(1,10); // 分页查询
```
### 37.Mime类型
```csharp
var mimeMapper = new MimeMapper();
var mime = mimeMapper.GetExtensionFromMime("image/jpeg"); // .jpg
var ext = mimeMapper.GetMimeFromExtension(".jpg"); // image/jpeg
```
### 38.日期时间扩展
```csharp
DateTime.Now.GetTotalSeconds(); // 获取该时间相对于1970-01-01 00:00:00的秒数
DateTime.Now.GetTotalMilliseconds(); // 获取该时间相对于1970-01-01 00:00:00的毫秒数
DateTime.Now.GetTotalMicroseconds(); // 获取该时间相对于1970-01-01 00:00:00的微秒数
DateTime.Now.GetTotalNanoseconds(); // 获取该时间相对于1970-01-01 00:00:00的纳秒数
var indate=DateTime.Parse("2020-8-3").In(DateTime.Parse("2020-8-2"),DateTime.Parse("2020-8-4"));//true
DateTime time="2021-1-1 8:00:00".ToDateTime(); //字符串转DateTime

//时间段计算工具
var range = new DateTimeRange(DateTime.Parse("2020-8-3"), DateTime.Parse("2020-8-5"));
range.Union(DateTime.Parse("2020-8-4"), DateTime.Parse("2020-8-6")); //连接两个时间段，结果：2020-8-3~2020-8-6
range.In(DateTime.Parse("2020-8-3"), DateTime.Parse("2020-8-6"));//判断是否在某个时间段内，true
var (intersected,range2) = range.Intersect(DateTime.Parse("2020-8-4"), DateTime.Parse("2020-8-6"));//两个时间段是否相交，(true,2020-8-3~2020-8-4)
range.Contains(DateTime.Parse("2020-8-3"), DateTime.Parse("2020-8-4"));//判断是否包含某个时间段，true
...
```
### 39.流转换
```csharp
stream.SaveAsMemoryStream(); // 任意流转换成内存流
stream.ToArray(); // 任意流转换成二进制数组
```
### 40.数值转换
```csharp
1.2345678901.Digits8(); // 将小数截断为8位
1.23.ConvertTo<int>(); // 小数转int
1.23.ConvertTo<T>(); // 小数转T基本类型
bool b=1.23.TryConvertTo<T>(out result); // 小数转T基本类型
var num=1.2345.ToDecimal(2); //转decimal并保留两位小数
```
### 41.简繁转换
```csharp
var str="个体".ToTraditional(); // 转繁体
var str="個體".ToSimplified(); // 转简体
```
### 42.INI配置文件操作
```csharp
INIFile ini=new INIFile("filename.ini");
ini.IniWriteValue(section,key,value); // 写值
ini.IniReadValue(section,key); // 读值
ini.ClearAllSection(); // 清空所有配置节
ini.ClearSection(section); // 清空配置节
```
### 43.雷达图计算引擎
应用场景：计算两个多边形的相似度，用户画像之类的
```csharp
var points=RadarChartEngine.ComputeIntersection(chart1,chart2); //获取两个多边形的相交区域
points.ComputeArea(); //计算多边形面积
```
### 44.树形结构实现
基本接口类：  
ITreeChildren：带Children属性的接口  
ITreeParent：带Parent属性的接口  
ITree：继承ITreeParent和ITreeChildren，同时多了Name属性  

相关扩展方法：
```csharp
trees.Filter(func); // 从树形集合中过滤
trees.Flatten(); // 将数据平铺开
tree.AllChildren(); // 获取所有的子级
tree.AllParent(); // 获取所有的父级
tree.IsRoot(); // 是否是根节点
tree.IsLeaf(); // 是否是叶子节点
tree.Level(); // 所处深度/层级
tree.Path(); // 全路径

var tree=list.ToTree(c => c.Id, c => c.Pid);//继承自ITreeParent<T>, ITreeChildren<T>的集合转换成树形结构
var tree=list.ToTreeGeneral(c => c.Id, c => c.Pid);//一般的集合转换成树形结构
```
### 45.Excel导出
```csharp
var stream=list.Select(item=>new{
    姓名=item.Name,
    年龄=item.Age,
    item.Gender
}).ToDataTable().ToExcel("Sheet1"); //自定义列名导出
var stream=list.ToDataTable().ToExcel("Sheet1");//默认字段名作为列名导出
```

# Asp.Net MVC和Asp.Net Core的支持断点续传和多线程下载的ResumeFileResult

在ASP.NET Core中通过MVC/WebAPI应用程序传输文件数据时使用断点续传以及多线程下载支持。

它提供了`ETag`标头以及`Last-Modified`标头。 它还支持以下前置条件标头：`If-Match`，`If-None-Match`，`If-Modified-Since`，`If-Unmodified-Since`，`If-Range`。
## 支持 ASP.NET Core 2.0+
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
<details>
    <summary>点击查看代码</summary>

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
另外，它可以提供`ETag`和`LastModified`标头。

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
</details>

### 推荐项目
基于EntityFrameworkCore和Lucene.NET实现的全文检索搜索引擎：[Masuit.LuceneEFCore.SearchEngine](https://github.com/ldqk/Masuit.LuceneEFCore.SearchEngine "Masuit.LuceneEFCore.SearchEngine")

开源博客系统：[Masuit.MyBlogs](https://github.com/ldqk/Masuit.MyBlogs "Masuit.MyBlogs")
### 友情赞助
![打赏支持](https://ae01.alicdn.com/kf/H9c0ef439b7ae4a5ba4151456f3c5f0a2N.jpg)
