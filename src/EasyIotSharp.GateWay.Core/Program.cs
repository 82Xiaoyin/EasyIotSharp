using EasyIotSharp.Core.Configuration;
using EasyIotSharp.GateWay.Core.Services;
using EasyIotSharp.GateWay.Core.Socket;
using EasyIotSharp.GateWay.Core.UI;
using EasyIotSharp.GateWay.Core.Util;
using EasyIotSharp.GateWay.Core.Util.Encrypotion;
using Microsoft.Extensions.Configuration;
using UPrime.Configuration;
using System;
using System.IO;
using System.Threading;
using UPrime;
using UPrime.Configuration;
using EasyIotSharp.Core.Extensions;
using log4net;

namespace EasyIotSharp.GateWay.Core
{
    public class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));
        static EasySocketBase easySocketBase = null;

        public static void Main(string[] args)
        {
            // 初始化 log4net
            var logRepository = LogManager.GetRepository(System.Reflection.Assembly.GetEntryAssembly());
            log4net.Config.XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
            
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
                   options.IocManager.AddAppOptions(appOptions);
               }
               ).Initialize();

            Logger.Info($"应用程序配置已加载，环境：{environment}");

            if (appOptions.SetMinThreads > 16)
            {
                ThreadPool.GetMinThreads(out int minWorker, out int minIOC);
                Logger.Info($"默认最小线程：worker:{minWorker} ioc: {minIOC}");

                if (ThreadPool.SetMinThreads(appOptions.SetMinThreads, minIOC))
                {
                    Logger.Info($"最小线程设置成功: worker = {appOptions.SetMinThreads}");
                }
                else
                {
                    Logger.Info($"最小线程数没有改变: worker = {minWorker}");
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
                    EasyIotSharp.GateWay.Core.LoadingConfig.RabbitMQ.RabbitMQConfig.CloseAllConnections();
                    Logger.Info("RabbitMQ连接已关闭");
                }
                catch (Exception ex)
                {
                    Logger.Error("关闭RabbitMQ连接异常", ex);
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
