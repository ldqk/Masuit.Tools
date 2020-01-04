using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System.ComponentModel;

namespace NetCoreTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
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
