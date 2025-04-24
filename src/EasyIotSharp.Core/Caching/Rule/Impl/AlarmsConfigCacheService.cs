using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Caching.Tenant;
using EasyIotSharp.Core.Dto.Tenant.Params;
using EasyIotSharp.Core.Dto.Tenant;
using System.Threading.Tasks;
using UPrime.Runtime.Caching;
using UPrime.Services.Dto;
using EasyIotSharp.Core.Dto.Rule;
using EasyIotSharp.Core.Dto;

namespace EasyIotSharp.Core.Caching.Rule.Impl
{
    public class AlarmsConfigCacheService : CachingServiceBase, IAlarmsConfigCacheService
    {
        public ICache Cache => CacheManager.GetCache($"{CachingConsts.Keys.Rule}AlarmsConfigCacheService");
        public const string KEY_RULE_ALARMSCONFIG_QUERY = "Rule:AlarmsConfig-{0}-{1}";
        public void Clear()
        {
            Cache.Clear();
        }
        public async Task<PagedResultDto<AlarmsConfigDto>> QueryAlarmsConfig(PagingInput input, Func<Task<PagedResultDto<AlarmsConfigDto>>> action)
        {
            var key = KEY_RULE_ALARMSCONFIG_QUERY.FormatWith(input.PageIndex,
                                          input.PageSize);
            return await Cache.GetAsyncExt(key, action, TimeSpan.FromHours(CachingConsts.TENANT_EXPIRES_MINUTES));
        }
    }
}
