using System;
using System.Collections.Generic;
using System.Text;

namespace EasyIotSharp.Core.Domain.Proejct
{
    /// <summary>
    /// 项目表
    /// </summary>
    /// <remarks>每个租户可以创建多个项目，每个项目包含测点、设备等数据收集部署</remarks>
    public class ProjectBase:BaseEntity<string>
    {
        /// <summary>
        /// 租户id
        /// </summary>
        public int TenantNumId { get; set; }

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
        /// 项目状态
        /// 0=初始化状态
        /// 1=正在运行状态
        /// </summary>
        public int State { get; set; }

        /// <summary>
        /// 项目地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// unity资源Id
        /// </summary>
        public string ResourceId { get; set; }

        /// <summary>
        /// 项目描述
        /// </summary>
        public string Remark { get; set; }
    }
}
