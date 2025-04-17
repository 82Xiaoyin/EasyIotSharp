using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace EasyIotSharp.DataProcessor.Domain
{
    public partial class Protocolconfig
    {
        public string Id { get; set; }
        public DateTime CreationTime { get; set; }
        public bool IsDelete { get; set; }
        public DateTime? DeleteTime { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string OperatorId { get; set; }
        public string OperatorName { get; set; }
        public string ProtocolId { get; set; }
        public string Identifier { get; set; }
        public string Label { get; set; }
        public string Placeholder { get; set; }
        public string Tag { get; set; }
        public int TagType { get; set; }
        public int IsRequired { get; set; }
        public int ValidateType { get; set; }
        public string ValidateMessage { get; set; }
        public int Sort { get; set; }
    }
}
