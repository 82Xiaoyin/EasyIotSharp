using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EasyIotSharp.Core.Dto.IO.Params
{
    public class UploadMediaFileInput
    {
        /// <summary>
        /// 图片
        /// </summary>
        [Required]
        public IFormFile FormFile { get; set; }
    }
}
