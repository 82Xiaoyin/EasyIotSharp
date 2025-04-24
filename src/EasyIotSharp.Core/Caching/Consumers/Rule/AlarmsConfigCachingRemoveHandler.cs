using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Caching.Queue.Impl;
using EasyIotSharp.Core.Caching.Queue;
using EasyIotSharp.Core.Events.Queue;
using UPrime.Dependency;
using UPrime.Events.Bus.Handlers;
using EasyIotSharp.Core.Events.Rule;
using EasyIotSharp.Core.Caching.Rule;
using EasyIotSharp.Core.Caching.Rule.Impl;

namespace EasyIotSharp.Core.Caching.Consumers.Rule
{
    public class AlarmsConfigCachingRemoveHandler : IEventHandler<AlarmsConfigEventData>, ITransientDependency
    {
        private readonly IAlarmsConfigCacheService _alarmsConfigCacheService;
        public AlarmsConfigCachingRemoveHandler(IAlarmsConfigCacheService alarmsConfigCacheService)
        {
            _alarmsConfigCacheService = alarmsConfigCacheService;
        }
        public void HandleEvent(AlarmsConfigEventData eventData)
        {
            for (int i = 1; i <= 5; i++)
            {
                _alarmsConfigCacheService.Cache.RemoveAsync(AlarmsConfigCacheService.KEY_RULE_ALARMSCONFIG_QUERY.FormatWith(i, 10));
            }
        }
    }
}
