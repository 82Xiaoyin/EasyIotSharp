using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyIotSharp.Core.Caching.Hardware;
using EasyIotSharp.Core.Domain.Hardware;
using EasyIotSharp.Core.Dto.Hardware;
using EasyIotSharp.Core.Dto.Hardware.Params;
using EasyIotSharp.Core.Events.Hardware;
using EasyIotSharp.Core.Repositories.Hardware;
using EasyIotSharp.Core.Repositories.Influxdb;
using SqlSugar;
using UPrime.AutoMapper;
using UPrime.Services.Dto;

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

        /// <summary>
        /// 传感器指标列表
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 新增传感器指标
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="BizException"></exception>
        public async Task InsertSensorQuota(InsertSensorQuotaInput input)
        {
            var isExistName = await _sensorQuotaRepository.FirstOrDefaultAsync(x => x.TenantNumId == ContextUser.TenantNumId && x.Name == input.Name && x.SensorId == input.SensorId && x.IsDelete == false);
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
            model.IsShow = true;
            model.IsDelete = false;
            model.CreationTime = DateTime.Now;
            model.UpdatedAt = DateTime.Now;
            model.OperatorId = ContextUser.UserId;
            model.OperatorName = ContextUser.UserName;
            await _sensorQuotaRepository.InsertAsync(model);

            //清除缓存
            await EventBus.TriggerAsync(new SensorQuotaEventData() { });
            await EventBus.TriggerAsync(new SensorQuotaBaseEventData() { });
        }

        /// <summary>
        /// 修改传感器指标
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="BizException"></exception>
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
            info.IsShow = input.IsShow;
            info.UpdatedAt = DateTime.Now;
            info.OperatorId = ContextUser.UserId;
            info.OperatorName = ContextUser.UserName;
            await _sensorQuotaRepository.UpdateAsync(info);

            //清除缓存
            await EventBus.TriggerAsync(new SensorQuotaEventData() { });
            await EventBus.TriggerAsync(new SensorQuotaBaseEventData() { });
        }

        /// <summary>
        /// 删除传感器指标
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="BizException"></exception>
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
            await EventBus.TriggerAsync(new SensorQuotaBaseEventData() { });
        }

        /// <summary>
        /// 传感器指标列表
        /// </summary>
        /// <returns></returns>
        public List<SensorQuota> GetSensorQuotaList()
        {
            var list = _sensorQuotaCacheService.GetSensorQuotaList(() => { return _sensorQuotaRepository.GetSensorQuotaList(); });
            if (list.Count == 0)
            {
                list = _sensorQuotaRepository.GetSensorQuotaList();
            }
            return list;
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="dataRespost">数据请求参数</param>
        /// <returns>响应结果</returns>
        public async Task<Response> GetResponse(DataRespost dataRespost)
        {
            var result = new Response();

            // 参数验证
            if (dataRespost?.SensorId == null || !dataRespost.SensorId.Any())
            {
                return result;
            }

            // 查询传感器信息
            var sensorList = await _sensorRepository.QueryByIds(dataRespost.SensorId.ToList());
            if (sensorList == null || !sensorList.Any())
            {
                return result;
            }

            var sensor = sensorList.First();
            var measurementName = $"raw_{sensor.BriefName}";

            try
            {
                // 添加时间指标
                result.Quotas.Add(new Quotas
                {
                    Name = "time",
                    Unit = null,
                    IsShow = true
                });

                // 获取传感器指标列表
                var sensorQuotaList = await _sensorQuotaRepository.GetSensorQuotaList(sensor.Id);
                if (sensorQuotaList == null || !sensorQuotaList.Any())
                {
                    return result;
                }

                // 构建SQL查询
                var sqlBuilder = new StringBuilder("select ");

                // 添加指标信息到结果并构建查询字段
                foreach (var item in sensorQuotaList)
                {
                    result.Quotas.Add(new Quotas
                    {
                        Name = item.Identifier,
                        Unit = item.Unit,
                        IsShow = true
                    });

                    sqlBuilder.Append(item.Identifier).Append(',');
                }

                // 移除最后一个逗号
                sqlBuilder.Length -= 1;

                // 添加表名和条件
                sqlBuilder.Append(" from ").Append(measurementName).Append(" where 1=1");

                // 添加时间范围条件（如果有）
                if (dataRespost.StartTime.HasValue && dataRespost.EndTime.HasValue)
                {
                    sqlBuilder.Append($" and time >= '{dataRespost.StartTime.Value:yyyy-MM-ddTHH:mm:ssZ}' and time <= '{dataRespost.EndTime.Value:yyyy-MM-ddTHH:mm:ssZ}'");
                }

                if (dataRespost.IsPage == true && dataRespost.PageIndex > 0 && dataRespost.PageSize > 0)
                {
                    sqlBuilder.Append($" Limit {dataRespost.PageSize}  Offset {dataRespost.PageIndex - 1}");
                }

                // 创建仓储实例
                var repository = InfluxdbRepositoryFactory.Create<Dictionary<string, object>>(
                    measurementName: measurementName,
                    tenantDatabase: ContextUser.TenantAbbreviation
                );

                // 查询数据
                var data = await repository.GetAsync(sqlBuilder.ToString());
                result.Data = data.Values;
            }
            catch (Exception ex)
            {
                // 记录异常信息
                Logger.Error($"获取传感器数据时发生异常: {ex.Message}", ex);
            }

            return result;
        }
    }
}
