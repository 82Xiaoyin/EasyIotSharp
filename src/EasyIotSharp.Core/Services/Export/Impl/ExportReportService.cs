using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Dto.Export.Params;
using EasyIotSharp.Core.Dto.Export;
using System.Threading.Tasks;
using EasyIotSharp.Core.Repositories.Export;
using EasyIotSharp.Core.Repositories.Export.Impl;
using UPrime.Services.Dto;
using EasyIotSharp.Core.Domain.Export;

namespace EasyIotSharp.Core.Services.Export.Impl
{
    public class ExportReportService : ServiceBase, IExportReportService
    {
        private readonly IExportReportRepository _exportReportRepository;

        public ExportReportService(IExportReportRepository exportReportRepository)
        {
            _exportReportRepository = exportReportRepository;
        }

        /// <summary>
        /// 分页查询导出记录
        /// </summary>
        /// <param name="input">查询参数</param>
        /// <returns>分页结果</returns>
        public async Task<PagedResultDto<ExportReportDto>> QueryExportReportPage(ExportReportInput input)
        {
            var (totalCount, items) = await _exportReportRepository.QueryExportReportPage(input.ProjectId, input.StartTime, input.EndTime, input.Type, input.PageIndex, input.PageSize, input.IsPage);
            return new PagedResultDto<ExportReportDto>
            {
                TotalCount = totalCount,
                Items = items
            };
        }

        /// <summary>
        /// 创建导出报告
        /// </summary>
        /// <param name="input">创建参数</param>
        /// <returns>新创建的记录ID</returns>
        public async Task CreateExportReport(ExportReportInsert input)
        {
            if (string.IsNullOrEmpty(input.Name))
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "名称不能为空");
            }

            var entity = new ExportReport
            {
                Id = Guid.NewGuid().ToString().Replace("-", ""),
                Name = input.Name,
                ProjectId = input.ProjectId,
                Type = input.Type,
                State = 0, // 初始状态：未执行
                UpdatedAt = DateTime.Now,
                CreationTime = DateTime.Now,
                OperatorId = "System",
                OperatorName = "System",
            };

            await _exportReportRepository.InsertAsync(entity);
        }
    }
}
