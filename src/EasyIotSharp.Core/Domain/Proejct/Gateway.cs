using System;
using System.Collections.Generic;
using System.Text;

namespace EasyIotSharp.Core.Domain.Proejct
{
    /// <summary>
    /// 网关设备表
    /// </summary>
    public class Gateway:BaseEntity<string>
    {
        /// <summary>
        /// 租户id
        /// </summary>
        public int TenantNumId { get; set; }

        /// <summary>
        /// 设备名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 设备状态
        /// 0=初始化
        /// 1=在线
        /// 2=离线
        /// </summary>
        public int State { get; set; }

        /// <summary>
        /// 协议id
        /// </summary>
        public string ProtocolId { get; set; }

        /// <summary>
        /// 项目id
        /// </summary>
        public string ProjectId { get; set; }

        /// <summary>
        ///IMEI号
        /// </summary>
        public string Imei { get; set; }

        /// <summary>
        /// 设备型号
        /// </summary>
        public string DeviceModel { get; set; }
    }
}
