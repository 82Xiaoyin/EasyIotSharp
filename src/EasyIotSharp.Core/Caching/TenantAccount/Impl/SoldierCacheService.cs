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
    public class SoldierCacheService : CachingServiceBase, ISoldierCacheService
    {
        public ICache Cache => CacheManager.GetCache($"{CachingConsts.Keys.TenantAccount}SoldierCacheService");
        public const string KEY_TENANTACCOUNT_SOLDIER_QUERY = "TenantAccount:Soldier-{0}-{1}";
        public void Clear()
        {
            Cache.Clear();
        }
        public async Task<PagedResultDto<SoldierDto>> QuerySoldier(QuerySoldierInput input, Func<Task<PagedResultDto<SoldierDto>>> action)
        {
            var key = KEY_TENANTACCOUNT_SOLDIER_QUERY.FormatWith(input.PageIndex,
                                          input.PageSize);
            return await Cache.GetAsyncExt(key, action, TimeSpan.FromHours(CachingConsts.TENANT_EXPIRES_MINUTES));
        }
    }
}