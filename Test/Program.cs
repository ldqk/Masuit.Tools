using Masuit.Tools;
using Masuit.Tools.Security;
using Masuit.Tools.Systems;
using System;

namespace Test
{
    static class Program
    {
        static void Main(string[] args)
        {
            var rsaKey = RsaCrypt.GenerateRsaKeys(512);
            Console.WriteLine(rsaKey.PrivateKey);
            var enc = new MyClass()
            {
                SdTime = DateTime.Now,
                MyProperty = "asdf"
            }.ToJsonString().RSAEncrypt();
            Console.WriteLine(enc);
            Console.WriteLine(HiPerfTimer.Execute(() =>
            {
                var dec = enc.RSADecrypt();
                Console.WriteLine(dec);
            }) * 1000);
        }
    }

    public class MyClass
    {
        public string MyProperty { get; set; }
        public DateTime SdTime { get; set; }

    }
}