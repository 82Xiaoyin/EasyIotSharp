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
    public class SensorPointCachingRemoveHandler : IEventHandler<SensorPointEventData>, ITransientDependency
    {
        private readonly ISensorPointCacheService _sensorPointCacheService;
        public SensorPointCachingRemoveHandler(ISensorPointCacheService sensorPointCacheService)
        {
            _sensorPointCacheService = sensorPointCacheService;
        }
        public void HandleEvent(SensorPointEventData eventData)
        {
            for (int i = 1; i <= 5; i++)
            {
                _sensorPointCacheService.Cache.RemoveAsync(SensorPointCacheService.KEY_PROJECT_SENSORPOINT_QUERY.FormatWith(i, 10));
            }
        }
    }
}