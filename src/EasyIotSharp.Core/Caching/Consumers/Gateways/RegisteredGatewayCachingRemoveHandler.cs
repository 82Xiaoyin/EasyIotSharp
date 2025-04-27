using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Caching.Hardware.Impl;
using EasyIotSharp.Core.Caching.Hardware;
using EasyIotSharp.Core.Events.Hardware;
using UPrime.Dependency;
using UPrime.Events.Bus.Handlers;
using EasyIotSharp.Core.Events.Gateways;
using EasyIotSharp.Core.Caching.Gateways;

namespace EasyIotSharp.Core.Caching.Consumers.Gateways
{
    public class RegisteredGatewayCachingRemoveHandler : IEventHandler<RegisteredGatewayEventData>, ITransientDependency
    {
        private readonly IRegisteredGatewayCacheService _registeredGatewayCacheService;
        public RegisteredGatewayCachingRemoveHandler(IRegisteredGatewayCacheService registeredGatewayCacheService)
        {
            _registeredGatewayCacheService = registeredGatewayCacheService;
        }
        public void HandleEvent(RegisteredGatewayEventData eventData)
        {
            _registeredGatewayCacheService.Cache.RemoveAsync(ProtocolCacheService.KEY_HARDWARE_PROTOCOL_QUERY.FormatWith());
        }
    }
}
