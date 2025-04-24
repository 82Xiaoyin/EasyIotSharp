using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Caching.Tenant;
using EasyIotSharp.Core.Dto.Tenant.Params;
using EasyIotSharp.Core.Dto.Tenant;
using System.Threading.Tasks;
using UPrime.Runtime.Caching;
using UPrime.Services.Dto;
using EasyIotSharp.Core.Dto.Hardware.Params;

namespace EasyIotSharp.Core.Caching.Hardware.Impl
{
    public class ProtocolConfigCacheService : CachingServiceBase, IProtocolConfigCacheService
    {
        public ICache Cache => CacheManager.GetCache($"{CachingConsts.Keys.HardwareCache}ProtocolConfigCacheService");
        public const string KEY_HARDWARE_PROTOCOLCONFIG_QUERY = "Hardware:QueryProtocolConfig-{0}-{1}";
        public void Clear()
        {
            Cache.Clear();
        }
        public async Task<PagedResultDto<QueryProtocolConfigByProtocolIdOutput>> QueryProtocolConfig(QueryProtocolConfigInput input, Func<Task<PagedResultDto<QueryProtocolConfigByProtocolIdOutput>>> action)
        {
            var key = KEY_HARDWARE_PROTOCOLCONFIG_QUERY.FormatWith(input.PageIndex,
                                          input.PageSize);
            return await Cache.GetAsyncExt(key, action, TimeSpan.FromHours(CachingConsts.THIRTY_EXPIRES_MINUTES));
        }
    }
}
