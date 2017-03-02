using System;
using System.IO;
using Masuit.Tools.Files;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            FileStream fs = new FileStream(@"D:\boot.vmdk", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            {
                fs.CopyToFileAsync(@"D:\1.bak");
            }
            Console.WriteLine("复制完成");
            Console.ReadKey();
        }
    }
}
