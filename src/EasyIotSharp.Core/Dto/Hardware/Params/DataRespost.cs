using System;
using System.Collections.Generic;
using System.Text;

namespace EasyIotSharp.Core.Dto.Hardware.Params
{
    public class DataRespost
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
        /// 测点Id
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
        /// 当前开始页
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// 页大小
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 是否分页
        /// </summary>
        public bool IsPage { get; set; }
    }
}
