using EasyIotSharp.Core.Domain.Hardware;
using EasyIotSharp.Core.Repositories.Mysql;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EasyIotSharp.Core.Repositories.Hardware
{
    public interface ISensorRepository : IMySqlRepositoryBase<Sensor, string>
    {

        /// <summary>
        /// 根据ID查询传感器单条记录
        /// </summary>
        /// <param name="ids">ID集合</param>
        /// <returns></returns>
        Sensor GetBySensor(string id);
        /// <summary>
        /// 通过条件分页查询传感器列表
        /// </summary>
        /// <param name="tenantNumId">租户NumId</param>
        /// <param name="keyword">名称/简称/厂家</param>
        /// <param name="pageIndex">起始页</param>
        /// <param name="pageSize">每页多少条数据</param>
        /// <param name="isPage">是否分页</param>
        /// <returns></returns>
        Task<(int totalCount, List<Sensor> items)> Query(int tenantNumId,
                                                         string keyword,
                                                         int pageIndex,
                                                         int pageSize,
                                                         bool isPage);

        /// <summary>
        /// 根据ID集合查询传感器列表
        /// </summary>
        /// <param name="ids">ID集合</param>
        /// <returns></returns>
        Task<List<Sensor>> QueryByIds(List<string> ids);
    }
}
