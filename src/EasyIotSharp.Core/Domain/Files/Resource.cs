﻿using System;
using System.Collections.Generic;
using System.Text;
using SqlSugar;

namespace EasyIotSharp.Core.Domain.Files
{
    /// <summary>
    /// 资源表
    /// </summary>
    public class Resource : BaseEntity<string>
    {

        /// <summary>
        /// 租户Id
        /// </summary>
        public string TenantId { get; set; }
        /// <summary>
        /// 资源名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 路由
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT")]
        public string Url { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public bool State { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string Remark { get; set; }
    }
}
