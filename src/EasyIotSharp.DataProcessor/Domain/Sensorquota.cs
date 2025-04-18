using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace EasyIotSharp.DataProcessor.Domain
{
    public partial class Sensorquota
    {
        public string Id { get; set; }
        public int TenantNumId { get; set; }
        public string SensorId { get; set; }
        public string Name { get; set; }
        public string Identifier { get; set; }
        public string ControlsType { get; set; }
        public int DataType { get; set; }
        public string Unit { get; set; }
        public int Precision { get; set; }
        public string Remark { get; set; }
        public int Sort { get; set; }
        public DateTime CreationTime { get; set; }
        public bool IsDelete { get; set; }
        public DateTime? DeleteTime { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string OperatorId { get; set; }
        public string OperatorName { get; set; }
    }
}
