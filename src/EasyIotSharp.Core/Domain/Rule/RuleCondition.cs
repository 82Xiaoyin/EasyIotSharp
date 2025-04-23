using System;
using System.Collections.Generic;
using System.Text;
using SqlSugar;

namespace EasyIotSharp.Core.Domain.Rule
{
    /// <summary>
    /// 规则链条件表
    /// </summary>
    [SugarTable("RuleCondition")]
    public class RuleCondition : BaseEntity<string>
    {
        /// <summary>
        /// 条件Code
        /// </summary>
        public string RuleConditionCode { get; set; }

        /// <summary>
        /// 父级Id
        /// </summary>
        public string ParentId { get; set; }

        /// <summary>
        /// 标签
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// 标签
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// 文本提示
        /// </summary>
        public string Placeholder { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// 标签类型
        /// </summary>
        public string TagType { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnable { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [SugarColumn(Length = 500)]
        public string Remark { get; set; }
    }
}
