# Masuit.Tools(码数吐司库)

[![许可证](https://img.shields.io/badge/license-Anti%20996-blue.svg)](https://github.com/996icu/996.ICU/blob/master/LICENSE)
[![nuget](https://img.shields.io/nuget/v/Masuit.Tools.Core.svg)](https://www.nuget.org/packages/Masuit.Tools.Core)
[![nuget](https://img.shields.io/nuget/dt/Masuit.Tools.Core.svg)](https://www.nuget.org/packages/Masuit.Tools.Core)
![codeSize](https://img.shields.io/github/languages/code-size/ldqk/Masuit.Tools.svg)
![编程语言](https://img.shields.io/github/languages/top/ldqk/Masuit.Tools.svg)
<a href="https://gitee.com/masuit/Masuit.Tools"><img src="https://gitee.com/static/images/logo-black.svg" height="24"></a>
<a href="https://github.com/ldqk/Masuit.Tools"><img src="https://upload.wikimedia.org/wikipedia/commons/thumb/9/95/Font_Awesome_5_brands_github.svg/54px-Font_Awesome_5_brands_github.svg.png" height="24"><img src="https://upload.wikimedia.org/wikipedia/commons/thumb/2/29/GitHub_logo_2013.svg/128px-GitHub_logo_2013.svg.png" height="24"></a>

新手友好的C#万能工具库，包含一些常用的操作类，大都是静态类，加密解密，反射操作，权重随机筛选算法，分布式短id，表达式树，linq扩展，文件压缩，多线程下载和FTP客户端，硬件信息，字符串扩展方法，日期时间扩展操作，中国农历，大文件拷贝，图像裁剪，验证码，断点续传，集合扩展、Excel导出等常用封装。

**诸多功能集一身，代码量不到2MB！**
[官网教程](https://masuit.com/55)
![Masuit Tools](https://user-images.githubusercontent.com/20254980/208012013-28813c43-daa2-4b64-9e4a-78829325a7a4.png)

项目开发模式：日常代码积累+网络搜集
⭐⭐⭐喜欢这个项目的话就Star、Fork、Follow素质三连关♂注一下吧⭐⭐⭐
关于本项目，如果你有任何不懂的地方或使用过程中遇到任何问题，可以直接提issue或私信联系我，我会为你提供**完全免费**的技术指导，当然，如果你觉得不好意思接受免费的指导，想适当打赏我也是不会拒绝的！🤣🤣🤣

## 本项目已得到[JetBrains](https://www.jetbrains.com/shop/eform/opensource)的支持！

<img src="https://www.jetbrains.com/shop/static/images/jetbrains-logo-inv.svg" height="100">

## Star趋势

<img src="https://starchart.cc/ldqk/Masuit.Tools.svg">

## 请注意：

一旦使用本开源项目以及引用了本项目或包含本项目代码的公司因为违反劳动法（包括但不限定非法裁员、超时用工、雇佣童工等）在任何法律诉讼中败诉的，一经发现，本项目作者有权利追讨本项目的使用费（**公司工商注册信息认缴金额的2-5倍作为本项目的授权费**），或者直接不允许使用任何包含本项目的源代码！任何性质的 `外包公司`或 `996公司`需要使用本类库，请联系作者进行商业授权！其他企业或个人可随意使用不受限。996那叫用人，也是废人。8小时工作制才可以让你有时间自我提升，将来有竞争力。反对996，人人有责！

## 建议开发环境

操作系统：Windows 10 1903及以上版本
开发工具：VisualStudio2019 v16.5及以上版本
SDK：.Net Core 2.1.0及以上所有版本

## 安装程序包

### 基础功能包

`.NET Framework版本的包因打包环境异常，无法正常发布更新，目前暂时停更~`

#### .NET Framework ≥ 4.6.1

```shell
PM> Install-Package Masuit.Tools.Net
```

#### .NET Standard ≥ 2.1 或只想使用一些基本功能

`通用项目推荐首选包`

```shell
PM> Install-Package Masuit.Tools.Abstraction
```

#### .NET Core ≥ 2.1

`.NET Core项目推荐首选包`

```shell
PM> Install-Package Masuit.Tools.Core
```

#### .NET Framework 4.5特供版

请注意：`这是.NET Framework 4.5的专用版本，相比4.6.1及.NET Core的版本，阉割了Redis、HTML、文件压缩、ASP.NET扩展、硬件监测、Session扩展等一些功能。`**如果你的项目版本高于4.5，请务必使用上述版本的包，以享受完整的功能体验！**

```shell
PM> Install-Package Masuit.Tools.Net45
```

### 增值包

#### Masuit.Tools.AspNetCore

`AspNetCore项目推荐首选包`
ASP.NET Core Web专用包，包含Masuit.Tools.Core的全部功能，并且增加了一些对ASP.NET Core Web功能的额外支持。

#### Masuit.Tools.Excel

Excel导入导出的专用独立包

#### Masuit.Tools.NoSQL.MongoDBClient

mongodb的封装操作类独立包

## 为工具库注册配置

工具库需要用到外部配置节，.NET Framework项目配置在web.config/app.config的AppSettings配置节中，.NET Core项目配置在appsettings.json中：

1. EmailDomainWhiteList，邮箱校验需要用到的白名单域名，英文逗号分隔，每个元素支持正则表达式，若未配置，则不启用邮箱校验白名单，示例: `"^\\w{1,5}@qq.com,^\\w{1,5}@163.com,^\\w{1,5}@gmail.com,^\\w{1,5}@outlook.com"`
2. EmailDomainBlockList，邮箱校验需要用到的黑名单域名，英文逗号分隔，每个元素支持正则表达式，且黑名单优先级高于白名单，若未配置，则不启用邮箱校验黑白名单

```csharp
public Startup(IConfiguration configuration)
{
    configuration.AddToMasuitTools(); // 若未调用，则默认自动尝试加载appsettings.json
}
```

## 特色功能示例代码

### 在线体验

https://replit.com/@ldqk/MasuitToolsDemo?v=1#main.cs

### 0. 一些创意类型

`DisposableDictionary`：可被Disposable的字典类型，用于存放Value是Disposable类型的数据，用法和普通字典一致

`NullableConcurrentDictionary`/`NullableDictionary`：Key可为null的字典类型，用法和普通字典一致

`ConcurrentHashSet`：并发HashSet，用法和HashSet一致

`ConcurrentLimitedQueue`：定长并发队列，特点是长度是固定的，用法与ConcurrentQueue一致

`LimitedQueue`：定长队列，特点是长度是固定的，用法与Queue一致

`LargeMemoryStream`：超大内存流，最大可支持1TB数据，推荐当数据流大于2GB时使用，用法与MemoryStream一致

`PooledMemoryStream`：池化内存流，可内存复用，用法与MemoryStream一致，性能比MemoryStream好

`ITree<T>`：树形实体接口约束，实现该接口可让类型实现一些树形操作

`ChineseCalendar`：中国农历类型，可以实现天干地支节气等数据的获取

`Clay`/`DynamicFactory`：粘土动态类型，可实现类似js的弱类型编程

`RadarChart`：雷达图类型，可用于做数据分析或用户行为画像

`Circle`：圆形类型，可实现⚪的相交相切相离的判断

`Sphere`：球体类型，可实现计算球体上两点的弧长计算，相交相切相离的判断

`MimeMapper`：mime类型映射

具体用法，可参阅后文详细示例：

### 1. 检验字符串是否是Email、手机号、URL、IP地址、身份证号等

```csharp
var (isMatch, match) = "337845818@qq.com".MatchEmail(); // 可在appsetting.json中添加EmailDomainWhiteList和EmailDomainBlockList配置邮箱域名黑白名单，逗号分隔，如"EmailDomainBlockList": "^\\w{1,5}@qq.com,^\\w{1,5}@163.com,^\\w{1,5}@gmail.com,^\\w{1,5}@outlook.com",
bool isInetAddress = "114.114.114.114".MatchInetAddress(); // 匹配IP地址
bool isUrl = "http://masuit.org/20/history".MatchUrl(); // 匹配url
bool isPhoneNumber = "15205201520".MatchPhoneNumber(); // 匹配手机号
bool isLandline = "01088888888".MatchLandline(); // 匹配座机号
bool isIdentifyCard = "312000199502230660".MatchIdentifyCard();// 校验中国大陆身份证号
bool isCNPatentNumber = "200410018477.9".MatchCNPatentNumber(); // 校验中国专利申请号或专利号，是否带校验位，校验位前是否带“.”，都可以校验，待校验的号码前不要带CN、ZL字样的前缀
```

### 2.硬件监测(仅支持Windows，部分函数仅支持物理机模式)

```csharp
float load = SystemInfo.CpuLoad;// 获取CPU占用率
long physicalMemory = SystemInfo.PhysicalMemory;// 获取物理内存总数
long memoryAvailable = SystemInfo.MemoryAvailable;// 获取物理内存可用率
double freePhysicalMemory = SystemInfo.GetFreePhysicalMemory();// 获取可用物理内存
double temperature = SystemInfo.GetCPUTemperature();// 获取CPU温度
int cpuCount = SystemInfo.GetCpuCount();// 获取CPU核心数
var ipAddress = SystemInfo.GetLocalIPs();// 获取本机所有IP地址
string localUsedIp = SystemInfo.GetLocalUsedIP();// 获取本机当前正在使用的IP地址
IList<string> macAddress = SystemInfo.GetMacAddress();// 获取本机所有网卡mac地址
string osVersion = Windows.GetOsVersion();// 获取操作系统版本
RamInfo ramInfo = SystemInfo.GetRamInfo();// 获取内存信息
var cpuSN=SystemInfo.GetCpuInfo()[0].SerialNumber; // CPU序列号
var driveSN=SystemInfo.GetDiskInfo()[0].SerialNumber; // 硬盘序列号

// 快速方法
var cpuInfos = CpuInfo.Locals; // 快速获取CPU的信息
var ramInfo = RamInfo.Local; // 快速获取内存的信息
var diskInfos = DiskInfo.Locals; // 快速获取硬盘的信息
var biosInfo = BiosInfo.Local; // 快速获取主板的信息
```

### 3.html的防XSS处理：

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
string s = html.HtmlSanitizerStandard();// 清理后：<div><span><a href="/users/account/LogOff">退出</a></span></div>
string s = html.HtmlSanitizerCustom(); // 自定义清理
```

### 4.整理Windows系统的内存：

类似于各大系统优化软件的加速球功能

```csharp
Windows.ClearMemorySilent();
```

### 5.任意进制转换/中文数字

#### 大写数字

```csharp
var num=123.45.ToChineseMoney(); // 壹佰贰拾叁元肆角伍分
var num=123.45.ToChineseNumber(); // 一百二十三点四五
```

#### 进制转换

可用于生成短id，短hash，随机字符串等操作，纯数学运算。

```csharp
NumberFormater nf = new NumberFormater(36);//内置2-91进制的转换
//NumberFormater nf = new NumberFormater("0123456789abcdefghijklmnopqrstuvwxyz");// 自定义进制字符，可用于生成验证码，自定义字符可支持任意进制，你传1w个字符进去那就支持一万进制(手动狗头)
string s36 = nf.ToString(12345678);
long num = nf.FromString("7clzi");
Console.WriteLine("12345678的36进制是：" + s36); // 7clzi
Console.WriteLine("36进制的7clzi是：" + num); // 12345678
var s = new NumberFormater(91).ToString(new Random().Next(100000, int.MaxValue)); //配合随机数生成随机字符串
```

```csharp
//扩展方法形式调用
var bin=12345678.ToBase(36);// 10进制转36进制：7clzi
var num="7clzi".FromBase(36);// 36进制转10进制：12345678
```

```csharp
//超大数字的进制转换
var num = "e6186159d38cd50e0463a55e596336bd".FromBaseBig(16); // 大数字16进制转10进制
Console.WriteLine(num); // 十进制：305849028665645097422198928560410015421
Console.WriteLine(num.ToBase(64)); // 64进制：3C665pQUPl3whzFlVpoPqZ，22位长度
Console.WriteLine(num.ToBase(36)); // 36进制：dmed4dkd5bhcg4qdktklun0zh，25位长度
Console.WriteLine(num.ToBase(7)); // 7进制：2600240311641665565300424545154525131265221035，46位长度
Console.WriteLine(num.ToBase(12)); // 12进制：5217744842749978a756b22135b16a5998a5，36位长度
Console.WriteLine(num.ToBase(41)); // 41进制：opzeBda2aytcEeudEquuesbk，24位长度
```

如果你想让进制符支持emoji，NumberFormater是不支持的，不过如果你确实有这么骚的需求，我还准备了UnicodeFormater类，用于支持emoji，用法和NumberFormater一模一样，并且，UnicodeFormater的功能包含NumberFormater的功能，但是，性能比NumberFormater差了许多。

```csharp
var formater = new UnicodeFormater("😀😁😂🤣😃😄😅😆😉😊😋😎😍😘🥰😗😙🥲😚🙂🤗🤩🤔🤨😑😶😶‍🌫🙄😏😣😥😮");
var s = formater.ToString(1234567890); // 😄🌫😶😋😋
var num = formater.FromString(s); // 1234567890
```

### 6.纳秒级性能计时器

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

### 7.产生分布式唯一有序短id(雪花id)

```csharp
var sf = SnowFlake.GetInstance();
string token = sf.GetUniqueId();// rcofqodori0w
string token = SnowFlake.NewId;// rcofqodori0w
string shortId = sf.GetUniqueShortId(8);// qodw9728
string token = SnowFlakeNew.NewId;// 改良版雪花id，对时间回拨不敏感
```

```csharp
var set = new HashSet<string>();
double time = HiPerfTimer.Execute(() =>
{
    for (int i = 0; i < 1000000; i++)
    {
        set.Add(SnowFlake.NewId);
    }
});
Console.WriteLine(set.Count == 1000000); //True
Console.WriteLine("产生100w个id耗时" + time + "s"); //2.6891495s
```

### 8.农历转换

```csharp
ChineseCalendar.CustomHolidays.Add(DateTime.Parse("2018-12-31"),"元旦节");//自定义节假日
ChineseCalendar today = new ChineseCalendar(DateTime.Parse("2018-12-31"));
Console.WriteLine(today.ChineseDateString);// 二零一八年十一月廿五
Console.WriteLine(today.AnimalString);// 生肖：狗
Console.WriteLine(today.GanZhiDateString);// 干支：戊戌年甲子月丁酉日
Console.WriteLine(today.DateHoliday);// 获取按公历计算的节假日
...
```

### 9.Linq表达式树扩展

```csharp
Expression<Func<string, bool>> where1 = s => s.StartsWith("a");
Expression<Func<string, bool>> where2 = s => s.Length > 10;
Func<string, bool> func = where1.And(where2)
    .AndIf(!string.IsNullOrEmpty(name),s=>s==name)
    .Compile(); // And和AndIf可供选择，满足条件再执行And
bool b=func("abcd12345678");//true
```

```csharp
Expression<Func<string, bool>> where1 = s => s.StartsWith("a");
Expression<Func<string, bool>> where2 = s => s.Length > 10;
Func<string, bool> func = where1
    .Or(where2)
    .OrIf(!string.IsNullOrEmpty(name),s=>s==name)
    .Compile(); // Or和OrIf可供选择，满足条件再执行Or
bool b=func("abc");// true
```

```csharp
queryable.WhereIf(!string.IsNullOrEmpty(name),e=>e.Name==name)
    .WhereIf(()=> age.HasValue,e=>e.Age>=age); // IQueryable的WhereIf扩展函数，满足条件再执行Where
```

### 10.模版引擎

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

### 11.List转Datatable

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

### 12.文件压缩解压

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

### 13.简易日志组件(又不是不能用.jpg)

```csharp
LogManager.LogDirectory=AppDomain.CurrentDomain.BaseDirectory+"/logs";
LogManager.Event+=info =>
{
    //todo:注册一些事件操作
};
LogManager.Info("记录一次消息");
LogManager.Error(new Exception("异常消息"));
```

### 14.多线程后台下载

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

### 15.加密解密/hash

```csharp
var enc="123456".MDString();// MD5
var enc="123456".MDString("abc");// MD5加盐
var enc="123456".MDString2();// MD5两次
var enc="123456".MDString2("abc");// MD5两次加盐
var enc="123456".MDString3();// MD5三次
var enc="123456".MDString3("abc");// MD5三次加盐

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
string s = "123".SHA256();// 生成SHA256摘要

// 零宽字符串，通常用作文章水印，以一种看不见的字符插入到文本中，使攻击者无法直接识别文本内容，从而起到保护文章的作用。
string pub="hello,world!";
string hidden="ldqk";
var str = pub.InjectZeroWidthString(hidden); // 扩展函数调用：将"ldqk"以零宽字符串的方式隐藏在"hello,world!"中
var str = ZeroWidthCodec.Encrypt(pub,hidden); // 类调用：将"ldqk"以零宽字符串的方式隐藏在"hello,world!"中
var dec = str.DecodeZeroWidthString(); // 扩展函数调用：将包含零宽字符串的密文解密出隐藏字符串"ldqk"
var dec = ZeroWidthCodec.Decrypt(str); // 类调用：将包含零宽字符串的密文解密出隐藏字符串"ldqk"
var enc = hidden.EncodeToZeroWidthText(); // 扩展函数调用：将字符串编码成零宽字符串
var enc = ZeroWidthCodec.Encode(str); // 类调用：将字符串编码成零宽字符串
```

### 16.实体校验

```csharp
public class MyClass
{
    [IsEmail] //可在appsetting.json中添加EmailDomainWhiteList配置邮箱域名白名单，逗号分隔
    public string Email { get; set; }

    [IsPhone]
    public string PhoneNumber { get; set; }

    [IsLandline]
    public string Landline { get; set; }

    [IsIPAddress]
    public string IP { get; set; }

    [MinValue(0, ErrorMessage = "年龄最小为0岁"), MaxValue(100, ErrorMessage = "年龄最大100岁")]
    public int Age { get; set; }

    [ComplexPassword]//密码复杂度校验
    public string Password { get; set; }
  
    [EnumOf] // 检测是否是有效枚举值
    public MyEnum MyEnum { get; set; }
  
    [MinItemsCount(1)] // 检测集合元素最少1个
    public List<string> Strs { get; set; }
}
```

### 17.HTML操作

```csharp
List<string> srcs = "html".MatchImgSrcs().ToList();// 获取html字符串里所有的img标签的src属性
var imgTags = "html".MatchImgTags();//获取html字符串里的所有的img标签
var str="html".RemoveHtmlTag(); // 去除html标签
...
```

### 18.IP地址和URL

```csharp
bool inRange = "192.168.2.2".IpAddressInRange("192.168.1.1","192.168.3.255");// 判断IP地址是否在这个地址段里
bool isPrivateIp = "172.16.23.25".IsPrivateIP();// 判断是否是私有地址
bool isExternalAddress = "http://baidu.com".IsExternalAddress();// 判断是否是外网的URL

//以下需要配置baiduAK
string isp = "114.114.114.114".GetISP(); // 获取ISP运营商信息
PhysicsAddress physicsAddress = "114.114.114.114".GetPhysicsAddressInfo().Result;// 获取详细地理信息对象
Tuple<string, List<string>> ipAddressInfo = "114.114.114.114".GetIPAddressInfo().Result;// 获取详细地理信息集合

uint number=ipAddress.ToUInt32(); // IP地址转10进制
uint number="114.114.114.114".IPToID(); // IP地址转10进制
```

### 19.对象属性值合并

```csharp
public class MyClass
{
    public string A { get; set; }

    public bool? B { get; set; }

    public int? C { get; set; }
}

var a = new MyClass()
{
    A = "aa"
};
var b = new MyClass()
{
    B = true
};
var c = new MyClass()
{
    C = 3
};
var merge = a.Merge(b, c); // 合并后对象：A = "aa"，B = true，C = 3
```

### 20.元素去重

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

### 21.枚举扩展

```csharp
[Flags]
public enum MyEnum
{
    [Display(Name = "读")]
    [Description("读")]
    [EnumDescription("读取操作","读","zh-CN")] // 多语言枚举描述
    [EnumDescription("Read","Read","en-US")]
    Read=1,
  
    [Display(Name = "写")]
    [Description("写")]
    Write=2,

    Delete=4,

    All=8
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

```csharp
var op=MyEnum.Read|MyEnum.Write|MyEnum.Delete;
var enums=op.Split(); // 拆分枚举值，得到枚举数组，这个函数建议使用在按位定值的枚举
```

### 22.定长队列和ConcurrentHashSet实现

`如果是.NET5及以上，推荐使用框架自带的Channel实现该功能`

```csharp
LimitedQueue<string> queue = new LimitedQueue<string>(32);// 声明一个容量为32个元素的定长队列
ConcurrentLimitedQueue<string> queue = new ConcurrentLimitedQueue<string>(32);// 声明一个容量为32个元素的线程安全的定长队列
```

```csharp
var set = new ConcurrentHashSet<string>(); // 用法和hashset保持一致
```

### 23.反射操作

```csharp
MyClass myClass = new MyClass();
PropertyInfo[] properties = myClass.GetProperties();// 获取属性列表
myClass.SetProperty("Email","1@1.cn");//给对象设置值
myClass.DeepClone(); // 对象深拷贝，带嵌套层级的
myClass.ToDictionary(); // 对象转字典
myClass.ToDynamic(); // 对象转换成动态可扩展类型
```

### 24.邮件发送

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

### 25.图像的简单处理

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
stream=maker.AddWatermark("水印文字","字体文件",字体大小,color,水印位置,边距); // 给图片添加水印
stream=maker.AddWatermark(水印图片,水印位置,边距,字体大小,字体); // 给图片添加水印

// 图像相似度对比
var hasher = new ImageHasher();
var hash1 = hasher.DifferenceHash256("图片1"); // 使用差分哈希算法计算图像的256位哈希
var hash2 = hasher.DifferenceHash256("图片2"); // 使用差分哈希算法计算图像的256位哈希
//var hash1 = hasher.AverageHash64("图片1"); // 使用平均值算法计算图像的64位哈希
//var hash2 = hasher.AverageHash64("图片2"); // 使用平均值算法计算图像的64位哈希
//var hash1 = hasher.DctHash("图片1"); // 使用DCT算法计算图像的64位哈希
//var hash2 = hasher.DctHash("图片2"); // 使用DCT算法计算图像的64位哈希
//var hash1 = hasher.MedianHash64("图片1"); // 使用中值算法计算给定图像的64位哈希
//var hash2 = hasher.MedianHash64("图片2"); // 使用中值算法计算给定图像的64位哈希
var sim=ImageHasher.Compare(hash1,hash2); // 图片的相似度，范围：[0,1]

var imageFormat=stream.GetImageType(); // 获取图片的真实格式
```

### 26.随机数

```csharp
Random rnd = new Random();
int num = rnd.StrictNext();//产生真随机数
double gauss = rnd.NextGauss(20,5);//产生正态高斯分布的随机数
var s = new NumberFormater(62).ToString(new Random().Next(100000, int.MaxValue));//生成随机字符串
```

### 27.权重随机筛选功能

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

```csharp
list.WeightedItems(3,e=>e.Price); // 按价格权重选出3个元素
list.WeightedBy(e=>e.Price); // 按价格权重选出1个元素
```

### 28.EF Core支持AddOrUpdate方法

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

### 29.敏感信息掩码

```csharp
"13123456789".Mask(); // 131****5678
"admin@masuit.com".MaskEmail(); // a****n@masuit.com
```

### 30.集合扩展

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

var nums=Enumerable.Range(1, 10).ExceptBy(Enumerable.Range(5, 10), i => i); // 按字段取差集
var nums=Enumerable.Range(1, 10).IntersectBy(Enumerable.Range(5, 10), i => i); // 按字段取交集
var nums=Enumerable.Range(1, 10).SequenceEqual(Enumerable.Range(5, 10), i => i); // 判断序列相等
var nums=Enumerable.Range(1, 10).OrderByRandom(); // 随机排序

// 多个集合取交集
var list=new List<List<MyClass>>(){
    new List<MyClass>(){
        new MyClass(){Name="aa",Age=11},
        new MyClass(){Name="bb",Age=12},
        new MyClass(){Name="cc",Age=13},
    },
    new List<MyClass>(){
        new MyClass(){Name="bb",Age=12},
        new MyClass(){Name="cc",Age=13},
        new MyClass(){Name="dd",Age=14},
    },
    new List<MyClass>(){
        new MyClass(){Name="cc",Age=13},
        new MyClass(){Name="dd",Age=14},
        new MyClass(){Name="ee",Age=15},
    },
};
var sect=list.IntersectAll(m=>m.Name); // new MyClass(){Name="cc",Age=13}

var list=new List<List<int>>(){
    new(){1,2,3},
    new(){2,3,4},
    new(){3,4,5}
};
var sect=list.IntersectAll();// [3]

// 集合元素改变其索引位置
list.ChangeIndex(item,3); // 将元素item的索引位置变为第3个
list.ChangeIndex(t=>t.Id=="123",2); // 将id为123的元素的索引位置变为第2个

var item=list.Percentile(50); // 取第50%分位数的元素
```

### 31.Mime类型

```csharp
var mimeMapper = new MimeMapper();
var ext = mimeMapper.GetExtensionFromMime("image/jpeg"); // .jpg
var mime = mimeMapper.GetMimeFromExtension(".jpg"); // image/jpeg

ContentType常量库：
var type=ContentType.Exe; // application/octet-stream
var type=ContentType.Jpeg; // image/jpeg
var type=DefaultMimeItems.Items.FirstOrDefault(t=>t.Extension=="jpg"); // image/jpeg
```

### 32.日期时间扩展

```csharp

double milliseconds = DateTime.Now.GetTotalMilliseconds();// 获取毫秒级时间戳
double microseconds = DateTime.Now.GetTotalMicroseconds();// 获取微秒级时间戳
double nanoseconds = DateTime.Now.GetTotalNanoseconds();// 获取纳秒级时间戳
double seconds = DateTime.Now.GetTotalSeconds();// 获取秒级时间戳
double minutes = DateTime.Now.GetTotalMinutes();// 获取分钟级时间戳

var indate=DateTime.Parse("2020-8-3").In(DateTime.Parse("2020-8-2"),DateTime.Parse("2020-8-4"));//true
DateTime time="2021-1-1 8:00:00".ToDateTime(); //字符串转DateTime

//时间段计算工具
var range = new DateTimeRange(DateTime.Parse("2020-8-3"), DateTime.Parse("2020-8-5"));
range.Union(DateTime.Parse("2020-8-4"), DateTime.Parse("2020-8-6")); //连接两个时间段，结果：2020-8-3~2020-8-6
range.In(DateTime.Parse("2020-8-3"), DateTime.Parse("2020-8-6"));//判断是否在某个时间段内，true
var (intersected,range2) = range.Intersect(DateTime.Parse("2020-8-4"), DateTime.Parse("2020-8-6"));//两个时间段是否相交，(true,2020-8-3~2020-8-4)
range.Contains(DateTime.Parse("2020-8-3"), DateTime.Parse("2020-8-4"));//判断是否包含某个时间段，true

range.GetUnionSet(List<DateTimeRange>); // 根据某个时间段查找在某批时间段中的最大并集
range.GetMaxTimePeriod(List<DateTimeRange>); // 获取一批时间段内存在相互重叠的最大时间段
...
```

### 33.流相关

```csharp
stream.SaveAsMemoryStream(); // 任意流转换成内存流
stream.ToArray(); // 任意流转换成二进制数组
stream.ToArrayAsync(); // 任意流转换成二进制数组
stream.ShuffleCode(); // 流洗码，在流的末端随即增加几个空字节，重要数据请谨慎使用，可能造成流损坏

// 池化内存流，用法与MemorySteam保持一致
using var ms=PooledMemoryStream();

// 大型内存流,最大可支持1TB内存数据，推荐当数据流大于2GB时使用，用法与MemorySteam保持一致
using var ms=LargeMemoryStream();

//文件流快速复制
FileStream fs = new FileStream(@"D:\boot.vmdk", FileMode.OpenOrCreate, FileAccess.ReadWrite);
{
        //fs.CopyToFile(@"D:\1.bak");//同步复制大文件
        fs.CopyToFileAsync(@"D:\1.bak");//异步复制大文件
        string md5 = fs.GetFileMD5Async().Result;//异步获取文件的MD5
        string sha1 = fs.GetFileSha1();//异步获取文件的SHA1
}
memoryStream.SaveFile("filename"); // 将内存流转储成文件
```

### 34.类型操作

```csharp
1.2345678901.Digits8(); // 将小数截断为8位
1.23.ConvertTo<int>(); // 小数转int
1.23.ConvertTo<T>(); // 小数转T基本类型
bool b=1.23.TryConvertTo<T>(out result); // 小数转T基本类型
var num=1.2345.ToDecimal(2); //转decimal并保留两位小数

1.23.ChangeTypeTo<T>(); //小数转T基本类型,ConvertTo和ChangeTypeTo的区别在于：ConvertTo只适用于基元类型的互转，ChangeTypeTo不仅适用于基元类型的互转还支持数组、字符串的转换(Parse)，ConvertTo的性能更高

type.IsPrimitive(); // 判断类型是否是值类型
type.IsSimpleType(); // 判断类型是否是常见的简单类型，基元类型为 Boolean、 Byte、 SByte、 Int16、 UInt16、 Int32、 UInt32、 Int64、 UInt64、 IntPtr、 UIntPtr、 Char、 Double 、 Single、枚举、Nullable<T>。
type.IsSimpleArrayType(); // 判断类型是否是常见类型的 数组形式 类型
type.IsSimpleListType(); // 判断类型是否是常见类型的 泛型形式 类型

myClass.ToJsonString(); //序列化成json字符串

string s=null;
string str=s.IfNullOrEmpty("aa");//如果为空则返回aa
string str=s.IfNullOrEmpty(()=>"aa");//如果为空则返回aa,延迟执行

bool contains=s.Contains(new[]{"aa","bb"});// 检测字符串中是否包含列表中的关键词(快速匹配)
bool contains=s.ContainsSafety(new[]{"aa","bb"});// 检测字符串中是否包含列表中的关键词(安全匹配)，没有计时攻击风险
bool contains=s.EndsWith(new[]{"aa","bb"});// 检测字符串中是否以列表中的任意关键词结尾
bool contains=s.StartsWith(new[]{"aa","bb"});// 检测字符串中是否以列表中的任意关键词开头

string str=s.Take(10); // 取字符串前10个字符

bool emoji=s.MatchEmoji(); // 匹配字符串是否包含emoji

var width=str.StringWidth(14); // 计算字符串以14号字体大小的渲染宽度像素
var width=str.StringWidth("微软雅黑",14); // 计算字符串以14号字体大小的微软雅黑字体的渲染宽度像素

```

### 35.INI配置文件操作(仅支持Windows)

```csharp
INIFile ini=new INIFile("filename.ini");
ini.IniWriteValue(section,key,value); // 写值
ini.IniReadValue(section,key); // 读值
ini.ClearAllSection(); // 清空所有配置节
ini.ClearSection(section); // 清空配置节
```

### 36.雷达图计算引擎

应用场景：计算两个多边形的相似度，用户画像之类的

```csharp
var points=RadarChartEngine.ComputeIntersection(chart1,chart2); //获取两个多边形的相交区域
points.ComputeArea(); //计算多边形面积
```

### 37.树形结构实现

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

### 38.简单的Excel导出

需要额外依赖包：`Masuit.Tools.Excel`

```csharp
var stream=list.Select(item=>new{
    姓名=item.Name,
    年龄=item.Age,
    item.Gender,
    Avatar=Image.FromStream(filestream) //图片列
}).ToDataTable().ToExcel("Sheet1"); //自定义列名导出
var stream=list.ToDataTable("Sheet1").ToExcel("文件密码");
```

#### 一些约定规则：

1. 图片列支持Stream、Bitmap、IEnumerable `<Stream>`、IEnumerable `<Bitmap>`、IDictionary<string,Stream>、IDictionary<string,MemoryStream>、IDictionary<string,Bitmap>类型；
2. 其中，如果是IDictionary类型的图片列，字典的键为图片超链接的完整url；
3. 默认字段名作为列名导出；
4. 若list是一个具体的强类型，默认会先查找每个字段的Description标记，若有Description标记，则取Description标记作为列名显示
5. ToExcel方法支持DataTable、List `<DataTable>`、Dictionary<string, DataTable>类型的直接调用

### 39.EFCore实用扩展

#### 跟踪实体变更对比

获取指定实体的变更

```csharp
var changes=dbContext.GetChanges<TEntity>();//获取变更字段信息
var added=dbContext.GetAdded<TEntity>();//获取添加的实体字段信息
var removed=dbContext.GetRemoved<TEntity>();//获取被移除的实体字段信息  
var allchanges=dbContext.GetAllChanges<TEntity>();//获取增删改的实体字段信息  
```

获取所有实体的变更

```csharp
var changes=dbContext.GetChanges();//获取变更字段信息
var added=dbContext.GetAdded();//获取添加的实体字段信息
var removed=dbContext.GetRemoved();//获取被移除的实体字段信息  
var allchanges=dbContext.GetAllChanges();//获取增删改的实体字段信息  
```

对比信息包含属性信息、旧值、新值、实体信息、键信息、变更状态等

#### nolock查询

sqlserver：

上下文注入Interceptor即可在任何查询时使用nolock查询

```csharp
services.AddDbContext<TContext>(opt => opt.UseSqlserver("ConnString", builder => builder.AddInterceptors(new QueryWithNoLockDbCommandInterceptor()));
```

通用数据库：

nolock本质是开启一个 `读未提交`级别的事务，此时的查询性能最好，但有可能会读取到脏数据。

```csharp
dbcontext.NoLock(ctx=>ctx.Users...ToList()); // 开启一个nolock上下文，上下文范围内自动nolock

// 执行单个nolock查询
await dbcontext.Users.Where(x=>x.Name=="aaa").ToListWithNoLockAsync(); 
await dbcontext.Users.Where(x=>x.Name=="aaa").FirstOrDefaultWithNoLockAsync(); 
await dbcontext.Users.Where(x=>x.Name=="aaa").SingleOrDefaultWithNoLockAsync(); 
await dbcontext.Users.AnyWithNoLockAsync(x=>x.Name=="aaa"); 
await dbcontext.Users.AllWithNoLockAsync(x=>x.Name=="aaa"); 
await dbcontext.Users.CountWithNoLockAsync(x=>x.Name=="aaa"); 

// 如果在上下文开启了重试机制,执行单个nolock查询需要开启策略查询
services.AddDbContext<TContext>(opt => opt.UseNpgsql("ConnString", builder => builder.EnableRetryOnFailure(10));

// 执行策略
dbcontext.ExecutionStrategy(stg=>stg.NoLock(ctx=>ctx.Users...ToList()));
dbcontext.ExecutionStrategy(ctx=>ctx.Users.Where(x=>x.Name=="aaa").ToListWithNoLockAsync());
```

#### 自动递归式Include扩展(通常用于树形表)

```csharp
// 如果你现在正在写类似这样的代码：
dbcontext.Category.Include(c=>c.Children).ThenInclude(c=>c.Children).ThenInclude(c=>c.Children).ThenInclude(c=>c.Children);

// 那么可以改成这样：
dbcontext.Category.IncludeRecursive(4, c => c.Children); // 自动Include 4次
```

### 40.任何类型支持链式调用

```csharp
a.Next(func1).Next(func2).Next(func3);
"123".Next(s=>s.ToInt32()).Next(x=>x*2).Next(x=>Math.Log(x));
```

### 41.Newtonsoft.Json的只允许字段(反)序列化行为的契约解释器

#### DeserializeOnlyContractResolver

该解释器针对类属性被DeserializeOnlyJsonPropertyAttribute/SerializeIgnoreAttribute标记的，在反序列化的时候生效，在序列化的时候忽略;被SerializeOnlyJsonPropertyAttribute/DeserializeIgnoreAttribute标记的，在序列化的时候生效，在反序列化的时候忽略

```csharp
public class ClassDto
    {
        // 序列化时忽略这个属性/反序列化时加载这个属性
        [DeserializeOnlyJsonProperty]
        //[SerializeIgnore]
        public string MyProperty { get; set; }

        // 反序列化时忽略这个属性/序列化时加载这个属性
        [SerializeOnlyJsonProperty]
        //[DeserializeIgnore]
        public int Num { get; set; }
    }
  
    JsonConvert.SerializeObject(new MyClass(),new JsonSerializerSettings()
    {
        ContractResolver = new DeserializeOnlyContractResolver() // 配置使用DeserializeOnlyContractResolver解释器
    });
```

如果是WebAPI全局使用：

```csharp
        //在Startup.ConfigureServices中
        services.AddMvc().AddNewtonsoftJson(options =>
             {
                 var resolver = new DeserializeOnlyContractResolver();
                 resolver.NamingStrategy = new CamelCaseNamingStrategy();
                 options.SerializerSettings.ContractResolver = resolver;
             });
```

#### FallbackJsonPropertyResolver

该解释器针对某个属性设置多个别名，反序列化时支持多个别名key进行绑定，弥补官方JsonProperty别名属性只能设置单一别名的不足

```csharp
    public class ClassDto
    {
        [FallbackJsonProperty("MyProperty","a","b")]
        public string MyProperty { get; set; }

        public int Num { get; set; }
    }
  
    JsonConvert.SerializeObject(new MyClass(),new JsonSerializerSettings()
    {
        ContractResolver = new FallbackJsonPropertyResolver() // 配置使用FallbackJsonPropertyResolver解释器
    });
```

#### CompositeContractResolver

该解释器是DeserializeOnlyContractResolver和FallbackJsonPropertyResolver的融合版

### 42. ASP.NET Core Action同时支持queryString、表单和json请求类型的模型绑点器BodyOrDefaultModelBinder

用法：
引入包：`Masuit.Tools.AspNetCore`

```shell
PM> Install-Package Masuit.Tools.AspNetCore
```

Startup配置：

```csharp
app.UseBodyOrDefaultModelBinder();
```

在action的参数模型前打上标记：`[FromBodyOrDefault]`即可，示例代码如下：

```csharp
        [HttpGet("query"),HttpPost("query")]
        public IActionResult Query([FromBodyOrDefault]QueryModel query)
        {
            return Ok(...);
        }
  
        [HttpGet("query"),HttpPost("query")]
        public IActionResult Query([FromBodyOrDefault]int id,[FromBodyOrDefault]string name)
        {
            return Ok(...);
        }
```

### 43. 字符串SimHash相似度算法

```csharp
var dis="12345678".HammingDistance("1234567");
var dis=new SimHash("12345678").HammingDistance(new SimHash("1234567"));
```

### 44. 真实文件类型探测

```csharp
// 多种方式，任君调用
var detector=new FileInfo(filepath).DetectFiletype();
//var detector=File.OpenRead(filepath).DetectFiletype();
//var detector=FileSignatureDetector.DetectFiletype(filepath);
detector.Precondition;//基础文件类型
detector.Extension;//真实扩展名
detector.MimeType;//MimeType
detector.FormatCategories;//格式类别
```

#### 默认支持的文件类型

|   扩展名   |                              说明                              |
| :--------: | :-------------------------------------------------------------: |
|    3GP    |                          3GPP, 3GPP 2                          |
|     7Z     |                              7-Zip                              |
|    APK    |                    ZIP based Android Package                    |
|    AVI    |                     Audio-Video Interleave                     |
|     SH     |                          Shell Script                          |
|   BPLIST   |                      Binary Property List                      |
|  BMP, DIB  |                             Bitmap                             |
|    BZ2    |                       Bunzip2 Compressed                       |
|    CAB    |                        Microsoft Cabinet                        |
|   CLASS   |                          Java Bytecode                          |
|   CONFIG   |                     .NET Configuration File                     |
| CRT, CERT |                           Certificate                           |
|    CUR    |                             Cursor                             |
|     DB     |              Windows Thumbs.db Thumbnail Database              |
|    DDS    |                       DirectDraw Surface                       |
|    DLL    |                 Windows Dynamic Linkage Library                 |
|    DMG    |                     Apple Disk Mount Image                     |
|    DMP    |                    Windows Memory Dump File                    |
|    DOC    |             Microsoft Office Word 97-2003 Document             |
|    DOCX    |             Microsoft Office Word OpenXML Document             |
|    EPUB    |                         e-Pub Document                         |
|    EXE    |                        Windows Executive                        |
|    FLAC    |                         Loseless Audio                         |
|    FLV    |                           Flash Video                           |
|    GIF    |                   Graphics Interchage Format                   |
|     GZ     |                          GZ Compressed                          |
|    HDP    |                     HD Photo(JPEG XR) Image                     |
|    HWP    |                   Legacy HWP, HWPML, CFBF HWP                   |
|    ICO    |                              Icon                              |
|    INI    |                       Initialization File                       |
|    ISO    |                       ISO-9660 Disc Image                       |
|    LNK    |                      Windows Shortcut Link                      |
|    JP2    |                         JPEG 2000 Image                         |
| JPG, JPEG |             Joint Photographic Experts Group Image             |
|    LZH    |                         LZH Compressed                         |
|    M4A    |               MP4 Container Contained Audio Only               |
|    M4V    |                  MP4 Container Contained Video                  |
|    MID    |                           Midi Sound                           |
|    MKA    |             Matroska Container Contained Audio Only             |
|    MKV    |               Matroska Container Contained Video               |
|    MOV    |                      QuickTime Movie Video                      |
|    MP4    |                MP4 Container Contained Contents                |
|    MSI    |                       Microsoft Installer                       |
|    OGG    |                       OGG Video or Audio                       |
|    ODF    |                      OpenDocument Formula                      |
|    ODG    |                      OpenDocument Graphics                      |
|    ODP    |                    OpenDocument Presentation                    |
|    ODS    |                    OpenDocument Spreadsheet                    |
|    ODT    |                        OpenDocument Text                        |
|    PAK    |                  PAK Archive or Quake Archive                  |
|    PDB    |                   Microsoft Program Database                   |
|    PDF    |                    Portable Document Format                    |
|    PFX    |       Microsoft Personal Information Exchange Certificate       |
|    PNG    |                 Portable Network Graphics Image                 |
|    PPT    |          Microsoft Office PowerPoint 97-2003 Document          |
|    PPTX    |          Microsoft Office PowerPoint OpenXML Document          |
|    PPSX    | Microsoft Office PowerPoint OpenXML Document for Slideshow only |
|    PSD    |                       Photoshop Document                       |
|    RAR    |                        WinRAR Compressed                        |
|    REG    |                        Windows Registry                        |
|    RPM    |                 RedHat Package Manager Package                 |
|    RTF    |                    Rich Text Format Document                    |
|    SLN    |                Microsoft Visual Studio Solution                |
|    SRT    |                         SubRip Subtitle                         |
|    SWF    |                         Shockwave Flash                         |
| SQLITE, DB |                         SQLite Database                         |
|    TAR    |             pre-ISO Type and UStar Type TAR Package             |
|    TIFF    |                 Tagged Image File Format Image                 |
|    TXT    |                           Plain Text                           |
|    WAV    |                           Wave Audio                           |
|    WASM    |                       Binary WebAssembly                       |
|    WEBM    |                           WebM Video                           |
|    WEBP    |                           WebP Image                           |
|    XAR    |                           XAR Package                           |
|    XLS    |             Microsoft Office Excel 97-2003 Document             |
|    XLSX    |             Microsoft Office Excep OpenXML Document             |
|    XML    |               Extensible Markup Language Document               |
|     Z     |                          Z Compressed                          |
|    ZIP    |                           ZIP Package                           |

### 45. 动态类型扩展

让动态类型支持属性访问器和索引器调用

```csharp
        var obj = DynamicFactory.NewObject();
        obj.Name = "Masuit";
        obj.Age = 18;
        obj["Gender"]="男"
        obj.MyClass = DynamicFactory.WithObject(new
        {
            X = 10,
            Y = 20,
            Z = new List<int> { 1, 2, 3, 4, 5 }
        });
        Assert.Equal(obj.Name, obj["Name"]);
        Assert.Equal(obj["Gender"], obj.Gender);
        Assert.Equal(obj["MyClass"]["X"], obj.MyClass.X);
        Assert.Equal(obj.MyClass.Z[2], obj["MyClass"]["Z"][2]);
```

普通类型转换成动态类型

```csharp
        var obj = new
        {
            Name = "Masuit"
        }.ToDynamic();
        obj.Age = 18;
        obj.MyClass = new
        {
            X = 10,
            Y = 20,
            Z = new List<int> { 1, 2, 3, 4, 5 }
        }.ToDynamic();
        obj.Prop = "test";
        _ = obj - "Prop"; // 删除属性
        _ = obj + "Prop"; // 增加属性

        Assert.Equal(obj.Name, obj["Name"]);
        Assert.Equal(obj["MyClass"]["X"], obj.MyClass.X);
```

### 46. 反病毒(仅支持Windows)

```csharp
// 要求系统WindowsDefender没有被停掉
var result = WindowsDefenderScanService.ScanFile(@"Y:\1.exe"); // 扫描文件
var result = WindowsDefenderScanService.ScanDirectory(@"Y:\"); // 扫描文件夹
var result = WindowsDefenderScanService.ScanStream(stream); // 扫描文件流

// 要求C:\Windows\System32\amsi.dll文件存在，可在WindowsDefender停止时工作
AmsiScanService.Scan(stream); // 扫描文件流
AmsiScanService.Scan(@"Y:\1.exe"); // 扫描文件
AmsiScanService.Scan(bytes); // 扫描二进制数组
```

### 47. 生成验证码

```csharp
var code=ValidateCode.ValidateCode(6); // 生成6位长度的验证码
var stream=code.CreateValidateGraphic(); // 生成验证码图片流
```

### 48. DistributedCache扩展

```csharp
var item=cache.Get<T>(key); // 获取值
var item=cache.GetOrAdd<T>(key,value); // 获取或添加值
var item=cache.GetOrAdd<T>(key,valueFactory); // 获取或添加值
cache.Set<T>(key,value); // 设置值
cache.AddOrUpdate<T>(key,value,valueFactory); // 添加或更新值
```

### 49. ViewData扩展

```csharp
var item=ViewData.GetValue<T>(key);//获取对象
var item=ViewData.GetValueOrDefault<T>(key,defaultValue);//获取对象
var item=ViewData.GetValueOrDefault<T>(key,defaultValueFactory);//获取对象
```

### 50. 线程上下文存取临时值

```csharp
CurrentContext<T>.SetData(value);//设置值
var item=CurrentContext<T>.GetData();//获取值

CurrentContext.SetData(value);//设置值
var item=CurrentContext.GetData<T>();//获取值
```

### 51. ASP.NET Core自动扫描注册服务

```csharp
// 自动扫描注册服务
services.AutoRegisterServices();

// 需要自动注册的服务打上ServiceInject标记即可。
[ServiceInject(ServiceLifetime.Scoped)]
public class MyClass:MyInterface{...}

[ServiceInject(ServiceLifetime.Scoped)]
public class MyService{...}
```

# Asp.Net MVC和Asp.Net Core的支持断点续传和多线程下载的ResumeFileResult

在ASP.NET Core中通过MVC/WebAPI应用程序传输文件数据时使用断点续传以及多线程下载支持。

它提供了 `ETag`标头以及 `Last-Modified`标头。 它还支持以下前置条件标头：`If-Match`，`If-None-Match`，`If-Modified-Since`，`If-Unmodified-Since`，`If-Range`。

## 支持 ASP.NET Core 2.0+

从.NET Core2.0开始，ASP.NET Core内部支持断点续传。 因此只是对FileResult做了一些扩展。 只留下了“Content-Disposition” Inline的一部分。 所有代码都依赖于基础.NET类。

## 如何使用

### .NET Framework

在你的控制器中，你可以像在 `FileResult`一样的方式使用它。

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

要使用ResumeFileResults，必须在 `Startup.cs`的 `ConfigureServices`方法调用中配置服务：

```csharp
using Masuit.Tools.AspNetCore.ResumeFileResults.Extensions;
```

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddResumeFileResult();
}
```

然后在你的控制器中，你可以像在 `FileResult`一样的方式使用它。

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
另外，它可以提供 `ETag`和 `LastModified`标头。

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
