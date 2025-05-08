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
using EasyIotSharp.Core.Extensions;
using System.Threading.Tasks;
using System.Linq;
using log4net;
using log4net.Config;
using System.Reflection;

namespace EasyIotSharp.DataProcessor
{
    class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));
        private static DataProcessingService _dataProcessingService;

        public static async Task Main(string[] args)
        {
            try
            {
                // 配置log4net
                var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
                var fileInfo = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.config"));
                XmlConfigurator.Configure(logRepository, fileInfo);

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

                Logger.Info($"应用程序配置已加载，环境：{environment}");
                AppDomain.CurrentDomain.ProcessExit += AppDomain_ProcessExit;

                ConsoleUI.ShowBanner();
                Logger.Info("正在启动EasyIotSharp数据处理器...");

                var host = CreateHostBuilder(args).Build();
                
                ConsoleUI.ShowProgress("正在初始化RabbitMQ服务...", async () =>
                {
                    await EasyIotSharp.DataProcessor.LoadingConfig.RabbitMQ.RabbitMQConfig.InitMQ();
                    Logger.Info("RabbitMQ初始化成功");
                });
                Thread.Sleep(10000);

                _dataProcessingService = host.Services.GetRequiredService<DataProcessingService>();
                await host.StartAsync();
                
                ConsoleUI.ShowSuccess("EasyIotSharp数据处理器已启动，按Ctrl+C退出");
                
                var exitEvent = new ManualResetEvent(false);
                Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = true;
                    Console.WriteLine("正在停止服务...");
                    Logger.Info("收到停止信号，正在停止服务...");
                    exitEvent.Set();
                };

                exitEvent.WaitOne();
                CleanupResources();
            }
            catch (Exception ex)
            {
                Logger.Error("程序启动异常", ex);
                ConsoleUI.ShowError($"程序启动异常: {ex.Message}");
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseConsoleLifetime()
                .ConfigureLogging(logging => 
                {
                    logging.ClearProviders();  // 清除默认的日志提供程序
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<DataProcessingService>();
                    services.AddHostedService(provider => provider.GetRequiredService<DataProcessingService>());
                    services.AddSingleton<IMessageReceiver, RabbitMQMessageReceiver>();
                    services.AddSingleton<IMessageProcessor, MessageProcessor>();
                    services.AddSingleton<IPerformanceMonitor, PerformanceMonitor>();
                    services.AddHostedService<DataProcessingService>();
                    services.AddSingleton<IDataRepository, InfluxDataRepository>();
                    services.AddSingleton<IMqttService, MqttService>();
                    services.AddSingleton<ISceneLinkageService, SceneLinkageService>();
                });

        static void AppDomain_ProcessExit(object sender, EventArgs e)
        {
            Logger.Info("应用程序正在退出，清理资源...");
            CleanupResources();
        }

        static void CleanupResources()
        {
            try
            {
                try
                {
                    _dataProcessingService?.StopAsync(CancellationToken.None).Wait();
                    Logger.Info("数据处理服务已停止");
                }
                catch (Exception ex)
                {
                    Logger.Error("停止数据处理服务异常", ex);
                }

                Logger.Info("所有资源已清理完毕");
            }
            catch (Exception ex)
            {
                Logger.Error("资源清理异常", ex);
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
    
