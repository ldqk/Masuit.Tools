using Masuit.Tools;
using Masuit.Tools.Database;
using Masuit.Tools.Excel;
using Masuit.Tools.Files;
using Masuit.Tools.Models;
using Masuit.Tools.Reflection;
using Masuit.Tools.Security;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Masuit.Tools.Systems;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace NetCoreTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var pkg = new ExcelPackage();
            pkg.Workbook.Worksheets.Add("Sheet1");
            var sheet = pkg.Workbook.Worksheets["Sheet1"];
            sheet.FillWorksheet(Enumerable.Range(1, 2).Select(i => new
            {
                序号 = i,
                图片 = new Dictionary<string, Stream>()
                {
                    ["https://ldqk.org/1383"] = File.OpenRead(@"D:\images\emotion\16.jpg"),
                    ["https://ldqk.org/1384"] = File.OpenRead(@"D:\images\emotion\16.jpg"),
                    ["https://ldqk.org/1385"] = File.OpenRead(@"D:\images\emotion\16.jpg"),
                }
            }).ToDataTable("aa"), null, 4, 3);
            sheet.Cells["A1:F1"].Merge = true;
            sheet.Cells["A1"].Value = "title";
            pkg.SaveAs("Y:\\1.xlsx");
            Console.WriteLine("ok");

            Console.ReadKey();
            Enumerable.Range(1, 2).Select(i => new
            {
                序号 = i,
                图片 = new Dictionary<string, FileStream>()
                {
                    ["https://ldqk.org/1383"] = File.OpenRead(@"D:\images\emotion\16.jpg")
                }
            }).ToDataTable("aa").ToExcel().SaveFile(@"Y:\2.xlsx");
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

    public class ClassCmd
    {
        public string MyProperty { get; set; }

        public int Num { get; set; }
    }

    public class ClassDto
    {
        [DeserializeOnlyJsonProperty]
        public string MyProperty { get; set; }

        public int Num { get; set; }
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
        [DeserializeOnlyJsonProperty]
        public string Name { get; set; }
    }
}
