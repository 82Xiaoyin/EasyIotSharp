using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Domain.Files;
using EasyIotSharp.Core.Dto.Enum;

namespace EasyIotSharp.Core.Dto.File.Params
{
    public class ResourceInput : PagingInput
    {
        /// <summary>
        /// 状态
        /// </summary>
        public bool? State { get; set; }

        public ResourceEnums ResourceType { get; set; }
    }
}
