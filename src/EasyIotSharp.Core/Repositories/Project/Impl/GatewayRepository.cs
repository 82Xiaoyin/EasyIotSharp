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
    /// <summary>
    /// 网关仓储实现类
    /// </summary>
    public class GatewayRepository : MySqlRepositoryBase<Gateway, string>, IGatewayRepository
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="databaseProvider">数据库提供者</param>
        public GatewayRepository(ISqlSugarDatabaseProvider databaseProvider) : base(databaseProvider)
        {
        }

        /// <summary>
        /// 获取单个网关信息
        /// </summary>
        /// <param name="id">网关ID</param>
        /// <returns>网关基础信息</returns>
        /// <remarks>
        /// 关联查询：
        /// - 租户信息（获取租户简称）
        /// </remarks>
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
                    Imei = g.Imei,
                    DeviceModel = g.DeviceModel,
                }).First();
        }

        /// <summary>
        /// 根据ID列表批量获取网关信息
        /// </summary>
        /// <param name="ids">网关ID列表</param>
        /// <returns>网关列表</returns>
        /// <remarks>
        /// 查询条件：
        /// 1. ID在指定列表中
        /// 2. 未删除的记录
        /// </remarks>
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

        /// <summary>
        /// 分页查询网关列表
        /// </summary>
        /// <param name="tenantNumId">租户编号</param>
        /// <param name="Keyword">关键字（网关名称）</param>
        /// <param name="state">状态</param>
        /// <param name="protocolId">协议ID</param>
        /// <param name="projectId">项目ID</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页大小</param>
        /// <returns>总数和网关列表</returns>
        /// <remarks>
        /// 查询条件：
        /// 1. 未删除的记录
        /// 2. 指定租户的记录（如果提供）
        /// 3. 名称包含关键字（如果提供）
        /// 4. 指定状态的记录（如果提供）
        /// 5. 指定协议的记录（如果提供）
        /// 6. 指定项目的记录（如果提供）
        /// 
        /// 排序方式：
        /// - 按创建时间降序
        /// </remarks>
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

        /// <summary>
        /// 根据ID列表批量查询网关
        /// </summary>
        /// <param name="ids">网关ID列表</param>
        /// <returns>网关列表</returns>
        /// <remarks>
        /// 使用 LinqKit 的 PredicateBuilder 构建动态查询条件
        /// 注意：
        /// 1. 排除已删除的网关
        /// 2. 使用临时变量避免闭包问题
        /// 3. 使用 OR 条件组合多个ID查询
        /// </remarks>
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
