using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyIotSharp.Core.Domain.Export;
using EasyIotSharp.Core.Dto.Export;
using EasyIotSharp.Core.Repositories.Mysql;
using LinqKit;

namespace EasyIotSharp.Core.Repositories.Export.Impl
{
    public class ExportReportRepository : MySqlRepositoryBase<ExportReport, string>, IExportReportRepository
    {
        public ExportReportRepository(ISqlSugarDatabaseProvider databaseProvider) : base(databaseProvider)
        {
        }

        /// <summary>
        /// 分页查询导出记录
        /// </summary>
        /// <param name="input">查询参数</param>
        /// <returns>总数和导出记录列表</returns>
        public async Task<(int totalCount, List<ExportReportDto> items)> QueryExportReportPage(string projectId,
                                                                       DateTime? startTime,
                                                                       DateTime? endTime,
                                                                       int? type,
                                                                       int pageIndex,
                                                                       int pageSize,
                                                                       bool isPage)
        {
            // 初始化查询条件
            var predicate = PredicateBuilder.New<ExportReport>(t => t.IsDelete == false);

            // 项目条件
            if (!string.IsNullOrWhiteSpace(projectId))
            {
                predicate = predicate.And(t => t.ProjectId == projectId);
            }

            if (type != null)
            {
                predicate = predicate.And(t => t.Type == type);
            }

            if (startTime.HasValue)
            {
                predicate = predicate.And(t => t.ExecuteTime >= startTime);
            }

            if (endTime.HasValue)
            {
                predicate = predicate.And(t => t.ExecuteTime <= endTime);
            }

            // 获取总记录数
            var totalCount = await Client.Queryable<ExportReport>()
                .Where(predicate)
                .CountAsync();

            if (totalCount == 0)
            {
                return (0, new List<ExportReportDto>());
            }
            var items = new List<ExportReportDto>();
            if (isPage)
            {
                // 分页查询数据
                items = await Client.Queryable<ExportReport>()
                   .Where(predicate)
                   .OrderByDescending(x => x.CreationTime)
                   .Select<ExportReportDto>()
                   .ToPageListAsync(pageIndex, pageSize);
            }
            else
            {

                // 分页查询数据
                items = await Client.Queryable<ExportReport>()
                   .Where(predicate)
                   .OrderByDescending(x => x.CreationTime)
                   .Select<ExportReportDto>()
                   .ToListAsync();
            }

            return (totalCount, items);
        }
    }
}
