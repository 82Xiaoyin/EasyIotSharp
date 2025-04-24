using EasyIotSharp.DataProcessor.Util;
using System;
using System.Threading;
using EasyIotSharp.DataProcessor.Processing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using EasyIotSharp.DataProcessor.Processing.Interfaces;
using EasyIotSharp.DataProcessor.Processing.Implementation;
using UPrime;
using EasyIotSharp.Core.Configuration;
using System.IO;
using UPrime.Configuration;
using Serilog; // 添加这个命名空间
using EasyIotSharp.Core.Extensions;
namespace EasyIotSharp.DataProcessor
{
    class Program
    {
        private static DataProcessingService _dataProcessingService;
        public static void Main(string[] args)
        {
            try
            {

                string environment = GetEnv(args);
                var config = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddYamlFile("appsettings.yml", optional: true, reloadOnChange: true)
                        .AddYamlFile($"appsettings.{environment}.yml", optional: true, reloadOnChange: true)
                        .AddCommandLine(args)
                        .Build();
                var appOptions = AppOptions.ReadFromConfiguration(config);
                UPrimeStarter.Create<DataProcessorModule>(
                   (options) =>
                   {
                       options.IocManager.AddAppOptions(appOptions);
                   }
                   ).Initialize();

                var configDictionary = config.ToDictionary("Serilog");
                var loggerConfig = new LoggerConfiguration().ReadFrom.Configuration(config);
                Log.Logger = loggerConfig.CreateLogger();
                Log.Information("Settings {@Settings}", configDictionary);
                AppDomain.CurrentDomain.ProcessExit += AppDomain_ProcessExit;

                ConsoleUI.ShowBanner();
                LogHelper.Info("正在启动EasyIotSharp数据处理器...");

                // 创建主机
                var host = CreateHostBuilder(args).Build();
                // 初始化RabbitMQ
                ConsoleUI.ShowProgress("正在初始化RabbitMQ服务...", () =>
                {
                   EasyIotSharp.DataProcessor.LoadingConfig.RabbitMQ.RabbitMQConfig.InitMQ();
                    LogHelper.Info("RabbitMQ初始化成功");
                });
                // 获取数据处理服务
                _dataProcessingService = host.Services.GetRequiredService<DataProcessingService>();
                
                // 启动主机
                host.RunAsync();
                
                ConsoleUI.ShowSuccess("EasyIotSharp数据处理器已启动，按Ctrl+C退出");
                
                var exitEvent = new ManualResetEvent(false);
                Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = true;
                    Console.WriteLine("正在停止服务...");
                    LogHelper.Info("收到停止信号，正在停止服务...");
                    exitEvent.Set();
                };

                exitEvent.WaitOne();
                CleanupResources();
            }
            catch (Exception ex)
            {
                LogHelper.Error($"程序启动异常: {ex.ToString()}");
                ConsoleUI.ShowError($"程序启动异常: {ex.Message}");
            }
        }

        // 创建主机构建器
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseConsoleLifetime()
                .ConfigureServices((hostContext, services) =>
                {
                    // 注册数据处理服务
                    services.AddSingleton<DataProcessingService>();
                    services.AddHostedService(provider => provider.GetRequiredService<DataProcessingService>());
                    // 添加以下代码到服务注册部分
                    services.AddSingleton<IMessageReceiver, RabbitMQMessageReceiver>();
                    services.AddSingleton<IMessageProcessor, MessageProcessor>();
                    services.AddSingleton<IPerformanceMonitor, PerformanceMonitor>();
                    services.AddHostedService<DataProcessingService>();
                })
                .ConfigureLogging((context, logging) =>
                {
                    logging.ClearProviders();  // 清除所有默认的日志提供程序
                    logging.AddConsole();      // 只添加控制台日志
                });
              

        // 在应用程序退出时关闭RabbitMQ连接
        static void AppDomain_ProcessExit(object sender, EventArgs e)
        {
            LogHelper.Info("应用程序正在退出，清理资源...");
            CleanupResources();
        }

        // 清理资源
        static void CleanupResources()
        {
            try
            {
                // 关闭RabbitMQ连接
                try
                {
                    _dataProcessingService?.StopAsync(CancellationToken.None).Wait();
                    LogHelper.Info("数据处理服务已停止");
                }
                catch (Exception ex)
                {
                    LogHelper.Error($"停止数据处理服务异常: {ex.Message}");
                }

                LogHelper.Info("所有资源已清理完毕");
            }
            catch (Exception ex)
            {
                LogHelper.Error($"资源清理异常: {ex.ToString()}");
            }
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
    
