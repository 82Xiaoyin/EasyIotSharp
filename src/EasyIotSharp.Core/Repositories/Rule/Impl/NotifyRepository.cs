using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Threading.Tasks;
using EasyIotSharp.Core.Domain.Rule;
using EasyIotSharp.Core.Domain.TenantAccount;
using EasyIotSharp.Core.Dto.Rule;
using EasyIotSharp.Core.Dto.Rule.Params;
using EasyIotSharp.Core.Repositories.Mysql;
using LinqKit;

namespace EasyIotSharp.Core.Repositories.Rule.Impl
{
    public class NotifyRepository : MySqlRepositoryBase<Notify, string>, INotifyRepository
    {
        public NotifyRepository(ISqlSugarDatabaseProvider databaseProvider) : base(databaseProvider)
        {
        }

        /// <summary>
        /// 列表
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<(int totalCount, List<NotifyDto> items)> Query(
            int pageIndex,
            int pageSize)
        {
            // 初始化条件
            var sql = Client.Queryable<Notify>()
                .Where(w => w.IsDelete == false);

            // 获取总记录数
            var totalCount = await sql.CountAsync();
            if (totalCount == 0)
            {
                return (0, new List<NotifyDto>());
            }

            var userList = await Client.Queryable<UserNotify>()
                .LeftJoin<Soldier>((un, u) => un.UserId == u.Id)
                .Select<UserNotifyDto>().ToListAsync();

            // 分页查询
            var items = await sql.Select<NotifyDto>().ToPageListAsync(pageIndex, pageSize);

            foreach (var item in items)
            {
                if (item.Type == 1)
                {
                    item.Users = userList.Where(w => w.NotifyId == item.Id).ToList();
                }
            }

            return (totalCount, items);
        }

        /// <summary>
        /// 获取通知记录
        /// </summary>
        /// <returns></returns>
        public async Task<(int totalCount, List<NotifyRecordDto> items)> QueryNotifyRecordDto(
            int? type,
            string keyWord,
            DateTime? starTime,
            DateTime? endTime,
            int pageIndex,
            int pageSize)
        {
            var sql = Client.Queryable<NotifyRecord>()
                .Where(w => w.IsDelete == false);

            if (type != null)
            {
                sql.Where(w => w.Type == type);
            }

            if (!string.IsNullOrEmpty(keyWord))
            {
                sql.Where(w => w.SendUserName == keyWord);
            }

            if (starTime.HasValue)
            {
                sql.Where(w => w.SendTime >= starTime);
            }

            if (endTime.HasValue)
            {
                sql.Where(w => w.SendTime <= endTime);
            }

            var totalCount = await sql.CountAsync();
            var items = await sql.Select<NotifyRecordDto>().ToPageListAsync(pageIndex, pageSize);
            return (totalCount, items);
        }

        /// <summary>
        /// 通过通知组Id 获取信息
        /// </summary>
        /// <param name="notifyId"></param>
        /// <returns></returns>
        public async Task<List<UserNotify>> QueryUserNotify(string notifyId)
        {
            return await Client.Queryable<UserNotify>().Where(w => w.IsDelete == false && w.NotifyId == notifyId).ToListAsync();
        }

        /// <summary>
        /// 插入中间表数据
        /// </summary>
        /// <param name="userNotifies"></param>
        /// <returns></returns>
        public async Task InsertUserNotifyList(List<UserNotify> userNotifies)
        {
            await Client.Insertable(userNotifies).ExecuteCommandAsync();
        }

        /// <summary>
        /// 修改中间表数据
        /// </summary>
        /// <param name="userNotifies"></param>
        /// <returns></returns>
        public async Task DeleteableUserNotifyList(List<UserNotify> userNotifies)
        {
            await Client.Deleteable(userNotifies).ExecuteCommandAsync();
        }
    }
}