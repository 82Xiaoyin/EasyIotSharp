using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Dto.Tenant.Params;
using EasyIotSharp.Core.Dto.Tenant;
using EasyIotSharp.Core.Repositories.Tenant;
using System.Threading.Tasks;
using EasyIotSharp.Core.Services.Queue;
using EasyIotSharp.Core.Services.TenantAccount;
using UPrime.Services.Dto;
using EasyIotSharp.Core.Repositories.Rule;
using EasyIotSharp.Core.Dto;
using EasyIotSharp.Core.Dto.Rule;
using UPrime.AutoMapper;
using EasyIotSharp.Core.Dto.TenantAccount.Params;
using EasyIotSharp.Core.Repositories.Tenant.Impl;
using EasyIotSharp.Core.Services.TenantAccount.Impl;
using EasyIotSharp.Core.Dto.Rule.Params;
using EasyIotSharp.Core.Domain.Rule;
using SqlSugar;
using EasyIotSharp.Core.Caching.Rule.Impl;
using EasyIotSharp.Core.Caching.Rule;
using EasyIotSharp.Core.Events.Tenant;
using EasyIotSharp.Core.Events.Rule;

namespace EasyIotSharp.Core.Services.Rule.Impl
{
    public class AlarmsConfigService : ServiceBase, IAlarmsConfigService
    {
        private readonly IAlarmsConfigRepository _alarmsConfigRepository;
        private readonly IAlarmsConfigCacheService _alarmsConfigCacheService;

        public AlarmsConfigService(IAlarmsConfigRepository alarmsConfigRepository,
                                   IAlarmsConfigCacheService alarmsConfigCacheService)
        {
            _alarmsConfigRepository = alarmsConfigRepository;
            _alarmsConfigCacheService = alarmsConfigCacheService;
        }

        /// <summary>
        /// 列表
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<PagedResultDto<AlarmsConfigDto>> QueryAlarmsConfig(PagingInput input)
        {
            if (input.IsPage.Equals(true)
               && input.PageIndex <= 5 && input.PageSize == 10)
            {
                return await _alarmsConfigCacheService.QueryAlarmsConfig(input, async () =>
                {
                    var query = await _alarmsConfigRepository.Query(input.PageIndex, input.PageSize);
                    int totalCount = query.totalCount;
                    var list = query.items.MapTo<List<AlarmsConfigDto>>();
                    return new PagedResultDto<AlarmsConfigDto>() { TotalCount = totalCount, Items = list };
                });
            }
            else
            {
                var query = await _alarmsConfigRepository.Query(input.PageIndex, input.PageSize);
                int totalCount = query.totalCount;
                var list = query.items.MapTo<List<AlarmsConfigDto>>();
                return new PagedResultDto<AlarmsConfigDto>() { TotalCount = totalCount, Items = list };
            }
        }

        /// <summary>
        /// 新增报警
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="BizException"></exception>
        public async Task InsertAlarmsConfig(InsertAlarmsConfig input)
        {
            var info = await _alarmsConfigRepository.FirstOrDefaultAsync(x => x.AlarmsName == input.AlarmsName && x.IsDelete == false);
            if (info.IsNotNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "报警名称重复");
            }

            var model = new AlarmsConfig();
            model.Id = Guid.NewGuid().ToString().Replace("-", "");
            model.AlarmsName = input.AlarmsName;
            model.Remark = input.Remark;
            model.Level = input.Level;
            model.State = true;
            model.NotifyId = input.NotifyId;
            model.IsDelete = false;
            model.CreationTime = DateTime.Now;
            model.UpdatedAt = DateTime.Now;
            model.OperatorId = ContextUser.UserId;
            model.OperatorName = ContextUser.UserName;
            await _alarmsConfigRepository.InsertAsync(model);

            //清除缓存
            await EventBus.TriggerAsync(new AlarmsConfigEventData() { });
        }

        /// <summary>
        /// 修改报警
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="BizException"></exception>
        public async Task UpdateAlarmsConfig(InsertAlarmsConfig input)
        {
            var info = await _alarmsConfigRepository.GetByIdAsync(input.Id);
            if (info.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "未找到指定数据");
            }
            var isExist = await _alarmsConfigRepository.FirstOrDefaultAsync(x => x.AlarmsName == input.AlarmsName && x.Id != input.Id && x.IsDelete == false);
            if (isExist.IsNotNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "租户名称重复");
            }
            info.AlarmsName = input.AlarmsName;
            info.Remark = input.Remark;
            info.Level = input.Level;
            info.State = input.State;
            info.NotifyId = input.NotifyId;
            info.UpdatedAt = DateTime.Now;
            info.OperatorId = ContextUser.UserId;
            info.OperatorName = ContextUser.UserName;
            await _alarmsConfigRepository.UpdateAsync(info);

            //清除缓存
            await EventBus.TriggerAsync(new AlarmsConfigEventData() { });
        }

        /// <summary>
        /// 删除报警
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="BizException"></exception>
        public async Task DeleteAlarmsConfig(DeleteInput input)
        {
            var info = await _alarmsConfigRepository.GetByIdAsync(input.Id);
            if (info.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "未找到指定数据");
            }

            info.IsDelete = true;
            info.UpdatedAt = DateTime.Now;
            info.OperatorId = ContextUser.UserId;
            info.OperatorName = ContextUser.UserName;
            await _alarmsConfigRepository.UpdateAsync(info);

            //清除缓存
            await EventBus.TriggerAsync(new AlarmsConfigEventData() { });
        }

        /// <summary>
        /// 修改状态
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="BizException"></exception>
        public async Task UpdateAlarmsConfigState(InsertAlarmsConfig input)
        {
            var info = await _alarmsConfigRepository.GetByIdAsync(input.Id);
            if (info.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "未找到指定数据");
            }

            info.State = input.State;
            info.UpdatedAt = DateTime.Now;
            info.OperatorId = ContextUser.UserId;
            info.OperatorName = ContextUser.UserName;
            await _alarmsConfigRepository.UpdateAsync(info);

            //清除缓存
            await EventBus.TriggerAsync(new AlarmsConfigEventData() { });
        }
    }
}
