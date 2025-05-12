using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Dto.Export.Params;
using EasyIotSharp.Core.Dto.Export;
using System.Threading.Tasks;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Services.Export
{
    /// <summary>
    /// 导出定时任务
    /// </summary>
    public interface IExportReportService
    {
        /// <summary>
        /// 导出报告列表
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<PagedResultDto<ExportReportDto>> QueryExportReportPage(ExportReportInput input);

        /// <summary>
        /// 创建导出报告
        /// </summary>
        /// <param name="input">创建参数</param>
        /// <returns>新创建的记录ID</returns>
        Task CreateExportReport(ExportReportInsert input);
    }
}
