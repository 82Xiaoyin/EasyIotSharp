using System;
using System.Collections.Generic;
using System.Text;

namespace EasyIotSharp.Core.Dto.Project
{
    public class SensorPointChart
    {
        /// <summary>
        /// 离线
        /// </summary>
        public int Offline { get; set; }

        /// <summary>
        /// 在线
        /// </summary>
        public int Online { get; set; }

        /// <summary>
        /// 占比
        /// </summary>
        public decimal OnlineRatio { get; set; }

        /// <summary>
        /// 总数
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// 报警数量
        /// </summary>
        public int AlarmsCount { get; set; }

        /// <summary>
        /// 报警占比
        /// </summary>
        public decimal AlarmsRatio { get; set; }
    }
}
