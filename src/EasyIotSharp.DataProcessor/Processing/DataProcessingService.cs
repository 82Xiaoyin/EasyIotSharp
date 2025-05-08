using System;
using System.Threading;
using System.Threading.Tasks;
using EasyIotSharp.DataProcessor.Processing.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using log4net;

namespace EasyIotSharp.DataProcessor.Processing
{
    /// <summary>
    /// 数据处理服务
    /// </summary>
    public class DataProcessingService : BackgroundService
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(DataProcessingService));
        
        // 消息接收器
        private readonly IMessageReceiver _messageReceiver;
        // 消息处理器
        private readonly IMessageProcessor _messageProcessor;
        // 性能监控器
        private readonly IPerformanceMonitor _performanceMonitor;

        /// <summary>
        /// 构造函数
        /// </summary>
        public DataProcessingService(
            IMessageReceiver messageReceiver,
            IMessageProcessor messageProcessor,
            IPerformanceMonitor performanceMonitor)
        {
            _messageReceiver = messageReceiver;
            _messageProcessor = messageProcessor;
            _performanceMonitor = performanceMonitor;

            // 订阅消息接收事件
            _messageReceiver.MessageReceived += OnMessageReceived;
        }

        /// <summary>
        /// 消息接收事件处理
        /// </summary>
        private async void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Logger.Info($"收到项目 {e.ProjectId} 的消息，长度: {e.RawMessage?.Length ?? 0}");
            try
            {
                await _messageProcessor.ProcessMessageAsync(e.ProjectId, e.RawMessage);
                Logger.Debug($"已将项目 {e.ProjectId} 的消息传递给处理器");
            }
            catch (Exception ex)
            {
                Logger.Error($"处理项目 {e.ProjectId} 的消息时发生错误", ex);
            }
        }

        /// <summary>
        /// 执行服务
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                AppContext.SetSwitch("System.Diagnostics.EventLog.EnableLogToConsole", false);

                await _messageReceiver.InitializeAsync();
                _performanceMonitor.Start();
                await _messageProcessor.StartProcessingAsync(stoppingToken);

                Logger.Info("数据处理服务已启动");
            }
            catch (Exception ex)
            {
                Logger.Error("执行服务时发生错误", ex);
                throw;
            }
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                _performanceMonitor.Stop();
                await _messageProcessor.StopProcessingAsync();
                await _messageReceiver.Shutdown();
                _messageReceiver.MessageReceived -= OnMessageReceived;

                Logger.Info("数据处理服务已停止");
            }
            catch (Exception ex)
            {
                Logger.Error("停止服务时发生错误", ex);
            }

            await base.StopAsync(cancellationToken);
        }
    }
}