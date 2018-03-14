using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NetCoreTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var sw = new Stopwatch();
            sw.Start();
            var t = SayHello();
            Thread.Sleep(4000);
            Console.WriteLine(t.Result);
            Console.WriteLine(sw.ElapsedMilliseconds);
            Console.ReadKey();
        }
        public static async Task<string> SayHello()
        {
            Thread.Sleep(5000);
            await Task.Delay(5000);
            return "hello world";
        }

    }
}
