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
    public class GatewayService : ServiceBase, IGatewayService
    {
        private readonly IProjectBaseRepository _projectBaseRepository;
        private readonly IGatewayRepository _gatewayRepository;
        private readonly IProtocolRepository _protocolRepository;
        private readonly IGatewayCacheService _gatewayCacheService;

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
            model.CreationTime = DateTime.Now;
            model.UpdatedAt = DateTime.Now;
            model.OperatorId = ContextUser.UserId;
            model.OperatorName = ContextUser.UserName;
            await _gatewayRepository.InsertAsync(model);

            //清除缓存
            await EventBus.TriggerAsync(new GatewayEventData() { });
        }

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
            info.UpdatedAt = DateTime.Now;
            info.OperatorId = ContextUser.UserId;
            info.OperatorName = ContextUser.UserName;
            await _gatewayRepository.UpdateAsync(info);

            //清除缓存
            await EventBus.TriggerAsync(new GatewayEventData() { });
        }

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
