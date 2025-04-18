using System.Threading;
using System.Threading.Tasks;

namespace EasyIotSharp.DataProcessor.Processing.Interfaces
{
    /// <summary>
    /// 消息处理器接口
    /// </summary>
    public interface IMessageProcessor
    {
        /// <summary>
        /// 启动消息处理
        /// </summary>
        Task StartProcessingAsync(CancellationToken cancellationToken);
        
        /// <summary>
        /// 停止消息处理
        /// </summary>
        Task StopProcessingAsync();
        
        /// <summary>
        /// 处理单条消息
        /// </summary>
        Task ProcessMessageAsync(string projectId, string message);
        
        /// <summary>
        /// 已处理消息数量
        /// </summary>
        long ProcessedCount { get; }
    }
}