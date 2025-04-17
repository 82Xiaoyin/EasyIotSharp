using EasyIotSharp.DataProcessor.Util;
using RabbitMQ.Client;
using System;
using System.Text;
using System.Threading;

namespace EasyIotSharp.DataProcessor.Model.RaddbitDTO
{
    /// <summary>
    /// RabbitMQ客户端
    /// </summary>
    public class RabbitMQClient : IDisposable
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string VirtualHost { get; set; }
        public string Exchange { get; set; }
        public string mqid { get; set; }
        
        private IConnection _connection;
        public IModel _channel;
        private bool _isInitialized = false;
        private readonly object _lockObj = new object();
        private int _retryCount = 3;
        private int _retryInterval = 2000; // 毫秒
        
        /// <summary>
        /// 初始化RabbitMQ连接
        /// </summary>
        public void Init()
        {
            lock (_lockObj)
            {
                if (_isInitialized && _connection != null && _connection.IsOpen && _channel != null && !_channel.IsClosed)
                {
                    LogHelper.Debug("RabbitMQ客户端已初始化，无需重复初始化");
                    return;
                }
                
                // 关闭现有连接
                CloseConnection();
                
                int retryAttempt = 0;
                while (retryAttempt < _retryCount)
                {
                    try
                    {
                        LogHelper.Info($"正在初始化RabbitMQ客户端: {Host}:{Port}, 尝试次数: {retryAttempt + 1}");
                        
                        var factory = new ConnectionFactory
                        {
                            HostName = Host,
                            Port = Port,
                            UserName = UserName,
                            Password = Password,
                            VirtualHost = string.IsNullOrEmpty(VirtualHost) ? "/" : VirtualHost,
                            AutomaticRecoveryEnabled = true,
                            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
                        };
                        
                        _connection = factory.CreateConnection();
                        _channel = _connection.CreateModel();
                        
                        // 添加连接关闭事件处理
                        _connection.ConnectionShutdown += Connection_ConnectionShutdown;
                        
                        // 使用幂等操作创建交换机（如果已存在则验证参数）
                        string formattedExchange = $"exchange_{Exchange}";
                        _channel.ExchangeDeclare(
                            exchange: formattedExchange,
                            type: ExchangeType.Direct,
                            durable: true,
                            autoDelete: false,
                            arguments: null);
                        LogHelper.Info($"使用或创建交换机: {formattedExchange}");

                        // 检查队列是否存在，如果不存在再创建
                        string queueName = $"queue_{Exchange}";
                        _channel.QueueDeclare(
                               queue: queueName,
                               durable: true,
                               exclusive: false,
                               autoDelete: false,
                               arguments: null);
                        LogHelper.Info($"创建新的队列: {queueName}");
                        
                        // 将队列绑定到交换机
                        _channel.QueueBind(
                            queue: queueName,
                            exchange: formattedExchange,
                            routingKey: Exchange);

                        LogHelper.Info($"成功创建交换机 [{formattedExchange}] 和队列 [{queueName}]");
                        
                        _isInitialized = true;
                        LogHelper.Info($"RabbitMQ客户端初始化成功: {Host}:{Port}, Exchange: {Exchange}");
                        return;
                    }
                    catch (Exception ex)
                    {
                        retryAttempt++;
                        LogHelper.Error($"RabbitMQ客户端初始化失败 (尝试 {retryAttempt}/{_retryCount}): {ex.Message}");
                        
                        if (retryAttempt >= _retryCount)
                        {
                            LogHelper.Error($"RabbitMQ客户端初始化失败，已达到最大重试次数: {_retryCount}");
                            throw new Exception($"无法连接到RabbitMQ服务器: {Host}:{Port}", ex);
                        }
                        
                        // 等待一段时间后重试
                        Thread.Sleep(_retryInterval);
                    }
                }
            }
        }
        
        /// <summary>
        /// 连接关闭事件处理
        /// </summary>
        private void Connection_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            LogHelper.Warn($"RabbitMQ连接已关闭: {e.ReplyText}");
            _isInitialized = false;
        }
        
        /// <summary>
        /// 发送消息
        /// </summary>
        public void SendMessage(string routingKey, byte[] message)
        {
            if (string.IsNullOrEmpty(routingKey))
            {
                LogHelper.Error("路由键不能为空");
                throw new ArgumentNullException(nameof(routingKey), "路由键不能为空");
            }
            
            if (message == null || message.Length == 0)
            {
                LogHelper.Error("消息内容不能为空");
                throw new ArgumentNullException(nameof(message), "消息内容不能为空");
            }
            
            int retryAttempt = 0;
            while (retryAttempt < _retryCount)
            {
                try
                {
                    if (!_isInitialized || _connection == null || !_connection.IsOpen || _channel == null || _channel.IsClosed)
                    {
                        LogHelper.Warn("RabbitMQ连接未初始化或已关闭，尝试重新初始化");
                        Init();
                    }
                    
                    var properties = _channel.CreateBasicProperties();
                    properties.Persistent = true;
                    properties.DeliveryMode = 2;
                    properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                    
                    // 使用格式化后的交换机名称
                    string formattedExchange = $"exchange_{Exchange}";
                    _channel.BasicPublish(
                        exchange: formattedExchange,
                        routingKey: routingKey,
                        basicProperties: properties,
                        body: message);
                    
                    LogHelper.Info($"成功发送消息到RabbitMQ，Exchange: {formattedExchange}, RoutingKey: {routingKey}, 消息大小: {message.Length} 字节");
                    return;
                }
                catch (Exception ex)
                {
                    retryAttempt++;
                    LogHelper.Error($"发送RabbitMQ消息失败 (尝试 {retryAttempt}/{_retryCount}): {ex.Message}");
                    
                    if (retryAttempt >= _retryCount)
                    {
                        LogHelper.Error($"发送RabbitMQ消息失败，已达到最大重试次数: {_retryCount}");
                        throw new Exception("发送RabbitMQ消息失败", ex);
                    }
                    
                    // 尝试重新初始化连接
                    CloseConnection();
                    Thread.Sleep(_retryInterval);
                }
            }
        }
        
        /// <summary>
        /// 关闭连接
        /// </summary>
        private void CloseConnection()
        {
            try
            {
                if (_channel != null)
                {
                    if (_channel.IsOpen)
                    {
                        _channel.Close();
                    }
                    _channel.Dispose();
                    _channel = null;
                }
                
                if (_connection != null)
                {
                    if (_connection.IsOpen)
                    {
                        _connection.Close();
                    }
                    _connection.Dispose();
                    _connection = null;
                }
                
                _isInitialized = false;
            }
            catch (Exception ex)
            {
                LogHelper.Error($"关闭RabbitMQ连接失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Close()
        {
            CloseConnection();
            LogHelper.Info("RabbitMQ连接已关闭");
        }
        
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Close();
        }
    }
}