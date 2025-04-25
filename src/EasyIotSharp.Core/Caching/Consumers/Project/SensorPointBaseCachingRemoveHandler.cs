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
    public class SensorPointBaseCachingRemoveHandler : IEventHandler<SensorPointBaseEventData>, ITransientDependency
    {
        private readonly ISensorPointBaseCacheService _sensorPointBaseCacheService;
        public SensorPointBaseCachingRemoveHandler(ISensorPointBaseCacheService sensorPointBaseCacheService)
        {
            _sensorPointBaseCacheService = sensorPointBaseCacheService;
        }
        public void HandleEvent(SensorPointBaseEventData eventData)
        {
            _sensorPointBaseCacheService.Cache.RemoveAsync(SensorPointBaseCacheService.KEY_PROJECT_SENSORPOINTBASE_QUERY.FormatWith());
        }
    }
}
