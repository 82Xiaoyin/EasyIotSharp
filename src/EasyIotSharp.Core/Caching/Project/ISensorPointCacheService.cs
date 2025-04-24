using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Dto.Project.Params;
using EasyIotSharp.Core.Dto.Project;
using System.Threading.Tasks;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Caching.Project
{
    public interface ISensorPointCacheService : ICacheService
    {
        /// <summary>
        /// 测点
        /// </summary>
        /// <param name="input"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        Task<PagedResultDto<SensorPointDto>> QuerySensorPoint(QuerySensorPointInput input, Func<Task<PagedResultDto<SensorPointDto>>> action);
    }
}
