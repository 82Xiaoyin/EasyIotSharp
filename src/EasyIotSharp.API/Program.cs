using Castle.Facilities.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;
using UPrime;
using UPrime.Castle.Log4Net;
using UPrime.Configuration;
using EasyIotSharp.Core.Configuration;
using EasyIotSharp.Core.Extensions;
using EasyIotSharp.Core;
namespace EasyIotSharp.API
{
    /// <summary>
    /// 应用启动程序入口
    ///    - 通过环境获取并初始化应用配置项
    ///    - 初始化UPrime引擎
    ///    - 初始化Serilog
    ///    - 创建应用程序
    /// Serilog: https://github.com/serilog/serilog
    /// </summary>
    public class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                CreateHostBuilder(args).Build().Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "program terminated unexpectedly.");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
                Log.Information($"program has closed.");
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            string environment = GetEnv(args);
            var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddYamlFile("appsettings.yml", optional: true, reloadOnChange: true)
                    .AddYamlFile($"appsettings.{environment}.yml", optional: true, reloadOnChange: true)
                    .AddCommandLine(args)
                    .Build();

            return CreateHostBuilder(config, environment);
        }

        public static IHostBuilder CreateHostBuilder(IConfiguration config, string environment)
        {
            var appOptions = AppOptions.ReadFromConfiguration(config);
            UPrimeStarter.Create<EasyIotSharpAPICoreModule>((options) =>
            {
                options.IocManager.IocContainer.AddFacility<LoggingFacility>(f => f.UseUpLog4Net().WithConfig("log4net.config"));
                options.IocManager.AddAppOptions(appOptions);
            }).Initialize();

            var configDictionary = config.ToDictionary("Serilog");
            var loggerConfig = new LoggerConfiguration().ReadFrom.Configuration(config);
            Log.Logger = loggerConfig.CreateLogger();

            Log.Information("Settings {@Settings}", configDictionary);

            var builder = Host.CreateDefaultBuilder()
                 .UseEnvironment(environment)
                 .UseSerilog()
                 .ConfigureWebHostDefaults(webBuilder =>
                 {
                     webBuilder
                     .UseConfiguration(config)
                     .UseContentRoot(Directory.GetCurrentDirectory())
                      .UseUrls("http://*:5000")  // 添加此行，监听所有网络接口
                     .ConfigureKestrel(c =>
                     {
                         // kestrel server options：
                         // https://docs.microsoft.com/zh-cn/dotnet/api/microsoft.aspnetcore.server.kestrel.core.kestrelserveroptions?view=aspnetcore-3.1
                         c.AddServerHeader = false;
                         c.ListenAnyIP(5000); // 添加此行，明确指定监听所有IP
                         c.Limits.MaxRequestBodySize = 1000 * 1024 * 1024; // 上传文件大小设置为1G
                     })
                     .UseStartup<Startup>();
                 })
                 .ConfigureServices((ctx, services) =>
                 {
                     services.AddSingleton(config);
                     services.AddAppOptions(appOptions);
                     services.AddHttpContextAccessor();
                 });

            return builder;
        }

        private static string GetEnv(string[] args)
        {
            string env = "dev";
            if (args.Length > 0)
            {
                foreach (string argValue in args)
                {
                    if (argValue.Contains("--env"))
                    {
                        env = argValue.ReplaceByEmpty("--env=").Trim();
                    }
                }
            }
            return env;
        }
    }
}