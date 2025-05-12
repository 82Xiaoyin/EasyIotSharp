using System;
using System.Collections.Generic;
using System.Text;

namespace EasyIotSharp.Core.Dto.Export.Params
{
    public class ExportReportInput : PagingInput
    {
        /// <summary>
        /// 项目Id
        /// </summary>
        public string ProjectId { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public int? Type { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }
    }
}
