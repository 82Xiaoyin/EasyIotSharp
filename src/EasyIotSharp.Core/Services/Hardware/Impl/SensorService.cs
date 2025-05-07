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
    /// <summary>
    /// 传感器服务实现类
    /// </summary>
    public class SensorService : ServiceBase, ISensorService
    {
        private readonly ISensorRepository _sensorRepository;
        private readonly ISensorCacheService _sensorCacheService;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="sensorRepository">传感器仓储</param>
        /// <param name="sensorCacheService">传感器缓存服务</param>
        public SensorService(ISensorRepository sensorRepository,
            ISensorCacheService sensorCacheService)
        {
            _sensorRepository = sensorRepository;
            _sensorCacheService = sensorCacheService;
        }

        /// <summary>
        /// 获取单个传感器信息
        /// </summary>
        /// <param name="id">传感器ID</param>
        /// <returns>传感器信息</returns>
        public async Task<SensorDto> GetSensor(string id)
        {
            var info = await _sensorRepository.FirstOrDefaultAsync(x => x.Id == id && x.IsDelete == false);
            return info.MapTo<SensorDto>();
        }

        /// <summary>
        /// 查询传感器列表
        /// </summary>
        /// <param name="input">查询参数</param>
        /// <returns>分页后的传感器列表</returns>
        /// <remarks>
        /// 当满足以下条件时使用缓存：
        /// 1. 无关键字搜索
        /// 2. 使用分页且在前5页
        /// 3. 每页大小为10
        /// </remarks>
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

        /// <summary>
        /// 新增传感器
        /// </summary>
        /// <param name="input">新增参数</param>
        /// <returns>无</returns>
        /// <exception cref="BizException">当传感器简称重复时抛出异常</exception>
        /// <remarks>
        /// 执行操作：
        /// 1. 检查简称是否重复
        /// 2. 创建新传感器记录
        /// 3. 清除相关缓存
        /// </remarks>
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

        /// <summary>
        /// 修改传感器信息
        /// </summary>
        /// <param name="input">修改参数</param>
        /// <returns>无</returns>
        /// <exception cref="BizException">
        /// 抛出异常的情况：
        /// 1. 传感器不存在
        /// 2. 传感器简称重复
        /// </exception>
        /// <remarks>
        /// 更新内容包括：
        /// 1. 基本信息（名称、简称、供应商等）
        /// 2. 更新时间和操作者信息
        /// 3. 清除相关缓存
        /// </remarks>
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

        /// <summary>
        /// 删除传感器
        /// </summary>
        /// <param name="input">删除参数</param>
        /// <returns>无</returns>
        /// <exception cref="BizException">当传感器不存在时抛出异常</exception>
        /// <remarks>
        /// 执行软删除：
        /// 1. 更新IsDelete状态
        /// 2. 记录操作者信息
        /// 3. 清除相关缓存
        /// </remarks>
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
        /// 获取传感器列表
        /// </summary>
        /// <returns>传感器列表</returns>
        /// <remarks>
        /// 优先从缓存获取数据：
        /// 1. 如果缓存中有数据，直接返回
        /// 2. 如果缓存为空，从数据库获取
        /// </remarks>
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
