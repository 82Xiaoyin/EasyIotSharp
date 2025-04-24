using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Dto.Queue.Params;
using EasyIotSharp.Core.Dto.Queue;
using System.Threading.Tasks;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Caching.Queue
{
    public interface IRabbitServerInfoCacheService : ICacheService
    {
        /// <summary>
        /// MQ服务器配置信息
        /// </summary>
        /// <param name="input"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        Task<PagedResultDto<RabbitServerInfoDto>> QueryRabbitServerInfo(QueryRabbitServerInfoInput input, Func<Task<PagedResultDto<RabbitServerInfoDto>>> action);
    }
}
