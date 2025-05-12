using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyIotSharp.Core.Domain.Export;
using EasyIotSharp.Core.Dto.Export;
using EasyIotSharp.Core.Repositories.Mysql;

namespace EasyIotSharp.Core.Repositories.Export
{
    public interface IExportReportRepository : IMySqlRepositoryBase<ExportReport, string>
    {
        /// <summary>
        /// 分页查询导出记录
        /// </summary>
        /// <param name="input">查询参数</param>
        /// <returns>总数和导出记录列表</returns>
        Task<(int totalCount, List<ExportReportDto> items)> QueryExportReportPage(string projectId,
                                                                       DateTime? startTime,
                                                                       DateTime? endTime,
                                                                       int? type,
                                                                       int pageIndex,
                                                                       int pageSize,
                                                                       bool isPage);
    }
}
