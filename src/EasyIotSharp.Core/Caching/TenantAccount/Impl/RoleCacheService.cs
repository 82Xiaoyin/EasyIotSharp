using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Dto.TenantAccount.Params;
using EasyIotSharp.Core.Dto.TenantAccount;
using System.Threading.Tasks;
using UPrime.Runtime.Caching;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Caching.TenantAccount.Impl
{
    public class RoleCacheService : CachingServiceBase, IRoleCacheService
    {
        public ICache Cache => CacheManager.GetCache($"{CachingConsts.Keys.TenantAccount}RoleCacheService");
        public const string KEY_TENANTACCOUNT_ROLE_QUERY = "TenantAccount:Role-{0}-{1}";
        public void Clear()
        {
            Cache.Clear();
        }
        public async Task<PagedResultDto<RoleDto>> QueryRole(QueryRoleInput input, Func<Task<PagedResultDto<RoleDto>>> action)
        {
            var key = KEY_TENANTACCOUNT_ROLE_QUERY.FormatWith(input.PageIndex,
                                          input.PageSize);
            return await Cache.GetAsyncExt(key, action, TimeSpan.FromHours(CachingConsts.TENANT_EXPIRES_MINUTES));
        }
    }
}