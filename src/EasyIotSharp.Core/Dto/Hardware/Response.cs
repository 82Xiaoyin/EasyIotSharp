using System;
using System.Collections.Generic;
using System.Text;

namespace EasyIotSharp.Core.Dto.Hardware
{
    public class Response
    {
        public object Data { get; set; }
        public List<Quotas> Quotas { get; set; } = new List<Quotas> { };
    }

    public class Quotas
    {
        public string Identifier { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public bool IsShow { get; set; }
    }
}
