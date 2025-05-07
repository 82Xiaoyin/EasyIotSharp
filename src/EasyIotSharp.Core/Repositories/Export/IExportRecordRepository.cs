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

        /// <summary>
        /// 分页查询导出记录
        /// </summary>
        /// <param name="input">查询参数</param>
        /// <returns>总数和导出记录列表</returns>
        Task<(int totalCount, List<ExportDataRecordDto> items)> QueryExportRecordPage(string tenantId,
                                                                       string projectId,
                                                                       int pageIndex,
                                                                       int pageSize,
                                                                       bool isPage);
    }
}
