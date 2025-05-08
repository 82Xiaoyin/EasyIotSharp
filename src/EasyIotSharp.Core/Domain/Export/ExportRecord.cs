using System;
using System.Collections.Generic;
using System.Text;
using SqlSugar;

namespace EasyIotSharp.Core.Domain.Export
{
    /// <summary>
    /// 导出记录
    /// </summary>
    public class ExportRecord : BaseEntity<string>
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 租户Id
        /// </summary>
        public string TenantId { get; set; }


        /// <summary>
        /// 项目Id
        /// </summary>
        public string ProjectId { get; set; }

        /// <summary>
        /// Json条件
        /// </summary>
        public string ConditionJson { get; set; }

        /// <summary>
        /// 执行时间
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? ExecuteTime { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public int State { get; set; }

        /// <summary>
        /// 资源Id
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public string ResourceId { get; set; }
    }
}
