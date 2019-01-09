using System;
using Masuit.Tools.Media;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System.Drawing;
using Masuit.Tools.Win32;

namespace NetCoreTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Random rnd = new Random();
            int num = rnd.StrictNext();//产生真随机数
            double gauss = rnd.NextGauss(20,5);//产生正态分布的随机数
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();

    }
}
