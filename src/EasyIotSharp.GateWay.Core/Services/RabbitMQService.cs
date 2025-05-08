using EasyIotSharp.GateWay.Core.LoadingConfig.RabbitMQ;
using EasyIotSharp.GateWay.Core.Util;
using System;
using System.Text;
using System.Text.Json;
using log4net;

namespace EasyIotSharp.GateWay.Core.Services
{
    /// <summary>
    /// RabbitMQ服务类
    /// 提供发送消息的方法
    /// </summary>
    public class RabbitMQService
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(RabbitMQService));

        /// <summary>
        /// 发送消息到指定项目的RabbitMQ
        /// </summary>
        public bool SendMessage(string projectId, object message)
        {
            try
            {
                var mqClient = RabbitMQConfig.GetMQClient(projectId);
                if (mqClient == null)
                {
                    Logger.Error($"未找到项目ID {projectId} 对应的RabbitMQ客户端");
                    return false;
                }
                
                string routingKey = RabbitMQConfig.GetRoutingKey(projectId);
                if (string.IsNullOrEmpty(routingKey))
                {
                    Logger.Error($"未找到项目ID {projectId} 对应的路由键");
                    return false;
                }
                
                string messageJson = message.ToString();
                byte[] messageBytes = Encoding.UTF8.GetBytes(messageJson);
                
                mqClient.SendMessage(routingKey, messageBytes);
                
                Logger.Info($"成功发送消息到项目 {projectId} 的RabbitMQ，路由键: {routingKey}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("发送RabbitMQ消息失败", ex);
                return false;
            }
        }
        
        /// <summary>
        /// 发送原始字节数据到指定项目的RabbitMQ
        /// </summary>
        public bool SendRawData(string projectId, byte[] data)
        {
            try
            {
                var mqClient = RabbitMQConfig.GetMQClient(projectId);
                if (mqClient == null)
                {
                    Logger.Error($"未找到项目ID {projectId} 对应的RabbitMQ客户端");
                    return false;
                }
                
                string routingKey = RabbitMQConfig.GetRoutingKey(projectId);
                if (string.IsNullOrEmpty(routingKey))
                {
                    Logger.Error($"未找到项目ID {projectId} 对应的路由键");
                    return false;
                }
                
                mqClient.SendMessage(routingKey, data);
                
                Logger.Info($"成功发送原始数据到项目 {projectId} 的RabbitMQ，路由键: {routingKey}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("发送RabbitMQ原始数据失败", ex);
                return false;
            }
        }
    }
}