using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Dto.Rule;
using EasyIotSharp.Core.Dto;
using System.Threading.Tasks;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Caching.Rule
{
    public interface INotifyCacheService : ICacheService
    {
        /// <summary>
        /// 通知组列表
        /// </summary>
        /// <param name="input"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        Task<PagedResultDto<NotifyDto>> QueryNotifyConfig(PagingInput input, Func<Task<PagedResultDto<NotifyDto>>> action);
    }
}
