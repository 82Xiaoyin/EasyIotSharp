using System;
using System.Collections.Generic;
using System.Text;

namespace EasyIotSharp.Core.Dto.Project.Params
{
    /// <summary>
    /// 根据id修改一条网关信息的入参类
    /// </summary>
    public class UpdateGatewayInput
    {
        /// <summary>
        /// 设备id
        /// </summary>
        public string Id { get; set; }

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
