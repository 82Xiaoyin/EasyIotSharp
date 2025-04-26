using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyIotSharp.DataProcessor.Model.RaddbitDTO;
using EasyIotSharp.DataProcessor.Processing.Interfaces;
using EasyIotSharp.DataProcessor.Util;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EasyIotSharp.DataProcessor.Processing.Implementation
{
    /// <summary>
    /// RabbitMQ消息接收器
    /// </summary>
    public class RabbitMQMessageReceiver : IMessageReceiver 
    {
        // 存储项目ID和对应的RabbitMQ通道映射关系
        private readonly Dictionary<string, IChannel> _channels = new Dictionary<string, IChannel>();
        // 存储项目ID和对应的队列名称映射关系
        private readonly Dictionary<string, string> _queueNames = new Dictionary<string, string>();
        // 存储所有RabbitMQ客户端实例
        private readonly List<RabbitMQClient> _rabbitClients = new List<RabbitMQClient>();
        
        /// <summary>
        /// 消息接收事件
        /// </summary>
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
        
        /// <summary>
        /// 初始化消息接收器
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                // 遍历所有已配置的RabbitMQ客户端
                foreach (var kvp in EasyIotSharp.DataProcessor.LoadingConfig.RabbitMQ.RabbitMQConfig.dicPid2MQClient)
                {
                    string projectId = kvp.Key;
                    RabbitMQClient client = kvp.Value;
                    
                    LogHelper.Info($"正在为项目 {projectId} 初始化 RabbitMQ 客户端");
                    
                    // 为每个项目订阅队列
                    if (await SubscribeProjectQueueAsync(client, projectId, "queue_"))
                    {
                        _rabbitClients.Add(client);
                    }
                }
                
                LogHelper.Info("RabbitMQ消息接收器初始化完成");
            }
            catch (Exception ex)
            {
                LogHelper.Error($"初始化RabbitMQ消息接收器时发生错误: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// 订阅项目队列
        /// </summary>
        private async Task<bool> SubscribeProjectQueueAsync(RabbitMQClient client, string projectId, string queuePrefix)
        {
            try
            {
                // 确保客户端已初始化
                await client.InitAsync();
                
                // 构建队列名称并保存
                string queueName = $"{queuePrefix}{projectId}";
                _queueNames[projectId] = queueName;
                
                // 获取RabbitMQ通道
                var channel = client._channel;
                if (channel == null)
                {
                    LogHelper.Error($"项目 {projectId} 的RabbitMQ通道为空");
                    return false;
                }
                
                // 设置QoS，提高每个消费者可以同时处理的消息数量
                await channel.BasicQosAsync(0, 2000, false);
                
                // 声明队列，确保队列存在
                var queueDeclareResult = await channel.QueueDeclareAsync(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);
                    
                // 记录队列中的消息数量 - 使用新的方法获取消息数量
                var messageCount = await channel.MessageCountAsync(queueName);
                LogHelper.Info($"队列 {queueName} 中有 {messageCount} 条待处理消息");
                
                // 创建异步消费者
                var consumer = new AsyncEventingBasicConsumer(channel);
                LogHelper.Info($"为项目 {projectId} 创建了异步消费者");
                
                 consumer.ReceivedAsync += async (model, ea) =>
                {
                    LogHelper.Info($"收到项目 {projectId} 的消息，DeliveryTag: {ea.DeliveryTag}");
                    try
                    {
                        // 获取消息内容
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        LogHelper.Debug($"项目 {projectId} 收到消息内容: {message.Substring(0, Math.Min(100, message.Length))}...");
                        
                        // 触发消息接收事件
                        if (MessageReceived != null)
                        {
                            LogHelper.Debug($"触发项目 {projectId} 的消息接收事件");
                             MessageReceived.Invoke(this, new MessageReceivedEventArgs
                            {
                                ProjectId = projectId,
                                RawMessage = message
                            });
                            LogHelper.Debug($"项目 {projectId} 的消息接收事件处理完成");
                        }
                        else
                        {
                            LogHelper.Warn($"项目 {projectId} 的消息接收事件未注册");
                        }
                        
                        // 确认消息已处理
                        await channel.BasicAckAsync(ea.DeliveryTag, false);
                        LogHelper.Debug($"已确认项目 {projectId} 的消息，DeliveryTag: {ea.DeliveryTag}");
                    }
                    catch (Exception ex)
                    {
                        // 处理失败，消息重新入队
                        LogHelper.Error($"处理项目 {projectId} 的消息时发生错误: {ex.Message}, 堆栈: {ex.StackTrace}");
                       await channel.BasicNackAsync(ea.DeliveryTag, false, true);
                    }
                };
                
                // 开始消费队列 - 使用批量确认模式提高性能
                string consumerTag = await channel.BasicConsumeAsync(
                    queue: queueName,
                    autoAck: false,  // 尝试修改为 true 进行测试
                    consumer: consumer);
                
                // 保存通道引用
                _channels[projectId] = channel;
                
                LogHelper.Info($"已订阅项目 {projectId} 的队列 {queueName}，消费者标签: {consumerTag}");
                
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Error($"订阅项目 {projectId} 队列时发生错误: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 关闭消息接收器
        /// </summary>
        public async Task Shutdown()
        {
            try
            {
                // 释放RabbitMQ客户端
                foreach (var client in _rabbitClients)
                {
                    try
                    {
                       await  client.Dispose();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error($"释放RabbitMQ客户端时发生错误: {ex.Message}");
                    }
                }
                
                LogHelper.Info("RabbitMQ连接已关闭");
            }
            catch (Exception ex)
            {
                LogHelper.Error($"关闭RabbitMQ消息接收器时发生错误: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 释放资源
        /// </summary>
        public async Task Dispose()
        {
           await  Shutdown();
        }
    }
}
 