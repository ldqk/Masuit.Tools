using Masuit.Tools.Core.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Swashbuckle.AspNetCore.Swagger;
using System.IO;

namespace Masuit.Tools.AspNetCore.ResumeFileResults.WebTest
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddResumeFileResult();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Title = "API文档",
                    Version = "v1",
                    Contact = new Contact()
                    {
                        Email = "admin@masuit.com",
                        Name = "懒得勤快",
                        Url = "https://masuit.com"
                    },
                    Description = "断点续传和多线程下载测试站点",
                    License = new License()
                    {
                        Name = "懒得勤快",
                        Url = "https://masuit.com"
                    }
                });
                c.DescribeAllEnumsAsStrings();
                var basePath = PlatformServices.Default.Application.ApplicationBasePath;
                var xmlPath = Path.Combine(basePath, "Masuit.Tools.AspNetCore.ResumeFileResults.WebTest.xml");
                c.IncludeXmlComments(xmlPath);
            });
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger().UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"{Configuration["Swagger:VirtualPath"]}/swagger/v1/swagger.json", "断点续传和多线程下载测试站点");
            });
            app.UseMvcWithDefaultRoute();
        }
    }
}