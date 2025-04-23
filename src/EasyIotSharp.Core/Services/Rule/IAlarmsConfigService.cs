using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Dto.Rule;
using EasyIotSharp.Core.Dto;
using System.Threading.Tasks;
using UPrime.Services.Dto;
using EasyIotSharp.Core.Dto.Rule.Params;
using EasyIotSharp.Core.Dto.Tenant.Params;

namespace EasyIotSharp.Core.Services.Rule
{
    public interface IAlarmsConfigService
    {
        /// <summary>
        /// 查询列表
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<PagedResultDto<AlarmsConfigDto>> QueryAlarmsConfig(PagingInput input);

        /// <summary>
        /// 新增报警
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task InsertAlarmsConfig(InsertAlarmsConfig input);

        /// <summary>
        /// 修改报警
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task UpdateAlarmsConfig(InsertAlarmsConfig input);

        /// <summary>
        /// 删除报警
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task DeleteAlarmsConfig(DeleteInput input);

        /// <summary>
        /// 修改状态
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task UpdateAlarmsConfigState(InsertAlarmsConfig input);
    }
}
