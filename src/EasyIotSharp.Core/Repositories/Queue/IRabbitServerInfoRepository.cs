using EasyIotSharp.Core.Domain.Queue;
using EasyIotSharp.Core.Repositories.Mysql;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyIotSharp.Core.Repositories.Queue
{
    /// <summary>
    /// RabbitMQ服务器配置信息仓储接口
    /// </summary>
    public interface IRabbitServerInfoRepository : IMySqlRepositoryBase<RabbitServerInfo, string>
    {
        /// <summary>
        /// 通过条件分页查询RabbitMQ服务器配置信息列表
        /// </summary>
        /// <param name="tenantNumId">租户ID</param>
        /// <param name="keyword">关键字（主机地址/用户名）</param>
        /// <param name="pageIndex">起始页</param>
        /// <param name="pageSize">每页多少条</param>
        /// <param name="isPage">是否分页</param>
        /// <returns></returns>
        Task<(int totalCount, List<RabbitServerInfo> items)> Query(int tenantNumId, string keyword, int isEnable,int pageIndex, int pageSize, bool isPage = true);
    }
}