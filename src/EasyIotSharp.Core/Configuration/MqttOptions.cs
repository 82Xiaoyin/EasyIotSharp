using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
namespace EasyIotSharp.Core.Configuration
{
    /// <summary>
    /// MQTT配置选项
    /// </summary>
    public class MqttOptions
    {
        /// <summary>
        /// MQTT服务器地址
        /// </summary>
        public string Host { get; internal set; }

        /// <summary>
        /// MQTT服务器端口
        /// </summary>
        public int Port { get; internal set; }

        /// <summary>
        /// MQTT客户端ID
        /// </summary>
        public string ClientId { get; internal set; }

        /// <summary>
        /// MQTT用户名
        /// </summary>
        public string Username { get; internal set; }

        /// <summary>
        /// MQTT密码
        /// </summary>
        public string Password { get; internal set; }

        /// <summary>
        /// MQTT主题格式
        /// </summary>
        public string TopicFormat { get; internal set; }

        public static MqttOptions ReadFromConfiguration(IConfiguration config)
        {
            MqttOptions options = new MqttOptions();
            var cs = config.GetSection("Mqtt");

            options.Host = cs.GetValue<string>(nameof(Host)) ?? "localhost";

            var portStr = cs.GetValue<string>(nameof(Port));
            options.Port = !string.IsNullOrEmpty(portStr) && int.TryParse(portStr, out int port) ? port : 1883;

            options.ClientId = cs.GetValue<string>(nameof(ClientId)) ?? $"EasyIotSharp_DataProcessor_{Guid.NewGuid()}";
            options.Username = cs.GetValue<string>(nameof(Username));
            options.Password = cs.GetValue<string>(nameof(Password));
            options.TopicFormat = cs.GetValue<string>(nameof(TopicFormat)) ?? "{0}/{1}/rawdata";

            return options;
        }
    }
}
