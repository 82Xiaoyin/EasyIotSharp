using System;
using System.Collections.Generic;
using System.Text;

namespace EasyIotSharp.Core.Dto.Rule
{
    public class AlarmsDto
    {
        public string time { get; set; }
        public bool acknowledged { get; set; }
        public string alarmlevel { get; set; }
        public string alarmtype { get; set; }
        public string conditiondetails { get; set; }
        public string message { get; set; }
        public bool notified { get; set; }
        public string pointid { get; set; }
        public string sensorid { get; set; }
        public string sensorname { get; set; }
        public string pointtype { get; set; }
        public string projectid { get; set; }
        public string remark { get; set; }
        public string ruleid { get; set; }
        public string rulename { get; set; }
        public string targetid { get; set; }
        public string threshold_operator { get; set; }
        public string threshold_value { get; set; }
        public string trigger_param { get; set; }
        public string trigger_value { get; set; }
    }
}
