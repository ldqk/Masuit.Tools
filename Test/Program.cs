using Masuit.Tools.Files;
using System.Collections.Generic;
using System.IO;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] bytes = ClassZip.ZipStream(new List<string>()
            {
                @"D:\vsix2017\ReSharper\JetBrains Resharper 2018.3 Patch\JetBrains Resharper 2018.2.x AutoPatch.cmd",
                @"D:\vsix2017\ReSharper\JetBrains Resharper 2018.3 Patch\JetBrains Resharper 2018.3 Patch.cmd",
                @"D:\vsix2017\ReSharper\JetBrains Resharper 2018.3 Patch\sfk190.exe",
            });
            FileStream fs = new FileStream(@"D:\1.zip", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            fs.Write(bytes, 0, bytes.Length);
        }
    }

    public class A
    {
        public C C { get; set; }
        public List<C> List { get; set; }
    }

    public class B
    {
        public C C { get; set; }
        public List<D> List { get; set; }
    }

    public class C
    {
        public string MyProperty { get; set; }
        public D Obj { get; set; }
    }

    public class D
    {
        public string MyProperty { get; set; }
        public C Obj { get; set; }
    }
}