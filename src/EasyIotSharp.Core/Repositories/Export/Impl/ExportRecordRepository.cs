using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyIotSharp.Core.Domain.Export;
using EasyIotSharp.Core.Domain.Hardware;
using EasyIotSharp.Core.Dto.Enum;
using EasyIotSharp.Core.Dto.Export;
using EasyIotSharp.Core.Dto.Export.Params;
using EasyIotSharp.Core.Repositories.Hardware;
using EasyIotSharp.Core.Repositories.Mysql;
using LinqKit;

namespace EasyIotSharp.Core.Repositories.Export.Impl
{
    public class ExportRecordRepository : MySqlRepositoryBase<ExportRecord, string>, IExportRecordRepository
    {
        public ExportRecordRepository(ISqlSugarDatabaseProvider databaseProvider) : base(databaseProvider)
        {
        }

        /// <summary>
        /// 查询出未执行的任务
        /// </summary>
        /// <returns></returns>
        public async Task<List<ExportRecordDto>> QueryExportRecord()
        {
            // 查询数据
            var items = await Client.Queryable<ExportRecord>()
                .Where(w => w.IsDelete == false && w.State == 0
                )
                .Select<ExportRecordDto>()
                .ToListAsync();

            return items;
        }

        /// <summary>
        /// 分页查询导出记录
        /// </summary>
        /// <param name="input">查询参数</param>
        /// <returns>总数和导出记录列表</returns>
        public async Task<(int totalCount, List<ExportDataRecordDto> items)> QueryExportRecordPage(string tenantId,
                                                                      string projectId,
                                                                      int pageIndex,
                                                                      int pageSize,
                                                                      bool isPage)
        {
            // 初始化查询条件
            var predicate = PredicateBuilder.New<ExportRecord>(t => t.IsDelete == false);

            // 租户条件
            if (!string.IsNullOrEmpty(tenantId))
            {
                predicate = predicate.And(t => t.TenantId == tenantId);
            }

            // 项目条件
            if (!string.IsNullOrWhiteSpace(projectId))
            {
                predicate = predicate.And(t => t.ProjectId == projectId);
            }

            // 获取总记录数
            var totalCount = await Client.Queryable<ExportRecord>()
                .Where(predicate)
                .CountAsync();

            if (totalCount == 0)
            {
                return (0, new List<ExportDataRecordDto>());
            }
            var items = new List<ExportDataRecordDto>();
            if (isPage)
            {
                // 分页查询数据
                items = await Client.Queryable<ExportRecord>()
                   .Where(predicate)
                   .OrderByDescending(x => x.CreationTime)
                   .Select<ExportDataRecordDto>()
                   .ToPageListAsync(pageIndex, pageSize);
            }
            else
            {

                // 分页查询数据
                items = await Client.Queryable<ExportRecord>()
                   .Where(predicate)
                   .OrderByDescending(x => x.CreationTime)
                   .Select<ExportDataRecordDto>()
                   .ToListAsync();
            }

            return (totalCount, items);
        }
    }
}
