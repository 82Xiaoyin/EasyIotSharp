using EasyIotSharp.Core.Configuration;
using EasyIotSharp.Core.Repositories.Hardware;
using EasyIotSharp.Core.Repositories.Project;
using EasyIotSharp.Core.Services.Hardware;
using EasyIotSharp.Core.Services.Project;
using EasyIotSharp.DataProcessor.Processing.Implementation;
using EasyIotSharp.DataProcessor.Processing.Interfaces;
using EasyIotSharp.GateWay.Core.Interfaces;
using EasyIotSharp.GateWay.Core.Model.AnalysisDTO;
using EasyIotSharp.GateWay.Core.Model.ConfigDTO;
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
        public MqttService(IDataRepository dataRepository)
        {
            // 使用UPrimeEngine获取配置
            _appOptions = UPrimeEngine.Instance.Resolve<AppOptions>();
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
                    .WithCleanSession();

                // 如果配置了用户名和密码，则添加认证信息
                if (!string.IsNullOrEmpty(mqttUsername))
                {
                    optionsBuilder.WithCredentials(mqttUsername, mqttPassword);
                }

                var options = optionsBuilder.Build();

                // 设置断开连接时的重连逻辑
                _mqttClient.DisconnectedAsync += async e =>
                {
                    Logger.Warn("MQTT客户端断开连接，尝试重新连接...");
                    await Task.Delay(TimeSpan.FromSeconds(5));
                    try
                    {
                        await _mqttClient.ConnectAsync(options);
                        Logger.Info("MQTT客户端重新连接成功");
                        
                        // 重新订阅主题
                        await LoadMqttTelemetryConfigAsync();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"MQTT客户端重新连接失败: {ex.Message}");
                    }
                };

                // 连接到MQTT服务器
                await _mqttClient.ConnectAsync(options);
                Logger.Info("MQTT客户端连接成功");
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
                Logger.Info("开始加载MQTT遥测配置...");
                
                // 订阅所有网关层的主题
                string gatewayTelemetryTopic = "devices/telemetry/#";
                await SubscribeToTopicAsync(gatewayTelemetryTopic);
                
                Logger.Info($"成功订阅网关遥测主题: {gatewayTelemetryTopic}");
            }
            catch (Exception ex)
            {
                Logger.Error($"加载MQTT配置失败: {ex.Message}");
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
                    // 提取网关ID
                    string gatewayId = topic.Substring("devices/telemetry/".Length);
                    
                    // 处理网关遥测数据
                    await ProcessGatewayTelemetryAsync(gatewayId, payload);
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
                //Logger.Info($"处理网关 {gatewayId} 的遥测数据");
                
                //// 获取网关仓储
                //var gatewayRepository = UPrimeEngine.Instance.Resolve<IGatewayRepository>();
                
                //// 获取网关信息
                //var gateway = await gatewayRepository.GetGateway(gatewayId);
                //if (gateway == null)
                //{
                //    Logger.Warn($"未找到网关ID为 {gatewayId} 的网关信息");
                //    return;
                //}
                
                //// 获取该网关下的所有测点
                //var sensorPointRepository = UPrimeEngine.Instance.Resolve<ISensorPointRepository>();
                //var sensorPoints = await sensorPointRepository.QueryList(gatewayId);
                
                //if (sensorPoints == null || !sensorPoints.Any())
                //{
                //    Logger.Warn($"网关 {gatewayId} 下未找到测点信息");
                //    return;
                //}
                
                //Logger.Info($"网关 {gatewayId} 下找到 {sensorPoints.Count()} 个测点");
                
                //// 解析遥测数据
                //var telemetryData = JsonConvert.DeserializeObject<Dictionary<string, object>>(payload);
                //if (telemetryData == null)
                //{
                //    Logger.Warn($"无法解析网关 {gatewayId} 的遥测数据: {payload}");
                //    return;
                //}
                
                //// 处理每个测点的数据
                //foreach (var sensorPoint in sensorPoints)
                //{
                //    // 获取测点的传感器信息
                //    var sensor = await _sensorService.GetSensor(sensorPoint.SensorId);
                //    if (sensor == null)
                //    {
                //        Logger.Warn($"未找到测点 {sensorPoint.Id} 对应的传感器信息");
                //        continue;
                //    }
                    
                //    // 获取传感器的所有指标
                //    var quotas = await _sensorQuotaService.GetQuotasBySensorId(sensor.Id);
                //    if (quotas == null || !quotas.Any())
                //    {
                //        Logger.Warn($"传感器 {sensor.Id} 未配置指标");
                //        continue;
                //    }
                    
                //    // 处理每个指标
                //    foreach (var quota in quotas)
                //    {
                //        // 检查遥测数据中是否包含该指标的标识符
                //        if (telemetryData.TryGetValue(quota.Identifier, out var value))
                //        {
                //            // 根据数据类型转换值
                //            object convertedValue = ConvertValueByDataType(value, quota.DataType);
                            
                //            // 构建测点数据
                //            var pointData = new Dictionary<string, object>
                //            {
                //                { "value", convertedValue },
                //                { "timestamp", DateTime.Now }
                //            };
                            
                //            // 存储到InfluxDB
                //            await StoreDataToInfluxDBAsync(sensorPoint.Id, quota.Identifier, pointData);
                            
                //            Logger.Info($"成功处理测点 {sensorPoint.Id} 的指标 {quota.Identifier}, 值: {convertedValue}");
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                Logger.Error($"处理网关 {gatewayId} 遥测数据失败: {ex.Message}");
            }
        }
        
       
    }
    
    /// <summary>
    /// MQTT测点配置
    /// </summary>
    
}