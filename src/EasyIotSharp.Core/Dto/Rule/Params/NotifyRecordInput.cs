using System;
using System.Collections.Generic;
using System.Text;

namespace EasyIotSharp.Core.Dto.Rule.Params
{
    public class NotifyRecordInput : PagingInput
    {

        public string KeyWord { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public int? Type { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StarTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }
    }
}
