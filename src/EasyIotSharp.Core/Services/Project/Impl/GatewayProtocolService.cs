using EasyIotSharp.Core.Caching.Project;
using EasyIotSharp.Core.Caching.Project.Impl;
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
    public class GatewayProtocolService:ServiceBase, IGatewayProtocolService
    {
        private readonly IGatewayProtocolRepository _gatewayProtocolRepository;
        private readonly IGatewayRepository _gatewayRepository;
        private readonly IProtocolRepository _protocolRepository;
        private readonly IGatewayProtocolCacheService _gatewayProtocolCacheService;

        public GatewayProtocolService(IGatewayProtocolRepository gatewayProtocolRepository,
                                      IGatewayRepository gatewayRepository,
                                      IProtocolRepository protocolRepository,
                                      IGatewayProtocolCacheService gatewayProtocolCacheService)
        {
            _gatewayProtocolRepository = gatewayProtocolRepository;
            _gatewayRepository = gatewayRepository;
            _protocolRepository = protocolRepository;
            _gatewayProtocolCacheService = gatewayProtocolCacheService;
        }

        public async Task<GatewayProtocolDto> GetGatewayProtocol(string id)
        {
            var info = await _gatewayProtocolRepository.FirstOrDefaultAsync(x => x.Id == id && x.IsDelete == false);
            var output = info.MapTo<GatewayProtocolDto>();
            if (output.IsNotNull())
            {
                var gateway = await _gatewayRepository.GetByIdAsync(output.GatewayId);
                if (gateway.IsNotNull())
                {
                    output.GatewayName = gateway.Name;
                }
                var protocol = await _protocolRepository.GetByIdAsync(output.ProtocolId);
                if (protocol.IsNotNull())
                {
                    output.ProtocolName = protocol.Name;
                }
            }
            return output;
        }

        public async Task<PagedResultDto<GatewayProtocolDto>> QueryGatewayProtocol(QueryGatewayProtocolInput input)
        {
            if (input.GatewayId.IsNull()
                && input.IsPage.Equals(true)
                && input.PageIndex <= 5 && input.PageSize == 10)
            {
                return await _gatewayProtocolCacheService.QueryGatewayProtocol(input, async () =>
                {
                    var query = await _gatewayProtocolRepository.Query(ContextUser.TenantNumId, input.GatewayId, input.PageIndex, input.PageSize);
                    int totalCount = query.totalCount;
                    var list = query.items.MapTo<List<GatewayProtocolDto>>();
                    var gateways = await _gatewayRepository.QueryByIds(list.Select(x => x.GatewayId).ToList());
                    var protocols = await _protocolRepository.QueryByIds(list.Select(x => x.ProtocolId).ToList());
                    foreach (var item in list)
                    {
                        var gateway = gateways.FirstOrDefault(x => x.Id == item.GatewayId);
                        if (gateway.IsNotNull())
                        {
                            item.GatewayName = gateway.Name;
                            item.ProjectId = gateway.ProjectId;
                        }
                        var protocol = protocols.FirstOrDefault(x => x.Id == item.ProtocolId);
                        if (protocol.IsNotNull())
                        {
                            item.ProtocolName = protocol.Name;
                        }
                    }
                    return new PagedResultDto<GatewayProtocolDto>() { TotalCount = totalCount, Items = list };
                });
            }
            else
            {
                var query = await _gatewayProtocolRepository.Query(ContextUser.TenantNumId, input.GatewayId, input.PageIndex, input.PageSize);
                int totalCount = query.totalCount;
                var list = query.items.MapTo<List<GatewayProtocolDto>>();
                var gateways = await _gatewayRepository.QueryByIds(list.Select(x => x.GatewayId).ToList());
                var protocols = await _protocolRepository.QueryByIds(list.Select(x => x.ProtocolId).ToList());
                foreach (var item in list)
                {
                    var gateway = gateways.FirstOrDefault(x => x.Id == item.GatewayId);
                    if (gateway.IsNotNull())
                    {
                        item.GatewayName = gateway.Name;
                        item.ProjectId = gateway.ProjectId;
                    }
                    var protocol = protocols.FirstOrDefault(x => x.Id == item.ProtocolId);
                    if (protocol.IsNotNull())
                    {
                        item.ProtocolName = protocol.Name;
                    }
                }
                return new PagedResultDto<GatewayProtocolDto>() { TotalCount = totalCount, Items = list };
            }
        }

        public async Task InsertGatewayProtocol(InsertGatewayProtocolInput input)
        {
            var model = new GatewayProtocol();
            model.Id = Guid.NewGuid().ToString().Replace("-", "");
            model.TenantNumId = ContextUser.TenantNumId;
            model.GatewayId = input.GatewayId;
            model.ProtocolId = input.ProtocolId;
            model.ConfigJson = input.ConfigJson;
            model.IsDelete = false;
            model.CreationTime = DateTime.Now;
            model.UpdatedAt = DateTime.Now;
            model.OperatorId = ContextUser.UserId;
            model.OperatorName = ContextUser.UserName;
            await _gatewayProtocolRepository.InsertAsync(model);
            //批量添加网关协议配置

            //清除缓存
            await EventBus.TriggerAsync(new GatewayProtocolEventData() { });
        }

        public async Task UpdateGatewayProtocol(UpdateGatewayProtocolInput input)
        {
            var info = await _gatewayProtocolRepository.FirstOrDefaultAsync(x => x.TenantNumId == ContextUser.TenantNumId && x.Id == input.Id && x.IsDelete == false);
            if (info.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "网关协议不存在");
            }
            info.ConfigJson = input.ConfigJson;
            info.UpdatedAt = DateTime.Now;
            info.OperatorId = ContextUser.UserId;
            info.OperatorName = ContextUser.UserName;
            await _gatewayProtocolRepository.UpdateAsync(info);
            //批量添加网关协议配置

            //清除缓存
            await EventBus.TriggerAsync(new GatewayProtocolEventData() { });
        }

        public async Task DeleteGatewayProtocol(DeleteGatewayProtocolInput input)
        {
            var info = await _gatewayProtocolRepository.FirstOrDefaultAsync(x => x.TenantNumId == ContextUser.TenantNumId && x.Id == input.Id && x.IsDelete == false);
            if (info.IsDelete != input.IsDelete)
            {
                info.IsDelete = input.IsDelete;
                info.UpdatedAt = DateTime.Now;
                info.OperatorId = ContextUser.UserId;
                info.OperatorName = ContextUser.UserName;
                await _gatewayProtocolRepository.UpdateAsync(info);
            }

            //清除缓存
            await EventBus.TriggerAsync(new GatewayProtocolEventData() { });
        }
    }
}
