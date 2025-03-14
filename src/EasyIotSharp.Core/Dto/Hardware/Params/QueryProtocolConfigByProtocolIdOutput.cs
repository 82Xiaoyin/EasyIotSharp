﻿using EasyIotSharp.Core.Domain.Hardware;
using System;
using System.Collections.Generic;
using System.Text;
using UPrime.AutoMapper;
using UPrime.CodeAnnotations;

namespace EasyIotSharp.Core.Dto.Hardware.Params
{
    /// <summary>
    /// 通过协议id获取协议配置列表的出参类
    /// </summary>
    [AutoMapFrom(typeof(ProtocolConfig))]
    public class QueryProtocolConfigByProtocolIdOutput
    {
        /// <summary>
        /// id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 协议id
        /// </summary>
        public string ProtocolId { get; set; }

        /// <summary>
        /// 协议名称
        /// </summary>
        public string ProtocolName { get; set; }

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
        /// <see cref="TagTypeMenu"/>
        /// 标签类型  select、number、text
        /// </summary>
        public string TagTypeName
        {
            get
            {
                return TagType.ToAlias();
            }
        }

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
        /// <see cref="DataTypeMenu"/>
        /// 验证数据类型
        /// </summary>
        public string ValidateTypeName
        {
            get
            {
                return ValidateType.ToAlias();
            }
        }

        /// <summary>
        /// 验证数据类型不正确提示
        /// </summary>
        public string ValidateMessage { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }


        /// <summary>
        /// 下拉
        /// </summary>
        public Dictionary<string,string> Options { get; set; }
    }
}
