using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Dto.Rule;
using EasyIotSharp.Core.Dto;
using System.Threading.Tasks;
using UPrime.Runtime.Caching;
using UPrime.Services.Dto;
using EasyIotSharp.Core.Dto.Rule.Params;

namespace EasyIotSharp.Core.Caching.Rule.Impl
{
    public class NotifyRecordCacheService : CachingServiceBase, INotifyRecordCacheService
    {
        public ICache Cache => CacheManager.GetCache($"{CachingConsts.Keys.Rule}NotifyRecordCacheService");
        public const string KEY_RULE_NOTIFYRECORD_QUERY = "Rule:NotifyRecord-{0}-{1}";
        public void Clear()
        {
            Cache.Clear();
        }
        public async Task<PagedResultDto<NotifyRecordDto>> QueryNotifyRecord(NotifyRecordInput input, Func<Task<PagedResultDto<NotifyRecordDto>>> action)
        {
            var key = KEY_RULE_NOTIFYRECORD_QUERY.FormatWith(input.PageIndex,
                                          input.PageSize);
            return await Cache.GetAsyncExt(key, action, TimeSpan.FromHours(CachingConsts.TENANT_EXPIRES_MINUTES));
        }
    }
}
