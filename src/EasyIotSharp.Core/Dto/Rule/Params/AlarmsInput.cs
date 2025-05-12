using System;
using System.Collections.Generic;
using System.Text;

namespace EasyIotSharp.Core.Dto.Rule.Params
{
    public class AlarmsInput : PagingInput
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

        /// <summary>
        /// 排序
        /// </summary>
        public bool IsSort { get; set; }

        /// <summary>
        /// 租户
        /// </summary>
        public string Abbreviation { get; set; }
    }
}
