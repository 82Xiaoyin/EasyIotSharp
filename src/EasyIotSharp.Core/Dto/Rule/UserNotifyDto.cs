using System;
using System.Collections.Generic;
using System.Text;

namespace EasyIotSharp.Core.Dto.Rule
{
    public class UserNotifyDto
    {
        public string NotifyId { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }

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
