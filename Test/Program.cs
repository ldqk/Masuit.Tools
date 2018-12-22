using Masuit.Tools.Strings;
using Masuit.Tools.Systems;
using System;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var timer = HiPerfTimer.Execute(() =>
            {
                //var dic = new Dictionary<string, int>();
                //for (int i = 0; i < 1000000; i++)
                //{
                //    dic.Add(DateTime.Now.Ticks.ToBinary(36), 0);
                //}
                long s = new NumberFormater(16).FromString("1a");
                Console.WriteLine(s);
            });
            Console.WriteLine(timer);
        }
    }
}