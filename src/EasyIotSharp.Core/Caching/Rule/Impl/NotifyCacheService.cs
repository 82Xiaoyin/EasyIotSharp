using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Dto.Rule;
using EasyIotSharp.Core.Dto;
using System.Threading.Tasks;
using UPrime.Runtime.Caching;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Caching.Rule.Impl
{
    public class NotifyCacheService : CachingServiceBase, INotifyCacheService
    {
        public ICache Cache => CacheManager.GetCache($"{CachingConsts.Keys.Rule}NotifyConfigCacheService");
        public const string KEY_RULE_NOTIFY_QUERY = "Rule:Notify-{0}-{1}";
        public void Clear()
        {
            Cache.Clear();
        }
        public async Task<PagedResultDto<NotifyDto>> QueryNotifyConfig(PagingInput input, Func<Task<PagedResultDto<NotifyDto>>> action)
        {
            var key = KEY_RULE_NOTIFY_QUERY.FormatWith(input.PageIndex,
                                          input.PageSize);
            return await Cache.GetAsyncExt(key, action, TimeSpan.FromHours(CachingConsts.TENANT_EXPIRES_MINUTES));
        }
    }
}
