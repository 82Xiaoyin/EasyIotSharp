using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace EasyIotSharp.DataProcessor.Domain
{
    public partial class Soldiers
    {
        public string Id { get; set; }
        public int TenantNumId { get; set; }
        public bool IsSuperAdmin { get; set; }
        public int IsManager { get; set; }
        public string Mobile { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsTest { get; set; }
        public int Sex { get; set; }
        public bool IsEnable { get; set; }
        public string Email { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public DateTime CreationTime { get; set; }
        public bool IsDelete { get; set; }
        public DateTime? DeleteTime { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string OperatorId { get; set; }
        public string OperatorName { get; set; }
    }
}
