using EasyIotSharp.Core.Domain.Proejct;
using EasyIotSharp.Core.Dto.Project;
using EasyIotSharp.Core.Repositories.Mysql;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EasyIotSharp.Core.Repositories.Project
{
    public interface IGatewayRepository : IMySqlRepositoryBase<Gateway, string>
    {

        /// <summary>
        /// 根据ID查询网关信息
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        GatewayBaseDto GetGateway(string id);

        /// <summary>
        /// 根据ID集合查询网关列表
        /// </summary>
        /// <param name="ids">ID集合</param>
        /// <returns></returns>
        List<Gateway> GetByIds(List<string> ids);
        /// <summary>
        /// 通过条件分页查询网关列表
        /// </summary>
        /// <param name="tenantNumId">租户NumId</param>
        /// <param name="Keyword">设备名称</param>
        /// <param name="state">设备状态 -1=不参与查询  0=初始化  1=在线  2=离线</param>
        /// <param name="protocolId">协议id</param>
        /// <param name="projectId">项目id</param>
        /// <param name="pageIndex">起始页</param>
        /// <param name="pageSize">每页多少条</param>
        /// <returns></returns>
        Task<(int totalCount, List<Gateway> items)> Query(int tenantNumId,
                                                          string Keyword,
                                                          int state,
                                                          string protocolId,
                                                          string projectId,
                                                          int pageIndex,
                                                          int pageSize);

        /// <summary>
        /// 根据ID集合查询网关列表
        /// </summary>
        /// <param name="ids">ID集合</param>
        /// <returns></returns>
        Task<List<Gateway>> QueryByIds(List<string> ids);
    }
}
