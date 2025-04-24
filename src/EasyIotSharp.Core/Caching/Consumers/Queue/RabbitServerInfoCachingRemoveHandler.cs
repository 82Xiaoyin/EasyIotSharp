using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Caching.Tenant.Impl;
using EasyIotSharp.Core.Caching.Tenant;
using EasyIotSharp.Core.Events.Tenant;
using UPrime.Dependency;
using UPrime.Events.Bus.Handlers;
using EasyIotSharp.Core.Events.Queue;
using EasyIotSharp.Core.Caching.Queue;
using EasyIotSharp.Core.Caching.Queue.Impl;

namespace EasyIotSharp.Core.Caching.Consumers.Queue
{
    public class RabbitServerInfoCachingRemoveHandler : IEventHandler<RabbitServerInfoEventData>, ITransientDependency
    {
        private readonly IRabbitServerInfoCacheService _rabbitServerInfoCacheService;
        public RabbitServerInfoCachingRemoveHandler(IRabbitServerInfoCacheService rabbitServerInfoCacheService)
        {
            _rabbitServerInfoCacheService = rabbitServerInfoCacheService;
        }
        public void HandleEvent(RabbitServerInfoEventData eventData)
        {
            for (int i = 1; i <= 5; i++)
            {
                _rabbitServerInfoCacheService.Cache.RemoveAsync(RabbitServerInfoCacheService.KEY_QUEUE_RABBITSERVERINFO_QUERY.FormatWith(i, 10));
            }
        }
    }
}
