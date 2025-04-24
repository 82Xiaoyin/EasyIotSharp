using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Dto.Rule.Params;
using EasyIotSharp.Core.Dto.Rule;
using System.Threading.Tasks;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Caching.Rule
{
    public interface INotifyRecordCacheService : ICacheService
    {
        /// <summary>
        /// 通知记录列表
        /// </summary>
        /// <param name="input"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        Task<PagedResultDto<NotifyRecordDto>> QueryNotifyRecord(NotifyRecordInput input, Func<Task<PagedResultDto<NotifyRecordDto>>> action);
    }
}
