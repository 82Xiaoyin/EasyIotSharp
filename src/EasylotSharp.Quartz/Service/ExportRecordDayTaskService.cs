using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Dto.Export;
using EasyIotSharp.Core.Services.Export;
using System.Threading.Tasks;
using log4net;
using Quartz;
using UPrime;
using EasyIotSharp.Core.Repositories.Mysql;
using EasyIotSharp.Core.Services.Tenant;
using EasyIotSharp.Core.Domain.Tenant;
using EasyIotSharp.Core.Services.Hardware;
using EasyIotSharp.Core.Services.Project;
using EasyIotSharp.Core.Services.Files;

namespace EasylotSharp.Quartz.Service
{
    /// <summary>
    /// 导出日报
    /// </summary>
    [DisallowConcurrentExecution]
    public class ExportRecordDayTaskService : IJob
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
                Logger.Info($"当前时间：{DateTime.Now}，开始执行日报");
                var _tenantService = UPrimeEngine.Instance.Resolve<ITenantService>();
                var list = await _tenantService.GetTenantList();
                await HandleTenant(list);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"导出日报任务执行失败: {ex.Message}");
                Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                Logger.Error("导出任务执行失败", ex);
                throw new JobExecutionException("导出任务执行失败", ex, false);
            }
        }


        private async Task HandleTenant(List<Tenant> tenant)
        {
            var resourceService = UPrimeEngine.Instance.Resolve<IResourceService>();
            var sensorQuotaService = UPrimeEngine.Instance.Resolve<ISensorQuotaService>();
            var sensorPointService = UPrimeEngine.Instance.Resolve<ISensorPointService>();
            foreach (var tenantItem in tenant) 
            {

            }
        }
    }
}
