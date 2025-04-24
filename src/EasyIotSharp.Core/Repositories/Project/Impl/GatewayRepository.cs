using EasyIotSharp.Core.Domain.Proejct;
using EasyIotSharp.Core.Dto.Project;
using EasyIotSharp.Core.Repositories.Mysql;
using LinqKit;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FluentValidation.Validators.PredicateValidator;

namespace EasyIotSharp.Core.Repositories.Project.Impl
{
    public class GatewayRepository : MySqlRepositoryBase<Gateway, string>, IGatewayRepository
    {
        public GatewayRepository(ISqlSugarDatabaseProvider databaseProvider) : base(databaseProvider)
        {
        }
        /// <summary>
        /// 根据网关id获取单条信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public GatewayBaseDto GetGateway(string id)
        {
            return Client.Queryable<Gateway>()
                .LeftJoin<EasyIotSharp.Core.Domain.Tenant.Tenant>((g, t) => g.TenantNumId == t.NumId)
                .Where((g, t) =>g.Id.Equals(id))
                .Select((g, t) => new GatewayBaseDto
                { 
                    Id = g.Id, 
                    TenantNumId = g.TenantNumId, 
                    TenantAbbreviation = t.Abbreviation,
                    Name = g.Name,
                    ProjectId = g.ProjectId,
                    State = g.State,
                    ProtocolId = g.ProtocolId,
                }).First();
        }

        /// <summary>
        /// 根据网关ids获取集合
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<Gateway> GetByIds(List<string> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return new List<Gateway>();
            }

            // 使用表达式构建查询条件
            var predicate = PredicateBuilder.New<Gateway>(false); // 初始化为空条件
            predicate = predicate.And(t => ids.Contains(t.Id));
            predicate = predicate.And(m => m.IsDelete == false); // 是否删除 = false

            // 查询数据
            var items = Client.Queryable<Gateway>().Where(predicate);
            return items.ToList();
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

            var query = Client.Queryable<Gateway>().Where(predicate)
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
