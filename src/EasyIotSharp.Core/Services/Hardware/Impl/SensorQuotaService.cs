using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Dto.Hardware.Params;
using EasyIotSharp.Core.Dto.Hardware;
using System.Threading.Tasks;
using EasyIotSharp.Core.Repositories.Hardware;
using UPrime.Services.Dto;
using EasyIotSharp.Core.Repositories.Hardware.Impl;
using UPrime.AutoMapper;
using EasyIotSharp.Core.Domain.Hardware;
using System.Linq;
using EasyIotSharp.Core.Caching.Hardware;
using EasyIotSharp.Core.Events.Tenant;
using EasyIotSharp.Core.Events.Hardware;

namespace EasyIotSharp.Core.Services.Hardware.Impl
{
    public class SensorQuotaService : ServiceBase, ISensorQuotaService
    {
        private readonly ISensorQuotaRepository _sensorQuotaRepository;
        private readonly ISensorRepository _sensorRepository;
        private readonly ISensorQuotaCacheService _sensorQuotaCacheService;

        public SensorQuotaService(ISensorQuotaRepository sensorQuotaRepository,
                                  ISensorRepository sensorRepository,
                                  ISensorQuotaCacheService sensorQuotaCacheService)
        {
            _sensorQuotaRepository = sensorQuotaRepository;
            _sensorRepository = sensorRepository;
            _sensorQuotaCacheService = sensorQuotaCacheService;
        }

        public async Task<SensorQuotaDto> GetSensorQuota(string id)
        {
            var info = await _sensorQuotaRepository.FirstOrDefaultAsync(x => x.Id == id && x.IsDelete == false);
            var output = info.MapTo<SensorQuotaDto>();
            if (output.IsNotNull())
            {
                var sensor = await _sensorRepository.GetByIdAsync(output.SensorId);
                if (sensor.IsNotNull())
                {
                    output.SensorName = sensor.Name;
                }
            }
            return output;
        }

        public async Task<PagedResultDto<SensorQuotaDto>> QuerySensorQuota(QuerySensorQuotaInput input)
        {
            if (string.IsNullOrEmpty(input.Keyword) 
                && string.IsNullOrEmpty(input.SensorId)
                && input.DataType.Equals(DataTypeMenu.None)
                && input.IsPage.Equals(true)
                && input.PageIndex <= 5 && input.PageSize == 10)
            {
                return await _sensorQuotaCacheService.QuerySensorQuota(input, async () =>
                {
                    var query = await _sensorQuotaRepository.Query(ContextUser.TenantNumId, input.SensorId, input.Keyword, input.DataType, input.PageIndex, input.PageSize, input.IsPage);
                    int totalCount = query.totalCount;
                    var list = query.items.MapTo<List<SensorQuotaDto>>();
                    var sensorIds = list.Select(x => x.SensorId).ToList();
                    if (sensorIds.Count > 0)
                    {
                        var sensors = await _sensorRepository.QueryByIds(sensorIds);
                        foreach (var item in list)
                        {
                            var sensor = sensors.FirstOrDefault(x => x.Id == item.SensorId);
                            if (sensor.IsNotNull())
                            {
                                item.SensorName = sensor.Name;
                            }
                        }
                    }
                    return new PagedResultDto<SensorQuotaDto>() { TotalCount = totalCount, Items = list };
                });
            }
            else
            {
                var query = await _sensorQuotaRepository.Query(ContextUser.TenantNumId, input.SensorId, input.Keyword, input.DataType, input.PageIndex, input.PageSize, input.IsPage);
                int totalCount = query.totalCount;
                var list = query.items.MapTo<List<SensorQuotaDto>>();
                var sensorIds = list.Select(x => x.SensorId).ToList();
                if (sensorIds.Count > 0)
                {
                    var sensors = await _sensorRepository.QueryByIds(sensorIds);
                    foreach (var item in list)
                    {
                        var sensor = sensors.FirstOrDefault(x => x.Id == item.SensorId);
                        if (sensor.IsNotNull())
                        {
                            item.SensorName = sensor.Name;
                        }
                    }
                }
                return new PagedResultDto<SensorQuotaDto>() { TotalCount = totalCount, Items = list };
            }
        }

        public async Task InsertSensorQuota(InsertSensorQuotaInput input)
        {
            var isExistName = await _sensorQuotaRepository.FirstOrDefaultAsync(x => x.TenantNumId == ContextUser.TenantNumId && x.Name == input.Name && x.SensorId==input.SensorId && x.IsDelete == false);
            if (isExistName.IsNotNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "传感器类型指标名称重复");
            }
            var isExistIdentifier = await _sensorQuotaRepository.FirstOrDefaultAsync(x => x.TenantNumId == ContextUser.TenantNumId && x.Identifier == input.Identifier && x.SensorId == input.SensorId && x.IsDelete == false);
            if (isExistIdentifier.IsNotNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "传感器类型指标标识符重复");
            }
            var model = new SensorQuota();
            model.Id = Guid.NewGuid().ToString().Replace("-", "");
            model.TenantNumId = ContextUser.TenantNumId;
            model.Name = input.Name;
            model.Identifier = input.Identifier;
            model.ControlsType = input.ControlsType;
            model.SensorId = input.SensorId;
            model.DataType = input.DataType;
            model.Unit = input.Unit;
            model.Precision = input.Precision;
            model.Remark = input.Remark;
            model.Sort = input.Sort;
            model.IsDelete = false;
            model.CreationTime = DateTime.Now;
            model.UpdatedAt = DateTime.Now;
            model.OperatorId = ContextUser.UserId;
            model.OperatorName = ContextUser.UserName;
            await _sensorQuotaRepository.InsertAsync(model);

            //清除缓存
            await EventBus.TriggerAsync(new SensorQuotaEventData() { });
        }

        public async Task UpdateSensorQuota(UpdateSensorQuotaInput input)
        {
            var info = await _sensorQuotaRepository.FirstOrDefaultAsync(x => x.TenantNumId == ContextUser.TenantNumId && x.Id == input.Id && x.IsDelete == false);
            if (info.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "传感器类型指标不存在");
            }
            var isExistName = await _sensorQuotaRepository.FirstOrDefaultAsync(x => x.TenantNumId == ContextUser.TenantNumId && x.Name == input.Name && x.IsDelete == false);
            if (isExistName.IsNotNull() && isExistName.Id != input.Id)
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "传感器类型指标名称重复");
            }
            var isExistIdentifier = await _sensorQuotaRepository.FirstOrDefaultAsync(x => x.TenantNumId == ContextUser.TenantNumId && x.Identifier == input.Identifier && x.IsDelete == false);
            if (isExistIdentifier.IsNotNull() && isExistName.Id != input.Id)
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "传感器类型指标标识符重复");
            }
            info.Name = input.Name;
            info.Identifier = input.Identifier;
            info.ControlsType = input.ControlsType;
            info.DataType = input.DataType;
            info.Unit = input.Unit;
            info.Precision = input.Precision;
            info.Remark = input.Remark;
            info.Sort = input.Sort;
            info.UpdatedAt = DateTime.Now;
            info.OperatorId = ContextUser.UserId;
            info.OperatorName = ContextUser.UserName;
            await _sensorQuotaRepository.UpdateAsync(info);

            //清除缓存
            await EventBus.TriggerAsync(new SensorQuotaEventData() { });
        }

        public async Task DeleteSensorQuota(DeleteSensorQuotaInput input)
        {
            var info = await _sensorQuotaRepository.GetByIdAsync(input.Id);
            if (info.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "传感器类型指标不存在");
            }
            if (info.IsDelete != input.IsDelete)
            {
                info.IsDelete = input.IsDelete;
                info.UpdatedAt = DateTime.Now;
                info.OperatorId = ContextUser.UserId;
                info.OperatorName = ContextUser.UserName;
                await _sensorQuotaRepository.UpdateAsync(info);
            }

            //清除缓存
            await EventBus.TriggerAsync(new SensorQuotaEventData() { });
        }
    }
}
