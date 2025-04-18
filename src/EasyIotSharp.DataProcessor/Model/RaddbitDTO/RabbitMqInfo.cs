using System;
using System.Collections.Generic;
using System.Text;

namespace EasyIotSharp.DataProcessor.Model.RaddbitDTO
{
    public class RabbitMqInfo
    {
        public int Id { get; set; }
        public string MqId { get; set; }
        public string ProjectId { get; set; }
        public string Extendstr { get; set; }
        public int Extendint { get; set; }
        public DateTime? CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Virtualhost { get; set; }
        public string Exchange { get; set; }
        public string RoutingKey { get; set; } // 如果数据库中没有此字段，可以在代码中设置默认值
    }
}
