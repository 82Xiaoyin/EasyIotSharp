using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Caching.Hardware;
using EasyIotSharp.Core.Dto.Hardware.Params;
using EasyIotSharp.Core.Dto.Hardware;
using System.Threading.Tasks;
using UPrime.Runtime.Caching;
using UPrime.Services.Dto;
using EasyIotSharp.Core.Dto.Gateways;

namespace EasyIotSharp.Core.Caching.Gateways.Impl
{
    public class RegisteredGatewayCacheService : CachingServiceBase, IRegisteredGatewayCacheService
    {
        public ICache Cache => CacheManager.GetCache($"{CachingConsts.Keys.Gateways}RegisteredGatewayCacheService");
        public const string KEY_GATEWAYS_REGISTEREDGATEWAY_QUERY = "Gateways:RegisteredGateway";
        public void Clear()
        {
            Cache.Clear();
        }
        public List<GatewayConnectionInfo> GetAllRegisteredGateways(Func<List<GatewayConnectionInfo>> action)
        {
            var key = KEY_GATEWAYS_REGISTEREDGATEWAY_QUERY.FormatWith();
            return Cache.GetExt(key, action, 120);
        }
    }
}

