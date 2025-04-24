using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Dto.Hardware.Params;
using EasyIotSharp.Core.Dto.Hardware;
using System.Threading.Tasks;
using UPrime.Runtime.Caching;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Caching.Hardware.Impl
{
    public class SensorCacheService : CachingServiceBase, ISensorCacheService
    {
        public ICache Cache => CacheManager.GetCache($"{CachingConsts.Keys.HardwareCache}SensorCacheService");
        public const string KEY_HARDWARE_SENSOR_QUERY = "Hardware:Sensor-{0}-{1}";
        public void Clear()
        {
            Cache.Clear();
        }
        public async Task<PagedResultDto<SensorDto>> QuerySensor(QuerySensorInput input, Func<Task<PagedResultDto<SensorDto>>> action)
        {
            var key = KEY_HARDWARE_SENSOR_QUERY.FormatWith(input.PageIndex,
                                          input.PageSize);
            return await Cache.GetAsyncExt(key, action, TimeSpan.FromHours(CachingConsts.THIRTY_EXPIRES_MINUTES));
        }
    }
}
