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
    public class SensorCachingRemoveHandler : IEventHandler<SensorEventData>, ITransientDependency
    {
        private readonly ISensorCacheService _sensorCacheService1;
        public SensorCachingRemoveHandler(ISensorCacheService sensorCacheService)
        {
            _sensorCacheService1 = sensorCacheService;
        }
        public void HandleEvent(SensorEventData eventData)
        {
            for (int i = 1; i <= 5; i++)
            {
                _sensorCacheService1.Cache.RemoveAsync(SensorCacheService.KEY_HARDWARE_SENSOR_QUERY.FormatWith(i, 10));
            }
        }
    }
}