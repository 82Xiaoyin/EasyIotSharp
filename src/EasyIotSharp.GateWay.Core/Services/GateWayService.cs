using EasyIotSharp.GateWay.Core.Socket;
using EasyIotSharp.GateWay.Core.UI;
using EasyIotSharp.GateWay.Core.Util;
using System;
using log4net;

namespace EasyIotSharp.GateWay.Core.Services
{
    public class GateWayService
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(GateWayService));
        private readonly EasySocketBase _socketBase;

        public GateWayService()
        {
            _socketBase = new EasyTcpServer();
        }

        public void InitializeServices()
        {
            ConsoleUI.ShowSeparator();
            
            // 初始化TCP服务器
            ConsoleUI.ShowProgress("正在初始化TCP服务器 (端口:5020)...", () =>
            {
                _socketBase.InitTCPServer(new Model.SocketDTO.InitParamsInfo 
                { 
                    LocalPort = 5020,
                    Description = "modbusRTU",
                    StartState = true,
                    Manufacturer = "modbusRTU",
                    TimeOutMS = 3600000
                });
                Logger.Info("TCP服务器初始化成功，端口：5020");
            });

            // 初始化RabbitMQ
            ConsoleUI.ShowProgress("正在初始化RabbitMQ服务...", () =>
            {
                EasyIotSharp.GateWay.Core.LoadingConfig.RabbitMQ.RabbitMQConfig.InitMQ();
                Logger.Info("RabbitMQ初始化成功");
            });

            ConsoleUI.ShowSeparator();
            ConsoleUI.ShowSuccess("所有服务已启动完成");
            Logger.Info("项目启动完成");
        }

        public void CleanupServices()
        {
            try
            {
                EasyIotSharp.GateWay.Core.LoadingConfig.RabbitMQ.RabbitMQConfig.CloseAllConnections();
                Logger.Info("RabbitMQ连接已关闭");
                Logger.Info("所有资源已清理完毕");
            }
            catch (Exception ex)
            {
                Logger.Error("资源清理异常", ex);
            }
        }
    }
}