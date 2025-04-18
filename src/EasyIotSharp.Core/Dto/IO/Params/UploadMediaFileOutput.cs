using System;
using System.Collections.Generic;
using System.Text;

namespace EasyIotSharp.Core.Dto.IO.Params
{
    public class UploadMediaFileOutput
    {
        /// <summary>
        /// 源地址
        /// </summary>
        public string SourceUrl { get; set; }

        /// <summary>
        /// 原文件名
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 文件大小
        /// </summary>
        public string Size { get; set; }
    }
}
