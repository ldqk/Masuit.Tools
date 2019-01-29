using Masuit.Tools.Hardware;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;

namespace NetCoreTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine((long)SystemInfo.GetRamInfo().MemoryAvailable);
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();

    }
}
