using EasyIotSharp.Core.Domain.Proejct;
using EasyIotSharp.Core.Repositories.Mysql;
using LinqKit;
using Nest;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Text;
using System.Threading.Tasks;
using static FluentValidation.Validators.PredicateValidator;

namespace EasyIotSharp.Core.Repositories.Project.Impl
{
    public class GatewayProtocolRepository : MySqlRepositoryBase<GatewayProtocol, string>, IGatewayProtocolRepository
    {
        public GatewayProtocolRepository(ISqlSugarDatabaseProvider databaseProvider) : base(databaseProvider)
        {
        }

        public async Task<(int totalCount, List<GatewayProtocol> items)> Query(int tenantNumId,
                                                                               string gatewayId,
                                                                               int pageIndex,
                                                                               int pageSize)
        {
            // 初始化条件
            var predicate = PredicateBuilder.New<GatewayProtocol>(t => t.IsDelete == false);
            if (tenantNumId > 0)
            {
                predicate = predicate.And(t => t.TenantNumId.Equals(tenantNumId));
            }

            if (!string.IsNullOrWhiteSpace(gatewayId))
            {
                predicate = predicate.And(t => t.GatewayId.Equals(gatewayId));
            }

            // 获取总记录数
            var totalCount = await CountAsync(predicate);
            if (totalCount == 0)
            {
                return (0, new List<GatewayProtocol>());
            }

            var query = Client.Queryable<GatewayProtocol>().Where(predicate)
                              .OrderBy(m => m.CreationTime, OrderByType.Desc)
                              .Skip((pageIndex - 1) * pageSize)
                              .Take(pageSize);
            // 查询数据
            var items = await query.ToListAsync();
            return (totalCount, items);
        }

        /// <summary>
        /// 通过网关id获取数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public GatewayProtocol GetGatewayProtocol(string id)
        {
            return  Client.Queryable<GatewayProtocol>().Where(w=>w.GatewayId.Equals(id))
                              .First();
        }
    }
}
