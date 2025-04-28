using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyIotSharp.Core.Domain.Export;
using EasyIotSharp.Core.Domain.Hardware;
using EasyIotSharp.Core.Dto.Export;
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

        public async Task<List<ExportRecordDto>> QueryExportRecord()
        {
            // 查询数据
            var items = await Client.Queryable<ExportRecord>()
                .Where(w => w.IsDelete == false && w.State == 1
                )
                .Select<ExportRecordDto>()
                .ToListAsync();

            return items;
        }
    }
}
