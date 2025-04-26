using EasyIotSharp.DataProcessor.Processing.Interfaces;
using EasyIotSharp.DataProcessor.Util;
using MQTTnet;
using MQTTnet.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UPrime;
using EasyIotSharp.Core.Configuration; // 使用本地配置命名空间

namespace EasyIotSharp.DataProcessor.Processing.Implementation
{
    public class MqttService : IMqttService
    {
        private readonly IMqttClient _mqttClient;
        private readonly MqttOptions _mqttOptions; // 使用MqttOptions
        
        public MqttService()
        {
            // 使用UPrimeEngine获取配置
            _mqttOptions = UPrimeEngine.Instance.Resolve<MqttOptions>();
             var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();
            InitializeMqttClient().Wait();
        }
        
        private async Task InitializeMqttClient()
        {
            try
            {
                // 从MqttOptions中读取配置
                var mqttHost = _mqttOptions.Host ?? "localhost";
                var mqttPort = _mqttOptions.Port > 0 ? _mqttOptions.Port : 1883;
                var mqttClientId = _mqttOptions.ClientId ?? $"EasyIotSharp_DataProcessor_{Guid.NewGuid()}";
                var mqttUsername = _mqttOptions.Username;
                var mqttPassword = _mqttOptions.Password;
                
                LogHelper.Info($"正在连接MQTT服务器: {mqttHost}:{mqttPort}");
                
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
                    LogHelper.Warn("MQTT客户端断开连接，尝试重新连接...");
                    await Task.Delay(TimeSpan.FromSeconds(5));
                    try
                    {
                        await _mqttClient.ConnectAsync(options);
                        LogHelper.Info("MQTT客户端重新连接成功");
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error($"MQTT客户端重新连接失败: {ex.Message}");
                    }
                };
                
                // 连接到MQTT服务器
                await _mqttClient.ConnectAsync(options);
                LogHelper.Info("MQTT客户端连接成功");
            }
            catch (Exception ex)
            {
                LogHelper.Error($"初始化MQTT客户端失败: {ex.Message}");
            }
        }
        
        public async Task PublishDataAsync(string projectId, string pointId, object data)
        {
            try
            {
                if (!_mqttClient.IsConnected)
                {
                    LogHelper.Warn("MQTT客户端未连接，尝试重新连接...");
                    await InitializeMqttClient();
                }
                
                // 从配置中读取主题格式
                var topicFormat = _mqttOptions.TopicFormat ?? "{0}/{1}/rawdata";
                
                // 构建主题: 项目ID/传感器ID/rawdata
                string topic = string.Format(topicFormat, projectId, pointId);
                
                // 序列化数据
                string payload = JsonConvert.SerializeObject(data);
                
                // 创建消息
                var message = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(payload)
                    .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                    .WithRetainFlag(false)
                    .Build();
                
                // 发布消息
                await _mqttClient.PublishAsync(message);
                LogHelper.Debug($"成功发布数据到MQTT主题: {topic}");
            }
            catch (Exception ex)
            {
                LogHelper.Error($"发布数据到MQTT失败: {ex.Message}");
            }
        }
        
        public async Task PublishBatchDataAsync(string projectId, IEnumerable<Dictionary<string, object>> dataPoints)
        {
            try
            {
                if (!_mqttClient.IsConnected)
                {
                    LogHelper.Warn("MQTT客户端未连接，尝试重新连接...");
                    await InitializeMqttClient();
                }
                
                // 创建任务列表
                var tasks = new List<Task>();
                
                foreach (var dataPoint in dataPoints)
                {
                    if (dataPoint.TryGetValue("pointId", out var pointId))
                    {
                        tasks.Add(PublishDataAsync(projectId, pointId.ToString(), dataPoint));
                    }
                }
                
                // 并行执行所有发布任务
                await Task.WhenAll(tasks);
                LogHelper.Debug($"成功批量发布数据到MQTT");
            }
            catch (Exception ex)
            {
                LogHelper.Error($"批量发布数据到MQTT失败: {ex.Message}");
            }
        }
    }
}