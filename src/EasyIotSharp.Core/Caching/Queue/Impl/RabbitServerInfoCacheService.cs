using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Caching.Tenant;
using EasyIotSharp.Core.Dto.Tenant.Params;
using EasyIotSharp.Core.Dto.Tenant;
using System.Threading.Tasks;
using UPrime.Runtime.Caching;
using UPrime.Services.Dto;
using EasyIotSharp.Core.Dto.Queue.Params;
using EasyIotSharp.Core.Dto.Queue;

namespace EasyIotSharp.Core.Caching.Queue.Impl
{
    public class RabbitServerInfoCacheService : CachingServiceBase, IRabbitServerInfoCacheService
    {
        public ICache Cache => CacheManager.GetCache($"{CachingConsts.Keys.Queue}RabbitServerInfo");
        public const string KEY_QUEUE_RABBITSERVERINFO_QUERY = "Queue:RabbitServerInfo-{0}-{1}";
        public const string KEY_QUEUE_RABBITPROJECT_QUERY = "Queue:RabbitProject";
        public void Clear()
        {
            Cache.Clear();
        }
        public async Task<PagedResultDto<RabbitServerInfoDto>> QueryRabbitServerInfo(QueryRabbitServerInfoInput input, Func<Task<PagedResultDto<RabbitServerInfoDto>>> action)
        {
            var key = KEY_QUEUE_RABBITSERVERINFO_QUERY.FormatWith(input.PageIndex,
                                          input.PageSize);
            return await Cache.GetAsyncExt(key, action, TimeSpan.FromHours(CachingConsts.THIRTY_EXPIRES_MINUTES));
        }

        public List<RabbitServerInfoDto> GetRabbitProject(Func<List<RabbitServerInfoDto>> action)
        {
            var key = KEY_QUEUE_RABBITPROJECT_QUERY.FormatWith();
            return Cache.GetExt(key, action, 60);
        }
        
    }
}
