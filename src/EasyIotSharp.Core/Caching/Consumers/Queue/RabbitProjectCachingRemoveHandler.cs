using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Caching.Queue.Impl;
using EasyIotSharp.Core.Caching.Queue;
using EasyIotSharp.Core.Events.Queue;
using UPrime.Dependency;
using UPrime.Events.Bus.Handlers;

namespace EasyIotSharp.Core.Caching.Consumers.Queue
{
    public class RabbitProjectCachingRemoveHandler : IEventHandler<RabbitProjectEventData>, ITransientDependency
    {
        private readonly IRabbitServerInfoCacheService _rabbitServerInfoCacheService;
        public RabbitProjectCachingRemoveHandler(IRabbitServerInfoCacheService rabbitServerInfoCacheService)
        {
            _rabbitServerInfoCacheService = rabbitServerInfoCacheService;
        }
        public void HandleEvent(RabbitProjectEventData eventData)
        {
                _rabbitServerInfoCacheService.Cache.RemoveAsync(RabbitServerInfoCacheService.KEY_QUEUE_RABBITPROJECT_QUERY.FormatWith());
        }
    }
}
