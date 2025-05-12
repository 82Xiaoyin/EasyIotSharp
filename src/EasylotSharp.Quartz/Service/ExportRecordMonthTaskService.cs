using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Services.Export;
using log4net;
using Quartz;
using System.Threading.Tasks;
using UPrime;

namespace EasylotSharp.Quartz.Service
{
    /// <summary>
    /// 导出月报
    /// </summary>
    [DisallowConcurrentExecution]
    public class ExportRecordMonthTaskService : IJob
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ExportRecordDayTaskService));

        /// <summary>
        /// 导出任务执行入口
        /// </summary>
        /// <param name="context">任务执行上下文</param>
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                Logger.Info($"当前时间：{DateTime.Now}，开始执行导出任务");
                var exportRecordService = UPrimeEngine.Instance.Resolve<IExportRecordService>();

                // 查询待处理的导出记录
                var exportRecords = await exportRecordService.QueryExportRecord();
                if (exportRecords == null || exportRecords.Count == 0)
                {
                    Console.WriteLine("没有需要处理的导出数据");
                    Logger.Info($"没有需要处理的导出数据");
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"导出任务执行失败: {ex.Message}");
                Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                Logger.Error("导出任务执行失败", ex);
                throw new JobExecutionException("导出任务执行失败", ex, false);
            }
        }
    }
}
