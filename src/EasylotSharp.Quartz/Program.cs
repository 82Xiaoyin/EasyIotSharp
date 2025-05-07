using System;
using Quartz.Impl;
using Quartz;
using System.Threading.Tasks;
using EasylotSharp.Quartz.Service;
using Microsoft.Extensions.DependencyInjection;
using EasyIotSharp.Core.Repositories.Mysql;
using EasylotSharp.Quartz.JobFactory;
using EasyIotSharp.Repositories.Mysql;
using EasyIotSharp.Core.Configuration;
using UPrime;
using EasyIotSharp.Core.Extensions;
using Microsoft.Extensions.Configuration;
using System.IO;
using UPrime.Configuration;
using Castle.Core.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Serilog;

namespace EasylotSharp.Quartz
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
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
            IJobDetail job = JobBuilder.Create<ExportRecordTaskServce>()
                .Build();

            // 使用 Cron 表达式定义触发器（每分钟执行一次）
            ITrigger trigger = TriggerBuilder.Create()
                .WithCronSchedule("* * * * * ?") // 每分钟执行一次
                .Build();

            // 调度任务
            await scheduler.ScheduleJob(job, trigger);

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