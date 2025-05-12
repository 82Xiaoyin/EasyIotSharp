using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Services.Export;
using log4net;
using Quartz;
using System.Threading.Tasks;
using UPrime;
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
    /// 导出月报任务服务
    /// 用于定时生成和导出项目的月度告警统计报告
    /// </summary>
    [DisallowConcurrentExecution]
    public class ExportRecordMonthTaskService : IJob
    {
        /// <summary>
        /// 日志记录器实例，用于记录任务执行过程中的日志信息
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ExportRecordMonthTaskService));

        /// <summary>
        /// 导出任务执行入口
        /// 负责获取所有租户信息并触发月报生成流程
        /// </summary>
        /// <param name="context">Quartz调度器提供的任务执行上下文</param>
        /// <returns>异步任务</returns>
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                Logger.Info($"当前时间：{DateTime.Now}，开始执行月报");
                var _tenantService = UPrimeEngine.Instance.Resolve<ITenantService>();
                Logger.Info("成功获取租户服务实例");

                var list = await _tenantService.GetTenantList();
                Logger.Info($"成功获取租户列表，共 {list?.Count ?? 0} 个租户");

                await HandleTenant(list);
                Logger.Info("月报导出任务执行完成");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"导出月报任务执行失败: {ex.Message}");
                Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                Logger.Error("导出任务执行失败", ex);
                throw new JobExecutionException("导出任务执行失败", ex, false);
            }
        }

        /// <summary>
        /// 处理租户数据
        /// 遍历所有租户和项目，为每个项目生成并上传月度告警统计报告
        /// </summary>
        /// <param name="tenant">待处理的租户列表</param>
        /// <returns>异步任务</returns>
        private async Task HandleTenant(List<Tenant> tenant)
        {
            // 初始化所需的各种服务实例
            var resourceService = UPrimeEngine.Instance.Resolve<IResourceService>();        // 资源服务，用于文件上传
            var sensorQService = UPrimeEngine.Instance.Resolve<ISensorService>();          // 传感器服务，获取传感器信息
            var sensorPointService = UPrimeEngine.Instance.Resolve<ISensorPointService>(); // 测点服务，获取测点信息
            var projectBaseService = UPrimeEngine.Instance.Resolve<IProjectBaseService>(); // 项目基础服务，获取项目信息
            var alarmsService = UPrimeEngine.Instance.Resolve<IAlarmsService>();          // 告警服务，获取告警数据
            var exportReportService = UPrimeEngine.Instance.Resolve<IExportReportService>(); // 导出报告服务，记录导出信息

            // 获取所有传感器和测点列表，用于后续的告警统计
            var sensorList = sensorQService.GetSensorList();
            var sensorPointList = sensorPointService.GetBySensorPointList();

            try
            {
                // 遍历处理每个租户的数据
                foreach (var tenantItem in tenant)
                {
                    Logger.Info($"开始处理租户：{tenantItem.Name}（{tenantItem.Abbreviation}）");
                    var projectList = await projectBaseService.GetProjectBaseDtos(tenantItem.NumId);
                    Logger.Info($"获取租户项目列表成功，共 {projectList?.Count ?? 0} 个项目");

                    foreach (var project in projectList)
                    {
                        string pdfPath = null;
                        try
                        {
                            // 生成报告文件名，格式：项目名称+年月
                            var name = project.Name + "月报" + DateTime.Now.AddMonths(-1).ToString("yyyy年MM月");
                            StringBuilder sb = new StringBuilder();
                            sb.AppendLine($"月报记录");
                            sb.AppendLine($"项目:{project.Name}");
                            sb.AppendLine();

                            // 计算上个月的时间范围，用于查询告警数据
                            var now = DateTime.Now;
                            var startTime = new DateTime(now.Year, now.Month, 1).AddMonths(-1).AddHours(8);  // 上月1号8点
                            var endTime = new DateTime(now.Year, now.Month, 1).AddHours(8).AddSeconds(-1);   // 本月1号8点前1秒

                            // 构建告警查询参数
                            var model = new AlarmsInput
                            {
                                IsPage = false,      // 不分页
                                IsSort = true,       // 启用排序
                                ProjectId = project.Id,
                                Abbreviation = tenantItem.Abbreviation,
                                StartTime = startTime,
                                EndTime = endTime
                            };

                            // 获取指定时间范围内的告警数据
                            var alarmData = await alarmsService.GetAlarmsData(model);

                            // 统计每个测点的告警数量
                            foreach (var item in sensorList)
                            {
                                sb.AppendLine($"测点名称:{item.Name}");
                                var sensorPointIds = sensorPointList
                                    .Where(w => w.ProjectId == project.Id && w.SensorId == item.Id)
                                    .Select(s => s.Id)
                                    .ToList();
                                var count = alarmData.Where(w => sensorPointIds.Contains(w.pointid)).Count();
                                sb.AppendLine($"告警数量为:{count}个");
                                sb.AppendLine();
                            }

                            // 生成PDF报告并保存到临时文件
                            using var memoryStream = PDFHelper.GenerateSimplePdf(sb.ToString(), name);

                            // 验证PDF生成结果
                            if (memoryStream == null || memoryStream.Length == 0)
                            {
                                throw new Exception($"生成PDF失败，项目：{project.Name}");
                            }

                            // 设置PDF文件保存路径
                            pdfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ExportFiles", $"{name}.pdf");
                            var directory = Path.GetDirectoryName(pdfPath);

                            // 确保导出目录存在
                            if (!Directory.Exists(directory))
                            {
                                Directory.CreateDirectory(directory);
                            }

                            // 将PDF内容写入文件
                            using (var fileStream = new FileStream(pdfPath, FileMode.Create, FileAccess.Write))
                            {
                                memoryStream.Position = 0;
                                await memoryStream.CopyToAsync(fileStream);
                            }
                            Logger.Info($"开始保存PDF文件到：{pdfPath}");
                            using (var fileStream = new FileStream(pdfPath, FileMode.Create, FileAccess.Write))
                            {
                                memoryStream.Position = 0;
                                await memoryStream.CopyToAsync(fileStream);
                            }
                            Logger.Info("PDF文件保存成功");

                            // 上传文件
                            var model2 = new ResourceInsert
                            {
                                Abbreviation = tenantItem.Abbreviation,
                                Name = name,
                                Remark = "月报自动生成"
                            };

                            // 上传PDF文件到资源服务
                            var resourceId = await UploadZipFile(pdfPath, model2, resourceService);
                            Logger.Info($"PDF文件上传成功，资源ID：{resourceId}");

                            // 创建导出报告记录
                            var insertexportReport = new ExportReportInsert
                            {
                                ExecuteTime = DateTime.Now,
                                Type = 1,
                                Name = name + ".pdf",
                                ProjectId = project.Id,
                                State = 1,
                                ResourceId = resourceId
                            };
                            await exportReportService.CreateExportReport(insertexportReport);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"处理项目 {project.Name} 时发生错误: {ex.Message}", ex);
                            continue;
                        }
                        finally
                        {
                            // 清理临时文件
                            if (pdfPath != null && File.Exists(pdfPath))
                            {
                                try
                                {
                                    File.Delete(pdfPath);
                                    Logger.Info($"已删除临时文件: {pdfPath}");
                                }
                                catch (Exception ex)
                                {
                                    Logger.Error($"删除临时文件失败: {pdfPath}, 错误: {ex.Message}");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("处理租户数据时发生错误", ex);
                throw new JobExecutionException("处理租户数据时发生错误", ex, false);
            }
        }

        /// <summary>
        /// 上传PDF文件到资源服务
        /// </summary>
        /// <param name="pdfPath">PDF文件的本地路径</param>
        /// <param name="model">资源上传参数模型</param>
        /// <param name="resourceService">资源服务接口</param>
        /// <returns>上传成功后返回的资源ID</returns>
        private async Task<string> UploadZipFile(string pdfPath, ResourceInsert model, IResourceService resourceService)
        {
            Logger.Info($"开始上传文件：{pdfPath}");
            try
            {
                using (FileStream fileStream = new FileStream(pdfPath, FileMode.Open, FileAccess.Read))
                {
                    Logger.Info($"文件大小：{fileStream.Length} 字节");
                    if (fileStream.Length == 0)
                    {
                        throw new JobExecutionException($"PDF文件为空: {pdfPath}", null, false);
                    }

                    FormFile formFile = new FormFile(
                        baseStream: fileStream,
                        baseStreamOffset: 0,
                        length: fileStream.Length,
                        name: "file",
                        fileName: Path.GetFileName(pdfPath))
                    {
                        Headers = new HeaderDictionary(),
                        ContentType = "application/pdf"
                    };

                    model.FormFile = formFile;
                    return await resourceService.UploadResponseSensor(model);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"文件上传失败：{ex.Message}", ex);
                throw;
            }
        }
    }
}
