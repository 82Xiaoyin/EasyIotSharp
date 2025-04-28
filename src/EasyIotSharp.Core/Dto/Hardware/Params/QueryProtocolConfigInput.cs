using System;
using System.Collections.Generic;
using System.Text;

namespace EasyIotSharp.Core.Dto.Hardware.Params
{
    /// <summary>
    /// 通过条件分页查询协议配置的入参类
    /// </summary>
    public class QueryProtocolConfigInput : PagingInput
    {
        /// <summary>
        /// 协议id
        /// </summary>
        public string ProtocolId { get; set; }

        /// <summary>
        /// label/placeholder/tag
        /// </summary>
        public string Keyword { get; set; }

        /// <summary>
        /// <see cref="TagTypeMenu"/>
        /// 标签类型  select、number、text
        /// </summary>
        public TagTypeMenu TagType { get; set; }
    }
}
