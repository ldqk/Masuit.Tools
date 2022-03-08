using System;
using Masuit.Tools;
using Masuit.Tools.Systems;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using NetCoreTest;
using Newtonsoft.Json;

string json1 = "{\"a\":\"aa\"}";
string json2 = "{\"b\":\"bb\"}";
string json3 = "{\"MyProperty\":\"mm\"}";
JsonConvert.DefaultSettings = () => new JsonSerializerSettings() { ContractResolver = new CompositeContractResolver() };
var m1 = JsonConvert.DeserializeObject<MyClass>(json1);
var m2 = JsonConvert.DeserializeObject<MyClass>(json2);
var m3 = JsonConvert.DeserializeObject<MyClass>(json3);
Console.ReadKey();

//CreateWebHostBuilder(args).Build().Run();
static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
    WebHost.CreateDefaultBuilder(args)
        .UseStartup<Startup>();

public class MyClass
{
    [SerializeIgnore, FallbackJsonProperty(nameof(MyProperty), "a", "b")]
    public string MyProperty { get; set; }
}
