using EasyIotSharp.Core.Caching.Hardware;
using EasyIotSharp.Core.Caching.Tenant;
using EasyIotSharp.Core.Domain.Hardware;
using EasyIotSharp.Core.Dto.Hardware;
using EasyIotSharp.Core.Dto.Hardware.Params;
using EasyIotSharp.Core.Events.Hardware;
using EasyIotSharp.Core.Events.Tenant;
using EasyIotSharp.Core.Repositories.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UPrime.AutoMapper;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Services.Hardware.Impl
{
    public class ProtocolConfigService : ServiceBase, IProtocolConfigService
    {
        private readonly IProtocolRepository _protocolRepository;
        private readonly IProtocolConfigRepository _protocolConfigRepository;
        private readonly IProtocolConfigExtRepository _protocolConfigExtRepository;

        private readonly IProtocolConfigCacheService _protocolConfigCacheService;

        public ProtocolConfigService(IProtocolRepository protocolRepository,
                                     IProtocolConfigRepository protocolConfigRepository,
                                     IProtocolConfigExtRepository protocolConfigExtRepository,
                                     IProtocolConfigCacheService protocolConfigCacheService)
        {
            _protocolRepository = protocolRepository;
            _protocolConfigRepository = protocolConfigRepository;
            _protocolConfigExtRepository = protocolConfigExtRepository;
            _protocolConfigCacheService = protocolConfigCacheService;
        }

        public async Task<ProtocolConfigDto> GetProtocolConfig(string id)
        {
            var info = await _protocolConfigRepository.FirstOrDefaultAsync(x => x.Id == id && x.IsDelete == false);
            var output = info.MapTo<ProtocolConfigDto>();
            if (output.IsNotNull())
            {
                var protocol = await _protocolRepository.GetByIdAsync(output.ProtocolId);
                if (protocol.IsNotNull())
                {
                    output.ProtocolName = protocol.Name;
                }
            }
            return output;
        }

        public async Task<PagedResultDto<QueryProtocolConfigByProtocolIdOutput>> QueryProtocolConfig(QueryProtocolConfigInput input)
        {
            if (string.IsNullOrEmpty(input.Keyword)
                && input.TagType.Equals(TagTypeMenu.None)
                && input.ProtocolId.IsNull()
                && input.IsPage.Equals(true)
                && input.PageIndex <= 5 && input.PageSize == 10)
            {
                return await _protocolConfigCacheService.QueryProtocolConfig(input, async () =>
                {
                    var query = await _protocolConfigRepository.Query(input.ProtocolId, input.Keyword, input.TagType, input.PageIndex, input.PageSize, input.IsPage);
                    int totalCount = query.totalCount;
                    var list = query.items.MapTo<List<QueryProtocolConfigByProtocolIdOutput>>();
                    var protocols = await _protocolRepository.QueryByIds(list.Select(x => x.ProtocolId).ToList());
                    var configExts = await _protocolConfigExtRepository.QueryByConfigIds(list.Select(x => x.Id).ToList());
                    foreach (var item in list)
                    {
                        var protocol = protocols.FirstOrDefault(x => x.Id == item.ProtocolId);
                        if (protocol.IsNotNull())
                        {
                            item.ProtocolName = protocol.Name;
                        }
                        var configExtList = configExts.Where(x => x.ProtocolConfigId == item.Id).ToList();
                        Dictionary<string, string> dic = new Dictionary<string, string>();
                        foreach (var item1 in configExtList)
                        {
                            dic.Add(item1.Value, item1.Label);
                        }
                        item.Options = new Dictionary<string, string>();
                        item.Options = dic;
                    }

                    return new PagedResultDto<QueryProtocolConfigByProtocolIdOutput>() { TotalCount = totalCount, Items = list };
                });
            }
            else
            {
                var query = await _protocolConfigRepository.Query(input.ProtocolId, input.Keyword, input.TagType, input.PageIndex, input.PageSize, input.IsPage);
                int totalCount = query.totalCount;
                var list = query.items.MapTo<List<QueryProtocolConfigByProtocolIdOutput>>();
                var protocols = await _protocolRepository.QueryByIds(list.Select(x => x.ProtocolId).ToList());
                var configExts = await _protocolConfigExtRepository.QueryByConfigIds(list.Select(x => x.Id).ToList());
                foreach (var item in list)
                {
                    var protocol = protocols.FirstOrDefault(x => x.Id == item.ProtocolId);
                    if (protocol.IsNotNull())
                    {
                        item.ProtocolName = protocol.Name;
                    }
                    var configExtList = configExts.Where(x => x.ProtocolConfigId == item.Id).ToList();
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    foreach (var item1 in configExtList)
                    {
                        dic.Add(item1.Value, item1.Label);
                    }
                    item.Options = new Dictionary<string, string>();
                    item.Options = dic;
                }

                return new PagedResultDto<QueryProtocolConfigByProtocolIdOutput>() { TotalCount = totalCount, Items = list };
            }

        }

        public async Task InsertProtocolConfig(InsertProtocolConfigInput input)
        {
            var isExistName = await _protocolConfigRepository.FirstOrDefaultAsync(x => x.Identifier == input.Identifier && x.IsDelete == false);
            if (isExistName.IsNotNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "标识符重复");
            }
            var model = new ProtocolConfig();
            model.Id = Guid.NewGuid().ToString().Replace("-", "");
            model.ProtocolId = input.ProtocolId;
            model.Identifier = input.Identifier;
            model.Label = input.Label;
            model.Placeholder = input.Placeholder;
            model.Tag = input.Tag;
            model.TagType = input.TagType;
            model.IsRequired = input.IsRequired;
            model.ValidateType = input.ValidateType;
            model.ValidateMessage = input.ValidateMessage;
            model.Sort = input.Sort;
            model.IsDelete = false;
            model.CreationTime = DateTime.Now;
            model.UpdatedAt = DateTime.Now;
            model.OperatorId = ContextUser.UserId;
            model.OperatorName = ContextUser.UserName;
            await _protocolConfigRepository.InsertAsync(model);
            var protocolConfigExts = new List<ProtocolConfigExt>();
            foreach (var item in input.Options)
            {
                protocolConfigExts.Add(new ProtocolConfigExt()
                {
                    Id = Guid.NewGuid().ToString().Replace("-", ""),
                    ProtocolConfigId = model.Id,
                    Label = item.Value,
                    Value = item.Key,
                    IsDelete = false,
                    CreationTime = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    OperatorId = ContextUser.UserId,
                    OperatorName = ContextUser.UserName
                });
            }
            if (protocolConfigExts.Count > 0)
            {
                await _protocolConfigExtRepository.InserManyAsync(protocolConfigExts);
            }

            //清除缓存
            await EventBus.TriggerAsync(new ProtocolConfigEventData() { });
        }

        public async Task UpdateProtocolConfig(UpdateProtocolConfigInput input)
        {
            var info = await _protocolConfigRepository.FirstOrDefaultAsync(x => x.Id == input.Id && x.IsDelete == false);
            if (info.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "协议配置不存在");
            }
            var isExistName = await _protocolConfigRepository.FirstOrDefaultAsync(x => x.Identifier == input.Identifier && x.IsDelete == false);
            if (isExistName.IsNotNull() && isExistName.Id != input.Id)
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "标识符重复");
            }
            info.Label = input.Label;
            info.Placeholder = input.Placeholder;
            info.Tag = input.Tag;
            info.TagType = input.TagType;
            info.IsRequired = input.IsRequired;
            info.ValidateType = input.ValidateType;
            info.ValidateMessage = input.ValidateMessage;
            info.Sort = input.Sort;
            info.UpdatedAt = DateTime.Now;
            info.OperatorId = ContextUser.UserId;
            info.OperatorName = ContextUser.UserName;
            await _protocolConfigRepository.UpdateAsync(info);

            if (input.IsUpdateExt == true)
            {
                //批量删除老的协议配置表
                await _protocolConfigExtRepository.DeleteManyByConfigId(input.Id);

                var protocolConfigExts = new List<ProtocolConfigExt>();
                foreach (var item in input.Options)
                {
                    protocolConfigExts.Add(new ProtocolConfigExt()
                    {
                        Id = Guid.NewGuid().ToString().Replace("-", ""),
                        ProtocolConfigId = input.Id,
                        Label = item.Value,
                        Value = item.Key,
                        IsDelete = false,
                        CreationTime = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        OperatorId = ContextUser.UserId,
                        OperatorName = ContextUser.UserName
                    });
                }
                if (protocolConfigExts.Count > 0)
                {
                    await _protocolConfigExtRepository.InserManyAsync(protocolConfigExts);
                }
            }

            //清除缓存
            await EventBus.TriggerAsync(new ProtocolConfigEventData() { });
        }

        public async Task DeleteProtocolConfig(DeleteProtocolConfigInput input)
        {
            var info = await _protocolConfigRepository.GetByIdAsync(input.Id);
            if (info.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "未找到指定数据");
            }
            if (info.IsDelete != input.IsDelete)
            {
                info.IsDelete = input.IsDelete;
                info.OperatorId = ContextUser.UserId;
                info.OperatorName = ContextUser.UserName;
                info.UpdatedAt = DateTime.Now;
                await _protocolConfigRepository.UpdateAsync(info);
            }

            //清除缓存
            await EventBus.TriggerAsync(new ProtocolConfigEventData() { });
        }
    }
}
