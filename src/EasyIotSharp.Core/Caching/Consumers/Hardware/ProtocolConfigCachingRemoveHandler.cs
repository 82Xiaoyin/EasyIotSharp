using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Caching.Tenant.Impl;
using EasyIotSharp.Core.Caching.Tenant;
using EasyIotSharp.Core.Events.Tenant;
using UPrime.Dependency;
using UPrime.Events.Bus.Handlers;
using EasyIotSharp.Core.Events.Hardware;
using EasyIotSharp.Core.Caching.Hardware;
using EasyIotSharp.Core.Caching.Hardware.Impl;

namespace EasyIotSharp.Core.Caching.Consumers.Hardware
{
    internal class ProtocolConfigCachingRemoveHandler : IEventHandler<ProtocolConfigEventData>, ITransientDependency
    {
        private readonly IProtocolConfigCacheService _protocolConfigCacheService;
        public ProtocolConfigCachingRemoveHandler(IProtocolConfigCacheService protocolConfigCacheService)
        {
            _protocolConfigCacheService = protocolConfigCacheService;
        }
        public void HandleEvent(ProtocolConfigEventData eventData)
        {
            for (int i = 1; i <= 5; i++)
            {
                _protocolConfigCacheService.Cache.RemoveAsync(ProtocolConfigCacheService.KEY_HARDWARE_PROTOCOLCONFIG_QUERY.FormatWith(i, 10));
            }
        }
    }
}
