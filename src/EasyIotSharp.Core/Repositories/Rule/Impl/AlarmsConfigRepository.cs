using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyIotSharp.Core.Domain.Rule;
using EasyIotSharp.Core.Dto.Rule;
using EasyIotSharp.Core.Repositories.Mysql;
using EasyIotSharp.Core.Repositories.Queue;
using LinqKit;

namespace EasyIotSharp.Core.Repositories.Rule.Impl
{
    public class AlarmsConfigRepository : MySqlRepositoryBase<AlarmsConfig, string>, IAlarmsConfigRepository
    {
        public AlarmsConfigRepository(ISqlSugarDatabaseProvider databaseProvider) : base(databaseProvider)
        {
        }

        /// <summary>
        /// 列表
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<(int totalCount, List<AlarmsConfigDto> items)> Query(
            int pageIndex,
            int pageSize)
        {
            // 初始化条件
            var sql = Client.Queryable<AlarmsConfig>().Where(w => w.IsDelete == false);

            // 获取总记录数
            var totalCount = await sql.CountAsync();
            if (totalCount == 0)
            {
                return (0, new List<AlarmsConfigDto>());
            }

            // 分页查询
            var items = await sql.Select<AlarmsConfigDto>().ToPageListAsync(pageIndex, pageSize);

            return (totalCount, items);
        }
    }
}
