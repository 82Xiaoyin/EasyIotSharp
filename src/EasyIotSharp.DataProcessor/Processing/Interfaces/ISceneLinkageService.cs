using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyIotSharp.DataProcessor.Processing.Interfaces
{
    public interface ISceneLinkageService
    {
        /// <summary>
        /// 处理场景联动
        /// </summary>
        /// <param name="projectId">项目ID</param>
        /// <param name="dataPoints">数据点列表</param>
        /// <returns>处理任务</returns>
        Task ProcessSceneLinkageAsync(string projectId, IEnumerable<Dictionary<string, object>> dataPoints);
        
        /// <summary>
        /// 检查是否有场景联动需要执行
        /// </summary>
        /// <param name="projectId">项目ID</param>
        /// <returns>是否存在场景联动</returns>
        Task<bool> HasSceneLinkageAsync(string projectId);
    }
}