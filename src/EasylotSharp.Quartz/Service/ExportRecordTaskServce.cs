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
                // 执行实际的业务逻辑

                var data = await _exportRecordService.QueryExportRecord();
                if (data == null || data.Count == 0)
                {
                    Console.WriteLine("不存在导出数据！");
                    await Task.CompletedTask;
                }
                Console.WriteLine(data);
                // 处理数据...
                foreach (ExportRecordDto dto in data)
                {
                    dto.Id = await ProcessingExportRecord(dto);
                    dto.State = 1;
                    await _exportRecordService.UpdateExportRecord(dto);
                }

                await Task.CompletedTask;

            }
            catch (Exception ex)
            {
                throw; // 重新抛出异常，让 Quartz 知道任务失败
            }
        }

        /// <summary>
        /// 处理导出的数据
        /// </summary>
        /// <returns></returns>
        public async Task<string> ProcessingExportRecord(ExportRecordDto dto)
        {
            var _resourceService = UPrimeEngine.Instance.Resolve<IResourceService>();
            var _sensorQuotaService = UPrimeEngine.Instance.Resolve<ISensorQuotaService>();
            var _sensorPointService = UPrimeEngine.Instance.Resolve<ISensorPointService>();


            var list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ExportDataInput>>(dto.ConditionJson);

            // 要创建的临时文件夹路径
            string folderPath = AppDomain.CurrentDomain.BaseDirectory + $@"\{dto.Name}";

            // 最终生成的 ZIP 文件路径
            string zipFilePath = AppDomain.CurrentDomain.BaseDirectory + $@"\{dto.Name}.zip";

            // 1. 如果文件夹已存在，先删除（避免冲突）
            if (Directory.Exists(folderPath))
                Directory.Delete(folderPath, true);

            // 2. 创建新文件夹
            Directory.CreateDirectory(folderPath);

            foreach (var item in list)
            {
                var dataRespost = new DataRespost();
                dataRespost.ProjectId = item.ProjectId;
                dataRespost.SensorId = item.SensorId;
                dataRespost.SensorQuotaId = item.SensorQuotaId;
                dataRespost.SensorPointId = item.SensorPointId;
                dataRespost.StartTime = item.StartTime;
                dataRespost.EndTime = item.EndTime;
                dataRespost.Abbreviation = item.Abbreviation;

                var dt = await _sensorQuotaService.GetSensorData(dataRespost);
                var sensorPoint = await _sensorPointService.GetSensorPoint(item.SensorId);

                StringBuilder stringBuilder = new StringBuilder();
                foreach (var name in dt.Quotas.Select(f => f.Name).ToList())
                {
                    stringBuilder.Append(name);
                }
                stringBuilder.AppendLine("\n");

                stringBuilder.AppendLine(dt.Data.ToString());

                await GenerateCsv(sensorPoint.Name, folderPath, zipFilePath, stringBuilder.ToString());
            }

            // 5. 压缩整个文件夹为 ZIP 文件
            ZipFile.CreateFromDirectory(folderPath, zipFilePath, CompressionLevel.Optimal, false);

            Console.WriteLine($"ZIP 文件已成功生成：{zipFilePath}");

            //
            var formFile = await CreateIFormFileFromPath(zipFilePath);

            var model = new ResourceInsert();
            model.FormFile = formFile;
            model.Name = dto.Name;
            model.Remark = "任务生成";

            var resourceId = await _resourceService.UploadResponseSensor(model);

            return resourceId;

        }

        public async Task<IFormFile> CreateIFormFileFromPath(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("文件未找到", filePath);

            var fileName = Path.GetFileName(filePath);
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            var formFile = new FormFile(
                baseStream: fileStream,
                baseStreamOffset: 0,
                length: fileStream.Length,
                name: "file",
                fileName: fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/octet-stream"
            };

            return formFile;
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
