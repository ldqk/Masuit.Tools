using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Test
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://www.hbtswl.com") };
            httpClient.DefaultRequestHeaders.UserAgent.Add(ProductInfoHeaderValue.Parse("Mozilla/5.0"));
            var res = httpClient.GetAsync("/").Result;
            var statusCode = res.StatusCode;
            Console.WriteLine(statusCode);
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