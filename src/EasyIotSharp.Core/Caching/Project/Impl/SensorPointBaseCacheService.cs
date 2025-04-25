using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Caching.Queue.Impl;
using EasyIotSharp.Core.Caching.Queue;
using EasyIotSharp.Core.Events.Queue;
using UPrime.Dependency;
using UPrime.Events.Bus.Handlers;
using EasyIotSharp.Core.Events.Project;
using EasyIotSharp.Core.Dto.Queue.Params;
using EasyIotSharp.Core.Dto.Queue;
using System.Threading.Tasks;
using UPrime.Runtime.Caching;
using UPrime.Services.Dto;
using EasyIotSharp.Core.Domain.Proejct;

namespace EasyIotSharp.Core.Caching.Project.Impl
{
    public class SensorPointBaseCacheService : CachingServiceBase, ISensorPointBaseCacheService
    {
        public ICache Cache => CacheManager.GetCache($"{CachingConsts.Keys.Project}SensorPointBase");
        public const string KEY_PROJECT_SENSORPOINTBASE_QUERY = "Queue:SensorPointBase";
        public void Clear()
        {
            Cache.Clear();
        }
        public List<SensorPoint> GetSensorPointBase(Func<List<SensorPoint>> action)
        {
            var key = KEY_PROJECT_SENSORPOINTBASE_QUERY.FormatWith();
            return Cache.GetExt(key, action, 60);
        }

    }
}
