using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Dto.Rule;
using EasyIotSharp.Core.Dto;
using System.Threading.Tasks;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Caching.Rule
{
    public interface IAlarmsConfigCacheService : ICacheService
    {
        /// <summary>
        /// 配置项目关系
        /// </summary>
        /// <param name="input"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        Task<PagedResultDto<AlarmsConfigDto>> QueryAlarmsConfig(PagingInput input, Func<Task<PagedResultDto<AlarmsConfigDto>>> action);
    }
}
