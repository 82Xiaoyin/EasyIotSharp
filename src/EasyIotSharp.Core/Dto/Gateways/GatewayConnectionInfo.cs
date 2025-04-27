using System;
using System.Collections.Generic;
using System.Text;

namespace EasyIotSharp.Core.Dto.Gateways
{
    /// <summary>
    /// 网关连接信息
    /// </summary>
    public class GatewayConnectionInfo
    {
        /// <summary>
        /// 连接ID
        /// </summary>
        public IntPtr ConnId { get; set; }

        /// <summary>
        /// 网关ID
        /// </summary>
        public string GatewayId { get; set; }

        /// <summary>
        /// 客户端IP
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        /// 客户端端口
        /// </summary>
        public ushort Port { get; set; }

        /// <summary>
        /// 连接时间
        /// </summary>
        public DateTime ConnectTime { get; set; }

        /// <summary>
        /// 最后活动时间
        /// </summary>
        public DateTime LastActiveTime { get; set; }

        /// <summary>
        /// 接收的数据包数量
        /// </summary>
        public int ReceivedPackets { get; set; }

        /// <summary>
        /// 接收的总字节数
        /// </summary>
        public long ReceivedBytes { get; set; }

        /// <summary>
        /// 最近接收的数据(最多保存20条)
        /// </summary>
        public List<GatewayDataRecord> RecentData { get; set; } = new List<GatewayDataRecord>();

        /// <summary>
        /// 是否已注册(收到注册包)
        /// </summary>
        public bool IsRegistered { get; set; }

        /// <summary>
        /// 注册时间
        /// </summary>
        public DateTime? RegisterTime { get; set; }
    }

    /// <summary>
    /// 网关数据记录
    /// </summary>
    public class GatewayDataRecord
    {
        /// <summary>
        /// 接收时间
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// 数据内容(十六进制字符串)
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// 数据长度
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// 数据类型(如：注册包、心跳包、数据包等)
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// 解析结果
        /// </summary>
        public string ParsedResult { get; set; }
    }
}
