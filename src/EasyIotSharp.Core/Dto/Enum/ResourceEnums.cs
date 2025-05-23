﻿using System;
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
        [EnumAlias("Sensor")]
        Sensor = 4,

        /// <summary>
        /// string
        /// </summary>
        [EnumAlias("File")]
        File = 3,

        /// <summary>
        /// int
        /// </summary>
        [EnumAlias("Image")]
        Image = 2,

        /// <summary>
        /// double
        /// </summary>
        [EnumAlias("Unity")]
        Unity = 1,

        /// <summary>
        /// string
        /// </summary>
        [EnumAlias("全部")]
        全部 = -1,
    }
}
