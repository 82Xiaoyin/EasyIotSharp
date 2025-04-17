namespace EasyIotSharp.DataProcessor.Processing.Interfaces
{
    /// <summary>
    /// 性能监控接口
    /// </summary>
    public interface IPerformanceMonitor
    {
        /// <summary>
        /// 启动监控
        /// </summary>
        void Start();
        
        /// <summary>
        /// 停止监控
        /// </summary>
        void Stop();
        
        /// <summary>
        /// 更新处理计数
        /// </summary>
        void UpdateProcessedCount(long count);
    }
}