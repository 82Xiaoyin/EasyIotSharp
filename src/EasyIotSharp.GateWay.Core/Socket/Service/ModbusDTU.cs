using EasyIotSharp.GateWay.Core.Model.SocketDTO;
using EasyIotSharp.GateWay.Core.Util;
using log4net;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using EasyIotSharp.GateWay.Core.Socket.Factory;
using System.Linq;
using System.Text.Json;
using EasyIotSharp.GateWay.Core.Util.ModbusUtil;
using EasyIotSharp.GateWay.Core.Services;
using EasyIotSharp.GateWay.Core.Model.AnalysisDTO;
using EasyIotSharp.GateWay.Core.Model.ConfigDTO;

namespace EasyIotSharp.GateWay.Core.Socket.Service
{
    public class ModbusDTU : EasyTCPSuper
    {
        public easyiotsharpContext _easyiotsharpContext;
        // 在类的成员变量中添加
        private RabbitMQService _rabbitMQService;
        
        // 修改构造函数
        public ModbusDTU(easyiotsharpContext easyiotsharpContext)
        {
            _easyiotsharpContext = easyiotsharpContext;
            _rabbitMQService = new RabbitMQService();
        }
        public override void DecodeData(TaskInfo taskData)
        {
            try
            {
                //1、server查询是否ConfigJson=null
                string modbusConfig = taskData.Client.ConfigJson;
                if (taskData.Packet == null || taskData.Packet.BData == null)
                {
                    LogHelper.Error("taskData.Packet == null || taskData.Packet.BData == null ");
                    return;
                }
                byte[] bReceived = taskData.Packet.BData;//队列的封包数据
                
                // 记录接收到的数据
                string dataType = string.IsNullOrEmpty(modbusConfig) ? "注册包" : "数据包";
                GatewayConnectionManager.Instance.UpdateGatewayData(taskData.Client.ConnId, bReceived, dataType);
                
                if (!string.IsNullOrEmpty(modbusConfig))
                {
                    // 如果是数据包，进行解析
                    ParseReceivedData(taskData.Client.ConnId, bReceived, modbusConfig);
                }
                else
                {
                    //3.1ModbusDevieConfig未查询到,则进行解码、
                    string strData = System.Text.Encoding.ASCII.GetString(bReceived, 0, bReceived.Length);//转换为Ascll码
                    LogHelper.Info("  收到注册包:  " + strData);
                    var gatewayprotocol = _easyiotsharpContext.Gatewayprotocol.Where(x=>x.GatewayId.Equals(strData)).FirstOrDefault();
                    if (gatewayprotocol == null)
                    {
                        LogHelper.Info("未找到注册包: " + strData);
                        return;
                    }
                    
                    // 注册网关ID与连接的关联
                    ProcessGatewayRegister(taskData.Client.ConnId, strData, bReceived);
                    
                    //3.2 查询ComModbusDevie配置、
                    taskData.Client.ConfigJson = gatewayprotocol.ConfigJson;
                    //3.3更新server的Extra
                    taskData.Server.SetExtra(taskData.Client.ConnId, taskData.Client);
                    //3.4 启动Task定时发送采集命令
                    Task tSendCmd = new Task(() => SendMsgToClient("hex", taskData), TaskCreationOptions.LongRunning);
                    tSendCmd.Start();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("ModbusDTU 异常:" + ex.ToString());
                return;
            }
        }

        /// <summary>
        /// 解析接收到的数据包
        /// </summary>
        private void ParseReceivedData(IntPtr connId, byte[] data, string configJson)
        {
            try
            {
                if (data == null || data.Length < 7) return;

                var connectionInfo = GatewayConnectionManager.Instance.GetConnectionByConnId(connId);
                if (connectionInfo == null || !connectionInfo.IsRegistered) return;

                // 使用JsonExtensions进行反序列化
                List<ModbusConfigModel> configModels = configJson.FromJson<List<ModbusConfigModel>>();

                if (configModels == null || configModels.Count == 0)
                {
                    LogHelper.Error("配置解析失败或为空");
                    return;
                }

                // 查找匹配的配置项
                foreach (var config in configModels)
                {
                    // 检查从站地址和功能码是否匹配
                    if (config.FormData.Address == data[0] && config.FormData.FunctionCode == data[1])
                    {
                        // 通过测点id拿到sensorid
                        if (string.IsNullOrEmpty(config.MeasurementPoint))
                        {
                            LogHelper.Error($"测点ID为空，跳过处理");
                            continue;
                        }

                        var sensorPoint = _easyiotsharpContext.Sensorpoint
                            .Where(x => x.Id == config.MeasurementPoint)
                            .FirstOrDefault();

                        if (sensorPoint == null)
                        {
                            LogHelper.Error($"未找到测点信息，测点ID: {config.MeasurementPoint}");
                            continue;
                        }

                        var sensor = _easyiotsharpContext.Sensor
                         .Where(x => x.Id == sensorPoint.SensorId)
                         .FirstOrDefault();

                        if (sensor == null)
                        {
                            LogHelper.Error($"未找到测点类型信息，测点类型id: {sensorPoint.SensorId}");
                            continue;
                        }
                        var sensorQuota = _easyiotsharpContext.Sensorquota
                            .Where(x => x.SensorId == sensorPoint.SensorId)
                            .OrderBy(x => x.Sort)
                            .ToList();

                        if (sensorQuota == null || !sensorQuota.Any())
                        {
                            LogHelper.Warn($"未找到传感器指标信息，传感器ID: {sensorPoint.SensorId}");
                        }

                        // 使用SensorDataFactory创建传感器数据对象
                        var sensorData = SensorDataFactory.CreateSensorData<LowFrequencyData>(
                            config.projectId,
                            DateTime.Now);
                        
                        // 设置测点类型
                        sensorData.PointType = sensor.BriefName;

                        if (data.Length >= 3 && data[2] > 0)
                        {
                            byte byteCount = data[2];
                            var pointData = new PointData
                            {
                                PointId = config.MeasurementPoint,
                                IndicatorCount = byteCount / 2
                            };

                            // 使用配置中的系数K
                            double k = config.FormData.K;

                            for (int i = 0; i < byteCount / 2; i++)
                            {
                                if (3 + i * 2 + 1 < data.Length)
                                {
                                    ushort registerValue = (ushort)((data[3 + i * 2] << 8) | data[3 + i * 2 + 1]);
                                    
                                    // 获取对应的指标名称
                                    string quotaName = i < sensorQuota.Count ? sensorQuota[i].Identifier : $"指标{i+1}";
                                    double quotaValue = registerValue * k;
                                    string Unit= i < sensorQuota.Count ? sensorQuota[i].Unit : $"指标{i + 1}";
                                    // 添加带有名称的值
                                    pointData.Values.Add(new NamedValue 
                                    { 
                                        Name = quotaName, 
                                        Value = quotaValue,
                                        Unit= Unit
                                    });
                                }
                            }

                            sensorData.Points.Add(pointData);
                            DataParserHelper.SendEncryptedData(sensorData, connectionInfo, _easyiotsharpContext, _rabbitMQService);
                        }
                        break; // 找到匹配的配置后退出循环
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error($"解析数据包异常: {ex.ToString()}");
            }
        }

        

        /// 处理网关注册包
        protected void ProcessGatewayRegister(IntPtr connId, string gatewayId, byte[] data)
        {
            try
            {
                // 获取客户端IP和端口
                string ip = "未知";
                ushort port = 0;
                
                // 注册网关ID与连接的关联
                GatewayConnectionManager.Instance.RegisterGateway(connId, gatewayId);
                LogHelper.Info($"网关 {gatewayId} 注册成功，连接ID: {connId}, IP: {ip}, 端口: {port}");
            }
            catch (Exception ex)
            {
                LogHelper.Error($"处理网关注册包异常: {ex.Message}");
            }
        }


        private static void SendMsgToClient(string dataFormart, TaskInfo taskData)
        {
            if (string.IsNullOrEmpty(taskData.Client.ConfigJson))
            {
                LogHelper.Error("ConfigJson为空，无法发送命令");
                return;
            }

            try
            {
                int defaultSleepTime = 20000;
                Dictionary<string, int> intervalMap = new Dictionary<string, int>();
                List<byte[]> commandList = new List<byte[]>();

                // 使用JsonExtensions进行反序列化
                List<ModbusConfigModel> configModels = taskData.Client.ConfigJson.FromJson<List<ModbusConfigModel>>();

                if (configModels == null || configModels.Count == 0)
                {
                    LogHelper.Error("配置解析失败或为空");
                    return;
                }

                // 解析配置并生成命令
                foreach (var config in configModels)
                {
                    try
                    {
                        var formData = config.FormData;
                        if (formData == null)
                        {
                            LogHelper.Warn("配置项缺少formData字段，跳过");
                            continue;
                        }

                        // 组装Modbus命令
                        byte[] cmd = new byte[8];
                        cmd[0] = formData.Address;                  // 从站地址
                        cmd[1] = formData.FunctionCode;             // 功能码
                        cmd[2] = (byte)(formData.StartingAddress >> 8);     // 起始地址高字节
                        cmd[3] = (byte)(formData.StartingAddress & 0xFF);   // 起始地址低字节
                        cmd[4] = (byte)(formData.Quantity >> 8);         // 数量高字节
                        cmd[5] = (byte)(formData.Quantity & 0xFF);       // 数量低字节

                        // 计算CRC16
                        byte[] bCRC = ModbusCRC.ModbusCRC16(cmd, 0, 6);
                        cmd[6] = bCRC[0];                       // CRC低字节
                        cmd[7] = bCRC[1];                       // CRC高字节
                        
                        commandList.Add(cmd);
                        
                        // 获取间隔时间
                        int sleepTime = formData.Interval * 1000; // 转换为毫秒
                        intervalMap[BitConverter.ToString(cmd)] = sleepTime;
                        defaultSleepTime = sleepTime;
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error($"解析配置项时发生错误: {ex.Message}");
                    }
                }

                if (commandList.Count == 0)
                {
                    LogHelper.Error("没有有效的命令可发送");
                    return;
                }

                // 发送命令的主循环
                Task.Run(() =>
                {
                    try
                    {
                        while (true)
                        {
                            foreach (var cmd in commandList)
                            {
                                try
                                {
                                    // 发送命令
                                    bool isok = taskData.Server.Send(taskData.Client.ConnId, cmd, cmd.Length);
                                    string cmdHex = BitConverter.ToString(cmd).Replace("-", " ");
                                    
                                    if (isok)
                                    {
                                        int cmdSleepTime = intervalMap.TryGetValue(BitConverter.ToString(cmd), out int time) ? time : defaultSleepTime;
                                        LogHelper.Info($"发送命令成功: {cmdHex}, 间隔: {cmdSleepTime}ms");
                                        
                                        // 记录发送的命令到网关连接管理器
                                        GatewayConnectionManager.Instance.UpdateGatewayData(
                                            taskData.Client.ConnId, 
                                            cmd, 
                                            "发送命令");
                                    }
                                    else
                                    {
                                        LogHelper.Error($"发送命令失败: {cmdHex}");
                                        return;
                                    }
                                    
                                    // 命令间隔
                                    Thread.Sleep(200);
                                }
                                catch (Exception ex)
                                {
                                    LogHelper.Error($"发送命令时发生错误: {ex.Message}");
                                }
                            }
                            
                            // 全局间隔
                            Thread.Sleep(defaultSleepTime);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error($"命令发送循环中断: {ex.ToString()}");
                    }
                });
            }
            catch (Exception ex)
            {
                LogHelper.Error($"SendMsgToClient() 发生异常: {ex.ToString()}");
            }
        }
    }
}
