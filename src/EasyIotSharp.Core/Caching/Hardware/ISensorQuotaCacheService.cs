﻿using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Dto.Hardware.Params;
using EasyIotSharp.Core.Dto.Hardware;
using System.Threading.Tasks;
using UPrime.Services.Dto;
using EasyIotSharp.Core.Domain.Hardware;

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

        /// <summary>
        /// 传感器指标列表
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        List<SensorQuota> GetSensorQuotaList(Func<List<SensorQuota>> action);
    }
}
