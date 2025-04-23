using System;
using System.Collections.Generic;
using System.Text;
using SqlSugar;

namespace EasyIotSharp.Core.Domain.Rule
{
    /// <summary>
    /// 场景联动
    /// </summary>
    [SugarTable("RuleChain")]
    public class RuleChain : BaseEntity<string>
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 项目Id
        /// </summary>
        public string ProjectId { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public bool State { get; set; }

        /// <summary>
        /// 条件JSON储存
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT")]
        public string RuleContentJson { get; set; }

        /// <summary>
        /// 报警JSON
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT")]
        public string AlarmsJSON { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Remark { get; set; }
    }
}
