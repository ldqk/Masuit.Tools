using Masuit.Tools;
using Masuit.Tools.Systems;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var timer = HiPerfTimer.Execute(() =>
            {
                var dic = new Dictionary<string, int>();
                var sf = SnowFlake.GetInstance();
                for (int i = 0; i < 1000000; i++)
                {
                    //Console.WriteLine(ObjectId.GenerateNewId());
                    var id = Stopwatch.GetTimestamp().ToBinary(36);
                    dic.Add(id, 0);
                }
            });
            Console.WriteLine(timer);
        }
    }
}