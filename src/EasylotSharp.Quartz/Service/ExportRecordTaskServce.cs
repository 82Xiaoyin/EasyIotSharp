using System;
using System.Collections.Generic;
using System.Text;
using Quartz;
using System.Threading.Tasks;
using UPrime;
using UPrime.Events.Bus;
using Microsoft.Extensions.Logging;
using EasyIotSharp.Core.Services.Export.Impl;
using EasyIotSharp.Core.Services.Export;
using EasyIotSharp.Core.Dto.Export.Params;
using EasyIotSharp.Core.Dto.Export;
using EasyIotSharp.Core.Services.Hardware.Impl;
using EasyIotSharp.Core.Services.Hardware;
using System.Linq;
using System.IO;
using EasyIotSharp.Core.Dto.Hardware.Params;
using Microsoft.Extensions.Primitives;
using System.IO.Compression;
using EasyIotSharp.Core.Services.Project;
using Microsoft.AspNetCore.Http;
using EasyIotSharp.Core.Dto.File.Params;
using EasyIotSharp.Core.Services.Files;
using EasyIotSharp.Core.Services.Project.Impl;
using System.Data;
using MongoDB.Bson.IO;
using Minio.DataModel;
using log4net;

namespace EasylotSharp.Quartz.Service
{
    [DisallowConcurrentExecution]
    public class ExportRecordTaskServce : IJob
    {
        // 添加静态日志记录器
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// 导出任务执行入口
        /// </summary>
        /// <param name="context">任务执行上下文</param>
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                Console.WriteLine($"当前时间：{DateTime.Now}，开始执行导出任务");
                var exportRecordService = UPrimeEngine.Instance.Resolve<IExportRecordService>();

                // 查询待处理的导出记录
                var exportRecords = await exportRecordService.QueryExportRecord();
                if (exportRecords == null || exportRecords.Count == 0)
                {
                    Console.WriteLine("没有需要处理的导出数据");
                    return;
                }

                // 处理每条导出记录
                foreach (ExportRecordDto record in exportRecords)
                {
                    try
                    {
                        Console.WriteLine($"开始处理导出记录: {record.Name}");
                        record.ResourceId = await ProcessExportRecord(record);
                        record.State = 1; // 设置处理成功状态
                        await exportRecordService.UpdateExportRecord(record);
                        Console.WriteLine($"成功处理导出记录: {record.Name}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"处理导出记录失败: {record.Name}, 错误: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"导出任务执行失败: {ex.Message}");
                Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                throw new JobExecutionException("导出任务执行失败", ex, false);
            }
        }

        /// <summary>
        /// 处理单条导出记录
        /// </summary>
        /// <param name="record">导出记录数据</param>
        /// <returns>资源ID</returns>
        private async Task<string> ProcessExportRecord(ExportRecordDto record)
        {
            // 创建临时文件路径
            string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, record.Name);
            string zipFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{record.Name}.zip");

            try
            {
                // 初始化所需服务
                var resourceService = UPrimeEngine.Instance.Resolve<IResourceService>();
                var sensorQuotaService = UPrimeEngine.Instance.Resolve<ISensorQuotaService>();
                var sensorPointService = UPrimeEngine.Instance.Resolve<ISensorPointService>();

                // 准备资源模型
                var resourceModel = new ResourceInsert
                {
                    Name = record.Name,
                    Remark = "任务生成"
                };

                // 清理并创建临时目录
                await CleanupAndCreateDirectory(folderPath, zipFilePath);

                // 解析导出条件
                var exportItems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ExportDataInput>>(record.ConditionJson);
                if (exportItems == null || exportItems.Count == 0)
                {
                    throw new Exception("导出条件为空");
                }

                // 处理每个导出项
                foreach (var item in exportItems)
                {
                    await ProcessSensorData(item, folderPath, sensorQuotaService, sensorPointService, resourceModel);
                }

                // 创建ZIP文件
                ZipFile.CreateFromDirectory(folderPath, zipFilePath, CompressionLevel.Optimal, false);
                Console.WriteLine($"ZIP文件生成成功：{zipFilePath}");

                // 上传资源文件并返回资源ID
                return await UploadZipFile(zipFilePath, resourceModel, resourceService);
            }
            finally
            {
                await CleanupTempFiles(folderPath, zipFilePath);
            }
        }

        /// <summary>
        /// 处理传感器数据并生成CSV文件
        /// </summary>
        private async Task ProcessSensorData(
            ExportDataInput item,
            string folderPath,
            ISensorQuotaService sensorQuotaService,
            ISensorPointService sensorPointService,
            ResourceInsert resourceModel)
        {
            // 准备数据请求参数
            var dataRequest = CreateDataRequest(item);
            resourceModel.Abbreviation = item.Abbreviation;

            // 获取传感器数据
            var sensorData = await sensorQuotaService.GetSensorData(dataRequest);
            var sensorPoint = await sensorPointService.GetSensorPoint(item.SensorPointId);

            // 生成CSV内容
            string csvContent = GenerateCsvContent(sensorData);

            // 保存CSV文件
            await SaveCsvFile(sensorPoint.Name, folderPath, csvContent);
        }

        /// <summary>
        /// 创建数据请求对象
        /// </summary>
        private DataRespost CreateDataRequest(ExportDataInput item)
        {
            return new DataRespost
            {
                ProjectId = item.ProjectId,
                SensorId = item.SensorId,
                SensorQuotaId = item.SensorQuotaId,
                SensorPointId = item.SensorPointId,
                StartTime = item.StartTime,
                EndTime = item.EndTime,
                Abbreviation = item.Abbreviation
            };
        }

        /// <summary>
        /// 生成CSV内容
        /// </summary>
        private string GenerateCsvContent(dynamic sensorData)
        {
            StringBuilder sb = new StringBuilder();

            // 生成表头
            List<string> headers = new List<string>();
            foreach (var quota in sensorData.Quotas)
            {
                headers.Add(quota.Name.Replace(",", "，"));
            }
            sb.AppendLine(string.Join(",", headers));

            // 生成数据行
            string dataJson = Newtonsoft.Json.JsonConvert.SerializeObject(sensorData.Data);
            List<string[]> dataRows = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string[]>>(dataJson);

            foreach (string[] row in dataRows)
            {
                List<string> formattedRow = new List<string>();
                for (int i = 0; i < row.Length; i++)
                {
                    string value = row[i] ?? string.Empty;
                    if (i == 0 && !string.IsNullOrEmpty(value))
                    {
                        // 处理日期时间
                        DateTime dateTime = Convert.ToDateTime(value);
                        formattedRow.Add(dateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                    }
                    else
                    {
                        // 处理其他数据
                        formattedRow.Add(value.Replace(",", "，"));
                    }
                }
                sb.AppendLine(string.Join(",", formattedRow));
            }

            return sb.ToString();
        }

        /// <summary>
        /// 保存CSV文件
        /// </summary>
        private async Task SaveCsvFile(string fileName, string folderPath, string content)
        {
            string destinationPath = Path.Combine(folderPath, $"{fileName}.csv");
            await File.WriteAllTextAsync(destinationPath, content, Encoding.UTF8);
            Console.WriteLine($"CSV文件已生成: {destinationPath}");
        }

        /// <summary>
        /// 清理并创建目录
        /// </summary>
        private async Task CleanupAndCreateDirectory(string folderPath, string zipPath)
        {
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
            }
            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }
            Directory.CreateDirectory(folderPath);
            await Task.CompletedTask;
        }

        /// <summary>
        /// 上传ZIP文件
        /// </summary>
        private async Task<string> UploadZipFile(string zipPath, ResourceInsert model, IResourceService resourceService)
        {
            using (FileStream fileStream = new FileStream(zipPath, FileMode.Open, FileAccess.Read))
            {
                FormFile formFile = new FormFile(
                    baseStream: fileStream,
                    baseStreamOffset: 0,
                    length: fileStream.Length,
                    name: "file",
                    fileName: Path.GetFileName(zipPath))
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "application/zip"
                };

                model.FormFile = formFile;
                return await resourceService.UploadResponseSensor(model);
            }
        }

        /// <summary>
        /// 清理临时文件
        /// </summary>
        private async Task CleanupTempFiles(string folderPath, string zipPath)
        {
            try
            {
                if (File.Exists(zipPath))
                {
                    File.Delete(zipPath);
                    Console.WriteLine($"已删除临时ZIP文件: {zipPath}");
                }
                if (Directory.Exists(folderPath))
                {
                    Directory.Delete(folderPath, true);
                    Console.WriteLine($"已删除临时文件夹: {folderPath}");
                }
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"清理临时文件失败: {ex.Message}");
            }
        }
    }
}
