using EasyIotSharp.Core.Dto.Tenant.Params;
using EasyIotSharp.Core.Dto.Tenant;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Caching.Tenant
{
    public interface ITenantCacheService : ICacheService
    {

        /// <summary>
        /// 通过条件分页查询租户列表
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<PagedResultDto<TenantDto>> QueryTenant(QueryTenantInput input, Func<Task<PagedResultDto<TenantDto>>> action);
    }
}
