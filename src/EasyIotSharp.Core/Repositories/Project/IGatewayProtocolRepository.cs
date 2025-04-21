using EasyIotSharp.Core.Domain.Proejct;
using EasyIotSharp.Core.Repositories.Mysql;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EasyIotSharp.Core.Repositories.Project
{
    public interface IGatewayProtocolRepository : IMySqlRepositoryBase<GatewayProtocol, string>
    {

        /// <summary>
        /// 通过网关id获取数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        GatewayProtocol GetGatewayProtocol(string id);
        /// <summary>
        /// 通过条件分页查询网关协议列表
        /// </summary>
        /// <param name="tenantNumId">租户NumId</param>
        /// <param name="gatewayId">网关id</param>
        /// <param name="pageIndex">起始页</param>
        /// <param name="pageSize">每页多少条</param>
        /// <returns></returns>
        Task<(int totalCount, List<GatewayProtocol> items)> Query(int tenantNumId,
                                                                  string gatewayId,
                                                                  int pageIndex,
                                                                  int pageSize);
    }
}
