using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Dto.Hardware.Params;
using System.Threading.Tasks;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Caching.Hardware
{
    public interface IProtocolConfigCacheService : ICacheService
    {
        /// <summary>
        /// 通过条件分页查询配置信息列表
        /// </summary>
        /// <param name="input"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        Task<PagedResultDto<QueryProtocolConfigByProtocolIdOutput>> QueryProtocolConfig(QueryProtocolConfigInput input, Func<Task<PagedResultDto<QueryProtocolConfigByProtocolIdOutput>>> action);
    }
}
