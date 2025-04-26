using EasyIotSharp.DataProcessor.Util;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyIotSharp.DataProcessor.Model.RaddbitDTO
{
    /// <summary>
    /// RabbitMQ客户端
    /// </summary>
    public class RabbitMQClient : IAsyncDisposable 
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string VirtualHost { get; set; }
        public string Exchange { get; set; }
        public string mqid { get; set; }
        
        private IConnection _connection;
        public IChannel _channel;
        private bool _isInitialized = false;
        private int _retryCount = 3;
        private int _retryInterval = 2000; // 毫秒
        
        /// <summary>
        /// 初始化RabbitMQ连接
        /// </summary>
        public async Task InitAsync()
        {
                if (_isInitialized && _connection != null && _connection.IsOpen && _channel != null && !_channel.IsClosed)
                {
                    LogHelper.Debug("RabbitMQ客户端已初始化，无需重复初始化");
                    return;
                }

                // 关闭现有连接
                await CloseConnectionAsync();
            
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
                        VirtualHost = VirtualHost,
                        AutomaticRecoveryEnabled = true,
                        NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                        ConsumerDispatchConcurrency = 2
                    };
                    
                    // 异步创建连接
                    _connection = await factory.CreateConnectionAsync(cancellationToken: default);
                    _channel = await _connection.CreateChannelAsync();
                        
                    // 配置通道
                    await _channel.BasicQosAsync(0, 1, false);
                        
                    // 添加连接关闭事件处理
                    _connection.ConnectionShutdownAsync += Connection_ConnectionShutdownAsync;
                    
                    // 使用幂等操作创建交换机（如果已存在则验证参数）
                    string formattedExchange = $"exchange_{Exchange}";
                    await _channel.ExchangeDeclareAsync(
                        exchange: formattedExchange,
                        type: ExchangeType.Direct,
                        durable: true,
                        autoDelete: false,
                        arguments: null);
                    LogHelper.Info($"使用或创建交换机: {formattedExchange}");

                    // 检查队列是否存在，如果不存在再创建
                    string queueName = $"queue_{Exchange}";
                   await  _channel.QueueDeclareAsync(
                           queue: queueName,
                           durable: true,
                           exclusive: false,
                           autoDelete: false,
                           arguments: null);
                    LogHelper.Info($"创建新的队列: {queueName}");
                    
                    // 将队列绑定到交换机
                   await _channel.QueueBindAsync(
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
        
        /// <summary>
        /// 连接关闭事件处理
        /// </summary>
        private async Task Connection_ConnectionShutdownAsync(object sender, ShutdownEventArgs e)
        {
            LogHelper.Warn($"RabbitMQ连接已关闭: {e.ReplyText}");
            _isInitialized = false;
            
            // 可以在这里添加异步重连逻辑
            try
            {
                await InitAsync();
            }
            catch (Exception ex)
            {
                LogHelper.Error($"RabbitMQ重连失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 发送消息
        /// </summary>
        public async Task SendMessageAsync(string routingKey, byte[] message)
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
                        await InitAsync();
                    }

                    // 使用新的方法创建消息属性
                    var properties = new BasicProperties
                    {
                        Persistent = true,
                        DeliveryMode = DeliveryModes.Persistent,
                        Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                    };
                    
                    string formattedExchange = $"exchange_{Exchange}";
                    await _channel.BasicPublishAsync(
                         exchange: formattedExchange,
                         routingKey: routingKey,
                         mandatory: false,
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
                    await  CloseConnectionAsync();
                    Thread.Sleep(_retryInterval);
                }
            }
        }
        
        /// <summary>
        /// 异步释放资源
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            try
            {
                if (_channel != null)
                {
                    if (_channel.IsOpen)
                    {
                      await  _channel.CloseAsync();
                    }
                    await _channel.DisposeAsync();
                    _channel = null;
                }
                
                if (_connection != null)
                {
                    if (_connection.IsOpen)
                    {
                      await  _connection.CloseAsync();
                    }
                    await _connection.DisposeAsync();
                    _connection = null;
                }
                
                _isInitialized = false;
                LogHelper.Info("RabbitMQ连接已异步关闭");
            }
            catch (Exception ex)
            {
                LogHelper.Error($"异步关闭RabbitMQ连接失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public async Task CloseConnectionAsync()
        {
            try
            {
                if (_channel != null)
                {
                    if (_channel.IsOpen)
                    {
                      await  _channel.CloseAsync();
                    }
                    await _channel.DisposeAsync();
                    _channel = null;
                }
                
                if (_connection != null)
                {
                    if (_connection.IsOpen)
                    {
                       await _connection.CloseAsync();
                    }
                    await _connection.DisposeAsync();
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
        /// 同步释放资源
        /// </summary>
        public async Task Dispose()
        {
           await CloseConnectionAsync();
        }
    }
}