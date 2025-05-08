using EasyIotSharp.Core.Domain.Proejct;
using EasyIotSharp.Core.Dto.Project;
using EasyIotSharp.Core.Dto.Project.Params;
using EasyIotSharp.Core.Repositories.Project;
using EasyIotSharp.Core.Repositories.Hardware;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UPrime.AutoMapper;
using UPrime.Services.Dto;
using System.Linq;
using EasyIotSharp.Core.Caching.Project;
using EasyIotSharp.Core.Caching.Project.Impl;
using EasyIotSharp.Core.Events.Tenant;
using EasyIotSharp.Core.Events.Project;

namespace EasyIotSharp.Core.Services.Project.Impl
{
    /// <summary>
    /// 网关服务实现类
    /// 提供网关的增删改查等基础操作
    /// </summary>
    public class GatewayService : ServiceBase, IGatewayService
    {
        private readonly IProjectBaseRepository _projectBaseRepository;
        private readonly IGatewayRepository _gatewayRepository;
        private readonly IProtocolRepository _protocolRepository;
        private readonly IGatewayCacheService _gatewayCacheService;
    
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="gatewayRepository">网关仓储</param>
        /// <param name="projectBaseRepository">项目基础仓储</param>
        /// <param name="protocolRepository">协议仓储</param>
        /// <param name="gatewayCacheService">网关缓存服务</param>
        public GatewayService(IGatewayRepository gatewayRepository,
                              IProjectBaseRepository projectBaseRepository,
                              IProtocolRepository protocolRepository,
                              IGatewayCacheService gatewayCacheService)
        {
            _projectBaseRepository = projectBaseRepository;
            _gatewayRepository = gatewayRepository;
            _protocolRepository = protocolRepository;
            _gatewayCacheService = gatewayCacheService;
        }
    
        /// <summary>
        /// 获取指定ID的网关信息
        /// </summary>
        /// <param name="id">网关ID</param>
        /// <returns>网关信息DTO</returns>
        public async Task<GatewayDto> GetGateway(string id)
        {
            var info = await _gatewayRepository.FirstOrDefaultAsync(x => x.Id == id && x.IsDelete == false);
            var output = info.MapTo<GatewayDto>();
            if (output.IsNotNull())
            {
                var project = await _projectBaseRepository.GetByIdAsync(output.ProjectId);
                if (project.IsNotNull())
                {
                    output.ProjectName = project.Name;
                }
                var protocol = await _protocolRepository.GetByIdAsync(output.ProtocolId);
                if (protocol.IsNotNull())
                {
                    output.ProtocolName = protocol.Name;
                }
            }
            return output;
        }
    
        /// <summary>
        /// 查询网关列表
        /// </summary>
        /// <param name="input">查询条件</param>
        /// <returns>分页后的网关列表</returns>
        /// <remarks>
        /// 当满足以下条件时会使用缓存：
        /// 1. 关键字为空
        /// 2. 项目ID为空
        /// 3. 协议ID为空
        /// 4. 状态为-1
        /// 5. 启用分页
        /// 6. 页码小于等于5且每页大小为10
        /// </remarks>
        public async Task<PagedResultDto<GatewayDto>> QueryGateway(QueryGatewayInput input)
        {
            if (string.IsNullOrEmpty(input.Keyword)
                && string.IsNullOrEmpty(input.ProjectId)
                && string.IsNullOrEmpty(input.ProtocolId)
                && input.State.Equals(-1)
                && input.IsPage.Equals(true)
                && input.PageIndex <= 5 && input.PageSize == 10)
            {
                return await _gatewayCacheService.QueryGateway(input, async () =>
                {
                    var query = await _gatewayRepository.Query(ContextUser.TenantNumId, input.Keyword, input.State, input.ProtocolId, input.ProjectId, input.PageIndex, input.PageSize);
                    int totalCount = query.totalCount;
                    var list = query.items.MapTo<List<GatewayDto>>();
                    var projects = await _projectBaseRepository.QueryByIds(list.Select(x => x.ProjectId).ToList());
                    var protocols = await _protocolRepository.QueryByIds(list.Select(x => x.ProtocolId).ToList());
                    foreach (var item in list)
                    {
                        var project = projects.FirstOrDefault(x => x.Id == item.ProjectId);
                        if (project.IsNotNull())
                        {
                            item.ProjectName = project.Name;
                        }
                        var protocol = protocols.FirstOrDefault(x => x.Id == item.ProtocolId);
                        if (protocol.IsNotNull())
                        {
                            item.ProtocolName = protocol.Name;
                        }
                    }
                    return new PagedResultDto<GatewayDto>() { TotalCount = totalCount, Items = list };
                });
            }
            else
            {
                var query = await _gatewayRepository.Query(ContextUser.TenantNumId, input.Keyword, input.State, input.ProtocolId, input.ProjectId, input.PageIndex, input.PageSize);
                int totalCount = query.totalCount;
                var list = query.items.MapTo<List<GatewayDto>>();
                var projects = await _projectBaseRepository.QueryByIds(list.Select(x => x.ProjectId).ToList());
                var protocols = await _protocolRepository.QueryByIds(list.Select(x => x.ProtocolId).ToList());
                foreach (var item in list)
                {
                    var project = projects.FirstOrDefault(x => x.Id == item.ProjectId);
                    if (project.IsNotNull())
                    {
                        item.ProjectName = project.Name;
                    }
                    var protocol = protocols.FirstOrDefault(x => x.Id == item.ProtocolId);
                    if (protocol.IsNotNull())
                    {
                        item.ProtocolName = protocol.Name;
                    }
                }
                return new PagedResultDto<GatewayDto>() { TotalCount = totalCount, Items = list };
            }
        }
    
        /// <summary>
        /// 新增网关
        /// </summary>
        /// <param name="input">网关信息</param>
        /// <returns>异步任务</returns>
        /// <exception cref="BizException">当网关名称重复时抛出异常</exception>
        public async Task InsertGateway(InsertGatewayInput input)
        {
            var isExistName = await _gatewayRepository.FirstOrDefaultAsync(x => x.TenantNumId == ContextUser.TenantNumId && x.Name == input.Name && x.ProjectId == input.ProjectId && x.IsDelete == false);
            if (isExistName.IsNotNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "网关名称重复");
            }
            var model = new Gateway();
            model.Id = Guid.NewGuid().ToString().Replace("-", "");
            model.TenantNumId = ContextUser.TenantNumId;
            model.Name = input.Name;
            model.State = 0;
            model.ProtocolId = input.ProtocolId;
            model.ProjectId = input.ProjectId;
            model.IsDelete = false;
            model.Imei = input.Imei;
            model.DeviceModel = input.DeviceModel;
            model.CreationTime = DateTime.Now;
            model.UpdatedAt = DateTime.Now;
            model.OperatorId = ContextUser.UserId;
            model.OperatorName = ContextUser.UserName;
            await _gatewayRepository.InsertAsync(model);

            //清除缓存
            await EventBus.TriggerAsync(new GatewayEventData() { });
        }
    
        /// <summary>
        /// 更新网关信息
        /// </summary>
        /// <param name="input">更新的网关信息</param>
        /// <returns>异步任务</returns>
        /// <exception cref="BizException">
        /// 抛出异常的情况：
        /// 1. 网关不存在
        /// 2. 更新后的网关名称与其他网关重复
        /// </exception>
        public async Task UpdateGateway(UpdateGatewayInput input)
        {
            var info = await _gatewayRepository.FirstOrDefaultAsync(x => x.TenantNumId == ContextUser.TenantNumId && x.Id == input.Id && x.IsDelete == false);
            if (info.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "网关不存在");
            }
            var isExistName = await _gatewayRepository.FirstOrDefaultAsync(x => x.TenantNumId == ContextUser.TenantNumId && x.Name == input.Name && x.ProjectId == info.ProjectId && x.IsDelete == false);
            if (isExistName.IsNotNull() && isExistName.Id != input.Id)
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "网关名称重复");
            }
            info.Name = input.Name;
            info.State = info.State;
            info.ProtocolId = input.ProtocolId;
            info.ProjectId = input.ProjectId;
            info.Imei = input.Imei;
            info.DeviceModel = input.DeviceModel;
            info.UpdatedAt = DateTime.Now;
            info.OperatorId = ContextUser.UserId;
            info.OperatorName = ContextUser.UserName;
            await _gatewayRepository.UpdateAsync(info);

            //清除缓存
            await EventBus.TriggerAsync(new GatewayEventData() { });
        }
    
        /// <summary>
        /// 删除网关（软删除）
        /// </summary>
        /// <param name="input">删除参数</param>
        /// <returns>异步任务</returns>
        /// <remarks>
        /// 执行软删除操作，更新网关的IsDelete状态
        /// 删除后会触发网关事件，清除相关缓存
        /// </remarks>
        public async Task DeleteGateway(DeleteGatewayInput input)
        {
            var info = await _gatewayRepository.FirstOrDefaultAsync(x => x.TenantNumId == ContextUser.TenantNumId && x.Id == input.Id && x.IsDelete == false);
            if (info.IsDelete != input.IsDelete)
            {
                info.IsDelete = input.IsDelete;
                info.UpdatedAt = DateTime.Now;
                info.OperatorId = ContextUser.UserId;
                info.OperatorName = ContextUser.UserName;
                await _gatewayRepository.UpdateAsync(info);
            }

            //清除缓存
            await EventBus.TriggerAsync(new GatewayEventData() { });
        }
    }
}
