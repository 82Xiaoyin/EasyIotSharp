using System.Threading.Tasks;
using EasyIotSharp.Core.Dto;
using EasyIotSharp.Core.Dto.Rule;
using EasyIotSharp.Core.Dto.Rule.Params;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Services.Rule
{
    public interface INotifyService
    {
        /// <summary>
        /// 列表
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<PagedResultDto<NotifyDto>> QueryNotifyConfig(Dto.PagingInput input);

        /// <summary>
        /// 新增通知
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task InsertNotifyConfig(InsertNotify input);

        /// <summary>
        /// 修改通知
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task UpdateNotifyConfig(InsertNotify input);

        /// <summary>
        /// 删除通知
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task DeleteNotifyConfig(DeleteInput input);

        /// <summary>
        /// 修改状态
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task UpdateNotifyConfigState(InsertNotify input);

        /// <summary>
        /// 获取通知记录
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<PagedResultDto<NotifyRecordDto>> QueryNotifyRecord(NotifyRecordInput input);
    }
}