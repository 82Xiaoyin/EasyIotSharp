using EasyIotSharp.Core.Configuration;
using Microsoft.Extensions.Configuration;
using UPrime.Configuration;
using Serilog;
using System;
using System.IO;
using System.Threading;
using UPrime;
using EasyIotSharp.Core.Extensions;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
using EasylotSharp.Quartz.Service;
using log4net.Config;
using System.Reflection;
using log4net;

namespace EasylotSharp.Quartz
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // 添加 log4net 配置初始化
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
            
            //var builder = WebApplication.CreateBuilder(args);

            //// 注册 Quartz 服务
            //builder.Services.AddQuartz(q =>
            //{
            //    q.UseMicrosoftDependencyInjectionJobFactory();

            //    var jobKey = new JobKey("MyJob");
            //    q.AddJob<MyJob>(jobKey, j => j.WithDescription("A simple job"));

            //    q.AddTrigger(t => t
            //        .ForJob(jobKey)
            //        .WithCronSchedule("0/5 * * * * ?") // 每5秒执行一次
            //    );
            //});

            //builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

            //// 注册服务（如日志、数据库访问等）
            //builder.Services.AddSingleton<ILogger, ConsoleLogger>();

            //var app = builder.Build();
            //app.Run();

            string environment = GetEnv(args);
            var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddYamlFile("appsettings.yml", optional: true, reloadOnChange: true)
                    .AddYamlFile($"appsettings.{environment}.yml", optional: true, reloadOnChange: true)
                    .AddCommandLine(args)
                    .Build();

            var appOptions = AppOptions.ReadFromConfiguration(config);
            UPrimeStarter.Create<QuartzModule>(
               (options) =>
               {
                   options.IocManager.AddAppOptions(appOptions);
               }
               ).Initialize();

            var configDictionary = config.ToDictionary("Serilog");
            var loggerConfig = new LoggerConfiguration().ReadFrom.Configuration(config);
            Log.Logger = loggerConfig.CreateLogger();
            Log.Information("Settings {@Settings}", configDictionary);

            // 创建调度器工厂
            ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
            IScheduler scheduler = await schedulerFactory.GetScheduler();

            // 创建作业
            IJobDetail exportRecordTask = JobBuilder.Create<ExportRecordTaskServce>()
                .Build();
            IJobDetail exportRecordDayTask = JobBuilder.Create<ExportRecordDayTaskService>()
                .Build();

            // 使用 Cron 表达式定义触发器（每分钟执行一次）
            ITrigger trigger = TriggerBuilder.Create()
                .WithCronSchedule("* * * * * ?") // 每分钟执行一次
                .Build();

            // 调度任务
            await scheduler.ScheduleJob(exportRecordDayTask, trigger);
            //await scheduler.ScheduleJob(exportRecordTask, trigger);

            // 启动调度器
            await scheduler.Start();

            Console.WriteLine("按任意键退出...");
            Console.ReadKey();

            // 停止调度器
            await scheduler.Shutdown();
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