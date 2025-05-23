using EasyIotSharp.DataProcessor.Processing.Interfaces;
using MQTTnet;
using MQTTnet.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UPrime;
using EasyIotSharp.Core.Configuration;
using log4net;

namespace EasyIotSharp.DataProcessor.Processing.Implementation
{
    public class MqttService : IMqttService
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MqttService));
        private readonly IMqttClient _mqttClient;
        private readonly AppOptions _appOptions; // 使用MqttOptions

        public MqttService()
        {
            // 使用UPrimeEngine获取配置
            _appOptions = UPrimeEngine.Instance.Resolve<AppOptions>();
            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();
            InitializeMqttClient().Wait();
        }

        private async Task InitializeMqttClient()
        {
            try
            {
                // 从MqttOptions中读取配置
                var mqttHost = _appOptions.MqttOptions.Host ?? "localhost";
                var mqttPort = _appOptions.MqttOptions.Port > 0 ? _appOptions.MqttOptions.Port : 1883;
                var mqttClientId = _appOptions.MqttOptions.ClientId ?? $"EasyIotSharp_DataProcessor_{Guid.NewGuid()}";
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
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("MQTT客户端重新连接失败", ex);
                    }
                };

                // 连接到MQTT服务器
                await _mqttClient.ConnectAsync(options);
                Logger.Info("MQTT客户端连接成功");
            }
            catch (Exception ex)
            {
                Logger.Error("初始化MQTT客户端失败", ex);
            }
        }

        public async Task PublishDataAsync(string projectId, string pointId, object data)
        {
            try
            {
                if (!_mqttClient.IsConnected)
                {
                    Logger.Warn("MQTT客户端未连接，尝试重新连接...");
                    await InitializeMqttClient();
                }

                // 从配置中读取主题格式
                var topicFormat = _appOptions.MqttOptions.TopicFormat ?? "{0}/{1}/rawdata";

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
                Logger.Debug($"成功发布数据到MQTT主题: {topic}");
            }
            catch (Exception ex)
            {
                Logger.Error("发布数据到MQTT失败", ex);
            }
        }

        public async Task PublishBatchDataAsync(string projectId, IEnumerable<Dictionary<string, object>> dataPoints)
        {
            try
            {
                if (!_mqttClient.IsConnected)
                {
                    Logger.Warn("MQTT客户端未连接，尝试重新连接...");
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
                Logger.Debug("成功批量发布数据到MQTT");
            }
            catch (Exception ex)
            {
                Logger.Error("批量发布数据到MQTT失败", ex);
            }
        }
    }
}