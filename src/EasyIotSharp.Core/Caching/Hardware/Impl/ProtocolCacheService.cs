using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Dto.Hardware.Params;
using System.Threading.Tasks;
using UPrime.Runtime.Caching;
using UPrime.Services.Dto;
using EasyIotSharp.Core.Dto.Hardware;

namespace EasyIotSharp.Core.Caching.Hardware.Impl
{
    public class ProtocolCacheService : CachingServiceBase, IProtocolCacheService
    {
        public ICache Cache => CacheManager.GetCache($"{CachingConsts.Keys.HardwareCache}ProtocolCacheService");
        public const string KEY_HARDWARE_PROTOCOL_QUERY = "Hardware:QueryProtocol-{0}-{1}";
        public void Clear()
        {
            Cache.Clear();
        }
        public async Task<PagedResultDto<ProtocolDto>> QueryProtocol(QueryProtocolInput input, Func<Task<PagedResultDto<ProtocolDto>>> action)
        {
            var key = KEY_HARDWARE_PROTOCOL_QUERY.FormatWith(input.PageIndex,
                                          input.PageSize);
            return await Cache.GetAsyncExt(key, action, TimeSpan.FromHours(CachingConsts.THIRTY_EXPIRES_MINUTES));
        }
    }
}
