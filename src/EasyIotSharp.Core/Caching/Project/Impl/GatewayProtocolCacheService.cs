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
    public class GatewayProtocolCacheService : CachingServiceBase, IGatewayProtocolCacheService
    {
        public ICache Cache => CacheManager.GetCache($"{CachingConsts.Keys.Project}GatewayProtocolCacheService");
        public const string KEY_PROJECT_GETEWAYPROTPCOL_QUERY = "Project:GatewayProtocol-{0}-{1}";
        public void Clear()
        {
            Cache.Clear();
        }
        public async Task<PagedResultDto<GatewayProtocolDto>> QueryGatewayProtocol(QueryGatewayProtocolInput input, Func<Task<PagedResultDto<GatewayProtocolDto>>> action)
        {
            var key = KEY_PROJECT_GETEWAYPROTPCOL_QUERY.FormatWith(input.PageIndex,
                                          input.PageSize);
            return await Cache.GetAsyncExt(key, action, TimeSpan.FromHours(CachingConsts.THIRTY_EXPIRES_MINUTES));
        }
    }
}
