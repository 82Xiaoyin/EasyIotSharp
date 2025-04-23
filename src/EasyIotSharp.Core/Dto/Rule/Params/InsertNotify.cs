using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Domain.Rule;

namespace EasyIotSharp.Core.Dto.Rule.Params
{
    public class InsertNotify : Notify
    {
        public List<UserNotifyDto> Users { get; set; } = new List<UserNotifyDto>();
    }
}
