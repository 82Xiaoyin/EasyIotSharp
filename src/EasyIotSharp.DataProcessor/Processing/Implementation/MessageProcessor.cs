using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Channels;
using EasyIotSharp.DataProcessor.Model.AnalysisDTO;
using EasyIotSharp.DataProcessor.Processing.Interfaces;
using EasyIotSharp.Core.Repositories.Influxdb;
using System.Linq;
using log4net;

namespace EasyIotSharp.DataProcessor.Processing.Implementation
{
    /// <summary>
    /// 消息处理器
    /// </summary>
    public class MessageProcessor : IMessageProcessor
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MessageProcessor));

        // 处理管道，用于异步处理消息队列
        private readonly Channel<ProcessingItem> _processingChannel;
        // 处理任务列表
        private readonly List<Task> _processingTasks = new List<Task>();
        // 已处理消息的总计数器
        private long _processedCount = 0;
        // 取消令牌源
        private CancellationTokenSource _cts;
        // 性能监控器
        private readonly IPerformanceMonitor _performanceMonitor;
        private readonly IDataRepository _dataRepository;
        private readonly IMqttService _mqttService;
        private readonly ISceneLinkageService _sceneLinkageService;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public MessageProcessor(
            IPerformanceMonitor performanceMonitor, 
            IDataRepository dataRepository,
            IMqttService mqttService,
            ISceneLinkageService sceneLinkageService)
        {
            _performanceMonitor = performanceMonitor;
            _dataRepository = dataRepository;
            _mqttService = mqttService;
            _sceneLinkageService = sceneLinkageService;
            
            // 创建处理管道，设置更大的容量提高吞吐量
            var options = new BoundedChannelOptions(200000)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = false,
                SingleWriter = false
            };
            _processingChannel = Channel.CreateBounded<ProcessingItem>(options);
        }
        
        /// <summary>
        /// 已处理消息数量
        /// </summary>
        public long ProcessedCount => Interlocked.Read(ref _processedCount);
        
        /// <summary>
        /// 启动消息处理
        /// </summary>
        public async Task StartProcessingAsync(CancellationToken cancellationToken)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            
            // 创建多个处理任务，增加并行度
            int processorCount = Environment.ProcessorCount * 4; // 增加处理线程数
            
            for (int i = 0; i < processorCount; i++)
            {
                _processingTasks.Add(ProcessDataAsync(_cts.Token));
            }
            
            Logger.Info($"启动了 {processorCount} 个数据处理任务");
            
            await Task.WhenAny(Task.WhenAll(_processingTasks), Task.Delay(Timeout.Infinite, cancellationToken));
        }

        /// <summary>
        /// 停止消息处理
        /// </summary>
        public async Task StopProcessingAsync()
        {
            try
            {
                _cts?.Cancel();
                _processingChannel.Writer.Complete();
                
                if (_processingTasks.Count > 0)
                {
                    await Task.WhenAll(_processingTasks);
                }
                
                Logger.Info("消息处理器已停止");
            }
            catch (Exception ex)
            {
                Logger.Error("停止消息处理器时发生错误", ex);
            }
        }

        /// <summary>
        /// 处理单条消息
        /// </summary>
        public async Task ProcessMessageAsync(string projectId, string message)
        {
            try
            {
                await _processingChannel.Writer.WriteAsync(new ProcessingItem
                {
                    ProjectId = projectId,
                    RawMessage = message
                });
                Logger.Debug($"成功将消息写入Channel，项目ID: {projectId}");
            }
            catch (Exception ex)
            {
                Logger.Error("将消息写入Channel时发生错误", ex);
            }
        }

        /// <summary>
        /// 处理数据
        /// </summary>
        private async Task ProcessDataAsync(CancellationToken stoppingToken)
        {
            try
            {
                Logger.Info("数据处理任务已启动，等待消息...");
                
                var batch = new List<ProcessingItem>(100);
                var batchTimer = new System.Diagnostics.Stopwatch();
                batchTimer.Start();
                
                await foreach (var item in _processingChannel.Reader.ReadAllAsync(stoppingToken))
                {
                    try
                    {
                        batch.Add(item);
                        
                        if (batch.Count >= 100 || batchTimer.ElapsedMilliseconds > 1000)
                        {
                            await ProcessBatchAsync(batch);
                            
                            var count = Interlocked.Add(ref _processedCount, batch.Count);
                            _performanceMonitor.UpdateProcessedCount(count);
                            
                            Logger.Info($"已批量处理 {batch.Count} 条消息，总计: {count}");
                            
                            batch.Clear();
                            batchTimer.Restart();
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("添加消息到批处理时发生错误", ex);
                    }
                }
                
                if (batch.Count > 0)
                {
                    await ProcessBatchAsync(batch);
                    var count = Interlocked.Add(ref _processedCount, batch.Count);
                    _performanceMonitor.UpdateProcessedCount(count);
                    Logger.Info($"已处理剩余 {batch.Count} 条消息，总计: {count}");
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                Logger.Info("数据处理任务被取消");
            }
            catch (Exception ex)
            {
                Logger.Error("数据处理任务发生错误", ex);
            }
        }

        // 批量处理消息
        private async Task ProcessBatchAsync(List<ProcessingItem> batch)
        {
            foreach (var item in batch)
            {
                try
                {
                    await ProcessMessage(item.ProjectId, item.RawMessage);
                }
                catch (Exception ex)
                {
                    Logger.Error($"批处理中处理项目 {item.ProjectId} 的消息时发生错误", ex);
                }
            }
        }

        /// <summary>
        /// 处理消息
        /// </summary>
        private async Task ProcessMessage(string projectId, string message)
        {
            try
            {
                Logger.Debug($"开始处理消息，项目ID: {projectId}, 消息内容: {message.Substring(0, Math.Min(100, message.Length))}...");
                
                try
                {
                    bool isHighFrequency = IsHighFrequencyData(message);
                    Logger.Debug($"消息类型判断结果: {(isHighFrequency ? "高频数据" : "低频数据")}");
                    
                    if (isHighFrequency)
                    {
                        var data = HighFrequencyData.FromEncryptedString<SensorDataBase>(message);
                        Logger.Debug($"成功解析高频数据，项目ID: {projectId}");
                    }
                    else
                    {
                        Logger.Debug($"尝试解析低频数据，项目ID: {projectId}");
                        var data = LowFrequencyData.FromEncryptedString<LowFrequencyData>(message);
                        Logger.Debug($"成功解析低频数据，项目ID: {projectId}, 测点类型: {data.PointType}, 测点数量: {data.Points?.Count ?? 0}");
                        
                        var measurementName = $"raw_{data.PointType}";
                        var dataPoints = data.Points.Select(point =>
                        {
                            var dynamicData = new Dictionary<string, object>
                            {
                                ["projectId"] = projectId,
                                ["pointType"] = data.PointType,
                                ["pointId"] = point.PointId,
                                ["time"] = data.Time
                            };

                            foreach (var metric in point.Values)
                            {
                                dynamicData[metric.Name] = metric.Value;
                            }

                            return dynamicData;
                        }).ToList();
                        
                        var tasks = new List<Task>
                        {
                            _dataRepository.SaveDataPointsAsync(measurementName, data.TenantAbbreviation, dataPoints),
                            _mqttService.PublishBatchDataAsync(projectId, dataPoints),
                            _sceneLinkageService.ProcessSceneLinkageAsync(projectId, dataPoints,data.TenantAbbreviation)
                        };
                        
                        await Task.WhenAll(tasks);
                        
                        Logger.Debug($"成功完成数据处理：入库、MQTT推送和场景联动，共 {dataPoints.Count} 个数据点");
                    }
                }
                catch (Exception parseEx)
                {
                    Logger.Error($"项目 {projectId} 的消息解析失败", parseEx);
                    SaveFailedMessage(projectId, message);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("处理消息时发生错误", ex);
            }
        }

        /// <summary>
        /// 判断是否为高频数据
        /// </summary>
        private bool IsHighFrequencyData(string message)
        {
            return message.Contains("\"dataType\":\"high\"") || 
                   message.Contains("\"type\":\"high\"") || 
                   message.Contains("\"samplingRate\":");
        }

        /// <summary>
        /// 保存解析失败的消息
        /// </summary>
        private void SaveFailedMessage(string projectId, string message)
        {
            try
            {
                string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FailedMessages");
                string filePath = Path.Combine(baseDir, $"{projectId}.json");
                
                Directory.CreateDirectory(baseDir);
                
                lock (projectId.GetHashCode().ToString())
                {
                    using (StreamWriter writer = File.AppendText(filePath))
                    {
                        writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}|{message}");
                    }
                }
                
                Logger.Debug($"已保存解析失败的消息到: {filePath}");
            }
            catch (Exception ex)
            {
                Logger.Error("保存解析失败的消息时发生错误", ex);
            }
        }
    }

    /// <summary>
    /// 处理项
    /// </summary>
    public class ProcessingItem
    {
        /// <summary>
        /// 项目ID
        /// </summary>
        public string ProjectId { get; set; }
        
        /// <summary>
        /// 原始消息
        /// </summary>
        public string RawMessage { get; set; }
    }
}