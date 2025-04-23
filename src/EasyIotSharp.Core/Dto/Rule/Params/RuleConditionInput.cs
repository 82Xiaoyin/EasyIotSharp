using System;
using System.Collections.Generic;
using System.Text;

namespace EasyIotSharp.Core.Dto.Rule.Params
{
    public class RuleConditionInput : PagingInput
    {
        /// <summary>
        /// Code
        /// </summary>
        public string RuleCode { get; set; }

        /// <summary>
        /// 文本参数
        /// </summary>
        public string KeyWord { get; set; }

        /// <summary>
        /// 是否有效
        /// </summary>
        public int IsEnable { get; set; }
    }
}
