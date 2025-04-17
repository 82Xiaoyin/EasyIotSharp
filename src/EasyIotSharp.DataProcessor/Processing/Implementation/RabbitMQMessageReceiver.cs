using System;
using System.Collections.Generic;
using System.Text;
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
    public class RabbitMQMessageReceiver : IMessageReceiver, IDisposable
    {
        // 存储项目ID和对应的RabbitMQ通道映射关系
        private readonly Dictionary<string, IModel> _channels = new Dictionary<string, IModel>();
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
        public void Initialize()
        {
            try
            {
                // 遍历所有已配置的RabbitMQ客户端
                foreach (var kvp in EasyIotSharp.DataProcessor.LoadingConfig.RabbitMQ.RabbitMQConfig.dicPid2MQClient)
                {
                    string projectId = kvp.Key;
                    RabbitMQClient client = kvp.Value;
                    
                    // 为每个项目订阅队列
                    if (SubscribeProjectQueue(client, projectId, "queue_"))
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
        private bool SubscribeProjectQueue(RabbitMQClient client, string projectId, string queuePrefix)
        {
            try
            {
                // 构建队列名称并保存
                string queueName = $"{queuePrefix}{projectId}";
                _queueNames[projectId] = queueName;
                
                // 获取RabbitMQ通道
                var channel = client._channel;
                
                // 设置QoS，提高每个消费者可以同时处理的消息数量
                channel.BasicQos(0, 2000, false);
                
                // 声明队列，确保队列存在
                var queueDeclareOk = channel.QueueDeclare(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);
                    
                // 记录队列中的消息数量
                LogHelper.Info($"队列 {queueName} 中有 {queueDeclareOk.MessageCount} 条待处理消息");
                
                // 创建异步消费者
                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.Received += async (model, ea) =>
                {
                    try
                    {
                        // 获取消息内容
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        
                        // 触发消息接收事件
                        MessageReceived?.Invoke(this, new MessageReceivedEventArgs
                        {
                            ProjectId = projectId,
                            RawMessage = message
                        });
                        
                        // 确认消息已处理
                        channel.BasicAck(ea.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        // 处理失败，消息重新入队
                        LogHelper.Error($"处理项目 {projectId} 的消息时发生错误: {ex.Message}");
                        channel.BasicNack(ea.DeliveryTag, false, true);
                    }
                };
                
                // 开始消费队列 - 使用批量确认模式提高性能
                string consumerTag = channel.BasicConsume(
                    queue: queueName,
                    autoAck: false,
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
        public void Shutdown()
        {
            try
            {
                // 释放RabbitMQ客户端
                foreach (var client in _rabbitClients)
                {
                    try
                    {
                        client.Dispose();
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
        public void Dispose()
        {
            Shutdown();
        }
    }
}