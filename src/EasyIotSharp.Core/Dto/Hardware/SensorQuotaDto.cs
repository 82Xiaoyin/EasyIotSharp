using EasyIotSharp.Core.Domain.Hardware;
using System;
using System.Collections.Generic;
using System.Text;
using UPrime.AutoMapper;

namespace EasyIotSharp.Core.Dto.Hardware
{
    /// <summary>
    /// 代表一个传感器类型的指标的"DTO"
    /// </summary>
    [AutoMapFrom(typeof(SensorQuota))]
    public class SensorQuotaDto
    {
        /// <summary>
        /// id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 租户id
        /// </summary>
        public int TenantNumId { get; set; }

        /// <summary>
        /// 传感器类型id
        /// </summary>
        public string SensorId { get; set; }

        /// <summary>
        /// 传感器类型名称
        /// </summary>
        public string SensorName { get; set; }

        /// <summary>
        /// 指标名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 指标标识符
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// 操作类型
        /// R=只读
        /// W=只写
        /// RW=读写
        /// </summary>
        public string ControlsType { get; set; }

        /// <summary>
        /// <see cref="DataTypeMenu"/>
        /// 数据类型
        /// </summary>
        public int DataType { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// 小数位精度
        /// 数据类型是double float  小数点取几位
        /// -1=全部
        /// 0=零位
        /// 1=一位
        /// </summary>
        public int Precision { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 排序字段
        /// (数字越大越靠前)
        /// </summary>
        public int Sort { get; set; }

        /// <summary>
        /// 是否展示
        /// </summary>
        public bool IsShow { get; set; }

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
