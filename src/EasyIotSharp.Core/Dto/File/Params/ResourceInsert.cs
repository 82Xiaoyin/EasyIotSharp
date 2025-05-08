using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Dto.Enum;
using Microsoft.AspNetCore.Http;

namespace EasyIotSharp.Core.Dto.File.Params
{
    public class ResourceInsert
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 文件类型
        /// </summary>
        public ResourceEnums ResourceType { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 文件流
        /// </summary>
        public IFormFile FormFile { get; set; }

        /// <summary>
        /// 租互别名
        /// </summary>
        public string Abbreviation { get; set; }
    }
}
