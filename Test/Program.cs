using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Masuit.Tools.NoSQL;
using StackExchange.Redis;

namespace Test
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            using (RedisHelper redisHelper = RedisHelper.GetInstance("192.168.3.148:6379", 2))
            {
                var count = redisHelper.GetServer().Keys(0,"o*").Count();
                Console.WriteLine(count);
            }
            Console.WriteLine("ok");
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