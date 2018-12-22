using Masuit.Tools;
using Masuit.Tools.Systems;
using System;
using System.Collections.Generic;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var timer = HiPerfTimer.Execute(() =>
            {
                var dic = new Dictionary<string, int>();
                for (int i = 0; i < 10000000; i++)
                {
                    dic.Add("".CreateShortToken(), 0);
                }

                Console.WriteLine("".CreateShortToken());
            });
            Console.WriteLine(timer);
        }
    }
}