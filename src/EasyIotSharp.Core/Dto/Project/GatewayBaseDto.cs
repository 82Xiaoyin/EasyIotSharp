using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Domain.Proejct;

namespace EasyIotSharp.Core.Dto.Project
{
    public class GatewayBaseDto: Gateway
    {
        /// <summary>
        /// 租户简称
        /// </summary>
        public string TenantAbbreviation { get; set; }
    }
}
