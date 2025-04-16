using EasyIotSharp.Core.Domain.Proejct;
using EasyIotSharp.Core.Repositories.Mysql;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EasyIotSharp.Core.Repositories.Project
{
    public interface ISensorPointRepository : IMySqlRepositoryBase<SensorPoint, string>
    {
        /// <summary>
        /// 通过条件分页查询项目分类列表
        /// </summary>
        /// <param name="tenantNumId">租户numId</param>
        /// <param name="keyword">测点名称</param>
        /// <param name="projectId">项目id</param>
        /// <param name="classificationId">分类id</param>
        /// <param name="gatewayId">网关id</param>
        /// <param name="sensorId">传感器id</param>
        /// <param name="pageIndex">起始页</param>
        /// <param name="pageSize">每页多少条数据</param>
        /// <param name="isPage">是否分页</param>
        /// <returns></returns>
        Task<(int totalCount, List<SensorPoint> items)> Query(int tenantNumId,
                                                              string keyword,
                                                              string projectId,
                                                              string classificationId,
                                                              string gatewayId,
                                                              string sensorId,
                                                              int state,
                                                              int pageIndex,
                                                              int pageSize,
                                                              bool isPage);

        /// <summary>
        /// 获取列表数据
        /// </summary>
        /// <returns></returns>
        Task<List<SensorPoint>> QueryList(string projectId);
    }
}
