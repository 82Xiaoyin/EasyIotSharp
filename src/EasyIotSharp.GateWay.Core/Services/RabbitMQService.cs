using EasyIotSharp.GateWay.Core.LoadingConfig.RabbitMQ;
using EasyIotSharp.GateWay.Core.Util;
using System;
using System.Text;
using System.Text.Json;

namespace EasyIotSharp.GateWay.Core.Services
{
    /// <summary>
    /// RabbitMQ服务类
    /// 提供发送消息的方法
    /// </summary>
    public class RabbitMQService
    {
        /// <summary>
        /// 发送消息到指定项目的RabbitMQ
        /// </summary>
        /// <param name="projectId">项目ID</param>
        /// <param name="message">消息内容</param>
        /// <returns>是否发送成功</returns>
        public bool SendMessage(string projectId, object message)
        {
            try
            {
                // 获取项目对应的MQ客户端
                var mqClient = RabbitMQConfig.GetMQClient(projectId);
                if (mqClient == null)
                {
                    LogHelper.Error($"未找到项目ID {projectId} 对应的RabbitMQ客户端");
                    return false;
                }
                
                // 获取路由键
                string routingKey = RabbitMQConfig.GetRoutingKey(projectId);
                if (string.IsNullOrEmpty(routingKey))
                {
                    LogHelper.Error($"未找到项目ID {projectId} 对应的路由键");
                    return false;
                }
                
                // 序列化消息
                string messageJson = message.ToString();
                byte[] messageBytes = Encoding.UTF8.GetBytes(messageJson);
                
                // 发送消息
                mqClient.SendMessage(routingKey, messageBytes);
                
                LogHelper.Info($"成功发送消息到项目 {projectId} 的RabbitMQ，路由键: {routingKey}");
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Error($"发送RabbitMQ消息失败: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 发送原始字节数据到指定项目的RabbitMQ
        /// </summary>
        /// <param name="projectId">项目ID</param>
        /// <param name="data">字节数据</param>
        /// <returns>是否发送成功</returns>
        public bool SendRawData(string projectId, byte[] data)
        {
            try
            {
                // 获取项目对应的MQ客户端
                var mqClient = RabbitMQConfig.GetMQClient(projectId);
                if (mqClient == null)
                {
                    LogHelper.Error($"未找到项目ID {projectId} 对应的RabbitMQ客户端");
                    return false;
                }
                
                // 获取路由键
                string routingKey = RabbitMQConfig.GetRoutingKey(projectId);
                if (string.IsNullOrEmpty(routingKey))
                {
                    LogHelper.Error($"未找到项目ID {projectId} 对应的路由键");
                    return false;
                }
                
                // 发送消息
                mqClient.SendMessage(routingKey, data);
                
                LogHelper.Info($"成功发送原始数据到项目 {projectId} 的RabbitMQ，路由键: {routingKey}");
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Error($"发送RabbitMQ原始数据失败: {ex.Message}");
                return false;
            }
        }
    }
}