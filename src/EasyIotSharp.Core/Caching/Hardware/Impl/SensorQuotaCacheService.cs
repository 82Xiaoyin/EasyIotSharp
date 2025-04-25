using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Dto.Hardware.Params;
using System.Threading.Tasks;
using UPrime.Runtime.Caching;
using UPrime.Services.Dto;
using EasyIotSharp.Core.Dto.Hardware;
using EasyIotSharp.Core.Domain.Hardware;

namespace EasyIotSharp.Core.Caching.Hardware.Impl
{
    public class SensorQuotaCacheService : CachingServiceBase, ISensorQuotaCacheService
    {
        public ICache Cache => CacheManager.GetCache($"{CachingConsts.Keys.HardwareCache}SensorQuotaCacheService");
        public const string KEY_HARDWARE_SENSORQUOTA_QUERY = "Hardware:SensorQuota-{0}-{1}";
        public const string KEY_HARDWARE_SENSORQUOTABASE_QUERY = "Hardware:SensorQuotaBase";
        public void Clear()
        {
            Cache.Clear();
        }
        public async Task<PagedResultDto<SensorQuotaDto>> QuerySensorQuota(QuerySensorQuotaInput input, Func<Task<PagedResultDto<SensorQuotaDto>>> action)
        {
            var key = KEY_HARDWARE_SENSORQUOTA_QUERY.FormatWith(input.PageIndex,
                                          input.PageSize);
            return await Cache.GetAsyncExt(key, action, TimeSpan.FromHours(CachingConsts.THIRTY_EXPIRES_MINUTES));
        }
        public List<SensorQuota> GetSensorQuotaList(Func<List<SensorQuota>> action)
        {
            var key = KEY_HARDWARE_SENSORQUOTABASE_QUERY.FormatWith();
            return Cache.GetExt(key, action, 60);
        }

    }
}
