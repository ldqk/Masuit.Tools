using System.Collections.Concurrent;
using System.Threading.Tasks;
using Masuit.Tools.Systems;

namespace Test
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            var redisLock = new RedisLock();
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