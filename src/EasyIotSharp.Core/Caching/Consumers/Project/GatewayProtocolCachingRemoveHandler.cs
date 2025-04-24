using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Caching.Project.Impl;
using EasyIotSharp.Core.Caching.Project;
using EasyIotSharp.Core.Events.Project;
using UPrime.Dependency;
using UPrime.Events.Bus.Handlers;

namespace EasyIotSharp.Core.Caching.Consumers.Project
{
    public class GatewayProtocolCachingRemoveHandler : IEventHandler<GatewayProtocolEventData>, ITransientDependency
    {
        private readonly IGatewayProtocolCacheService _gatewayProtocolCacheService;
        public GatewayProtocolCachingRemoveHandler(IGatewayProtocolCacheService gatewayProtocolCacheService)
        {
            _gatewayProtocolCacheService = gatewayProtocolCacheService;
        }
        public void HandleEvent(GatewayProtocolEventData eventData)
        {
            for (int i = 1; i <= 5; i++)
            {
                _gatewayProtocolCacheService.Cache.RemoveAsync(GatewayProtocolCacheService.KEY_PROJECT_GETEWAYPROTPCOL_QUERY.FormatWith(i, 10));
            }
        }
    }
}