using EasyIotSharp.Core.Caching.Gateways;
using EasyIotSharp.Core.Caching.Gateways.Impl;
using EasyIotSharp.Core.Domain.Proejct;
using EasyIotSharp.Core.Dto.Gateways;
using EasyIotSharp.Core.Events.Hardware;
using EasyIotSharp.Core.Repositories.Project;
using EasyIotSharp.Core.Services.Rule;
using EasyIotSharp.GateWay.Core.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Transactions;
using UPrime;
using UPrime.Events.Bus;

namespace EasyIotSharp.GateWay.Core.Socket
{
    /// <summary>
    /// 网关连接管理器
    /// </summary>
    public class GatewayConnectionManager
    {
        private readonly IRegisteredGatewayCacheService _registeredGatewayCacheService;
        // 修改为懒加载单例模式
        private static volatile GatewayConnectionManager _instance;
        private static readonly object _lock = new object();

        /// <summary>
        /// 单例实例
        /// </summary>
        public static GatewayConnectionManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new GatewayConnectionManager();
                            LogHelper.Info("GatewayConnectionManager单例已初始化");
                        }
                    }
                }
                return _instance;
            }
        }

        private readonly Timer _statusCheckTimer;
        private const int STATUS_CHECK_INTERVAL = 30000; // 30秒检查一次
        private const int GATEWAY_TIMEOUT = 360000; // 60秒没有心跳就认为离线

        /// <summary>
        /// 连接ID到网关连接信息的映射
        /// </summary>
        private static readonly ConcurrentDictionary<IntPtr, GatewayConnectionInfo> _connectionMap =
            new ConcurrentDictionary<IntPtr, GatewayConnectionInfo>();

        /// <summary>
        /// 网关ID到连接ID的映射
        /// </summary>
        private static readonly ConcurrentDictionary<string, IntPtr> _gatewayMap =
            new ConcurrentDictionary<string, IntPtr>();

        /// <summary>
        /// 添加新连接
        /// </summary>
        public void AddConnection(IntPtr connId, string ip, ushort port)
        {
            var connectionInfo = new GatewayConnectionInfo
            {
                ConnId = connId,
                IP = ip,
                Port = port,
                ConnectTime = DateTime.Now,
                LastActiveTime = DateTime.Now,
                ReceivedPackets = 0,
                ReceivedBytes = 0,
                IsRegistered = false
            };

            _connectionMap.TryAdd(connId, connectionInfo);
        }

        public GatewayConnectionManager()
        {
            _registeredGatewayCacheService = UPrime.UPrimeEngine.Instance.Resolve<IRegisteredGatewayCacheService>();
            _statusCheckTimer = new Timer(CheckGatewayStatus, null, STATUS_CHECK_INTERVAL, STATUS_CHECK_INTERVAL);
        }

        /// <summary>
        /// 检查所有网关状态
        /// </summary>
        private  void CheckGatewayStatus(object state)
        {
            var _gatewayRepository = UPrimeEngine.Instance.Resolve<IGatewayRepository>();
            try
            {
                var now = DateTime.Now;
                var offlineGateways = new List<string>();

                foreach (var connection in _connectionMap.Values)
                {
                    if (connection.IsRegistered && !string.IsNullOrEmpty(connection.GatewayId))
                    {
                        var timeSinceLastActive = (now - connection.LastActiveTime).TotalMilliseconds;
                        if (timeSinceLastActive > GATEWAY_TIMEOUT)
                        {
                            offlineGateways.Add(connection.GatewayId);
                            // 从映射中移除超时的连接
                            _connectionMap.TryRemove(connection.ConnId, out _);
                            _gatewayMap.TryRemove(connection.GatewayId, out _);
                        }
                    }
                }

                if (offlineGateways.Any())
                {
                    using (var scope = new TransactionScope())
                    {
                        try
                        {
                            var gateways = _gatewayRepository.GetByIds(offlineGateways);

                            foreach (var gateway in gateways)
                            {
                                gateway.State = 2; // 离线状态
                                 _gatewayRepository.Update(gateway);
                            }

                            scope.Complete();

                            foreach (var gatewayId in offlineGateways)
                            {
                                LogHelper.Warn($"网关 {gatewayId} 因超时未收到心跳被标记为离线");
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Error($"更新网关离线状态时发生错误: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error($"检查网关状态时发生错误: {ex.Message}");
            }
        }

        public  void RegisterGateway(IntPtr connId, string gatewayId)
        {
            var _gatewayRepository = UPrimeEngine.Instance.Resolve<IGatewayRepository>();
            if (_connectionMap.TryGetValue(connId, out var connectionInfo))
            {
                connectionInfo.GatewayId = gatewayId;
                connectionInfo.IsRegistered = true;
                connectionInfo.RegisterTime = DateTime.Now;
                connectionInfo.LastActiveTime = DateTime.Now;

                _gatewayMap.AddOrUpdate(gatewayId, connId, (_, __) => connId);

                try
                {
                    using (var scope = new TransactionScope())
                    {
                        var gateway = _gatewayRepository.GetGateway(gatewayId);

                        if (gateway != null)
                        {
                            gateway.State = 1; // 在线状态
                             _gatewayRepository.Update(gateway);
                            scope.Complete();
                        }
                    }
                    LogHelper.Info($"网关 {gatewayId} 已注册并更新状态为在线");
                }
                catch (Exception ex)
                {
                    LogHelper.Error($"更新网关 {gatewayId} 注册状态时发生错误: {ex.Message}");
                }
            }
        }

        public  void RemoveConnection(IntPtr connId)
        {
            var _gatewayRepository = UPrimeEngine.Instance.Resolve<IGatewayRepository>();
            if (_connectionMap.TryRemove(connId, out var connectionInfo) &&
                !string.IsNullOrEmpty(connectionInfo.GatewayId))
            {
                _gatewayMap.TryRemove(connectionInfo.GatewayId, out _);

                try
                {
                    using (var scope = new TransactionScope())
                    {
                        var gateway = _gatewayRepository.GetGateway(connectionInfo.GatewayId);

                        if (gateway != null)
                        {
                            gateway.State = 2; // 离线状态
                             _gatewayRepository.Update(gateway);
                            scope.Complete();
                        }
                    }
                    LogHelper.Info($"网关 {connectionInfo.GatewayId} 连接已断开并更新状态为离线");
                }
                catch (Exception ex)
                {
                    LogHelper.Error($"更新网关 {connectionInfo.GatewayId} 离线状态时发生错误: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 更新网关心跳
        /// </summary>
        public void UpdateGatewayHeartbeat(IntPtr connId)
        {
            if (_connectionMap.TryGetValue(connId, out var connectionInfo))
            {
                connectionInfo.LastActiveTime = DateTime.Now;
            }
        }

        // 修改现有的 UpdateGatewayData 方法
        public void UpdateGatewayData(IntPtr connId, byte[] data, string dataType = "数据包")
        {
            if (_connectionMap.TryGetValue(connId, out var connectionInfo))
            {
                // 更新最后活动时间
                connectionInfo.LastActiveTime = DateTime.Now;
                connectionInfo.ReceivedPackets++;
                connectionInfo.ReceivedBytes += data.Length;

                // 将数据转换为十六进制字符串
                string hexData = BitConverter.ToString(data).Replace("-", " ");

                // 添加数据记录
                var dataRecord = new GatewayDataRecord
                {
                    Time = DateTime.Now,
                    Data = hexData,
                    Length = data.Length,
                    DataType = dataType
                };

                // 保持最近20条记录
                connectionInfo.RecentData.Add(dataRecord);
                if (connectionInfo.RecentData.Count > 20)
                {
                    connectionInfo.RecentData.RemoveAt(0);
                    // 清理缓存
                    _registeredGatewayCacheService.Clear();
                }
                var list = _registeredGatewayCacheService.GetAllRegisteredGateways(() => { return _connectionMap.Values.Where(c => c.IsRegistered).ToList(); });
                if (list.Count == 0)
                {
                    list = _connectionMap.Values.Where(c => c.IsRegistered).ToList();
                }
            }
        }

        // 在 GatewayConnectionManager 类中添加更新解析结果的方法
        /// <summary>
        /// 更新网关数据解析结果
        /// </summary>
        public void UpdateGatewayDataParsedResult(IntPtr connId, byte[] data, string parsedResult)
        {
            if (_connectionMap.TryGetValue(connId, out var connectionInfo))
            {
                string hexData = BitConverter.ToString(data).Replace("-", " ");

                // 查找最近的数据记录
                var dataRecord = connectionInfo.RecentData
                    .FirstOrDefault(r => r.Data == hexData && string.IsNullOrEmpty(r.ParsedResult));

                if (dataRecord != null)
                {
                    dataRecord.ParsedResult = parsedResult;
                }
            }
        }

        /// <summary>
        /// 通过网关ID获取连接信息
        /// </summary>
        public GatewayConnectionInfo GetConnectionByGatewayId(string gatewayId)
        {
            if (_gatewayMap.TryGetValue(gatewayId, out var connId) &&
                _connectionMap.TryGetValue(connId, out var connectionInfo))
            {
                return connectionInfo;
            }

            return null;
        }

        /// <summary>
        /// 通过连接ID获取连接信息
        /// </summary>
        public GatewayConnectionInfo GetConnectionByConnId(IntPtr connId)
        {
            _connectionMap.TryGetValue(connId, out var connectionInfo);
            return connectionInfo;
        }

        /// <summary>
        /// 获取所有连接信息
        /// </summary>
        public List<GatewayConnectionInfo> GetAllConnections()
        {
            return _connectionMap.Values.ToList();
        }

        /// <summary>
        /// 获取所有已注册的网关连接
        /// </summary>
        public List<GatewayConnectionInfo> GetAllRegisteredGateways()
        {
            var list = _registeredGatewayCacheService.GetAllRegisteredGateways(() => { return _connectionMap.Values.Where(c => c.IsRegistered).ToList(); });
            if (list.Count == 0)
            {
                list = _connectionMap.Values.Where(c => c.IsRegistered).ToList();
            }
            return list;
        }
    }
}