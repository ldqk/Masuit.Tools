using Masuit.Tools;
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
            Console.WriteLine("100.125.124.105".IsPrivateIP());
            var rsaKey = RsaCrypt.GenerateRsaKeys(RsaKeyType.PKCS8, 2048);
            Console.WriteLine(rsaKey.PrivateKey);
            Console.WriteLine(rsaKey.PublicKey);
            var enc = "123456".RSAEncrypt();
            Console.WriteLine(enc);
            Console.Beep();
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
