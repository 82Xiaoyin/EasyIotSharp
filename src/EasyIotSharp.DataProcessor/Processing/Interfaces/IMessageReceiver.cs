using System;
using System.Threading.Tasks;

namespace EasyIotSharp.DataProcessor.Processing.Interfaces
{
    /// <summary>
    /// 消息接收器接口
    /// </summary>
    public interface IMessageReceiver
    {
        /// <summary>
        /// 初始化消息接收器
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// 关闭消息接收器
        /// </summary>
        Task Shutdown();

        /// <summary>
        /// 消息接收事件
        /// </summary>
        event EventHandler<MessageReceivedEventArgs> MessageReceived;
    }

    /// <summary>
    /// 消息接收事件参数
    /// </summary>
    public class MessageReceivedEventArgs : EventArgs
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