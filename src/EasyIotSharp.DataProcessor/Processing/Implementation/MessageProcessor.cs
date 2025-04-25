using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Channels;
using EasyIotSharp.DataProcessor.Model.AnalysisDTO;
using EasyIotSharp.DataProcessor.Processing.Interfaces;
using EasyIotSharp.DataProcessor.Util;
using EasyIotSharp.Core.Repositories.Influxdb;
using System.Linq;

namespace EasyIotSharp.DataProcessor.Processing.Implementation
{
    /// <summary>
    /// 消息处理器
    /// </summary>
    public class MessageProcessor : IMessageProcessor
    {
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
            
            LogHelper.Info($"启动了 {processorCount} 个数据处理任务");
            
            // 等待所有任务完成或取消
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
                
                // 等待所有处理任务完成
                if (_processingTasks.Count > 0)
                {
                    await Task.WhenAll(_processingTasks);
                }
                
                LogHelper.Info("消息处理器已停止");
            }
            catch (Exception ex)
            {
                LogHelper.Error($"停止消息处理器时发生错误: {ex.Message}");
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
                LogHelper.Debug($"成功将消息写入Channel，项目ID: {projectId}");
            }
            catch (Exception ex)
            {
                LogHelper.Error($"将消息写入Channel时发生错误: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 处理数据
        /// </summary>
        private async Task ProcessDataAsync(CancellationToken stoppingToken)
        {
            try
            {
                LogHelper.Info("数据处理任务已启动，等待消息...");
                
                // 使用批处理提高性能
                var batch = new List<ProcessingItem>(100);
                var batchTimer = new System.Diagnostics.Stopwatch();
                batchTimer.Start();
                
                await foreach (var item in _processingChannel.Reader.ReadAllAsync(stoppingToken))
                {
                    try
                    {
                        // 添加到批处理
                        batch.Add(item);
                        
                        // 当批处理达到一定大小或经过一定时间时处理批次
                        if (batch.Count >= 100 || batchTimer.ElapsedMilliseconds > 1000)
                        {
                            // 处理批次
                            await ProcessBatchAsync(batch);
                            
                            // 更新计数器
                            var count = Interlocked.Add(ref _processedCount, batch.Count);
                            _performanceMonitor.UpdateProcessedCount(count);
                            
                            LogHelper.Info($"已批量处理 {batch.Count} 条消息，总计: {count}");
                            
                            // 清空批次并重置计时器
                            batch.Clear();
                            batchTimer.Restart();
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error($"添加消息到批处理时发生错误: {ex.Message}");
                    }
                }
                
                // 处理剩余的批次
                if (batch.Count > 0)
                {
                    await ProcessBatchAsync(batch);
                    var count = Interlocked.Add(ref _processedCount, batch.Count);
                    _performanceMonitor.UpdateProcessedCount(count);
                    LogHelper.Info($"已处理剩余 {batch.Count} 条消息，总计: {count}");
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                LogHelper.Info("数据处理任务被取消");
            }
            catch (Exception ex)
            {
                LogHelper.Error($"数据处理任务发生错误: {ex.Message}");
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
                    LogHelper.Error($"批处理中处理项目 {item.ProjectId} 的消息时发生错误: {ex.Message}");
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
                // 记录原始消息内容，便于调试
                LogHelper.Debug($"开始处理消息，项目ID: {projectId}, 消息内容: {message.Substring(0, Math.Min(100, message.Length))}...");
                
                try
                {
                    // 简单判断是否为高频数据
                    bool isHighFrequency = IsHighFrequencyData(message);
                    LogHelper.Debug($"消息类型判断结果: {(isHighFrequency ? "高频数据" : "低频数据")}");
                    
                    if (isHighFrequency)
                    {
                        // 处理高频数据
                        var data = HighFrequencyData.FromEncryptedString<SensorDataBase>(message);
                        LogHelper.Debug($"成功解析高频数据，项目ID: {projectId}");
                    }
                    else
                    {
                        // 处理低频数据
                        LogHelper.Debug($"尝试解析低频数据，项目ID: {projectId}");
                        var data = LowFrequencyData.FromEncryptedString<LowFrequencyData>(message);
                        LogHelper.Debug($"成功解析低频数据，项目ID: {projectId}, 测点类型: {data.PointType}, 测点数量: {data.Points?.Count ?? 0}");
                        
                        // 创建测量名称
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

                            // 添加所有指标值
                            foreach (var metric in point.Values)
                            {
                                dynamicData[metric.Name] = metric.Value;
                            }

                            return dynamicData;
                        }).ToList(); // 转换为List以避免多次枚举
                        
                        // 并行执行三个操作：入库、MQTT推送、场景联动
                        var tasks = new List<Task>
                        {
                            // 1. 入库到InfluxDB
                            _dataRepository.SaveDataPointsAsync(measurementName, data.TenantAbbreviation, dataPoints),
                            
                            // 2. 推送到MQTT
                            _mqttService.PublishBatchDataAsync(projectId, dataPoints),
                            
                            // 3. 处理场景联动
                            _sceneLinkageService.ProcessSceneLinkageAsync(projectId, dataPoints)
                        };
                        
                        // 等待所有任务完成
                        await Task.WhenAll(tasks);
                        
                        LogHelper.Debug($"成功完成数据处理：入库、MQTT推送和场景联动，共 {dataPoints.Count} 个数据点");
                    }
                }
                catch (Exception parseEx)
                {
                    // 解析失败，记录详细错误信息
                    LogHelper.Error($"项目 {projectId} 的消息解析失败: {parseEx.Message}, 异常类型: {parseEx.GetType().Name}, 堆栈: {parseEx.StackTrace}");
                    
                    // 保存解析失败的消息
                    SaveFailedMessage(projectId, message);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error($"处理消息时发生错误: {ex.Message}, 异常类型: {ex.GetType().Name}, 堆栈: {ex.StackTrace}");
            }
        }
        
        /// <summary>
        /// 判断是否为高频数据
        /// </summary>
        private bool IsHighFrequencyData(string message)
        {
            // 简单判断方法：检查消息中是否包含高频数据的特征
            // 这里需要根据实际数据格式调整判断条件
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
                // 使用Path.Combine确保路径分隔符在不同操作系统下正确
                string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FailedMessages");
                string filePath = Path.Combine(baseDir, $"{projectId}.json");
                
                // 确保目录存在
                Directory.CreateDirectory(baseDir);
                
                // 使用文件锁避免并发写入问题
                lock (projectId.GetHashCode().ToString())
                {
                    // 追加写入消息，每条消息占一行
                    using (StreamWriter writer = File.AppendText(filePath))
                    {
                        writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}|{message}");
                    }
                }
                
                // 记录保存位置
                LogHelper.Debug($"已保存解析失败的消息到: {filePath}");
            }
            catch (Exception ex)
            {
                LogHelper.Error($"保存解析失败的消息时发生错误: {ex.Message}");
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