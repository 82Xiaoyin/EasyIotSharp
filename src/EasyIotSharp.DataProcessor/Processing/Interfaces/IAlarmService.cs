using EasyIotSharp.Core.Domain.Rule;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyIotSharp.DataProcessor.Processing.Interfaces
{
    /// <summary>
    /// 告警服务接口
    /// </summary>
    public interface IAlarmService
    {
        /// <summary>
        /// 根据告警场景ID获取告警配置
        /// </summary>
        /// <param name="alarmSceneId">告警场景ID</param>
        /// <returns>告警配置</returns>
        Task<AlarmsConfig> GetAlarmConfigByIdAsync(string alarmSceneId);
    }
}