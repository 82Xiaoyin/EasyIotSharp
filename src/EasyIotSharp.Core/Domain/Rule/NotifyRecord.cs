using System;
using System.Collections.Generic;
using System.Text;
using SqlSugar;

namespace EasyIotSharp.Core.Domain.Rule
{
    /// <summary>
    /// 通知记录
    /// </summary>
    [SugarTable("NotifyRecord")]
    public class NotifyRecord : BaseEntity<string>
    {
        /// <summary>
        /// 类型（1.成员 2.邮箱 3.短信 4.语音 5.钉钉）
        /// </summary>
        public int Type { get; set; }
        /// <summary>
        /// 发生时间
        /// </summary>
        public DateTime SendTime { get; set; }

        /// <summary>
        /// 发生内容
        /// </summary>
        public string SendContent { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>
        public string SendUserId { get; set; }

        /// <summary>
        /// 用户Name
        /// </summary>
        public string SendUserName { get; set; }

        /// <summary>
        /// 发生结果
        /// </summary>
        public string SendResult { get; set; }
    }
}
