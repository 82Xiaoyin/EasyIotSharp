using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Dto.Hardware.Params;
using EasyIotSharp.Core.Dto.Hardware;
using System.Threading.Tasks;
using UPrime.Services.Dto;
using EasyIotSharp.Core.Domain.Hardware;

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

        /// <summary>
        /// 列表
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        List<Sensor> GetSensorList(Func<List<Sensor>> action);
    }
}
