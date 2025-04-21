using EasyIotSharp.Core.Dto.Tenant;
using EasyIotSharp.Core.Dto.Tenant.Params;
using EasyIotSharp.Core.Services.Tenant;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UPrime.Runtime.Caching;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Caching.Tenant.Impl
{
    class TenantCacheService : CachingServiceBase, ITenantCacheService
    {
        public ICache Cache => CacheManager.GetCache($"{CachingConsts.Keys.TenantCache}TenantCacheService");
        public const string KEY_TENANT_QUERY = "Tenant:QueryTenant-{0}-{1}";
        public void Clear()
        {
            Cache.Clear();
        }
        public async Task<PagedResultDto<TenantDto>> QueryTenant(QueryTenantInput input, Func<Task<PagedResultDto<TenantDto>>> action)
        {
            var key = KEY_TENANT_QUERY.FormatWith(input.PageIndex,
                                          input.PageSize);
            return await Cache.GetAsyncExt(key, action, TimeSpan.FromHours(CachingConsts.TENANT_EXPIRES_MINUTES));
        }
    }
}
