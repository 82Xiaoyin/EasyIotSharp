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
    public class GatewayCachingRemoveHandler : IEventHandler<GatewayEventData>, ITransientDependency
    {
        private readonly IGatewayCacheService _gatewayCacheService;
        public GatewayCachingRemoveHandler(IGatewayCacheService gatewayCacheService)
        {
            _gatewayCacheService = gatewayCacheService;
        }
        public void HandleEvent(GatewayEventData eventData)
        {
            for (int i = 1; i <= 5; i++)
            {
                _gatewayCacheService.Cache.RemoveAsync(GatewayCacheService.KEY_PROJECT_GETEWAY_QUERY.FormatWith(i, 10));
            }
        }
    }
}