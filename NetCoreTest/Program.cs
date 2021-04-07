using Masuit.Tools;
using Masuit.Tools.Core.Validator;
using Masuit.Tools.Models;
using Masuit.Tools.Reflection;
using Masuit.Tools.Security;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
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
            var cpa = new ComplexPasswordAttribute(6, 30)
            {
                MustNumber = false,
                MustSymbol = true,
                MustLetter = false
            };
            var valid = cpa.IsValid("000000a");
            Console.WriteLine(cpa.ErrorMessage);


            Console.ReadKey();
            var myClass = new MyClass()
            {
                MyProperty1 = 1,
                Id = "mcc",
                Pid = "mc",
                Parent = new MyClass()
                {
                    Id = "mc",
                    Pid = "ccc",
                    Parent = new MyClass()
                    {
                        Id = "ccc"
                    }
                }
            };
            var allParent = myClass.AllParent().Append(myClass);
            var tree = allParent.ToTreeGeneral(c => c.Id, c => c.Pid);
            Console.WriteLine(tree.ToJsonString(new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented
            }));
            var path = myClass.Path(c => c.Id);
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

    public class MyClass : ITree<MyClass>
    {
        [Description("test")]
        public string Pid { get; set; }
        public int? MyProperty1 { get; set; }

        /// <summary>
        /// 名字
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 父节点
        /// </summary>
        [JsonIgnore]
        public virtual MyClass Parent { get; set; }

        /// <summary>
        /// 子级
        /// </summary>
        [JsonIgnore]
        public ICollection<MyClass> Children { get; set; } = new List<MyClass>();

        /// <summary>
        /// 名字
        /// </summary>
        public string Name { get; set; }
    }
}
