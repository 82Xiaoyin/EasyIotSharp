using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Caching.Hardware.Impl;
using EasyIotSharp.Core.Caching.Hardware;
using EasyIotSharp.Core.Events.Hardware;
using UPrime.Dependency;
using UPrime.Events.Bus.Handlers;

namespace EasyIotSharp.Core.Caching.Consumers.Hardware
{
    public class SensorBaseCachingRemoveHandler : IEventHandler<SensorBaseEventData>, ITransientDependency
    {
        private readonly ISensorCacheService _sensorCacheService1;
        public SensorBaseCachingRemoveHandler(ISensorCacheService sensorCacheService)
        {
            _sensorCacheService1 = sensorCacheService;
        }
        public void HandleEvent(SensorBaseEventData eventData)
        {
            _sensorCacheService1.Cache.RemoveAsync(SensorCacheService.KEY_HARDWARE_SENSORBASE_QUERY.FormatWith());
        }
    }
}