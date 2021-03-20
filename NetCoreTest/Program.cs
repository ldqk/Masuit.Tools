using Masuit.Tools.Models;
using Masuit.Tools.Reflection;
using Masuit.Tools.Security;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace NetCoreTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var myClass = new MyClass()
            {
                MyProperty1 = 1,
                Name = "1",
                Parent = new MyClass()
                {
                    Name = "mc",
                    Parent = new MyClass()
                    {
                        Name = "ccc"
                    }
                }
            };
            var path = myClass.Path(c => c.Name);
            Console.WriteLine(path);

            myClass.SetProperty(nameof(MyClass.MyProperty1), 1);
            Console.ReadKey();
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

    public class MyClass : ITreeParent<MyClass>
    {
        [Description("test")]
        public string MyProperty { get; set; }
        public int? MyProperty1 { get; set; }

        /// <summary>
        /// 名字
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 父节点
        /// </summary>
        public virtual MyClass Parent { get; set; }

        /// <summary>
        /// 子级
        /// </summary>
        public ICollection<MyClass> Children { get; set; } = new List<MyClass>();
    }
}
