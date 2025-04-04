﻿using System;
using System.Collections.Generic;
using System.Text;

namespace EasyIotSharp.Core.Dto.Hardware.Params
{
    /// <summary>
    /// 通过id删除一条协议配置的入参类
    /// </summary>
    public class DeleteProtocolConfigInput
    {
        /// <summary>
        /// 协议id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDelete { get; set; }
    }
}
