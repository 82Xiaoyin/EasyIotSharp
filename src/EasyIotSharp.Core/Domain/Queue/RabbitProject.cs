using System;
using System.Collections.Generic;
using System.Text;
using SqlSugar;

namespace EasyIotSharp.Core.Domain.Queue
{
    /// <summary>
    /// 配置项目关系表
    /// </summary>
    [SugarTable("Rabbit_Project")]
    public class RabbitProject : BaseEntity<string>
    {
        public string RabbitServerInfoId { get; set; }
        public string ProjectId { get; set; }
    }
}
