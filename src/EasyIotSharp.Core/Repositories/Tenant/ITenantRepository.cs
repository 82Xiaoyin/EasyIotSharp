using EasyIotSharp.Core.Repositories.Mysql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyIotSharp.Core.Repositories.Tenant
{
    /// <summary>
    /// 租户仓储接口类
    /// </summary>
    public interface ITenantRepository: IMySqlRepositoryBase<EasyIotSharp.Core.Domain.Tenant.Tenant, string>
    {
        /// <summary>
        /// 通过条件分页查询租户列表
        /// </summary>
        /// <param name="keyword">租户名称</param>
        /// <param name="expiredType">-1=不参与查询 0=待授权 1=生效 2=已过期</param>
        /// <param name="contractStartTime">合同开始日期</param>
        /// <param name="contractEndTime">合同结束日期</param>
        /// <param name="isFreeze">-1=不参与查询 1=已冻结 2=未冻结</param>
        /// <param name="pageIndex">起始页</param>
        /// <param name="pageSize">每页多少条</param>
        /// <returns></returns>
        Task<(int totalCount, List<EasyIotSharp.Core.Domain.Tenant.Tenant> items)> Query(
            string keyword,
            int expiredType,
            DateTime? contractStartTime,
            DateTime? contractEndTime,
            int isFreeze,
            int pageIndex,
            int pageSize);

        /// <summary>
        /// 获取所有租户
        /// </summary>
        /// <returns></returns>
        Task<List<EasyIotSharp.Core.Domain.Tenant.Tenant>> GetTenantList();
    }
}