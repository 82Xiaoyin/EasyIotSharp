﻿using EasyIotSharp.Core.Domain.Proejct;
using EasyIotSharp.Core.Repositories.Mysql;
using LinqKit;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyIotSharp.Core.Repositories.Project.Impl
{
    public class GatewayRepository: MySqlRepositoryBase<Gateway, string>, IGatewayRepository
    {
        public GatewayRepository(ISqlSugarDatabaseProvider databaseProvider) : base(databaseProvider)
        {
        }

        public async Task<(int totalCount, List<Gateway> items)> Query(int tenantNumId,
                                                                      string Keyword,
                                                                      int state,
                                                                      string protocolId,
                                                                      string projectId,
                                                                      int pageIndex,
                                                                      int pageSize)
        {
            // 初始化条件
            var predicate = PredicateBuilder.New<Gateway>(t => t.IsDelete == false);
            if (tenantNumId > 0)
            {
                predicate = predicate.And(t => t.TenantNumId.Equals(tenantNumId));
            }

            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                predicate = predicate.And(t => t.Name.Contains(Keyword));
            }

            if (state > -1)
            {
                predicate = predicate.And(t => t.State.Equals(state));
            }

            if (!string.IsNullOrWhiteSpace(protocolId))
            {
                predicate = predicate.And(t => t.ProtocolId.Equals(protocolId));
            }

            if (!string.IsNullOrWhiteSpace(projectId))
            {
                predicate = predicate.And(t => t.ProjectId.Equals(projectId));
            }

            // 获取总记录数
            var totalCount = await CountAsync(predicate);
            if (totalCount == 0)
            {
                return (0, new List<Gateway>());
            }

            var query = GetDbClient().Queryable<Gateway>().Where(predicate)
                              .OrderBy(m => m.CreationTime, OrderByType.Desc)
                              .Skip((pageIndex - 1) * pageSize)
                              .Take(pageSize);
            // 查询数据
            var items = await query.ToListAsync();
            return (totalCount, items);
        }
        public async Task<List<Gateway>> QueryByIds(List<string> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return new List<Gateway>();
            }

            // 使用表达式构建查询条件
            var predicate = PredicateBuilder.New<Gateway>(false); // 初始化为空条件
            foreach (var id in ids)
            {
                var tempId = id; // 避免闭包问题
                predicate = predicate.Or(m => m.Id == tempId);
            }
            predicate = predicate.And(m => m.IsDelete == false); // 是否删除 = false

            // 查询数据
            var items = await GetListAsync(predicate);
            return items.ToList();
        }
    }
}
