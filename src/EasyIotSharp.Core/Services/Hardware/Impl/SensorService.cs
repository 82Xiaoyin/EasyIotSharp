using EasyIotSharp.Core.Caching.Hardware;
using EasyIotSharp.Core.Caching.Hardware.Impl;
using EasyIotSharp.Core.Domain.Hardware;
using EasyIotSharp.Core.Dto.Hardware;
using EasyIotSharp.Core.Dto.Hardware.Params;
using EasyIotSharp.Core.Events.Hardware;
using EasyIotSharp.Core.Events.Tenant;
using EasyIotSharp.Core.Repositories.Hardware;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UPrime.AutoMapper;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Services.Hardware.Impl
{
    public class SensorService : ServiceBase, ISensorService
    {
        private readonly ISensorRepository _sensorRepository;
        private readonly ISensorCacheService _sensorCacheService;

        public SensorService(ISensorRepository sensorRepository,
            ISensorCacheService sensorCacheService)
        {
            _sensorRepository = sensorRepository;
            _sensorCacheService = sensorCacheService;
        }

        public async Task<SensorDto> GetSensor(string id)
        {
            var info = await _sensorRepository.FirstOrDefaultAsync(x => x.Id == id && x.IsDelete == false);
            return info.MapTo<SensorDto>();
        }

        public async Task<PagedResultDto<SensorDto>> QuerySensor(QuerySensorInput input)
        {
            if (string.IsNullOrEmpty(input.Keyword)
                                    && input.IsPage.Equals(true)
                                    && input.PageIndex <= 5 && input.PageSize == 10)
            {
                return await _sensorCacheService.QuerySensor(input, async () =>
                {
                    var query = await _sensorRepository.Query(ContextUser.TenantNumId, input.Keyword, input.PageIndex, input.PageSize, input.IsPage);
                    int totalCount = query.totalCount;
                    var list = query.items.MapTo<List<SensorDto>>();

                    return new PagedResultDto<SensorDto>() { TotalCount = totalCount, Items = list };
                });
            }
            else
            {
                var query = await _sensorRepository.Query(ContextUser.TenantNumId, input.Keyword, input.PageIndex, input.PageSize, input.IsPage);
                int totalCount = query.totalCount;
                var list = query.items.MapTo<List<SensorDto>>();

                return new PagedResultDto<SensorDto>() { TotalCount = totalCount, Items = list };
            }
        }

        public async Task InsertSensor(InsertSensorInput input)
        {
            var isExistBriefName = await _sensorRepository.FirstOrDefaultAsync(x => x.TenantNumId == ContextUser.TenantNumId && x.BriefName == input.BriefName && x.IsDelete == false);
            if (isExistBriefName.IsNotNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "传感器简称重复");
            }
            var model = new Sensor();
            model.Id = Guid.NewGuid().ToString().Replace("-", "");
            model.TenantNumId = ContextUser.TenantNumId;
            model.Name = input.Name;
            model.BriefName = input.BriefName;
            model.Supplier = input.Supplier;
            model.SensorModel = input.SensorModel;
            model.Remark = input.Remark;
            model.IsDelete = false;
            model.CreationTime = DateTime.Now;
            model.UpdatedAt = DateTime.Now;
            model.OperatorId = ContextUser.UserId;
            model.OperatorName = ContextUser.UserName;
            await _sensorRepository.InsertAsync(model);

            //清除缓存
            await EventBus.TriggerAsync(new SensorEventData() { });
            await EventBus.TriggerAsync(new SensorBaseEventData() { });
        }

        public async Task UpdateSensor(UpdateSensorInput input)
        {
            var info = await _sensorRepository.FirstOrDefaultAsync(x => x.TenantNumId == ContextUser.TenantNumId && x.Id == input.Id && x.IsDelete == false);
            if (info.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "传感器不存在");
            }
            var isExistBriefName = await _sensorRepository.FirstOrDefaultAsync(x => x.TenantNumId == ContextUser.TenantNumId && x.BriefName == input.BriefName && x.IsDelete == false);
            if (isExistBriefName.IsNotNull() && isExistBriefName.Id != input.Id)
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "传感器名称重复");
            }
            info.Name = input.Name;
            info.BriefName = input.BriefName;
            info.Supplier = input.Supplier;
            info.SensorModel = input.SensorModel;
            info.Remark = input.Remark;
            info.UpdatedAt = DateTime.Now;
            info.OperatorId = ContextUser.UserId;
            info.OperatorName = ContextUser.UserName;
            await _sensorRepository.UpdateAsync(info);

            //清除缓存
            await EventBus.TriggerAsync(new SensorEventData() { });
            await EventBus.TriggerAsync(new SensorBaseEventData() { });
        }

        public async Task DeleteSensor(DeleteSensorInput input)
        {
            var info = await _sensorRepository.GetByIdAsync(input.Id);
            if (info.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "传感器类型不存在");
            }
            if (info.IsDelete != input.IsDelete)
            {
                info.IsDelete = input.IsDelete;
                info.UpdatedAt = DateTime.Now;
                info.OperatorId = ContextUser.UserId;
                info.OperatorName = ContextUser.UserName;
                await _sensorRepository.UpdateAsync(info);
            }

            //清除缓存
            await EventBus.TriggerAsync(new SensorEventData() { });
            await EventBus.TriggerAsync(new SensorBaseEventData() { });
        }

        /// <summary>
        /// 列表
        /// </summary>
        /// <returns></returns>
        public List<Sensor> GetSensorList()
        {
            var list = _sensorCacheService.GetSensorList(() => { return _sensorRepository.GetSensorList(); });
            if (list.Count == 0)
            {
                list = _sensorRepository.GetSensorList();
            }
            return list;
        }
    }
}
