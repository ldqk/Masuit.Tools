using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Masuit.Tools;

namespace Test
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            var dic = new Dictionary<string, string>();
            for (int i = 0; i < 1000000; i++)
            {
                var token = "".CreateShortToken(22);
                dic.Add(token, token);
                Console.WriteLine(token);
            }
        }

        public static ConcurrentDictionary<string, object> LockDic { get; set; } = new ConcurrentDictionary<string, object>();
        public static int Count { get; set; }
        public static async Task<string> Test()
        {
            //using (new AsyncLock(LockDic.GetOrAdd("aa", new object())).LockAsync())
            {
                await Task.Run(() =>
                {
                    for (int i = 0; i < 100; i++)
                    {
                        Count++;
                    }
                });
                await Task.Run(() =>
                {
                    for (int i = 0; i < 100; i++)
                    {
                        Count--;
                    }
                });
                return "";
            }
        }
    }
}