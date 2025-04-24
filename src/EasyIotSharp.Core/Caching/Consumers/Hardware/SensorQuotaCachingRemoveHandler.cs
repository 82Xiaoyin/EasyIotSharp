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
    public class SensorQuotaCachingRemoveHandler : IEventHandler<SensorQuotaEventData>, ITransientDependency
    {
        private readonly ISensorQuotaCacheService _sensorQuotaCacheService;
        public SensorQuotaCachingRemoveHandler(ISensorQuotaCacheService sensorQuotaCacheService)
        {
            _sensorQuotaCacheService = sensorQuotaCacheService;
        }
        public void HandleEvent(SensorQuotaEventData eventData)
        {
            for (int i = 1; i <= 5; i++)
            {
                _sensorQuotaCacheService.Cache.RemoveAsync(SensorQuotaCacheService.KEY_HARDWARE_SENSORQUOTA_QUERY.FormatWith(i, 10));
            }
        }
    }
}