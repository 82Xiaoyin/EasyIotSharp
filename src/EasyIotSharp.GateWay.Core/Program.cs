using EasyIotSharp.Core.Configuration;
using EasyIotSharp.GateWay.Core.Services;
using EasyIotSharp.GateWay.Core.Socket;
using EasyIotSharp.GateWay.Core.UI;
using EasyIotSharp.GateWay.Core.Util;
using EasyIotSharp.GateWay.Core.Util.Encrypotion;
using Microsoft.Extensions.Configuration;
using UPrime.Configuration;
using Serilog;
using System;
using System.IO;
using System.Threading;
using UPrime;
using UPrime.Castle.Log4Net;
using UPrime.Configuration;
using EasyIotSharp.Core;

namespace EasyIotSharp.GateWay.Core
{
    class Program
    {
        static EasySocketBase easySocketBase = null;

        public static void Main(string[] args)
        {
            string environment = GetEnv(args);
            var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddYamlFile("appsettings.yml", optional: true, reloadOnChange: true)
                    .AddYamlFile($"appsettings.{environment}.yml", optional: true, reloadOnChange: true)
                    .AddCommandLine(args)
                    .Build();
            var appOptions = AppOptions.ReadFromConfiguration(config);
            UPrimeStarter.Create<GateWayCoreModule>(
               (options) =>
               {
                   options.IocManager.IocContainer.AddFacility<LoggingFacility>(f => f.UseUpLog4Net().WithConfig("log4net.config"));
                   options.IocManager.AddAppOptions(appOptions);
               }
               ).Initialize();

            var configDictionary = config.ToDictionary("Serilog");
            var loggerConfig = new LoggerConfiguration().ReadFrom.Configuration(config);
            Log.Logger = loggerConfig.CreateLogger();
            Log.Information("Settings {@Settings}", configDictionary);

            if (appOptions.SetMinThreads > 16)
            {
                //https://stackexchange.github.io/StackExchange.Redis/Timeouts
                //https://docs.microsoft.com/en-us/dotnet/api/system.threading.threadpool.setminthreads?view=netcore-2.0#System_Threading_ThreadPool_SetMinThreads_System_Int32_System_Int32_
                ThreadPool.GetMinThreads(out int minWorker, out int minIOC);
                Log.Information($"默认最小线程：worker:{minWorker} ioc: {minIOC}");

                if (ThreadPool.SetMinThreads(appOptions.SetMinThreads, minIOC))
                {
                    Log.Information($"最小线程设置成功: worker = {appOptions.SetMinThreads}");
                }
                else
                {
                    Log.Information($"最小线程数没有改变: worker = {minWorker}");
                }
            }


            try
            {
                AppDomain.CurrentDomain.ProcessExit += AppDomain_ProcessExit;
                
                ConsoleUI.ShowBanner();
                
                var serviceManager = new GateWayService();
                serviceManager.InitializeServices();
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
                    EasyIotSharp.GateWay.Core.LoadingConfig.RabbitMQ.RabbitMQConfig.CloseAllConnections();
                    LogHelper.Info("RabbitMQ连接已关闭");
                }
                catch (Exception ex)
                {
                    LogHelper.Error($"关闭RabbitMQ连接异常: {ex.Message}");
                }
                
                // 其他资源清理...
                
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
