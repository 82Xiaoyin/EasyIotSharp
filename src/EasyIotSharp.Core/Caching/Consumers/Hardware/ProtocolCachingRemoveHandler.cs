using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Caching.Hardware.Impl;
using EasyIotSharp.Core.Caching.Hardware;
using EasyIotSharp.Core.Events.Hardware;
using UPrime.Dependency;
using UPrime.Events.Bus.Handlers;

namespace EasyIotSharp.Core.Caching.Consumers.Hardware
{
    public class ProtocolCachingRemoveHandler : IEventHandler<ProtocolEventData>, ITransientDependency
    {
        private readonly IProtocolCacheService _protocolCacheService;
        public ProtocolCachingRemoveHandler(IProtocolCacheService protocolCacheService)
        {
            _protocolCacheService = protocolCacheService;
        }
        public void HandleEvent(ProtocolEventData eventData)
        {
            for (int i = 1; i <= 5; i++)
            {
                _protocolCacheService.Cache.RemoveAsync(ProtocolCacheService.KEY_HARDWARE_PROTOCOL_QUERY.FormatWith(i, 10));
            }
        }
    }
}
