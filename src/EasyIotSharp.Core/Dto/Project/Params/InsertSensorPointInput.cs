﻿using System;
using System.Collections.Generic;
using System.Text;

namespace EasyIotSharp.Core.Dto.Project.Params
{
    /// <summary>
    /// 添加一条测点信息的入参类
    /// </summary>
    public class InsertSensorPointInput
    {
        /// <summary>
        /// 测点名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 项目id
        /// </summary>
        public string ProjectId { get; set; }

        /// <summary>
        /// 分类id
        /// </summary>
        public string ClassificationId { get; set; }


        /// <summary>
        /// 传感器Id
        /// </summary>
        public string SensorId { get; set; }
    }
}
