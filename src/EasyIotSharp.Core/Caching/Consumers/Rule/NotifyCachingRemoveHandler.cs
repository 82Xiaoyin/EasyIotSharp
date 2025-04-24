using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Caching.Rule.Impl;
using EasyIotSharp.Core.Caching.Rule;
using EasyIotSharp.Core.Events.Rule;
using UPrime.Dependency;
using UPrime.Events.Bus.Handlers;

namespace EasyIotSharp.Core.Caching.Consumers.Rule
{
    public class NotifyCachingRemoveHandler : IEventHandler<NotifyEventData>, ITransientDependency
    {
        private readonly INotifyCacheService _notifyConfigCacheService;
        public NotifyCachingRemoveHandler(INotifyCacheService notifyConfigCacheService)
        {
            _notifyConfigCacheService = notifyConfigCacheService;
        }
        public void HandleEvent(NotifyEventData eventData)
        {
            for (int i = 1; i <= 5; i++)
            {
                _notifyConfigCacheService.Cache.RemoveAsync(NotifyCacheService.KEY_RULE_NOTIFY_QUERY.FormatWith(i, 10));
            }
        }
    }
}
