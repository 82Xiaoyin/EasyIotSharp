using EasyIotSharp.Infrastructure.Database;
using EasyIotSharp.Infrastructure.Extensions;
using EasyIotSharp.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyIotSharp.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            // ���� AppSettings ���õ���̬�� Config����� Config ����Ӧ�������Ƶ��߼���
            Config.AppSettings = Configuration.GetSection("AppSettings").Get<Dictionary<string, string>>();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // ע�����֧��
            services.AddCustomCors();
            // ע�� Swagger
            services.AddCustomSwagger();
            // ��ȡ�����ַ���
            string dbConnectionString = Config.AppSettings["ConnectionStrings"];
            // ע�� SqlSugarDbContext
            services.AddSqlSugar(dbConnectionString);
            // ע�� CodeFirstInitializer
            services.AddSingleton<CodeFirstInitializer>();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, CodeFirstInitializer initializer)
        {    // ��ʼ�����ݿ�����÷�װ�õķ�����
            initializer.InitializeTables();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // ʹ��ȫ���쳣�����м��
                app.UseMiddleware<ExceptionMiddleware>();
            }
            // ���� CORS
            app.UseCors("AllowAllOrigins");
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
