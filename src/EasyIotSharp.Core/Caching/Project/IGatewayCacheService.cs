using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Dto.Project.Params;
using EasyIotSharp.Core.Dto.Project;
using System.Threading.Tasks;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Caching.Project
{
    public interface IGatewayCacheService : ICacheService
    {
        /// <summary>
        /// 网关设备
        /// </summary>
        /// <param name="input"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        Task<PagedResultDto<GatewayDto>> QueryGateway(QueryGatewayInput input, Func<Task<PagedResultDto<GatewayDto>>> action);
    }
}
