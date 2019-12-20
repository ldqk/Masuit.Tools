using Masuit.Tools.Win32;
using System;

namespace Test
{
    static class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(Windows.GetLocalUsedIP());
        }
    }
}