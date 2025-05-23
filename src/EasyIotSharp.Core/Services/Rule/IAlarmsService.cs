﻿using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Dto.Rule.Params;
using EasyIotSharp.Core.Dto.Rule;
using System.Threading.Tasks;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Services.Rule
{
    public interface IAlarmsService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<PagedResultDto<AlarmsDto>> GetAlarmsList(AlarmsInput input);
        /// <summary>
        /// 报警列表
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<List<AlarmsDto>> GetAlarmsData(AlarmsInput input);

        /// <summary>
        /// 报警列表修改
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task UpdateAlarms(AlarmsDto input);
    }
}
