using EasyIotSharp.Core.Domain.Queue;
using EasyIotSharp.Core.Repositories.Mysql;
using System.Collections.Generic;
using System.Threading.Tasks;
using SqlSugar;
using LinqKit;

namespace EasyIotSharp.Core.Repositories.Queue.Impl
{
    /// <summary>
    /// RabbitMQ服务器配置信息仓储实现类
    /// </summary>
    public class RabbitServerInfoRepository : MySqlRepositoryBase<RabbitServerInfo, string>, IRabbitServerInfoRepository
    {
        public RabbitServerInfoRepository(ISqlSugarDatabaseProvider databaseProvider) : base(databaseProvider)
        {
        }

        /// <summary>
        /// 分页查询RabbitMQ服务器配置信息列表
        /// </summary>
        /// <param name="tenantNumId">租户编号（大于0时参与查询）</param>
        /// <param name="keyword">关键字（模糊匹配主机地址或用户名）</param>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="isPage">是否分页</param>
        /// <returns>返回总记录数和分页数据</returns>
        public async Task<(int totalCount, List<RabbitServerInfo> items)> Query(int tenantNumId, string keyword, int isEnable, int pageIndex, int pageSize, bool isPage = true)
        {
            // 初始化条件
            var predicate = PredicateBuilder.New<RabbitServerInfo>(r => r.IsDelete == false);

            // 租户编号过滤
            if (tenantNumId > 0)
            {
                predicate = predicate.And(r => r.TenantNumId == tenantNumId);
            }

            // 关键字模糊查询
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                predicate = predicate.And(r => r.Host.Contains(keyword) || r.Username.Contains(keyword));
            }

            if (isEnable > -1)
            {
                predicate = predicate.And(r => r.IsEnable == (isEnable == 0 ? true : false));
            }

            // 获取总记录数
            var totalCount = await CountAsync(predicate);
            if (totalCount == 0)
            {
                return (0, new List<RabbitServerInfo>());
            }

            if (isPage)
            {
                // 手动拼接排序和分页逻辑
                var query = Client.Queryable<RabbitServerInfo>().Where(predicate)
                                .OrderByDescending(m => m.CreationTime)
                                .Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize);
                // 分页查询，默认按 CreationTime 降序排序
                var items = await query.ToListAsync();
                return (totalCount, items);
            }
            else
            {
                // 手动拼接排序逻辑
                var query = await Client.Queryable<RabbitServerInfo>().Where(predicate)
                                .OrderByDescending(m => m.CreationTime)
                                .ToListAsync();
                return (totalCount, query);
            }
        }
    }
}