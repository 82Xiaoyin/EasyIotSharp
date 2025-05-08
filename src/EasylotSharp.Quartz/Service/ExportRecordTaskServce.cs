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

namespace EasylotSharp.Quartz.Service
{
    [DisallowConcurrentExecution] // 防止并发执行
    public class ExportRecordTaskServce : IJob
    {
        /// <summary>
        /// 导出数据
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                Console.WriteLine($"当前时间：{DateTime.Now}，执行 Cron 任务逻辑");
                var _exportRecordService = UPrimeEngine.Instance.Resolve<IExportRecordService>();

                var data = await _exportRecordService.QueryExportRecord();
                if (data == null || data.Count == 0)
                {
                    Console.WriteLine("不存在导出数据！");
                    return; // 直接返回，不需要 await Task.CompletedTask
                }

                foreach (ExportRecordDto dto in data)
                {
                    try
                    {
                        Console.WriteLine($"开始处理导出记录: {dto.Name}");
                        dto.ResourceId = await ProcessingExportRecord(dto);
                        dto.State = 1;
                        await _exportRecordService.UpdateExportRecord(dto);
                        Console.WriteLine($"成功处理导出记录: {dto.Name}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"处理导出记录失败: {dto.Name}, 错误: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"任务执行失败: {ex.Message}");
                Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                throw new JobExecutionException("导出任务执行失败", ex, false);
            }
        }

        /// <summary>
        /// 处理导出的数据
        /// </summary>
        /// <returns></returns>
        public async Task<string> ProcessingExportRecord(ExportRecordDto dto)
        {
            // 要创建的临时文件夹路径
            string folderPath = AppDomain.CurrentDomain.BaseDirectory + $@"\{dto.Name}";

            // 最终生成的 ZIP 文件路径
            string zipFilePath = AppDomain.CurrentDomain.BaseDirectory + $@"\{dto.Name}.zip";
            try 
            {
                var _resourceService = UPrimeEngine.Instance.Resolve<IResourceService>();
                var _sensorQuotaService = UPrimeEngine.Instance.Resolve<ISensorQuotaService>();
                var _sensorPointService = UPrimeEngine.Instance.Resolve<ISensorPointService>();
            
                var model = new ResourceInsert();
                model.Name = dto.Name;
                model.Remark = "任务生成";
            
                var list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ExportDataInput>>(dto.ConditionJson);

            
                // 检查并删除已存在的 ZIP 文件
                if (File.Exists(zipFilePath))
                {
                    File.Delete(zipFilePath);
                }
            
                // 1. 如果文件夹已存在，先删除（避免冲突）
                if (Directory.Exists(folderPath))
                {
                    Directory.Delete(folderPath, true);
                }
            
                // 2. 创建新文件夹
                Directory.CreateDirectory(folderPath);
            
                foreach (var item in list)
                {
                    int number = 0;
                    int strNumber = 0;
                    int sNumber = 0;
                    var dataRespost = new DataRespost();
                    dataRespost.ProjectId = item.ProjectId;
                    dataRespost.SensorId = item.SensorId;
                    dataRespost.SensorQuotaId = item.SensorQuotaId;
                    dataRespost.SensorPointId = item.SensorPointId;
                    dataRespost.StartTime = item.StartTime;
                    dataRespost.EndTime = item.EndTime;
                    dataRespost.Abbreviation = item.Abbreviation;
                    model.Abbreviation = item.Abbreviation;
            
                    var dt = await _sensorQuotaService.GetSensorData(dataRespost);
                    var sensorPoint = await _sensorPointService.GetSensorPoint(item.SensorPointId);
            
                    StringBuilder stringBuilder = new StringBuilder();
                    string tableName = string.Empty;
                    foreach (var name in dt.Quotas.Select(f => f.Name).ToList())
                    {
                        number++;
                        tableName += name;
                        tableName += ",";
                    }
                    stringBuilder.AppendLine(tableName);
                    var strdt = Newtonsoft.Json.JsonConvert.SerializeObject(dt.Data);
                    var strList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string[]>>(strdt);
            
                    foreach (var str in strList)
                    {
                        string table = string.Empty;
                        for (int i = 0; i < number; i++)
                        {
                            if (i == 0)
                            {
                                table += Convert.ToDateTime(str[i]).ToShortDateString();
                                table += ",";
                            }
                            else
                            {
                                table += str[i];
                                table += ",";
                            }
                        }
                        stringBuilder.AppendLine(table);
                    }
            
                    await GenerateCsv(sensorPoint.Name, folderPath, zipFilePath, stringBuilder.ToString());
                }
            
                // 5. 压缩整个文件夹为 ZIP 文件
                ZipFile.CreateFromDirectory(folderPath, zipFilePath, CompressionLevel.Optimal, false);
            
                Console.WriteLine($"ZIP 文件已成功生成：{zipFilePath}");
            
                // 使用 using 语句确保资源正确释放
                using (var stream = new FileStream(zipFilePath, FileMode.Open, FileAccess.Read))
                {
                    var formFile = new FormFile(
                        baseStream: stream,
                        baseStreamOffset: 0,
                        length: stream.Length,
                        name: "file",
                        fileName: Path.GetFileName(zipFilePath))
                    {
                        Headers = new HeaderDictionary(),
                        ContentType = "application/zip"
                    };
            
                    model.FormFile = formFile;
                    var resourceId = await _resourceService.UploadResponseSensor(model);
                    return resourceId;
                }
            }
            finally 
            {
                // 清理临时文件
                try
                {
                    if (File.Exists(zipFilePath))
                    {
                        File.Delete(zipFilePath);
                    }
                    if (Directory.Exists(folderPath))
                    {
                        Directory.Delete(folderPath, true);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"清理临时文件失败: {ex.Message}");
                }
            }
        }

        public async Task GenerateCsv(string csvFileName, string folderPath, string zipFilePath, string stringBuilder)
        {

            string csvFilePath = AppDomain.CurrentDomain.BaseDirectory + $@"{csvFileName}.csv";

            // 将字符串转为字节数组
            byte[] byteArray = Encoding.UTF8.GetBytes(stringBuilder);

            // 转为 MemoryStream
            using (MemoryStream memoryStream = new MemoryStream(byteArray))
            {
                // 可以写入文件
                using (FileStream fileStream = new FileStream(csvFilePath, FileMode.Create))
                {
                    memoryStream.CopyTo(fileStream);

                }
            }

            // 3. 构造目标 CSV 文件路径（放在文件夹内）
            string destinationCsvPath = Path.Combine(folderPath, Path.GetFileName(csvFilePath));

            // 4. 将 CSV 文件复制到文件夹中
            File.Copy(csvFilePath, destinationCsvPath);

            Console.WriteLine("CSV 文件已生成");
        }
    }
}
