using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Dto.Hardware.Params;
using EasyIotSharp.Core.Dto.Hardware;
using System.Threading.Tasks;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Caching.Hardware
{
    public interface ISensorQuotaCacheService : ICacheService
    {
        /// <summary>
        /// 传感器列表
        /// </summary>
        /// <param name="input"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        Task<PagedResultDto<SensorQuotaDto>> QuerySensorQuota(QuerySensorQuotaInput input, Func<Task<PagedResultDto<SensorQuotaDto>>> action);
    }
}
