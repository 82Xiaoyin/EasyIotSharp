using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Domain.Files;
using EasyIotSharp.Core.Dto.Enum;
using Microsoft.AspNetCore.Http;
using SqlSugar;

namespace EasyIotSharp.Core.Dto.File.Params
{
    public class UpdateResourceInput 
    {
        /// <summary>
        /// 主键
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 租户Id
        /// </summary>
        public string TenantId { get; set; }
        /// <summary>
        /// 资源名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 路由
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public bool State { get; set; }

        /// <summary>
        /// 类型
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
