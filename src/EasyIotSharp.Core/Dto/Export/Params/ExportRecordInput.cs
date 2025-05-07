using System;
using System.Collections.Generic;
using System.Text;

namespace EasyIotSharp.Core.Dto.Export.Params
{
    public class ExportRecordInput : PagingInput
    {
        /// <summary>
        /// 项目ID
        /// </summary>
        public string ProjectId { get; set; }
    }
}
