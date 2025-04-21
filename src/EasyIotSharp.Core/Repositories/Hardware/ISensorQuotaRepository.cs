using EasyIotSharp.Core.Domain.Hardware;
using EasyIotSharp.Core.Repositories.Mysql;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EasyIotSharp.Core.Repositories.Hardware
{
    public interface ISensorQuotaRepository : IMySqlRepositoryBase<SensorQuota, string>
    {
        /// <summary>
        /// 根据id获取单条指标记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        List<SensorQuota> GetSensorQuotaList(string id);
        /// <summary>
        /// 通过条件分页查询传感器指标列表
        /// </summary>
        /// <param name="tenantNumId">租户NumId</param>
        /// <param name="sensorId">传感器类型id</param>
        /// <param name="keyword">名称/标识符</param>
        /// <param name="dataType">数据类型  -1=全部 1=string 2=int 3=double 4=float 5=bool </param>
        /// <param name="pageIndex">起始页</param>
        /// <param name="pageSize">每页多少条数据</param>
        /// <param name="isPage">是否分页</param>
        /// <returns></returns>
        Task<(int totalCount, List<SensorQuota> items)> Query(int tenantNumId,
                                                                  string sensorId,
                                                                  string keyword,
                                                                  DataTypeMenu dataType,
                                                                  int pageIndex,
                                                                  int pageSize,
                                                                  bool isPage);
    }
}
