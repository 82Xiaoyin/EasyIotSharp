using EasyIotSharp.Core.Dto.Export.Params;
using EasyIotSharp.Core.Dto.Export;
using System.Threading.Tasks;
using EasyIotSharp.Core.Services.Export;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UPrime.Services.Dto;
using EasyIotSharp.API.Filters;
using UPrime.WebApi;
using EasyIotSharp.Core.Dto;

namespace EasyIotSharp.API.Controllers
{
    public class ExportController : ApiControllerBase
    {
        private readonly IExportRecordService _exportRecordService;

        public ExportController()
        {
            _exportRecordService = UPrime.UPrimeEngine.Instance.Resolve<IExportRecordService>();
        }

        /// <summary>
        /// 资源列表
        /// </summary>
        /// <param name="input">查询参数</param>
        /// <returns>分页结果</returns>
        [HttpPost("/Export/ExportRecord/Query")]
        [Authorize]
        public async Task<PagedResultDto<ExportDataRecordDto>> QueryExportRecord([FromBody] ExportRecordInput input)
        {
            return await _exportRecordService.QueryExportRecord(input);

        }

        /// <summary>
        /// 创建导出记录
        /// </summary>
        /// <param name="input">创建参数</param>
        /// <returns></returns>
        [HttpPost("/Export/ExportRecord/Insert")]
        [Authorize]
        public async Task<UPrimeResponse<string>> CreateExportRecord([FromBody] ExportRecordInsert input)
        {
            UPrimeResponse<string> res = new UPrimeResponse<string>();
            res.Result = await _exportRecordService.CreateExportRecord(input);
            return res;
        }

        /// <summary>
        /// 更新导出记录
        /// </summary>
        /// <param name="input">创建参数</param>
        /// <returns></returns>
        [HttpPost("/Export/ExportRecord/Update")]
        [Authorize]
        public async Task<UPrimeResponse> UpdateExportRecord([FromBody] ExportRecordInsert input)
        {
            await _exportRecordService.UpdateExportRecord(input); ;
            return new UPrimeResponse();
        }

        /// <summary>
        /// 删除导出记录
        /// </summary>
        /// <param name="id">记录ID</param>
        [HttpPost("/Export/ExportRecord/Delete")]
        [Authorize]
        public async Task<UPrimeResponse> DeleteExportRecord([FromBody] DeleteInput input)
        {
            await _exportRecordService.DeleteExportRecord(input.Id);
            return new UPrimeResponse();
        }

    }
}
