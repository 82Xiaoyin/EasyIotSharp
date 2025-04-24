using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Dto.Hardware.Params;
using EasyIotSharp.Core.Dto.Hardware;
using System.Threading.Tasks;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Caching.Hardware
{
    public interface ISensorCacheService : ICacheService
    {
        /// <summary>
        /// 传感器类型
        /// </summary>
        /// <param name="input"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        Task<PagedResultDto<SensorDto>> QuerySensor(QuerySensorInput input, Func<Task<PagedResultDto<SensorDto>>> action);
    }
}
