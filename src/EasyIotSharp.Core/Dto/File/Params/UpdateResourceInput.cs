using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Domain.Files;
using Microsoft.AspNetCore.Http;

namespace EasyIotSharp.Core.Dto.File.Params
{
    public class UpdateResourceInput : Resource
    {

        /// <summary>
        /// 文件流
        /// </summary>
        public IFormFile FormFile { get; set; }
    }
}
