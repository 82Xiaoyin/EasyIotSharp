using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Domain.Rule;

namespace EasyIotSharp.Core.Dto.Rule
{
    public class AlarmsConfigDto : AlarmsConfig
    {
        public string NotifyName { get; set; }
    }
}
