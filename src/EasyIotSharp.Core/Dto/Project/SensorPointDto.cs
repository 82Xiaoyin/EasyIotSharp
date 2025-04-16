using EasyIotSharp.Core.Domain.Proejct;
using System;
using System.Collections.Generic;
using System.Text;
using UPrime.AutoMapper;

namespace EasyIotSharp.Core.Dto.Project
{
    /// <summary>
    /// 代表一条测点信息的"DTO"
    /// </summary>
    [AutoMapFrom(typeof(SensorPoint))]
    public class SensorPointDto
    {
        /// <summary>
        /// 测点id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 租户id
        /// </summary>
        public int TenantNumId { get; set; }

        /// <summary>
        /// 测点名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 项目id
        /// </summary>
        public string ProjectId { get; set; }

        /// <summary>
        /// 项目名称
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        /// 分类id
        /// </summary>
        public string ClassificationId { get; set; }

        /// <summary>
        /// 分类名称
        /// </summary>
        public string ClassificationName { get; set; }

        /// <summary>
        /// 网关id
        /// </summary>
        public string GatewayId { get; set; }

        /// <summary>
        /// 网关名称
        /// </summary>
        public string GatewayName { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public int State { get; set; }

        /// <summary>
        /// 传感器Id
        /// </summary>
        public string SensorId { get; set; }

        /// <summary>
        /// 传感器名称
        /// </summary>
        public string SensorName { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// 操作人标识
        /// </summary>
        public string OperatorId { get; set; }

        /// <summary>
        /// 操作人
        /// </summary>
        public string OperatorName { get; set; }
    }
}
