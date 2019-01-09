using Masuit.Tools.DateTimeExt;
using System;

namespace Test
{
    static class Program
    {
        static void Main(string[] args)
        {
            DateTime time = DateTime.Now;
            Console.WriteLine(time.GetTotalMilliseconds());
            Console.WriteLine(time.GetTotalMicroseconds());
            Console.WriteLine(time.GetTotalNanoseconds());
        }
    }
}