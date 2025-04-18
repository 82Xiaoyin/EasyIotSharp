using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace EasyIotSharp.DataProcessor.Domain
{
    public partial class Tenants
    {
        public string Id { get; set; }
        public int NumId { get; set; }
        public string Name { get; set; }
        public string StoreKey { get; set; }
        public string ContractName { get; set; }
        public string ContractOwnerName { get; set; }
        public string ContractOwnerMobile { get; set; }
        public DateTime ContractStartTime { get; set; }
        public DateTime ContractEndTime { get; set; }
        public string Mobile { get; set; }
        public string Owner { get; set; }
        public string StoreLogoUrl { get; set; }
        public string Remark { get; set; }
        public string Email { get; set; }
        public string ManagerId { get; set; }
        public int VersionTypeId { get; set; }
        public bool IsFreeze { get; set; }
        public string FreezeDes { get; set; }
        public DateTime CreationTime { get; set; }
        public bool IsDelete { get; set; }
        public DateTime? DeleteTime { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string OperatorId { get; set; }
        public string OperatorName { get; set; }
    }
}
