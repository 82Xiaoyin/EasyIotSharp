using System;
using System.Collections.Generic;
using System.Text;

namespace EasyIotSharp.Core.Dto.Export
{
    public class ExportDataRecordDto
    {
        /// <summary>
        /// 主键
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 租户Id
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// 项目Id
        /// </summary>
        public string ProjectId { get; set; }

        /// <summary>
        /// 执行时间
        /// </summary>
        public DateTime? ExecuteTime { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public int State { get; set; }

        /// <summary>
        /// 资源Id
        /// </summary>
        public string ResourceId { get; set; }
    }
}
