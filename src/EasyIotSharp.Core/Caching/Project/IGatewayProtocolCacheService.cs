using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Dto.Project.Params;
using EasyIotSharp.Core.Dto.Project;
using System.Threading.Tasks;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Caching.Project
{
    public interface IGatewayProtocolCacheService : ICacheService
    {

        /// <summary>
        /// 获取网关
        /// </summary>
        /// <param name="input"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        Task<PagedResultDto<GatewayProtocolDto>> QueryGatewayProtocol(QueryGatewayProtocolInput input, Func<Task<PagedResultDto<GatewayProtocolDto>>> action);
    }
}
