using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Domain.Rule;

namespace EasyIotSharp.Core.Dto.Rule
{
    public class NotifyDto : Notify
    {
        public List<UserNotifyDto> Users { get; set; } = new List<UserNotifyDto>();
    }
}
