using Masuit.Tools.Net;
using System;

namespace Test
{
    static class Program
    {
        static void Main(string[] args)
        {
            var mtd = new MultiThreadDownloader("https://git.imweb.io/ldqk0/imgbed/raw/master/2020/01/05/sdcgssa1wlxc.jpg", Environment.GetEnvironmentVariable("temp"), "Y:\\1.jpg", 8);
            mtd.Configure(req =>
            {
                req.Referer = "https://masuit.com";
                req.Headers.Add("Origin", "https://baidu.com");
            });
            mtd.TotalProgressChanged += (sender, e) =>
              {
                  var downloader = sender as MultiThreadDownloader;
                  Console.WriteLine("下载进度：" + downloader.TotalProgress + "%");
                  Console.WriteLine("下载速度：" + downloader.TotalSpeedInBytes / 1024 / 1024 + "MBps");
              };
            mtd.FileMergeProgressChanged += (sender, e) =>
              {
                  Console.WriteLine("下载完成");
              };
            mtd.Start();//开始下载
            Console.ReadKey();
        }
    }
}