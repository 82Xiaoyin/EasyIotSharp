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

namespace EasyIotSharp.Core.Repositories.Project.Impl
{
    public class ProjectBaseRepository : MySqlRepositoryBase<ProjectBase, string>, IProjectBaseRepository
    {
        public ProjectBaseRepository(ISqlSugarDatabaseProvider databaseProvider) : base(databaseProvider)
        {
        }

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
                 .Where((p, rp, rs) => p.IsDelete == false);

            // 初始化条件
            if (tenantNumId > 0)
            {
                sql = sql.Where((p, rp, rs) => p.TenantNumId.Equals(tenantNumId));
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                sql = sql.Where((p, rp, rs) => p.Name.Contains(keyword) || p.Remark.Contains(keyword));
            }

            if (state > -1)
            {
                sql = sql.Where((p, rp, rs) => p.State.Equals(state));
            }

            if (createStartTime.HasValue && createEndTime.HasValue)
            {
                sql = sql.Where((p, rp, rs) => p.CreationTime >= createStartTime.Value && p.CreationTime <= createEndTime.Value);
            }
            else
            {
                if (createStartTime.HasValue)
                {
                    sql = sql.Where((p, rp, rs) => p.CreationTime >= createStartTime.Value);
                }
                if (createEndTime.HasValue)
                {
                    sql = sql.Where((p, rp, rs) => p.CreationTime <= createEndTime.Value);
                }
            }

            // 获取总记录数
            var totalCount = await sql.CountAsync();
            if (totalCount == 0)
            {
                return (0, new List<ProjectBaseDto>());
            }

            // 分页查询
            var items = await sql.Select((p, rp, rs) => new ProjectBaseDto
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
                State = p.State==1?true:false,
                Host = rs.Host,
                RabbitServerInfoId = rs.Id
            }).ToPageListAsync(pageIndex, pageSize);

            return (totalCount, items);
        }

        public async Task<ProjectBaseDto> QueryByProjectBaseFirst(string id)
        {
            var sql = Client.Queryable<ProjectBase>()
                 .LeftJoin<RabbitProject>((p, rp) => p.Id == rp.ProjectId && rp.IsDelete == false)
                 .LeftJoin<RabbitServerInfo>((p, rp, rs) => rp.RabbitServerInfoId == rs.Id && rs.IsDelete == false)
                 .Where((p, rp, rs) => p.IsDelete == false);

            var items = await sql.Select((p, rp, rs) => new ProjectBaseDto
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
                RabbitServerInfoId = rs.Id
            }).FirstAsync();

            return items;
        }

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

        public async Task<RabbitProject> QueryRabbitProject(string projectId)
        {
            return await Client.Queryable<RabbitProject>().Where(w => w.ProjectId == projectId && w.IsDelete == false).FirstAsync();
        }

        public async Task AddRabbitProject(RabbitProject rabbitProject)
        {
            await Client.Insertable(rabbitProject).ExecuteCommandAsync();
        }

        public async Task UpdateRabbitProject(RabbitProject rabbitProject)
        {
            await Client.Updateable(rabbitProject).ExecuteCommandAsync();
        }
    }
}
