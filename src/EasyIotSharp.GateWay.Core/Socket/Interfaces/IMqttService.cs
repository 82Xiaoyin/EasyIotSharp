using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyIotSharp.GateWay.Core.Interfaces
{
    public interface IMqttService
    {
        /// <summary>
        /// 发布数据到MQTT
        /// </summary>
        /// <param name="projectId">项目ID</param>
        /// <param name="pointId">测点ID</param>
        /// <param name="data">数据内容</param>
        /// <returns>发布任务</returns>
        //Task PublishDataAsync(string projectId, string pointId, object data);
        
        /// <summary>
        /// 批量发布数据到MQTT
        /// </summary>
        /// <param name="projectId">项目ID</param>
        /// <param name="dataPoints">数据点列表</param>
        /// <returns>发布任务</returns>
        //Task PublishBatchDataAsync(string projectId, IEnumerable<Dictionary<string, object>> dataPoints);
    }
}