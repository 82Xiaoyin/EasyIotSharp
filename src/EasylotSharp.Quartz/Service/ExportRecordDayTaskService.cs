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
using EasyIotSharp.Core.Domain.Export;
using Microsoft.Extensions.Primitives;
using System.Linq;
using EasyIotSharp.Core.Services.Rule.Impl;
using EasyIotSharp.Core.Dto.Rule.Params;
using EasylotSharp.Quartz.Util.Export;
using Nest;
using EasyIotSharp.Core.Services.Rule;
using EasyIotSharp.Core.Dto.Export.Params;
using System.IO;
using EasyIotSharp.Core.Dto.File.Params;
using Microsoft.AspNetCore.Http;

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
            var sensorQService = UPrimeEngine.Instance.Resolve<ISensorService>();
            var sensorPointService = UPrimeEngine.Instance.Resolve<ISensorPointService>();
            var projectBaseService = UPrimeEngine.Instance.Resolve<IProjectBaseService>();
            var alarmsService = UPrimeEngine.Instance.Resolve<IAlarmsService>();
            var exportReportService = UPrimeEngine.Instance.Resolve<IExportReportService>();
            var sensorList = sensorQService.GetSensorList();
            var sensorPointList = sensorPointService.GetBySensorPointList();
            foreach (var tenantItem in tenant)
            {
                var projectList = await projectBaseService.GetProjectBaseDtos(tenantItem.NumId);
                var sensors = sensorList.Where(w => w.TenantNumId == tenantItem.NumId).ToList();
                foreach (var project in projectList)
                {
                    var name = project.Name + DateTime.Now.ToString("yyyy年MM月dd日");
                    StringBuilder sb = new StringBuilder();
                    sb.Append($"项目:{project.Name}");
                    var model = new AlarmsInput();
                    model.IsPage = false;
                    model.IsSort = true;
                    model.ProjectId = project.Id;
                    model.Abbreviation = tenantItem.Abbreviation;

                    model.StartTime = DateTime.Now.Date.AddDays(1).AddHours(8);
                    model.StartTime = DateTime.Now.Date.AddDays(-1).AddHours(8);
                    ////真实使用数据代码
                    //model.StartTime = DateTime.Now.Date.AddHours(8);
                    //model.StartTime = DateTime.Now.Date.AddDays(-1).AddHours(8);
                    var alarmData = await alarmsService.GetAlarmsData(model);

                    foreach (var item in sensorList)
                    {
                        sb.Append($"测点名称:{item.Name}");
                        var sensorPointIds = sensorPointList.Where(w => w.ProjectId == project.Id && w.SensorId == item.Id).Select(s => s.Id).ToList();
                        var count = alarmData.Where(w => sensorPointIds.Contains(w.pointid)).Count();

                        sb.Append($"告警数量为:{count}个 \n");

                    }

                    // 生成PDF并保存到文件
                    var memoryStream = PDFHelper.GenerateSimplePdf(sb.ToString(), name);
                    // 定义保存路径
                    string savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ExportFiles", $"{name}.pdf");

                    // 确保目录存在
                    Directory.CreateDirectory(Path.GetDirectoryName(savePath));

                    // 将MemoryStream保存为文件
                    using (var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write))
                    {
                        memoryStream.Position = 0;
                        memoryStream.CopyTo(fileStream);
                    }

                    // 准备资源模型
                    var resourceModel = new ResourceInsert
                    {
                        Abbreviation = tenantItem.Abbreviation,
                        Name = name,
                        Remark = "任务生成"
                    };


                    var resourceId = await UploadZipFile(savePath, resourceModel, resourceService);

                    var insertexportReport = new ExportReportInsert
                    {
                        ExecuteTime = DateTime.Now,
                        Type = 0,
                        Name = name + ".pdf", // 添加.pdf后缀
                        ProjectId = project.Id,
                        State = 1,
                        ResourceId = resourceId
                    };
                    await exportReportService.CreateExportReport(insertexportReport);
                }


            }
        }
        private async Task<string> UploadZipFile(string pdfPath, ResourceInsert model, IResourceService resourceService)
        {
            using (FileStream fileStream = new FileStream(pdfPath, FileMode.Open, FileAccess.Read))
            {
                FormFile formFile = new FormFile(
                    baseStream: fileStream,
                    baseStreamOffset: 0,
                    length: fileStream.Length,
                    name: "file",
                    fileName: Path.GetFileName(pdfPath))
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "application/zip"
                };

                model.FormFile = formFile;
                return await resourceService.UploadResponseSensor(model);
            }
        }
    }
}
