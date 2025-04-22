using System;
using System.Collections.Generic;
using System.Text;
using SqlSugar;

namespace EasyIotSharp.Core.Domain.Rule
{
    /// <summary>
    /// 用户通知关系表
    /// </summary>
    [SugarTable("UserNotify")]
    public class UserNotify : BaseEntity<string>
    {
        /// <summary>
        /// 通知组Id
        /// </summary>
        public string NotifyId { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// 是否短信通知
        /// </summary>
        public bool IsSms { get; set; }

        /// <summary>
        /// 是否邮箱通知
        /// </summary>
        public bool IsEmail { get; set; }

        /// <summary>
        /// 是否语音通知
        /// </summary>
        public bool IsPhone { get; set; }
    }
}
