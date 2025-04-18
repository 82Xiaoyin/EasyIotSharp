using System;
using System.Threading;
using EasyIotSharp.DataProcessor.Processing.Interfaces;
using EasyIotSharp.DataProcessor.Util;

namespace EasyIotSharp.DataProcessor.Processing.Implementation
{
    /// <summary>
    /// 性能监控器
    /// </summary>
    public class PerformanceMonitor : IPerformanceMonitor, IDisposable
    {
        // 上次统计时的消息计数
        private long _lastProcessedCount = 0;
        // 上次统计时间
        private DateTime _lastStatTime = DateTime.Now;
        // 性能监控定时器
        private Timer _performanceTimer;
        
        /// <summary>
        /// 启动监控
        /// </summary>
        public void Start()
        {
            _lastStatTime = DateTime.Now;
            _lastProcessedCount = 0;
            
            // 启动性能监控，每3秒执行一次
            _performanceTimer = new Timer(MonitorPerformance, null, TimeSpan.Zero, TimeSpan.FromSeconds(3));
            
            LogHelper.Info("性能监控已启动");
        }
        
        /// <summary>
        /// 停止监控
        /// </summary>
        public void Stop()
        {
            _performanceTimer?.Dispose();
            _performanceTimer = null;
            
            LogHelper.Info("性能监控已停止");
        }
        
        /// <summary>
        /// 更新处理计数
        /// </summary>
        public void UpdateProcessedCount(long count)
        {
            // 这个方法在高频调用时可能会导致性能问题，所以我们在MessageProcessor中限制了调用频率
        }
        
        /// <summary>
        /// 监控性能
        /// </summary>
        private void MonitorPerformance(object state)
        {
            try
            {
                var now = DateTime.Now;
                var interval = (now - _lastStatTime).TotalSeconds;
                var currentCount = GetCurrentProcessedCount();
                var processedSinceLastTime = currentCount - _lastProcessedCount;
                var throughput = processedSinceLastTime / interval;
                
                LogHelper.Info($"处理吞吐量: {throughput:F2} 条/秒, 总计: {currentCount} 条");
                Console.Title = $"EasyIotSharp数据处理器 - 吞吐量: {throughput:F2} 条/秒, 总计: {currentCount} 条";
                
                _lastStatTime = now;
                _lastProcessedCount = currentCount;
            }
            catch (Exception ex)
            {
                LogHelper.Error($"监控性能时发生错误: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 获取当前处理计数
        /// </summary>
        private long GetCurrentProcessedCount()
        {
            // 这里需要从MessageProcessor获取计数，可以通过依赖注入或事件来实现
            // 简化起见，我们暂时返回0，实际实现时需要修改
            return 0;
        }
        
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Stop();
        }
    }
}