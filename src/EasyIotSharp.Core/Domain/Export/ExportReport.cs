using System;
using System.Collections.Generic;
using System.Text;
using SqlSugar;

namespace EasyIotSharp.Core.Domain.Export
{
    /// <summary>
    /// 导出记录
    /// </summary>
    public class ExportReport : BaseEntity<string>
    {
        /// <summary>
        /// 项目名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 项目Id
        /// </summary>
        public string ProjectId { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public int State { get; set; }

        /// <summary>
        /// 执行时间
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? ExecuteTime { get; set; }

        /// <summary>
        /// 资源Id
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public string ResourceId { get; set; }
    }
}
