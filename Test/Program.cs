using Masuit.Tools.Security;
using System;

namespace Test
{
    static class Program
    {
        static void Main(string[] args)
        {
            RsaKey keys = RsaCrypt.GenerateRsaKeys();
            Console.WriteLine(keys.PublicKey);
            Console.WriteLine(keys.PrivateKey);
        }
    }
}