using System;
using System.Collections.Generic;
using System.Text;

namespace EasyIotSharp.Core.Dto.Project.Params
{
    /// <summary>
    /// 添加一个项目信息的入参类
    /// </summary>
    public class InsertProjectBaseInput
    {
        /// <summary>
        /// 服务器配置表Id
        /// </summary>
        public string RabbitServerInfoId { get; set; } 
        /// <summary>
        /// 项目名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 经度
        /// </summary>
        public string Longitude { get; set; }

        /// <summary>
        /// 纬度
        /// </summary>
        public string latitude { get; set; }

        /// <summary>
        /// 项目地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 项目描述
        /// </summary>
        public string Remark { get; set; }
    }
}
