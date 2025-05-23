﻿using System;
using System.Collections.Generic;
using System.Text;

namespace EasyIotSharp.Core.Dto.Hardware.Params
{
    public class DataRespost
    {
        /// <summary>
        /// 租户别名
        /// </summary>
        public string Abbreviation { get; set; }

        /// <summary>
        /// 设备Id
        /// </summary>
        public string SensorId { get; set; }

        /// <summary>
        /// 测点Id
        /// </summary>
        public string SensorPointId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string SensorQuotaId { get; set; }

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

        /// <summary>
        /// 是否排序
        /// </summary>
        public bool IsSort { get; set; }
    }
}
