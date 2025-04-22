using System;
using System.Collections.Generic;
using System.Text;
using SqlSugar;

namespace EasyIotSharp.Core.Domain.Rule
{
    /// <summary>
    /// 配置项目关系表
    /// </summary>
    [SugarTable("AlarmsConfig")]
    public class AlarmsConfig : BaseEntity<string>
    {
        /// <summary>
        /// 报警名称
        /// </summary>
        public string AlarmsName { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 级别
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public bool State { get; set; }

        /// <summary>
        /// 通知组Id
        /// </summary>
        public string NotifyId { get; set; }
    }

}
