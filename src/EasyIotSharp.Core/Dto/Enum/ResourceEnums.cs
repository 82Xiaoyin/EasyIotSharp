using System;
using System.Collections.Generic;
using System.Text;
using UPrime.CodeAnnotations;

namespace EasyIotSharp.Core.Dto.Enum
{
    /// <summary>
    /// 资源枚举
    /// </summary>
    public enum ResourceEnums
    {
        /// <summary>
        /// string
        /// </summary>
        [EnumAlias("File")]
        File = 1,

        /// <summary>
        /// int
        /// </summary>
        [EnumAlias("Image")]
        Image = 2,

        /// <summary>
        /// double
        /// </summary>
        [EnumAlias("Unity")]
        Unity = 3,
    }
}
