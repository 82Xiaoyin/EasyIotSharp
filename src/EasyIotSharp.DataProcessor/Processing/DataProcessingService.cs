using System;
using System.Threading;
using System.Threading.Tasks;
using EasyIotSharp.DataProcessor.Processing.Interfaces;
using EasyIotSharp.DataProcessor.Util;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace EasyIotSharp.DataProcessor.Processing
{
    /// <summary>
    /// 数据处理服务
    /// </summary>
    public class DataProcessingService : BackgroundService
    {
        // 配置接口
        private readonly IConfiguration _configuration;
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
            IConfiguration configuration,
            IMessageReceiver messageReceiver,
            IMessageProcessor messageProcessor,
            IPerformanceMonitor performanceMonitor)
        {
            _configuration = configuration;
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
            await _messageProcessor.ProcessMessageAsync(e.ProjectId, e.RawMessage);
        }
        
        /// <summary>
        /// 执行服务
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                AppContext.SetSwitch("System.Diagnostics.EventLog.EnableLogToConsole", false);
                
                // 初始化消息接收器
                _messageReceiver.Initialize();
                
                // 启动性能监控
                _performanceMonitor.Start();
                
                // 启动消息处理
                await _messageProcessor.StartProcessingAsync(stoppingToken);
                
                LogHelper.Info("数据处理服务已启动");
            }
            catch (Exception ex)
            {
                LogHelper.Error($"执行服务时发生错误: {ex.Message}");
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
                // 停止性能监控
                _performanceMonitor.Stop();
                
                // 停止消息处理
                await _messageProcessor.StopProcessingAsync();
                
                // 关闭消息接收器
                _messageReceiver.Shutdown();
                
                // 取消订阅事件
                _messageReceiver.MessageReceived -= OnMessageReceived;
                
                LogHelper.Info("数据处理服务已停止");
            }
            catch (Exception ex)
            {
                LogHelper.Error($"停止服务时发生错误: {ex.Message}");
            }
            
            await base.StopAsync(cancellationToken);
        }
    }
}