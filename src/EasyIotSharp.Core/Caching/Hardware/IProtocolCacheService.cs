using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Dto.Hardware.Params;
using EasyIotSharp.Core.Dto.Hardware;
using System.Threading.Tasks;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Caching.Hardware
{
    public interface IProtocolCacheService : ICacheService
    {
        /// <summary>
        /// 获取协议列表
        /// </summary>
        /// <param name="input"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        Task<PagedResultDto<ProtocolDto>> QueryProtocol(QueryProtocolInput input, Func<Task<PagedResultDto<ProtocolDto>>> action);
    }
}
