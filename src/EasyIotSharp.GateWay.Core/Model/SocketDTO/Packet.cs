 using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace EasyIotSharp.GateWay.Core.Model.SocketDTO
{
    /// <summary>
    /// 封包
    /// </summary>
    public class Packet
    {
        public PacketType Type { get; set; } // 封包类型

        public byte[] BData { get; set; }  // 数据
    }
    /// <summary>
    /// 封包类型
    /// </summary>
    public enum PacketType
    {
        /// <summary>
        /// 回显
        /// </summary>
        Echo = 1,
        /// <summary>
        /// 时间
        /// </summary>
        Time
    }
}
