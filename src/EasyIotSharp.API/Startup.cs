using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Collections.Generic;
using System.IO;
using UPrime;
using EasyIotSharp.API.Filters;
using EasyIotSharp.Core.Configuration;
using EasyIotSharp.Core.Repositories.Tenant;
using EasyIotSharp.Core.Repositories.Tenant.Impl;
using EasyIotSharp.Core.Services.Tenant;
using EasyIotSharp.Core.Services.Tenant.Impl;
using EasyIotSharp.Core.Repositories.Mysql;
using EasyIotSharp.Repositories.Mysql;

namespace EasyIotSharp.API
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            app.Use(next => new RequestDelegate(
           async context =>
           {
               context.Request.EnableBuffering();
               await next(context);
           }));

            var appOptions = UPrimeEngine.Instance.Resolve<AppOptions>();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseCors($"{appOptions.Name}-policy");
            app.UseFileServer(new FileServerOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "StaticFiles")),
                RequestPath = "/static",
                EnableDirectoryBrowsing = false
            });
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}");
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", appOptions.Name);
                c.DefaultModelExpandDepth(2);
                c.DefaultModelRendering(ModelRendering.Model);
                c.DefaultModelsExpandDepth(-1);
                c.DocExpansion(DocExpansion.None);
            });
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var appOptions = UPrimeEngine.Instance.Resolve<AppOptions>();

            services.AddCors(options =>
            {
                options.AddPolicy($"{appOptions.Name}-policy",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                               .AllowAnyHeader()
                               .WithMethods("POST", "GET", "OPTIONS");
                    });
            });
            services.AddHttpClient();
            services.AddMvc(options =>
            {
                //filters
                options.Filters.Add<GlobalExceptionFilter>();
            }).AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });
            services.Configure<KestrelServerOptions>(x => x.AllowSynchronousIO = true)
                .Configure<IISServerOptions>(x => x.AllowSynchronousIO = true);

            //swagger
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = $"{appOptions.Name}：项目服务"
                });

                //添加Jwt验证设置,添加请求头信息
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Id = "Bearer",
                                Type = ReferenceType.SecurityScheme
                            }
                        },
                        new List<string>()
                    }
                });

                //放置接口Auth授权按钮
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Value Bearer {token}",
                    Name = "u-token",//jwt默认的参数名称
                    In = ParameterLocation.Header,//jwt默认存放Authorization信息的位置(请求头中)
                    Type = SecuritySchemeType.ApiKey
                });

                var basePath = Directory.GetCurrentDirectory();
                options.IncludeXmlComments(Path.Combine(basePath, "EasyIotSharp.Core.xml"));
                options.IncludeXmlComments(Path.Combine(basePath, "EasyIotSharp.API.xml"));
                options.DocumentFilter<Swagger_TagDesc>();
            });
        }
    }
}