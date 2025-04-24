using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Dto.Project.Params;
using EasyIotSharp.Core.Dto.Project;
using System.Threading.Tasks;
using UPrime.Runtime.Caching;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Caching.Project.Impl
{
    public class GatewayCacheService : CachingServiceBase, IGatewayCacheService
    {
        public ICache Cache => CacheManager.GetCache($"{CachingConsts.Keys.Project}GatewayCacheService");
        public const string KEY_PROJECT_GETEWAY_QUERY = "Project:Gateway-{0}-{1}";
        public void Clear()
        {
            Cache.Clear();
        }
        public async Task<PagedResultDto<GatewayDto>> QueryGateway(QueryGatewayInput input, Func<Task<PagedResultDto<GatewayDto>>> action)
        {
            var key = KEY_PROJECT_GETEWAY_QUERY.FormatWith(input.PageIndex,
                                          input.PageSize);
            return await Cache.GetAsyncExt(key, action, TimeSpan.FromHours(CachingConsts.THIRTY_EXPIRES_MINUTES));
        }
    }
}
