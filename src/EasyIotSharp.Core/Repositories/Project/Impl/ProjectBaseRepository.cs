using EasyIotSharp.Core.Domain.Proejct;
using EasyIotSharp.Core.Repositories.Mysql;
using LinqKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SqlSugar;
using EasyIotSharp.Core.Domain.Queue;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using EasyIotSharp.Core.Dto.Project;
using EasyIotSharp.Core.Domain.Files;

namespace EasyIotSharp.Core.Repositories.Project.Impl
{
    /// <summary>
    /// 项目基础信息仓储实现类
    /// </summary>
    public class ProjectBaseRepository : MySqlRepositoryBase<ProjectBase, string>, IProjectBaseRepository
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="databaseProvider">数据库提供者</param>
        public ProjectBaseRepository(ISqlSugarDatabaseProvider databaseProvider) : base(databaseProvider)
        {
        }

        /// <summary>
        /// 查询项目列表
        /// </summary>
        /// <param name="tenantNumId">租户编号</param>
        /// <param name="keyword">关键字（项目名称或备注）</param>
        /// <param name="state">状态</param>
        /// <param name="createStartTime">创建开始时间</param>
        /// <param name="createEndTime">创建结束时间</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页大小</param>
        /// <returns>返回总数和项目列表</returns>
        /// <remarks>
        /// 关联查询：
        /// 1. RabbitProject（消息队列项目配置）
        /// 2. RabbitServerInfo（消息队列服务器信息）
        /// 3. Resource（资源信息）
        /// </remarks>
        public async Task<(int totalCount, List<ProjectBaseDto> items)> Query(int tenantNumId,
                                                      string keyword,
                                                      int state,
                                                      DateTime? createStartTime,
                                                      DateTime? createEndTime,
                                                      int pageIndex,
                                                      int pageSize)
        {
            var sql = Client.Queryable<ProjectBase>()
                 .LeftJoin<RabbitProject>((p, rp) => p.Id == rp.ProjectId && rp.IsDelete == false)
                 .LeftJoin<RabbitServerInfo>((p, rp, rs) => rp.RabbitServerInfoId == rs.Id && rs.IsDelete == false)
                 .LeftJoin<Resource>((p, rp, rs, r) => r.Id == p.ResourceId)
                 .Where((p, rp, rs, r) => p.IsDelete == false);

            // 初始化条件
            if (tenantNumId > 0)
            {
                sql = sql.Where((p, rp, rs, r) => p.TenantNumId.Equals(tenantNumId));
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                sql = sql.Where((p, rp, rs, r) => p.Name.Contains(keyword) || p.Remark.Contains(keyword));
            }

            if (state > -1)
            {
                sql = sql.Where((p, rp, rs, r) => p.State.Equals(state));
            }

            if (createStartTime.HasValue && createEndTime.HasValue)
            {
                sql = sql.Where((p, rp, rs, r) => p.CreationTime >= createStartTime.Value && p.CreationTime <= createEndTime.Value);
            }
            else
            {
                if (createStartTime.HasValue)
                {
                    sql = sql.Where((p, rp, rs, r) => p.CreationTime >= createStartTime.Value);
                }
                if (createEndTime.HasValue)
                {
                    sql = sql.Where((p, rp, rs, r) => p.CreationTime <= createEndTime.Value);
                }
            }

            // 获取总记录数
            var totalCount = await sql.CountAsync();
            if (totalCount == 0)
            {
                return (0, new List<ProjectBaseDto>());
            }

            // 分页查询
            var items = await sql.Select((p, rp, rs, r) => new ProjectBaseDto
            {
                Id = p.Id,
                TenantNumId = p.TenantNumId,
                Name = p.Name,
                Address = p.Address,
                CreationTime = p.CreationTime,
                latitude = p.latitude,
                Longitude = p.Longitude,
                OperatorId = p.OperatorId,
                OperatorName = p.OperatorName,
                Remark = p.Remark,
                State = p.State == 1 ? true : false,
                Host = rs.Host,
                RabbitServerInfoId = rs.Id,
                ResourceId = r.Id,
                ResourceUrl = r.Url,
            }).ToPageListAsync(pageIndex, pageSize);

            return (totalCount, items);
        }

        /// <summary>
        /// 查询单个项目详细信息
        /// </summary>
        /// <param name="id">项目ID</param>
        /// <returns>项目详细信息</returns>
        /// <remarks>
        /// 关联查询：
        /// 1. RabbitProject（消息队列项目配置）
        /// 2. RabbitServerInfo（消息队列服务器信息）
        /// </remarks>
        public async Task<ProjectBaseDto> QueryByProjectBaseFirst(string id)
        {
            var sql = Client.Queryable<ProjectBase>()
                 .LeftJoin<RabbitProject>((p, rp) => p.Id == rp.ProjectId && rp.IsDelete == false)
                 .LeftJoin<RabbitServerInfo>((p, rp, rs) => rp.RabbitServerInfoId == rs.Id && rs.IsDelete == false)
                  .LeftJoin<Resource>((p, rp, rs, r) => r.Id == p.ResourceId)
                 .Where((p, rp, rs) => p.IsDelete == false);

            var items = await sql.Select((p, rp, rs,r) => new ProjectBaseDto
            {
                Id = p.Id,
                TenantNumId = p.TenantNumId,
                Name = p.Name,
                Address = p.Address,
                CreationTime = p.CreationTime,
                latitude = p.latitude,
                Longitude = p.Longitude,
                OperatorId = p.OperatorId,
                OperatorName = p.OperatorName,
                Remark = p.Remark,
                State = p.State == 1 ? true : false,
                Host = rs.Host,
                RabbitServerInfoId = rs.Id,
                ResourceUrl = r.Url,
            }).FirstAsync();

            return items;
        }

        /// <summary>
        /// 根据ID列表批量查询项目
        /// </summary>
        /// <param name="ids">项目ID列表</param>
        /// <returns>项目列表</returns>
        /// <remarks>
        /// 使用 LinqKit 的 PredicateBuilder 构建动态查询条件
        /// 注意：
        /// 1. 排除已删除的项目
        /// 2. 使用临时变量避免闭包问题
        /// </remarks>
        public async Task<List<ProjectBase>> QueryByIds(List<string> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return new List<ProjectBase>();
            }

            // 使用表达式构建查询条件
            var predicate = PredicateBuilder.New<ProjectBase>(false); // 初始化为空条件
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

        /// <summary>
        /// 查询项目关联的消息队列配置
        /// </summary>
        /// <param name="projectId">项目ID</param>
        /// <returns>消息队列项目配置信息</returns>
        /// <remarks>
        /// 仅返回未删除的配置信息
        /// </remarks>
        public async Task<RabbitProject> QueryRabbitProject(string projectId)
        {
            return await Client.Queryable<RabbitProject>().Where(w => w.ProjectId == projectId && w.IsDelete == false).FirstAsync();
        }

        /// <summary>
        /// 添加项目消息队列配置
        /// </summary>
        /// <param name="rabbitProject">消息队列项目配置信息</param>
        /// <returns>无</returns>
        public async Task AddRabbitProject(RabbitProject rabbitProject)
        {
            await Client.Insertable(rabbitProject).ExecuteCommandAsync();
        }

        /// <summary>
        /// 更新项目消息队列配置
        /// </summary>
        /// <param name="rabbitProject">消息队列项目配置信息</param>
        /// <returns>无</returns>
        public async Task UpdateRabbitProject(RabbitProject rabbitProject)
        {
            await Client.Updateable(rabbitProject).ExecuteCommandAsync();
        }
    }
}
