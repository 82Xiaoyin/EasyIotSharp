using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Caching.Rule;
using EasyIotSharp.Core.Dto.Rule;
using EasyIotSharp.Core.Dto;
using System.Threading.Tasks;
using UPrime.Runtime.Caching;
using UPrime.Services.Dto;
using EasyIotSharp.Core.Dto.TenantAccount.Params;
using EasyIotSharp.Core.Dto.TenantAccount;

namespace EasyIotSharp.Core.Caching.TenantAccount.Impl
{
    public class MenuCacheService : CachingServiceBase, IMenuCacheService
    {
        public ICache Cache => CacheManager.GetCache($"{CachingConsts.Keys.TenantAccount}MenuCacheService");
        public const string KEY_TENANTACCOUNT_MENU_QUERY = "TenantAccount:Menu-{0}-{1}";
        public void Clear()
        {
            Cache.Clear();
        }
        public async Task<PagedResultDto<MenuTreeDto>> QueryMenu(QueryMenuInput input, Func<Task<PagedResultDto<MenuTreeDto>>> action)
        {
            var key = KEY_TENANTACCOUNT_MENU_QUERY.FormatWith(input.PageIndex,
                                          input.PageSize);
            return await Cache.GetAsyncExt(key, action, TimeSpan.FromHours(CachingConsts.TENANT_EXPIRES_MINUTES));
        }
    }
}