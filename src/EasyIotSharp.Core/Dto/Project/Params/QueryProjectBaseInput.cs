using System;
using System.Collections.Generic;
using System.Text;

namespace EasyIotSharp.Core.Dto.Project.Params
{
    /// <summary>
    /// 分页查询项目信息的入参类
    /// </summary>
    public class QueryProjectBaseInput : PagingInput
    {
        /// <summary>
        /// 项目名称/备注
        /// </summary>
        public string Keyword { get; set; }

        /// <summary>
        /// 项目状态
        /// -1=查询全部
        /// 0=初始化状态
        /// 1=正在运行状态
        /// </summary>
        public bool? State { get; set; }

        /// <summary>
        /// 创建开始时间
        /// </summary>
        public DateTime? CreateStartTime { get; set; }

        /// <summary>
        /// 创建结束时间
        /// </summary>
        public DateTime? CreateEndTime { get; set; }
    }
}
