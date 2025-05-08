using EasyIotSharp.Core.Dto.Export;
using EasyIotSharp.Core.Dto.Export.Params;
using System.Collections.Generic;
using System.Threading.Tasks;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Services.Export
{
    /// <summary>
    /// 导出记录服务接口
    /// </summary>
    public interface IExportRecordService
    {
        /// <summary>
        /// 获取列表
        /// </summary>
        /// <returns></returns>
        Task<List<ExportRecordDto>> QueryExportRecord();

        /// <summary>
        /// 创建导出记录
        /// </summary>
        /// <param name="input">创建参数</param>
        /// <returns>新创建的记录ID</returns>
        Task<string> CreateExportRecord(ExportRecordInsert input);

        /// <summary>
        /// 更新导出记录
        /// </summary>
        /// <param name="input">更新参数</param>
        Task UpdateExportRecord(ExportRecordDto input);

        /// <summary>
        /// 删除导出记录
        /// </summary>
        /// <param name="id">记录ID</param>
        Task DeleteExportRecord(string id);

        /// <summary>
        /// 分页查询导出记录
        /// </summary>
        /// <param name="input">查询参数</param>
        /// <returns>分页结果</returns>
        Task<PagedResultDto<ExportDataRecordDto>> QueryExportRecord(ExportRecordInput input);
    }
}
