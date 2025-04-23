using System;
using System.Collections.Generic;
using System.Text;
using SqlSugar;

namespace EasyIotSharp.Core.Domain.Rule
{
    /// <summary>
    /// 通知组
    /// </summary>
    [SugarTable("Notify")]
    public class Notify : BaseEntity<string>
    {
        /// <summary>
        /// 通知组名称
        /// </summary>
        public string NotifyName { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public bool State { get; set; }

        /// <summary>
        /// 类型（1.成员 2.邮箱 3.短信 4.语音 5.钉钉）
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string NotifyContent { get; set; }
    }
}
