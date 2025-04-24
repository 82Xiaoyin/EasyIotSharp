using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Dto.Project.Params;
using EasyIotSharp.Core.Dto.Project;
using System.Threading.Tasks;
using UPrime.Runtime.Caching;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Caching.Project.Impl
{
    public class SensorPointCacheService : CachingServiceBase, ISensorPointCacheService
    {
        public ICache Cache => CacheManager.GetCache($"{CachingConsts.Keys.Project}SensorPointCacheService");
        public const string KEY_PROJECT_SENSORPOINT_QUERY = "Project:SensorPoint-{0}-{1}";
        public void Clear()
        {
            Cache.Clear();
        }
        public async Task<PagedResultDto<SensorPointDto>> QuerySensorPoint(QuerySensorPointInput input, Func<Task<PagedResultDto<SensorPointDto>>> action)
        {
            var key = KEY_PROJECT_SENSORPOINT_QUERY.FormatWith(input.PageIndex,
                                          input.PageSize);
            return await Cache.GetAsyncExt(key, action, TimeSpan.FromHours(CachingConsts.THIRTY_EXPIRES_MINUTES));
        }
    }
}
