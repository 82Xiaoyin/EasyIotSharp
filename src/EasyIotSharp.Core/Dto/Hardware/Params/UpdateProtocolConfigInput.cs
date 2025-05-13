using System;
using System.Collections.Generic;
using System.Text;

namespace EasyIotSharp.Core.Dto.Hardware.Params
{
    /// <summary>
    /// 根据id修改一条协议配置的入参类
    /// </summary>
    public class UpdateProtocolConfigInput
    {
        /// <summary>
        /// 协议id
        /// </summary>
        public string ProtocolId { get; set; }
        /// <summary>
        /// 协议配置id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 标识符
        /// （反序列化到class里面对应的字段名）
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// 标签
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// 文本提示
        /// </summary>
        public string Placeholder { get; set; }

        /// <summary>
        /// 标签
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// <see cref="TagTypeMenu"/>
        /// 标签类型  select、number、text
        /// </summary>
        public TagTypeMenu TagType { get; set; }

        /// <summary>
        /// 是否必填
        /// </summary>
        public int IsRequired { get; set; }

        /// <summary>
        /// <see cref="DataTypeMenu"/>
        /// 验证数据类型
        /// </summary>
        public DataTypeMenu ValidateType { get; set; }

        /// <summary>
        /// 验证数据类型不正确提示
        /// </summary>
        public string ValidateMessage { get; set; }

        /// <summary>
        /// 排序字段
        /// (数字越大越靠前)
        /// </summary>
        public int Sort { get; set; }

        /// <summary>
        /// 是否修改协议配置扩展
        /// </summary>
        public bool IsUpdateExt { get; set; }

        /// <summary>
        /// 下拉
        /// </summary>
        public Dictionary<string, string> Options { get; set; }
    }
}
