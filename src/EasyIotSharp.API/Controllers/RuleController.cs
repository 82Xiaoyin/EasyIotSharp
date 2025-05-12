using EasyIotSharp.Core.Dto.Project.Params;
using EasyIotSharp.Core.Dto.Project;
using System.Threading.Tasks;
using EasyIotSharp.Core.Services.Project;
using EasyIotSharp.Core.Services.Project.Impl;
using EasyIotSharp.Core.Services.Rule;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UPrime.Services.Dto;
using UPrime.WebApi;
using EasyIotSharp.API.Filters;
using EasyIotSharp.Core.Dto;
using EasyIotSharp.Core.Dto.Rule;
using EasyIotSharp.Core.Dto.Rule.Params;
using EasyIotSharp.Core.Dto.Queue.Params;
using EasyIotSharp.Core.Services.Rule.Impl;
using System.Collections.Generic;

namespace EasyIotSharp.API.Controllers
{
    public class RuleController : ApiControllerBase
    {
        private readonly IAlarmsConfigService _alarmsConfigService;
        private readonly IAlarmsService _alarmsService;
        private readonly INotifyService _notifyService;
        public RuleController()
        {
            _alarmsConfigService = UPrime.UPrimeEngine.Instance.Resolve<IAlarmsConfigService>();
            _alarmsService = UPrime.UPrimeEngine.Instance.Resolve<IAlarmsService>();
            _notifyService = UPrime.UPrimeEngine.Instance.Resolve<INotifyService>();
        }

        #region 报警

        /// <summary>
        /// 通过条件分页查询报警配置
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Rule/AlarmsConfig/Query")]
        [Authorize]
        public async Task<UPrimeResponse<PagedResultDto<AlarmsConfigDto>>> QueryProjectBase([FromBody] PagingInput input)
        {
            UPrimeResponse<PagedResultDto<AlarmsConfigDto>> res = new UPrimeResponse<PagedResultDto<AlarmsConfigDto>>();
            res.Result = await _alarmsConfigService.QueryAlarmsConfig(input);
            return res;
        }

        /// <summary>
        /// 新增报警信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Rule/AlarmsConfig/Insert")]
        [Authorize]
        public async Task<UPrimeResponse> InsertAlarmsConfig([FromBody] InsertAlarmsConfig input)
        {
            await _alarmsConfigService.InsertAlarmsConfig(input);
            return new UPrimeResponse();
        }

        /// <summary>
        /// 修改报警信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Rule/AlarmsConfig/Update")]
        [Authorize]
        public async Task<UPrimeResponse> UpdateAlarmsConfig([FromBody] InsertAlarmsConfig input)
        {
            await _alarmsConfigService.UpdateAlarmsConfig(input);
            return new UPrimeResponse();
        }

        /// <summary>
        /// 删除报警信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Rule/AlarmsConfig/Delete")]
        [Authorize]
        public async Task<UPrimeResponse> DeleteAlarmsConfig([FromBody] DeleteInput input)
        {
            await _alarmsConfigService.DeleteAlarmsConfig(input);
            return new UPrimeResponse();
        }

        /// <summary>
        /// 修改报警状态
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Rule/AlarmsConfig/UpdateState")]
        [Authorize]
        public async Task<UPrimeResponse> UpdateAlarmsConfigState([FromBody] InsertAlarmsConfig input)
        {
            await _alarmsConfigService.UpdateAlarmsConfigState(input);
            return new UPrimeResponse();
        }

        #endregion


        #region 通知组

        /// <summary>
        /// 通过条件分页查询通知组
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Rule/Notify/Query")]
        [Authorize]
        public async Task<UPrimeResponse<PagedResultDto<NotifyDto>>> QueryNotifyConfig([FromBody] PagingInput input)
        {
            UPrimeResponse<PagedResultDto<NotifyDto>> res = new UPrimeResponse<PagedResultDto<NotifyDto>>();
            res.Result = await _notifyService.QueryNotifyConfig(input);
            return res;
        }

        /// <summary>
        /// 新增通知组
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Rule/Notify/Insert")]
        [Authorize]
        public async Task<UPrimeResponse> InsertNotifyConfig([FromBody] InsertNotify input)
        {
            await _notifyService.InsertNotifyConfig(input);
            return new UPrimeResponse();
        }

        /// <summary>
        /// 修改通知组
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Rule/Notify/Update")]
        [Authorize]
        public async Task<UPrimeResponse> UpdateNotifyConfig([FromBody] InsertNotify input)
        {
            await _notifyService.UpdateNotifyConfig(input);
            return new UPrimeResponse();
        }

        /// <summary>
        /// 删除通知组
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Rule/Notify/Delete")]
        [Authorize]
        public async Task<UPrimeResponse> DeleteNotifyConfig([FromBody] DeleteInput input)
        {
            await _notifyService.DeleteNotifyConfig(input);
            return new UPrimeResponse();
        }

        /// <summary>
        /// 修改通知组
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Rule/Notify/UpdateState")]
        [Authorize]
        public async Task<UPrimeResponse> UpdateNotifyConfigState([FromBody] InsertNotify input)
        {
            await _notifyService.UpdateNotifyConfigState(input);
            return new UPrimeResponse();
        }
        #endregion

        #region 通知记录

        /// <summary>
        /// 获取通知记录
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Rule/NotifyRecord/Query")]
        [Authorize]
        public async Task<UPrimeResponse<PagedResultDto<NotifyRecordDto>>> QueryProjectBase([FromBody] NotifyRecordInput input)
        {
            UPrimeResponse<PagedResultDto<NotifyRecordDto>> res = new UPrimeResponse<PagedResultDto<NotifyRecordDto>>();
            res.Result = await _notifyService.QueryNotifyRecord(input);
            return res;
        }

        #endregion


        #region 报警记录
        
        /// <summary>
        /// 获取报警记录
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Rule/Alarms/Query")]
        //[Authorize]
        public async Task<UPrimeResponse<List<AlarmsDto>>> QueryProjectBase([FromBody] AlarmsInput input)
        {
            UPrimeResponse<List<AlarmsDto>> res = new UPrimeResponse<List<AlarmsDto>>();
            res.Result = await _alarmsService.GetAlarmsData(input);
            return res;
        }

        /// <summary>
        /// 修改报警内容状态
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("/Rule/Alarms/UpdateState")]
        [Authorize]
        public async Task<UPrimeResponse> UpdateAlarms([FromBody] AlarmsDto input)
        {
            await _alarmsService.UpdateAlarms(input);
            return new UPrimeResponse();
        }

        #endregion
    }
}
