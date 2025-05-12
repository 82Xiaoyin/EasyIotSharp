using EasyIotSharp.Core.Dto.Gateways;
using EasyIotSharp.Core.Repositories.Project;
using EasyIotSharp.GateWay.Core.Model.AnalysisDTO;
using EasyIotSharp.GateWay.Core.Services;
using EasyIotSharp.GateWay.Core.Socket;
using log4net;
using MongoDB.Bson.IO;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using UPrime;

namespace EasyIotSharp.GateWay.Core.Util
{
    public static class DataParserHelper
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(DataParserHelper));
        public static async Task SendEncryptedData(
            LowFrequencyData sensorData,
            GatewayConnectionInfo connectionInfo,
            RabbitMQService rabbitMQService)
        {
            var _gatewayRepository = UPrimeEngine.Instance.Resolve<IGatewayRepository>();
            if (string.IsNullOrEmpty(connectionInfo?.GatewayId)) return;

            var gateway = _gatewayRepository.GetGateway(connectionInfo.GatewayId);
            if (gateway != null && !string.IsNullOrEmpty(gateway.ProjectId))
            {
                try
                {
                    sensorData.TenantAbbreviation = gateway.TenantAbbreviation;
                    string encryptedData = sensorData.ToEncryptedString();
                    await rabbitMQService.SendMessage(gateway.ProjectId, encryptedData);
                    Logger.Info($"发送加密数据到RabbitMQ成功, 网关ID: {connectionInfo.GatewayId}");
                }
                catch (Exception ex)
                {
                    Logger.Error($"发送到RabbitMQ失败: {ex.ToString()}");
                }
            }
        }


    }
}