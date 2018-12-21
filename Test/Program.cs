using Masuit.Tools.Strings;
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
                NumberFormater nf = new NumberFormater(36);
                var set = new HashSet<string>();
                for (int i = 0; i < 1000000; i++)
                {
                    string ts = nf.ToString(Stopwatch.GetTimestamp());
                    //Console.WriteLine(ts);
                    set.Add(ts);
                }
                Console.WriteLine(set.Count);
            });
            Console.WriteLine(timer);
        }
    }
}