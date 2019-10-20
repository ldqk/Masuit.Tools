using Masuit.Tools.Core.Database;
using Masuit.Tools.Hardware;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace NetCoreTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var table = new List<MyClass>() { new MyClass() { MyProperty = "sss", MyProperty1 = 222 } }.Select(c => new { 列1 = c.MyProperty, 列2 = c.MyProperty1 }).ToDataTable();

            Console.WriteLine((long)SystemInfo.GetRamInfo().MemoryAvailable);
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();

    }

    public class MyClass
    {
        [Description("test")]
        public string MyProperty { get; set; }
        public int MyProperty1 { get; set; }

    }
}
