using Masuit.Tools.Security;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using System.ComponentModel;

namespace NetCoreTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var rsaKey = RsaCrypt.GenerateRsaKeys();
            var enc = "123456".RSAEncrypt();
            var dec = enc.RSADecrypt();
            Console.WriteLine(dec);
            Console.ReadKey();
            //CreateWebHostBuilder(args).Build().Run();
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
