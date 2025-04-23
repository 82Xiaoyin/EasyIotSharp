using EasyIotSharp.Core.Domain.Rule;
using EasyIotSharp.Core.Dto.Rule;
using EasyIotSharp.Core.Repositories.Mysql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyIotSharp.Core.Repositories.Rule
{
    public interface INotifyRepository : IMySqlRepositoryBase<Notify, string>
    {
        /// <summary>
        /// 列表
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<(int totalCount, List<NotifyDto> items)> Query(
             int pageIndex,
             int pageSize);

        /// <summary>
        /// 列表
        /// </summary>
        /// <param name="type"></param>
        /// <param name="keyWord"></param>
        /// <param name="starTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
         Task<(int totalCount, List<NotifyRecordDto> items)> QueryNotifyRecordDto(
            int? type,
            string keyWord,
            DateTime? starTime,
            DateTime? endTime,
            int pageIndex,
            int pageSize);

        /// <summary>
        /// 通过Id获取用户信息
        /// </summary>
        /// <param name="notifyId"></param>
        /// <returns></returns>
        Task<List<UserNotify>> QueryUserNotify(string notifyId);
        
        /// <summary>
        /// 新增用户通知关系
        /// </summary>
        /// <param name="userNotifies"></param>
        /// <returns></returns>
        Task InsertUserNotifyList(List<UserNotify> userNotifies);

        /// <summary>
        /// 修改用户通知关系表
        /// </summary>
        /// <param name="userNotifies"></param>
        /// <returns></returns>
        Task DeleteableUserNotifyList(List<UserNotify> userNotifies);
    }
}