using System;
using System.Collections.Generic;
using System.Text;

namespace EasyIotSharp.Core.Dto.Export.Params
{
    public class ExportDataInput
    {
        /// <summary>
        /// 设备Id
        /// </summary>
        public string SensorId { get; set; }

        /// <summary>
        /// 测点Id
        /// </summary>
        public string SensorPointId { get; set; }

        /// <summary>
        /// 测点记录类型Id
        /// </summary>
        public string SensorQuotaId { get; set; }

        /// <summary>
        /// 项目Id
        /// </summary>
        public string ProjectId { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }

    }
}
