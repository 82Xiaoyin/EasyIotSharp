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
    public class SensorQuotaBaseCachingRemoveHandler : IEventHandler<SensorBaseEventData>, ITransientDependency
    {
        private readonly ISensorQuotaCacheService _sensorQuotaCacheService;
        public SensorQuotaBaseCachingRemoveHandler(ISensorQuotaCacheService sensorQuotaCacheService)
        {
            _sensorQuotaCacheService = sensorQuotaCacheService;
        }
        public void HandleEvent(SensorBaseEventData eventData)
        {
            _sensorQuotaCacheService.Cache.RemoveAsync(SensorQuotaCacheService.KEY_HARDWARE_SENSORQUOTABASE_QUERY.FormatWith());
        }
    }
}