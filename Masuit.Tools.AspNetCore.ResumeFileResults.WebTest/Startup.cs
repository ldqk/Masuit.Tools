using Masuit.Tools.Core.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.WebEncoders;
using Microsoft.OpenApi.Models;
using System;
using System.Text.Encodings.Web;
using System.Text.Unicode;

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
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = $"接口文档",
                    Description = $"HTTP API ",
                    Contact = new OpenApiContact { Name = "懒得勤快", Email = "admin@masuit.com", Url = new Uri("https://masuit.coom") },
                    License = new OpenApiLicense { Name = "懒得勤快", Url = new Uri("https://masuit.com") }
                });
                var xmlPath = AppContext.BaseDirectory + "Masuit.Tools.AspNetCore.ResumeFileResults.WebTest.xml";
                c.IncludeXmlComments(xmlPath);
            });
            services.Configure<WebEncoderOptions>(options =>
            {
                options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All);
            }); //解决razor视图中中文被编码的问题
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Latest).AddControllersAsServices();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger().UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"{Configuration["Swagger:VirtualPath"]}/swagger/v1/swagger.json", "断点续传和多线程下载测试站点");
            });
            app.UseRouting(); // 放在 UseStaticFiles 之后
            app.UseEndpoints(endpoints =>
           {
               endpoints.MapControllers(); // 属性路由
               endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}"); // 默认路由
           });
        }
    }
}