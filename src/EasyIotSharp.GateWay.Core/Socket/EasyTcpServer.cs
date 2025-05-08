using EasyIotSharp.GateWay.Core.Model.SocketDTO;
using EasyIotSharp.GateWay.Core.Socket.Factory;
using EasyIotSharp.GateWay.Core.Util;
using HPSocket;
using HPSocket.Tcp;
using HPSocket.Thread;
using log4net;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using ThreadPool = HPSocket.Thread.ThreadPool;
using Timer = System.Timers.Timer;

namespace EasyIotSharp.GateWay.Core.Socket
{
    public class EasyTcpServer : EasySocketBase, IDisposable
    {
        private readonly ITcpServer _server = new TcpServer();

        /// <summary>
        /// 线程池
        /// </summary>
        private readonly ThreadPool _threadPool = new ThreadPool();
        /// <summary>
        /// 定时器
        /// </summary>
        private readonly Timer _timer = new Timer(5000)
        {
            AutoReset = true,
        };

        /// <summary>
        /// 线程池回调函数
        /// </summary>
        private TaskProcEx _taskTaskProc;
        delegate void AddLogHandler(string log);
        private EasyTCPSuper tcpManufacture = null;
        private InitParamsInfo _initParamsInfo = null;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(EasyTcpServer));

        /// <summary>
        /// 最大封包长度
        /// </summary>
        private const int MaxPacketSize = 8192;

        public override async void InitTCPServer(InitParamsInfo initParamsInfo)
        {
            try
            {
                if (initParamsInfo.StartState)
                {
                    _server.SocketBufferSize = MaxPacketSize; // 4K
                    _server.Port = (ushort)initParamsInfo.LocalPort;
                    _server.OnAccept += OnAccept;
                    _server.OnReceive += OnReceive;
                    _server.OnClose += OnClose;
                    _server.OnShutdown += OnShutdown;
                    _initParamsInfo = initParamsInfo;
                    // 线程池回调函数
                    _taskTaskProc = TaskTaskProc;
                    tcpManufacture = EasyTCPFactory.CreateManufacturer(initParamsInfo.Description);
                    if (tcpManufacture == null)
                    {
                        Logger.Error("TCPFactory.CreateManufacturer 工厂创建对象失败！manufacturer = " + initParamsInfo.Description);
                        return;
                    }
                    // 定时输出线程池任务数
                    _timer.Elapsed += (_, args) =>
                    {
                        _server.DisconnectSilenceConnections(initParamsInfo.TimeOutMS, true);
                        if (_server.HasStarted && _threadPool.HasStarted && DateTime.Now.Minute == 0 && DateTime.Now.Second > 54)
                        {
                            // AddLog($"线程池当前在执行的任务数: {_threadPool.TaskCount}, 任务队列数: {_threadPool.QueueSize}");
                            Logger.Info("client当前链接数 ：" + _server.ConnectionCount + $"线程池当前在执行的任务数: {_threadPool.TaskCount}, 任务队列数: {_threadPool.QueueSize}");
                        }
                    };
                    _timer.Start();
                    // 2个线程处理耗时操作, 作为相对耗时的任务, 可根据业务需求多开线程处理
                    if (!_threadPool.Start(2, RejectedPolicy.WaitFor))
                    {
                        throw new Exception($"线程池启动失败, 错误码: {_threadPool.SysErrorCode}");
                    }

                    // 启动服务
                    if (!_server.Start())
                    {
                        Logger.Error($"error code: {_server.ErrorCode}, error message: {_server.ErrorMessage}");
                    }
                    Logger.Info("TcpServer 已启动 ..................................端口号 Port: " + _server.Port);

                    // 停止并等待线程池任务全部完成
                    await _threadPool.WaitAsync();
                    // 等待服务停止
                    await _server.WaitAsync();
                }
                else
                {
                    // 停止并等待线程池任务全部完成
                    await _threadPool.StopAsync();
                    // 等待服务停止
                    await _server.StopAsync();
                    _server.OnReceive -= OnReceive;
                    _server.OnAccept -= OnAccept;
                    _server.OnClose -= OnClose;
                    _server.OnShutdown -= OnShutdown;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("TCPUtil.InitTCPServr 初始化失败！=> " + ex.ToString());
                return;
            }
        }

        private HandleResult OnShutdown(IServer sender)
        {
            //AddLog($"OnShutdown({sender.Address}:{sender.Port})");

            return HandleResult.Ok;
        }

        /// <summary>
        /// Server连接事件
        /// </summary>
        private HandleResult OnAccept(IServer sender, IntPtr connId, IntPtr client)
        {
            // 获取客户端地址
            if (!sender.GetRemoteAddress(connId, out var ip, out var port))
            {
                return HandleResult.Error;
            }
            // 设置附加数据, 用来做粘包处理
            sender.SetExtra(connId, new ClientInfo<IntPtr>
            {
                ConnId = connId,
                ConfigJson=""
            });
            
            // 添加到网关连接管理器
            GatewayConnectionManager.Instance.AddConnection(connId, ip, port);
            
            Logger.Info($"OnAccept({connId}), ip: {ip}, port: {port}");
            return HandleResult.Ok;
        }

        /// <summary>
        /// 数据监听事件
        /// </summary>
        private HandleResult OnReceive(IServer sender, IntPtr connId, byte[] data)
        {
            var client = sender.GetExtra<ClientInfo<IntPtr>>(connId);
            if (client == null)
            {
                return HandleResult.Error;
            }
            
            // 更新网关数据记录
            GatewayConnectionManager.Instance.UpdateGatewayData(connId, data);
            
            HandleResult result;
            result = OnProcessFullPacket(sender, client, data);
            return result;
        }

        private HandleResult OnClose(IServer sender, IntPtr connId, SocketOperation socketOperation, int errorCode)
        {
            var client = sender.GetExtra<ClientInfo<IntPtr>>(connId);
            if (client != null)
            {
                sender.RemoveExtra(connId);
                if (client.PacketData != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(client.PacketData);
                }
                
                // 从网关连接管理器中移除
                GatewayConnectionManager.Instance.RemoveConnection(connId);
                
                return HandleResult.Error;
            }
            Logger.Info($"OnClose({connId}), socket operation: {socketOperation}, error code: {errorCode}");
            return HandleResult.Ok;
        }

        private HandleResult OnProcessFullPacket(IServer sender, ClientInfo client, byte[] data)
        {
            // 这里来的都是完整的包, 但是这里不做耗时操作, 仅把数据放入队列
            // 向线程池提交任务
            var result = HandleResult.Ok;
            var packet = new Packet()
            {
                BData = data
            };
            if (!_threadPool.Submit(_taskTaskProc, new TaskInfo
            {
                Client = client,
                Server = sender,
                Packet = packet,
                _initParamsInfo = this._initParamsInfo
            }))
            {
                result = HandleResult.Error;
            }
            return result;
        }
        /// <summary>
        /// 线程池任务回调函数
        /// </summary>
        /// <param name="obj">任务参数</param>
        private void TaskTaskProc(object obj)
        {

            if (!(obj is TaskInfo))
            {
                return;
            }
            TaskInfo taskData = (TaskInfo)obj;
            tcpManufacture.DecodeData(taskData);
        }
       
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            try
            {
                _timer?.Stop();
                _timer?.Dispose();
                _threadPool?.Stop();
                _server?.Stop();
            }
            catch (Exception ex)
            {
                Logger.Error("释放资源异常", ex);
            }
        }
    }
}
