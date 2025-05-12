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
    /// 导出日报任务服务
    /// 用于定时生成和导出项目的日报告，包含告警统计信息
    /// </summary>
    [DisallowConcurrentExecution]
    public class ExportRecordDayTaskService : IJob
    {
        /// <summary>
        /// 日志记录器实例
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ExportRecordDayTaskService));

        /// <summary>
        /// 导出任务执行入口
        /// 处理所有租户的日报导出任务
        /// </summary>
        /// <param name="context">Quartz任务执行上下文</param>
        /// <returns>异步任务</returns>
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                Logger.Info($"当前时间：{DateTime.Now}，开始执行日报");
                var _tenantService = UPrimeEngine.Instance.Resolve<ITenantService>();
                Logger.Info("成功获取租户服务实例");
                
                var list = await _tenantService.GetTenantList();
                Logger.Info($"成功获取租户列表，共 {list?.Count ?? 0} 个租户");
                
                await HandleTenant(list);
                Logger.Info("日报导出任务执行完成");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"导出日报任务执行失败: {ex.Message}");
                Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                Logger.Error("导出任务执行失败", ex);
                throw new JobExecutionException("导出任务执行失败", ex, false);
            }
        }


        /// <summary>
        /// 处理租户数据
        /// 遍历所有租户和项目，生成并上传日报PDF文件
        /// </summary>
        /// <param name="tenant">租户列表</param>
        /// <returns>异步任务</returns>
        private async Task HandleTenant(List<Tenant> tenant)
        {
            Logger.Info("开始处理租户数据");
            // 初始化所需服务
            var resourceService = UPrimeEngine.Instance.Resolve<IResourceService>();        // 资源服务，用于文件上传
            var sensorQService = UPrimeEngine.Instance.Resolve<ISensorService>();          // 传感器服务
            var sensorPointService = UPrimeEngine.Instance.Resolve<ISensorPointService>(); // 测点服务
            var projectBaseService = UPrimeEngine.Instance.Resolve<IProjectBaseService>(); // 项目基础服务
            var alarmsService = UPrimeEngine.Instance.Resolve<IAlarmsService>();          // 告警服务
            var exportReportService = UPrimeEngine.Instance.Resolve<IExportReportService>(); // 导出报告服务

            // 获取传感器和测点列表
            var sensorList = sensorQService.GetSensorList();
            Logger.Info($"获取传感器列表成功，共 {sensorList?.Count ?? 0} 个传感器");
            
            var sensorPointList = sensorPointService.GetBySensorPointList();
            Logger.Info($"获取测点列表成功，共 {sensorPointList?.Count ?? 0} 个测点");

            try
            {
                // 遍历处理每个租户
                foreach (var tenantItem in tenant)
                {
                    Logger.Info($"开始处理租户：{tenantItem.Name}（{tenantItem.Abbreviation}）");
                    var projectList = await projectBaseService.GetProjectBaseDtos(tenantItem.NumId);
                    Logger.Info($"获取租户项目列表成功，共 {projectList?.Count ?? 0} 个项目");

                    foreach (var project in projectList)
                    {
                        Logger.Info($"开始处理项目：{project.Name}");
                        string pdfPath = null;
                        try
                        {
                            // 生成报告名称，格式：项目名称+日期
                            var name = project.Name + "日报" + DateTime.Now.ToString("yyyy年MM月dd日");
                            StringBuilder sb = new StringBuilder();
                            sb.AppendLine($"日报记录");
                            sb.AppendLine($"项目:{project.Name}");
                            sb.AppendLine();

                            // 设置告警查询参数
                            var model = new AlarmsInput
                            {
                                IsPage = false,
                                IsSort = true,
                                ProjectId = project.Id,
                                Abbreviation = tenantItem.Abbreviation,
                                StartTime = DateTime.Now.Date.AddDays(-1).AddHours(8), // 从昨天8点开始
                                EndTime = DateTime.Now.Date.AddHours(8)                // 到今天8点结束
                            };

                            // 获取告警数据
                            var alarmData = await alarmsService.GetAlarmsData(model);
                            Logger.Info($"获取告警数据成功，共 {alarmData?.Count() ?? 0} 条告警记录");

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

                            // 生成PDF并保存到临时文件
                            using var memoryStream = PDFHelper.GenerateSimplePdf(sb.ToString(), name);
                            Logger.Info($"PDF生成成功：{name}");
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
                                memoryStream.CopyTo(fileStream);
                            }

                            // 准备上传资源模型
                            var resourceModel = new ResourceInsert
                            {
                                Abbreviation = tenantItem.Abbreviation,
                                Name = name,
                                Remark = "日报生成"
                            };

                            // 上传PDF文件并获取资源ID
                            var resourceId = await UploadZipFile(pdfPath, resourceModel, resourceService);
                            Logger.Info($"PDF文件上传成功，资源ID：{resourceId}");

                            // 创建导出报告记录
                            var insertexportReport = new ExportReportInsert
                            {
                                ExecuteTime = DateTime.Now,
                                Type = 0,
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
                            throw;
                        }
                        finally
                        {
                            // 清理临时文件
                            if (!string.IsNullOrEmpty(pdfPath) && File.Exists(pdfPath))
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
                throw;
            }
        }

        /// <summary>
        /// 上传PDF文件到资源服务
        /// </summary>
        /// <param name="pdfPath">PDF文件路径</param>
        /// <param name="model">资源上传模型</param>
        /// <param name="resourceService">资源服务接口</param>
        /// <returns>上传后的资源ID</returns>
        private async Task<string> UploadZipFile(string pdfPath, ResourceInsert model, IResourceService resourceService)
        {
            Logger.Info($"开始上传文件：{pdfPath}");
            try
            {
                using (FileStream fileStream = new FileStream(pdfPath, FileMode.Open, FileAccess.Read))
                {
                    Logger.Info($"文件大小：{fileStream.Length} 字节");
                    // 创建表单文件对象
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
                    var result = await resourceService.UploadResponseSensor(model);
                    Logger.Info($"文件上传成功，返回资源ID：{result}");
                    return result;
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
