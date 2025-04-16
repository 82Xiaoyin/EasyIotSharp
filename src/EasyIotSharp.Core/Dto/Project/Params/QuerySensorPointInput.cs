using System;
using System.Collections.Generic;
using System.Text;

namespace EasyIotSharp.Core.Dto.Project.Params
{
    /// <summary>
    /// 通过条件查询查询测点信息的入参类
    /// </summary>
    public class QuerySensorPointInput : PagingInput
    {
        /// <summary>
        /// 测点名称
        /// </summary>
        public string Keyword { get; set; }

        /// <summary>
        /// 项目id
        /// </summary>
        public string ProjectId { get; set; }

        /// <summary>
        /// 分类id
        /// </summary>
        public string ClassificationId { get; set; }

        /// <summary>
        /// 网关id
        /// </summary>
        public string GatewayId { get; set; }

        /// <summary>
        /// 传感器Id
        /// </summary>
        public string SensorId { get; set; }
    }
}
