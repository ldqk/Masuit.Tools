using System;
using System.Collections.Generic;
using System.Linq;
using Masuit.Tools;

namespace Test
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            //FileStream fs = new FileStream(@"D:\boot.vmdk", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            //{
            //    fs.CopyToFileAsync(@"D:\1.bak");
            //    string md5 = fs.GetFileMD5Async().Result;//获取文件的MD5
            //}
            //Console.WriteLine("复制完成");

            //bool isUrl;
            //var match = "//music.163.com/#/search/m/?%23%2Fmy%2Fm%2Fmusic%2Fempty=&s=fade&type=1!k".MatchUrl(out isUrl);
            //Console.WriteLine(isUrl);
            //foreach (Group g in match.Groups)
            //{
            //    if (g.Captures.Count > 0)
            //    {
            //        foreach (Capture c in g.Captures)
            //        {
            //            Console.WriteLine(c.Index + " : " + c.Value);
            //        }
            //    }
            //    else
            //    {
            //        Console.WriteLine(g.Index + " : " + g.Value);
            //    }
            //}

            //bool b1 = "512002199509230611".MatchIdentifyCard();//False
            //bool b2 = "140108197705058894".MatchIdentifyCard();//True
            //Console.WriteLine(b1 + "----" + b2);

            //bool isIP;
            //"114.114.256.114".MatchInetAddress(out isIP);//False
            //"114.114.114.114".MatchInetAddress(out isIP);//True
            //Console.WriteLine(isIP);
            //Console.WriteLine(WindowsCommand.Execute("help"));
            //string match = "vawevbgw".MatchRandomImgSrc();
            //Console.WriteLine(match);
            //PhysicsAddress address = "4.2.2.1".GetPhysicsAddressInfo();

            List<MyClass> mc = new List<MyClass> { new MyClass() { Name = "aaa", Age = 10, MyClass3 = new MyClass3(), MyClass2s = new List<MyClass2>() { new MyClass2() { Age = 22, Name = "dddd" } } }, new MyClass() { Name = "aaa", Age = 10, MyClass3 = new MyClass3(), MyClass2s = new List<MyClass2>() { new MyClass2() { Age = 22, Name = "dddd" } } }, new MyClass() { Name = "aaa", Age = 10, MyClass3 = new MyClass3(), MyClass2s = new List<MyClass2>() { new MyClass2() { Age = 22, Name = "dddd" } } }, new MyClass() { Name = "aaa", Age = 10, MyClass3 = new MyClass3(), MyClass2s = new List<MyClass2>() { new MyClass2() { Age = 22, Name = "dddd" } } } };
            List<MyClass2> list = mc.Map<MyClass, MyClass2>().ToList();

            Console.ReadKey();
        }
    }

    public class MyClass
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public MyClass3 MyClass3 { get; set; }
        public List<MyClass2> MyClass2s { get; set; }
    }

    public class MyClass2
    {
        public string Name { get; set; }
        public int Age { get; set; }

    }

    public class MyClass3
    {
        public string MyProperty { get; set; }

    }
}
