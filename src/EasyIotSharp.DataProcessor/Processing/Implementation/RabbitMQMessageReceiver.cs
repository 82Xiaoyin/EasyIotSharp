using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyIotSharp.DataProcessor.Model.RaddbitDTO;
using EasyIotSharp.DataProcessor.Processing.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using log4net;

namespace EasyIotSharp.DataProcessor.Processing.Implementation
{
    /// <summary>
    /// RabbitMQ消息接收器
    /// </summary>
    public class RabbitMQMessageReceiver : IMessageReceiver 
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(RabbitMQMessageReceiver));
        
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
                foreach (var kvp in EasyIotSharp.DataProcessor.LoadingConfig.RabbitMQ.RabbitMQConfig.dicPid2MQClient)
                {
                    string projectId = kvp.Key;
                    RabbitMQClient client = kvp.Value;
                    
                    client.ConnectionReestablished += async (sender, e) =>
                    {
                        Logger.Info($"RabbitMQ连接已重建，重新启动项目 {projectId} 的消费者...");
                        await ResubscribeProjectQueueAsync(client, projectId, "queue_");
                    };
                    
                    Logger.Info($"正在为项目 {projectId} 初始化 RabbitMQ 客户端");
                    
                    if (await SubscribeProjectQueueAsync(client, projectId, "queue_"))
                    {
                        _rabbitClients.Add(client);
                    }
                }
                
                Logger.Info("RabbitMQ消息接收器初始化完成");
            }
            catch (Exception ex)
            {
                Logger.Error("初始化RabbitMQ消息接收器时发生错误", ex);
                throw;
            }
        }
        
        /// <summary>
        /// 重新订阅项目队列（用于重连后恢复消费）
        /// </summary>
        private async Task<bool> ResubscribeProjectQueueAsync(RabbitMQClient client, string projectId, string queuePrefix)
        {
            Logger.Info($"重新订阅项目 {projectId} 的队列");
            
            if (_channels.TryGetValue(projectId, out var existingChannel))
            {
                try
                {
                    if (existingChannel != null && existingChannel.IsOpen)
                    {
                        await existingChannel.CloseAsync();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warn($"关闭项目 {projectId} 的旧通道时发生错误", ex);
                }
            }
            
            return await SubscribeProjectQueueAsync(client, projectId, queuePrefix);
        }
        
        /// <summary>
        /// 订阅项目队列
        /// </summary>
        private async Task<bool> SubscribeProjectQueueAsync(RabbitMQClient client, string projectId, string queuePrefix)
        {
            try
            {
                await client.InitAsync();
                
                string queueName = $"{queuePrefix}{projectId}";
                _queueNames[projectId] = queueName;
                
                var channel = client._channel;
                if (channel == null)
                {
                    Logger.Error($"项目 {projectId} 的RabbitMQ通道为空");
                    return false;
                }
                
                await channel.BasicQosAsync(0, 2000, false);
                
                var queueDeclareResult = await channel.QueueDeclareAsync(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);
                    
                var messageCount = await channel.MessageCountAsync(queueName);
                Logger.Info($"队列 {queueName} 中有 {messageCount} 条待处理消息");
                
                var consumer = new AsyncEventingBasicConsumer(channel);
                Logger.Info($"为项目 {projectId} 创建了异步消费者");
                
                consumer.ReceivedAsync += async (model, ea) =>
                {
                    Logger.Info($"收到项目 {projectId} 的消息，DeliveryTag: {ea.DeliveryTag}");
                    try
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        Logger.Debug($"项目 {projectId} 收到消息内容: {message.Substring(0, Math.Min(100, message.Length))}...");
                        
                        if (MessageReceived != null)
                        {
                            Logger.Debug($"触发项目 {projectId} 的消息接收事件");
                            MessageReceived.Invoke(this, new MessageReceivedEventArgs
                            {
                                ProjectId = projectId,
                                RawMessage = message
                            });
                            Logger.Debug($"项目 {projectId} 的消息接收事件处理完成");
                        }
                        else
                        {
                            Logger.Warn($"项目 {projectId} 的消息接收事件未注册");
                        }
                        
                        await channel.BasicAckAsync(ea.DeliveryTag, false);
                        Logger.Debug($"已确认项目 {projectId} 的消息，DeliveryTag: {ea.DeliveryTag}");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"处理项目 {projectId} 的消息时发生错误", ex);
                        await channel.BasicNackAsync(ea.DeliveryTag, false, true);
                    }
                };
                
                string consumerTag = await channel.BasicConsumeAsync(
                    queue: queueName,
                    autoAck: false,
                    consumer: consumer);
                
                _channels[projectId] = channel;
                
                Logger.Info($"已订阅项目 {projectId} 的队列 {queueName}，消费者标签: {consumerTag}");
                
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"订阅项目 {projectId} 队列时发生错误", ex);
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
                foreach (var client in _rabbitClients)
                {
                    try
                    {
                        await client.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("释放RabbitMQ客户端时发生错误", ex);
                    }
                }
                
                Logger.Info("RabbitMQ连接已关闭");
            }
            catch (Exception ex)
            {
                Logger.Error("关闭RabbitMQ消息接收器时发生错误", ex);
            }
        }
        
        /// <summary>
        /// 释放资源
        /// </summary>
        public async Task Dispose()
        {
            await Shutdown();
        }
    }
}
 