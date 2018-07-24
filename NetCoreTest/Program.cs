using System;
using Masuit.Tools;
using Masuit.Tools.Core.Logging;

namespace NetCoreTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(AppContext.BaseDirectory);
            LogManager.Event += info =>
              {
                  Console.WriteLine(info.ToJsonString());
              };
            LogManager.Error(new Exception("测试异常"));
        }
    }
}
