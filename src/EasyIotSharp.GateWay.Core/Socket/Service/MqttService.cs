using EasyIotSharp.Core.Configuration;
using EasyIotSharp.Core.Dto.Gateways;
using EasyIotSharp.Core.Repositories.Hardware;
using EasyIotSharp.Core.Repositories.Project;
using EasyIotSharp.Core.Services.Hardware;
using EasyIotSharp.Core.Services.Project;
using EasyIotSharp.DataProcessor.Processing.Implementation;
using EasyIotSharp.DataProcessor.Processing.Interfaces;
using EasyIotSharp.GateWay.Core.Interfaces;
using EasyIotSharp.GateWay.Core.Model.AnalysisDTO;
using EasyIotSharp.GateWay.Core.Model.ConfigDTO;
using EasyIotSharp.GateWay.Core.Services;
using EasyIotSharp.GateWay.Core.Util;
using log4net;
using MQTTnet;
using MQTTnet.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UPrime;
using IMqttService = EasyIotSharp.GateWay.Core.Interfaces.IMqttService;

namespace EasyIotSharp.GateWay.Core.Socket.Service
{
   public class MqttService : IMqttService
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MqttService));
        private readonly IMqttClient _mqttClient;
        private readonly AppOptions _appOptions;
        
        // 存储已订阅主题的集合
        private readonly HashSet<string> _subscribedTopics = new HashSet<string>();
        
        // 用于处理遥测数据的服务
        private readonly ISensorPointService _sensorPointService;
        private readonly ISensorService _sensorService;
        private readonly ISensorQuotaService _sensorQuotaService;
        private readonly IDataRepository _dataRepository;
        private RabbitMQService _rabbitMQService;
        public MqttService(IDataRepository dataRepository)
        {
            // 使用UPrimeEngine获取配置
            _appOptions = UPrimeEngine.Instance.Resolve<AppOptions>();
            _rabbitMQService = new RabbitMQService();
            _dataRepository = dataRepository;
            // 获取服务实例
            _sensorPointService = UPrimeEngine.Instance.Resolve<ISensorPointService>();
            _sensorService = UPrimeEngine.Instance.Resolve<ISensorService>();
            _sensorQuotaService = UPrimeEngine.Instance.Resolve<ISensorQuotaService>();
            
            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();
            
            // 注册消息接收处理程序 - 使用新版本的API
            _mqttClient.ApplicationMessageReceivedAsync += HandleReceivedMessageAsync;
            
            // 注册连接成功事件处理程序 - 使用新版本的API
            _mqttClient.ConnectedAsync += HandleConnectedAsync;
            
            InitializeMqttClient().Wait();
        }

        private async Task InitializeMqttClient()
        {
            try
            {
                // 从MqttOptions中读取配置
                var mqttHost = _appOptions.MqttOptions.Host ?? "localhost";
                var mqttPort = _appOptions.MqttOptions.Port > 0 ? _appOptions.MqttOptions.Port : 1883;
                var mqttClientId = _appOptions.MqttOptions.ClientId ?? $"EasyIotSharp_TelemetrySubscriber_{Guid.NewGuid()}";
                var mqttUsername = _appOptions.MqttOptions.Username;
                var mqttPassword = _appOptions.MqttOptions.Password;

                Logger.Info($"正在连接MQTT服务器: {mqttHost}:{mqttPort}");

                // 构建MQTT客户端选项
                var optionsBuilder = new MqttClientOptionsBuilder()
                    .WithTcpServer(mqttHost, mqttPort)
                    .WithClientId(mqttClientId)
                    .WithCleanSession(false) // 保持会话状态
                    .WithKeepAlivePeriod(TimeSpan.FromSeconds(60));

                // 如果配置了用户名和密码，则添加认证信息
                if (!string.IsNullOrEmpty(mqttUsername))
                {
                    optionsBuilder.WithCredentials(mqttUsername, mqttPassword);
                }

                var options = optionsBuilder.Build();

                _mqttClient.DisconnectedAsync += async e =>
                {
                    Logger.Warn("MQTT客户端断开连接，尝试重新连接...");
                    await Task.Delay(TimeSpan.FromSeconds(5));
                    try
                    {
                        await _mqttClient.ConnectAsync(options);

                        if (_mqttClient.IsConnected)
                        {
                            Logger.Info("MQTT客户端重新连接成功");

                            // 确保连接成功后再重新订阅主题
                            await Task.Delay(500); // 短暂延迟，确保连接稳定
                            await LoadMqttTelemetryConfigAsync();

                            // 验证订阅是否成功
                            Logger.Info($"重新订阅后的主题数量: {_subscribedTopics.Count}");
                        }
                        else
                        {
                            Logger.Warn("MQTT客户端连接状态异常，显示已连接但IsConnected为false");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"MQTT客户端重新连接失败: {ex.Message}");
                    }
                };

                // 连接到MQTT服务器
                await _mqttClient.ConnectAsync(options);
              
            }
            catch (Exception ex)
            {
                Logger.Error($"初始化MQTT客户端失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 处理连接成功事件
        /// </summary>
        private async Task HandleConnectedAsync(MqttClientConnectedEventArgs e)
        {
            Logger.Info("MQTT客户端连接成功");
            await LoadMqttTelemetryConfigAsync();
        }


        /// <summary>
        /// 加载MQTT遥测配置
        /// </summary>
        private async Task LoadMqttTelemetryConfigAsync()
        {
            try
            {
                Logger.Info($"开始加载MQTT遥测配置，当前连接状态: {_mqttClient.IsConnected}");

                if (!_mqttClient.IsConnected)
                {
                    Logger.Warn("MQTT客户端未连接，无法订阅主题");
                    return;
                }

                // 清空已订阅主题集合，确保重新订阅
                _subscribedTopics.Clear();

                // 订阅所有网关层的主题
                string gatewayTelemetryTopic = "devices/telemetry/#";
                await SubscribeToTopicAsync(gatewayTelemetryTopic);

                Logger.Info($"成功订阅网关遥测主题: {gatewayTelemetryTopic}，已订阅主题数量: {_subscribedTopics.Count}");
            }
            catch (Exception ex)
            {
                Logger.Error($"加载MQTT配置失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 订阅MQTT主题
        /// </summary>
        private async Task SubscribeToTopicAsync(string topic)
        {
            try
            {
                if (_subscribedTopics.Contains(topic))
                {
                    Logger.Debug($"主题 {topic} 已订阅，跳过");
                    return;
                }
                
                if (!_mqttClient.IsConnected)
                {
                    Logger.Warn("MQTT客户端未连接，无法订阅主题");
                    return;
                }
                
                var topicFilter = new MqttTopicFilterBuilder()
                    .WithTopic(topic)
                    .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build();
                
                var result = await _mqttClient.SubscribeAsync(topicFilter);
                
                // 如果所有订阅项的结果代码都小于等于GrantedQoS2（表示订阅成功）
                if (result.Items.All(x => x.ResultCode <= MQTTnet.Client.MqttClientSubscribeResultCode.GrantedQoS2))
                {
                    // 将成功订阅的主题添加到已订阅主题集合中
                    _subscribedTopics.Add(topic);
                    // 记录成功订阅的日志
                    Logger.Info($"成功订阅主题: {topic}");
                }
                else
                {
                    // 如果订阅失败，记录错误日志，包含失败的结果代码
                    Logger.Error($"订阅主题 {topic} 失败: {result.Items.FirstOrDefault()?.ResultCode}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"订阅主题 {topic} 失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 处理接收到的MQTT消息
        /// </summary>
        private async Task HandleReceivedMessageAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            try
            {
                string topic = e.ApplicationMessage.Topic;
                string payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
                
                Logger.Debug($"收到MQTT消息，主题: {topic}, 内容: {payload}");
                
                // 检查是否是网关遥测数据
                if (topic.StartsWith("devices/telemetry"))
                {
                    // 提取网关ID - 使用分割方法更可靠
                    string[] topicParts = topic.Split('/');
                    if (topicParts.Length >= 3)
                    {
                        string gatewayId = topicParts[2];

                        // 处理网关遥测数据
                        await ProcessGatewayTelemetryAsync(gatewayId, payload);
                    }
                    else
                    {
                        Logger.Warn($"无效的主题格式: {topic}，无法提取网关ID");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"处理MQTT消息失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 处理网关遥测数据
        /// </summary>
        private async Task ProcessGatewayTelemetryAsync(string gatewayId, string payload)
        {
            try
            {
                Logger.Debug($"开始处理网关遥测数据，网关ID: {gatewayId}");
                
                // 直接将JSON反序列化为SensorDataBase对象
                var sensorData = JsonConvert.DeserializeObject<SensorDataBase>(payload);
                
                // 检查基本数据是否有效
                if (sensorData == null || string.IsNullOrEmpty(sensorData.ProjectId))
                {
                    Logger.Warn($"遥测数据为空或缺少项目ID，网关ID: {gatewayId}");
                    return;
                }
                
                // 根据数据类型处理
                if (sensorData.DataType == DataType.HighFrequency)
                {
                    // 高频数据处理
                    var highFreqData = JsonConvert.DeserializeObject<HighFrequencyData>(payload);
                    if (highFreqData == null || highFreqData.Points == null || !highFreqData.Points.Any())
                    {
                        Logger.Warn($"高频数据格式错误或Points为空，网关ID: {gatewayId}");
                        return;
                    }
                    
                    // TODO: 处理高频数据
                    Logger.Info($"收到高频数据，网关ID: {gatewayId}, 测点数: {highFreqData.Points.Count}");
                }
                else
                {
                    // 低频数据处理
                    var lowFreqData = JsonConvert.DeserializeObject<LowFrequencyData>(payload);
                    if (lowFreqData == null || lowFreqData.Points == null || !lowFreqData.Points.Any())
                    {
                        Logger.Warn($"低频数据格式错误或Points为空，网关ID: {gatewayId}");
                        return;
                    }
                    
                    // 获取第一个测点ID用于日志
                    string firstPointId = lowFreqData.Points.First().PointId;
                    
                    // 验证测点是否存在于系统中
                    var sensorPointList = _sensorPointService.GetBySensorPointList();
                    var sensorPoint = sensorPointList.FirstOrDefault(x => x.Id.Equals(firstPointId));
                    if (sensorPoint == null)
                    {
                        Logger.Warn($"未找到测点信息，测点ID: {firstPointId}");
                        return;
                    }
                    
                    // 发送数据到RabbitMQ
                    await DataParserHelper.SendEncryptedData(lowFreqData, new GatewayConnectionInfo { GatewayId = gatewayId }, _rabbitMQService);
                    Logger.Info($"成功处理MQTT遥测数据，网关ID: {gatewayId}, 测点数: {lowFreqData.Points.Count}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"处理网关遥测数据失败: {ex.Message}", ex);
            }
        }
    }
}