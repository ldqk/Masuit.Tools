using Masuit.Tools.AspNetCore.ResumeFileResults.WebTest;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;

namespace Masuit.Tools.Core.UnitTest.AspNetCore
{
    public abstract class TestBase
    {
        protected TestBase()
        {
            var path = Path.GetDirectoryName(typeof(Startup).GetTypeInfo().Assembly.Location);
            var di = new DirectoryInfo(path).Parent.Parent.Parent;

            // Arrange
            //var hostBuilder = Host.CreateDefaultBuilder().ConfigureWebHostDefaults(configurationBuilder => configurationBuilder.UseStartup<Startup>().UseContentRoot(di.FullName));
            //var host = hostBuilder.Build();
            //host.Run();
            //todo:.NET Core3.0创建测试服务器
            Server = new TestServer(new WebHostBuilder().UseStartup<Startup>());
            Client = Server.CreateClient();
        }

        public HttpClient Client { get; private set; }

        public EntityTagHeaderValue EntityTag { get; } = new EntityTagHeaderValue("\"TestFile\"");

        public DateTimeOffset LastModified { get; } = new DateTimeOffset(2016, 1, 1, 0, 0, 0, TimeSpan.Zero);

        public TestServer Server { get; private set; }
    }
}