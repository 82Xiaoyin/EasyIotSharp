using System.Collections.Generic;
using System.Threading.Tasks;
using EasyIotSharp.Core.Domain.Export;
using EasyIotSharp.Core.Dto.Export;
using EasyIotSharp.Core.Repositories.Mysql;

namespace EasyIotSharp.Core.Repositories.Export
{
    public interface IExportRecordRepository : IMySqlRepositoryBase<ExportRecord, string>
    {
        /// <summary>
        /// 获取未执行的导出任务
        /// </summary>
        /// <returns></returns>
        Task<List<ExportRecordDto>> QueryExportRecord();
    }
}
