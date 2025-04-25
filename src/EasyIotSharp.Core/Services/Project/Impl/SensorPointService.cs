using EasyIotSharp.Core.Caching.Project;
using EasyIotSharp.Core.Domain.Proejct;
using EasyIotSharp.Core.Dto.Project;
using EasyIotSharp.Core.Dto.Project.Params;
using EasyIotSharp.Core.Events.Project;
using EasyIotSharp.Core.Events.Tenant;
using EasyIotSharp.Core.Repositories.Hardware;
using EasyIotSharp.Core.Repositories.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UPrime.AutoMapper;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Services.Project.Impl
{
    public class SensorPointService : ServiceBase, ISensorPointService
    {
        private readonly IProjectBaseRepository _projectBaseRepository;
        private readonly IClassificationRepository _classificationRepository;
        private readonly IGatewayRepository _gatewayRepository;
        private readonly ISensorPointRepository _sensorPointRepository;
        private readonly ISensorRepository _sensorRepository;
        private readonly ISensorPointCacheService _sensorPointCacheService;
        private readonly ISensorPointBaseCacheService _sensorPointBaseCacheService;

        public SensorPointService(ISensorPointRepository sensorPointRepository,
                                  IProjectBaseRepository projectBaseRepository,
                                  IClassificationRepository classificationRepository,
                                  IGatewayRepository gatewayRepository,
                                  ISensorRepository sensorRepository,
                                  ISensorPointCacheService sensorPointCacheService,
                                  ISensorPointBaseCacheService sensorPointBaseCacheService)
        {
            _projectBaseRepository = projectBaseRepository;
            _classificationRepository = classificationRepository;
            _gatewayRepository = gatewayRepository;
            _sensorRepository = sensorRepository;
            _sensorPointRepository = sensorPointRepository;
            _sensorPointCacheService = sensorPointCacheService;
            _sensorPointBaseCacheService = sensorPointBaseCacheService;
        }

        public async Task<SensorPointDto> GetSensorPoint(string id)
        {
            var info = await _sensorPointRepository.FirstOrDefaultAsync(x => x.Id == id && x.IsDelete == false);
            var output = info.MapTo<SensorPointDto>();
            if (output.IsNotNull())
            {
                var project = await _projectBaseRepository.GetByIdAsync(output.ProjectId);
                if (project.IsNotNull())
                {
                    output.ProjectName = project.Name;
                }
                var classification = await _classificationRepository.GetByIdAsync(output.ClassificationId);
                if (classification.IsNotNull())
                {
                    output.ClassificationName = classification.Name;
                }
                var gateway = await _gatewayRepository.GetByIdAsync(output.GatewayId);
                if (gateway.IsNotNull())
                {
                    output.GatewayName = gateway.Name;
                }
                var sensor = await _sensorRepository.GetByIdAsync(output.SensorId);
                if (sensor.IsNotNull())
                {
                    output.SensorName = sensor.Name;
                }
            }
            return output;
        }

        public async Task<PagedResultDto<SensorPointDto>> QuerySensorPoint(QuerySensorPointInput input)
        {
            if (string.IsNullOrEmpty(input.Keyword)
               && input.State.Equals(-1)
               && string.IsNullOrEmpty(input.ProjectId)
               && string.IsNullOrEmpty(input.ClassificationId)
               && string.IsNullOrEmpty(input.GatewayId)
               && string.IsNullOrEmpty(input.SensorId)
               && input.IsPage.Equals(true)
               && input.PageIndex <= 5 && input.PageSize == 10)
            {
                return await _sensorPointCacheService.QuerySensorPoint(input, async () =>
                {
                    var query = await _sensorPointRepository.Query(ContextUser.TenantNumId, input.Keyword, input.ProjectId, input.ClassificationId, input.GatewayId, input.SensorId, input.State, input.PageIndex, input.PageSize, input.IsPage);
                    int totalCount = query.totalCount;
                    var list = query.items.MapTo<List<SensorPointDto>>();
                    var projects = await _projectBaseRepository.QueryByIds(list.Select(x => x.ProjectId).ToList());
                    var classifications = await _classificationRepository.QueryByIds(list.Select(x => x.ClassificationId).ToList());
                    var gateways = await _gatewayRepository.QueryByIds(list.Select(x => x.GatewayId).ToList());
                    var sensors = await _sensorRepository.QueryByIds(list.Select(x => x.SensorId).ToList());
                    foreach (var item in list)
                    {
                        var project = projects.FirstOrDefault(x => x.Id == item.ProjectId);
                        if (project.IsNotNull())
                        {
                            item.ProjectName = project.Name;
                        }
                        var classification = classifications.FirstOrDefault(x => x.Id == item.ClassificationId);
                        if (classification.IsNotNull())
                        {
                            item.ClassificationName = classification.Name;
                        }
                        var gateway = gateways.FirstOrDefault(x => x.Id == item.GatewayId);
                        if (gateway.IsNotNull())
                        {
                            item.GatewayName = gateway.Name;
                        }
                        var sensor = sensors.FirstOrDefault(x => x.Id == item.SensorId);
                        if (sensor.IsNotNull())
                        {
                            item.SensorName = sensor.Name;
                        }
                    }
                    return new PagedResultDto<SensorPointDto>() { TotalCount = totalCount, Items = list };
                });
            }
            else
            {
                var query = await _sensorPointRepository.Query(ContextUser.TenantNumId, input.Keyword, input.ProjectId, input.ClassificationId, input.GatewayId, input.SensorId, input.State, input.PageIndex, input.PageSize, input.IsPage);
                int totalCount = query.totalCount;
                var list = query.items.MapTo<List<SensorPointDto>>();
                var projects = await _projectBaseRepository.QueryByIds(list.Select(x => x.ProjectId).ToList());
                var classifications = await _classificationRepository.QueryByIds(list.Select(x => x.ClassificationId).ToList());
                var gateways = await _gatewayRepository.QueryByIds(list.Select(x => x.GatewayId).ToList());
                var sensors = await _sensorRepository.QueryByIds(list.Select(x => x.SensorId).ToList());
                foreach (var item in list)
                {
                    var project = projects.FirstOrDefault(x => x.Id == item.ProjectId);
                    if (project.IsNotNull())
                    {
                        item.ProjectName = project.Name;
                    }
                    var classification = classifications.FirstOrDefault(x => x.Id == item.ClassificationId);
                    if (classification.IsNotNull())
                    {
                        item.ClassificationName = classification.Name;
                    }
                    var gateway = gateways.FirstOrDefault(x => x.Id == item.GatewayId);
                    if (gateway.IsNotNull())
                    {
                        item.GatewayName = gateway.Name;
                    }
                    var sensor = sensors.FirstOrDefault(x => x.Id == item.SensorId);
                    if (sensor.IsNotNull())
                    {
                        item.SensorName = sensor.Name;
                    }
                }
                return new PagedResultDto<SensorPointDto>() { TotalCount = totalCount, Items = list };
            }
        }

        public async Task InsertSensorPoint(InsertSensorPointInput input)
        {
            var isExistName = await _sensorPointRepository.FirstOrDefaultAsync(x => x.Name == input.Name && x.TenantNumId == ContextUser.TenantNumId && x.ProjectId == input.ProjectId && x.IsDelete == false);
            if (isExistName.IsNotNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "测点名称重复");
            }
            var model = new SensorPoint();
            model.Id = Guid.NewGuid().ToString().Replace("-", "");
            model.TenantNumId = ContextUser.TenantNumId;
            model.Name = input.Name;
            model.ProjectId = input.ProjectId;
            model.ClassificationId = input.ClassificationId;
            model.GatewayId = "";
            model.SensorId = input.SensorId;
            model.IsDelete = false;
            model.CreationTime = DateTime.Now;
            model.UpdatedAt = DateTime.Now;
            model.OperatorId = ContextUser.UserId;
            model.OperatorName = ContextUser.UserName;
            await _sensorPointRepository.InsertAsync(model);

            //清除缓存
            await EventBus.TriggerAsync(new SensorPointEventData() { });
            await EventBus.TriggerAsync(new SensorPointBaseEventData() { });
        }

        public async Task UpdateSensorPoint(UpdateSensorPointInput input)
        {
            var info = await _sensorPointRepository.FirstOrDefaultAsync(x => x.TenantNumId == ContextUser.TenantNumId && x.Id == input.Id && x.IsDelete == false);
            if (info.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "测点不存在");
            }
            var isExistName = await _sensorPointRepository.FirstOrDefaultAsync(x => x.TenantNumId == ContextUser.TenantNumId && x.Name == input.Name && x.ProjectId == info.ProjectId && x.GatewayId == input.GatewayId && x.IsDelete == false);
            if (isExistName.IsNotNull() && isExistName.Id != input.Id)
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "测点名称重复");
            }
            info.Name = input.Name;
            info.ProjectId = info.ProjectId;
            info.ClassificationId = input.ClassificationId;
            info.GatewayId = input.GatewayId;
            info.SensorId = input.SensorId;
            info.UpdatedAt = DateTime.Now;
            info.OperatorId = ContextUser.UserId;
            info.OperatorName = ContextUser.UserName;
            await _sensorPointRepository.UpdateAsync(info);

            //清除缓存
            await EventBus.TriggerAsync(new SensorPointEventData() { });
            await EventBus.TriggerAsync(new SensorPointBaseEventData() { });
        }

        public async Task DeleteSensorPoint(DeleteSensorPointInput input)
        {
            var info = await _sensorPointRepository.FirstOrDefaultAsync(x => x.TenantNumId == ContextUser.TenantNumId && x.Id == input.Id && x.IsDelete == false);
            if (info.IsDelete != input.IsDelete)
            {
                info.IsDelete = input.IsDelete;
                info.UpdatedAt = DateTime.Now;
                info.OperatorId = ContextUser.UserId;
                info.OperatorName = ContextUser.UserName;
                await _sensorPointRepository.UpdateAsync(info);
            }

            //清除缓存
            await EventBus.TriggerAsync(new SensorPointEventData() { });
            await EventBus.TriggerAsync(new SensorPointBaseEventData() { });
        }

        /// <summary>
        /// echart 图
        /// </summary>
        /// <returns></returns>
        public async Task<SensorPointChart> QuerySensorPointChart(ChartInput input)
        {
            var reuslt = new SensorPointChart();
            var list = await _sensorPointRepository.QueryList(input.ProjectId);
            reuslt.Offline = list.Where(w => w.State == 0).Count();
            reuslt.Online = list.Where(w => w.State == 1).Count();
            reuslt.OnlineRatio = reuslt.Online == 0 ? 0 : reuslt.Online / reuslt.Offline;

            reuslt.Total = list.Count();
            reuslt.AlarmsCount = list.Where(w => w.IsAlarms == true).Count();
            reuslt.AlarmsRatio = reuslt.AlarmsCount == 0 ? 0 : reuslt.AlarmsCount / reuslt.Total;

            return reuslt;
        }


        /// <summary>
        /// 列表数据
        /// </summary>
        /// <returns></returns>
        public List<SensorPoint> GetBySensorPointList()
        {
            var list = _sensorPointBaseCacheService.GetSensorPointBase(() => { return _sensorPointRepository.GetSensorPointList(); });
            if (list.Count == 0)
            {
                list = _sensorPointRepository.GetSensorPointList();
            }
            return list;
        }
    }
}
