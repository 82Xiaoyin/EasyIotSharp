using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace EasyIotSharp.DataProcessor.Domain
{
    public partial class RabbitProject
    {
        public string Id { get; set; }
        public string RabbitServerInfoId { get; set; }
        public string ProjectId { get; set; }
        public DateTime CreationTime { get; set; }
        public bool IsDelete { get; set; }
        public DateTime? DeleteTime { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string OperatorId { get; set; }
        public string OperatorName { get; set; }
    }
}
