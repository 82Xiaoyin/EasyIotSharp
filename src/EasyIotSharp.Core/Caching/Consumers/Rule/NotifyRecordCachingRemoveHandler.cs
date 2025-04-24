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
    public class NotifyRecordCachingRemoveHandler : IEventHandler<NotifyRecordEventData>, ITransientDependency
    {
        private readonly INotifyRecordCacheService _notifyRecordCacheService;
        public NotifyRecordCachingRemoveHandler(INotifyRecordCacheService notifyRecordCacheService)
        {
            _notifyRecordCacheService = notifyRecordCacheService;
        }
        public void HandleEvent(NotifyRecordEventData eventData)
        {
            for (int i = 1; i <= 5; i++)
            {
                _notifyRecordCacheService.Cache.RemoveAsync(NotifyRecordCacheService.KEY_RULE_NOTIFYRECORD_QUERY.FormatWith(i, 10));
            }
        }
    }
}
