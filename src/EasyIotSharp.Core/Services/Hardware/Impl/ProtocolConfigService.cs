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
    /// <summary>
    /// 协议配置服务实现类
    /// </summary>
    public class ProtocolConfigService : ServiceBase, IProtocolConfigService
    {
        private readonly IProtocolRepository _protocolRepository;
        private readonly IProtocolConfigRepository _protocolConfigRepository;
        private readonly IProtocolConfigExtRepository _protocolConfigExtRepository;
        private readonly IProtocolConfigCacheService _protocolConfigCacheService;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="protocolRepository">协议仓储</param>
        /// <param name="protocolConfigRepository">协议配置仓储</param>
        /// <param name="protocolConfigExtRepository">协议配置扩展仓储</param>
        /// <param name="protocolConfigCacheService">协议配置缓存服务</param>
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

        /// <summary>
        /// 通过id获取一条协议配置信息
        /// </summary>
        /// <param name="id">协议配置ID</param>
        /// <returns>协议配置信息</returns>
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

        /// <summary>
        /// 查询协议配置列表
        /// </summary>
        /// <param name="input">查询参数</param>
        /// <returns>分页后的协议配置列表</returns>
        /// <remarks>
        /// 当满足以下条件时使用缓存：
        /// 1. 无关键字搜索
        /// 2. 标签类型为None
        /// 3. 无协议ID筛选
        /// 4. 使用分页且在前5页
        /// 5. 每页大小为10
        /// </remarks>
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

        /// <summary>
        /// 新增协议配置
        /// </summary>
        /// <param name="input">新增参数</param>
        /// <returns>无</returns>
        /// <exception cref="BizException">当标识符重复时抛出异常</exception>
        /// <remarks>
        /// 同时会处理：
        /// 1. 基本配置信息
        /// 2. 扩展配置信息（Options）
        /// 3. 清除相关缓存
        /// </remarks>
        public async Task InsertProtocolConfig(InsertProtocolConfigInput input)
        {
            if (input.ProtocolId.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "协议id不能为空");
            }
            var isExistName = await _protocolConfigRepository.FirstOrDefaultAsync(x => x.Identifier == input.Identifier && x.ProtocolId == input.ProtocolId && x.IsDelete == false);
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

        /// <summary>
        /// 修改协议配置
        /// </summary>
        /// <param name="input">修改参数</param>
        /// <returns>无</returns>
        /// <exception cref="BizException">
        /// 抛出异常的情况：
        /// 1. 协议配置不存在
        /// 2. 标识符重复
        /// </exception>
        /// <remarks>
        /// 更新内容包括：
        /// 1. 基本配置信息
        /// 2. 可选更新扩展配置（由IsUpdateExt控制）
        /// 3. 清除相关缓存
        /// </remarks>
        public async Task UpdateProtocolConfig(UpdateProtocolConfigInput input)
        {
            var info = await _protocolConfigRepository.FirstOrDefaultAsync(x => x.Id == input.Id && x.IsDelete == false);
            if (info.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "协议配置不存在");
            }
            if (input.ProtocolId.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "协议id不能为空");
            }
            var isExistName = await _protocolConfigRepository.FirstOrDefaultAsync(x => x.Identifier == input.Identifier && x.ProtocolId==input.ProtocolId && x.IsDelete == false);
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

        /// <summary>
        /// 删除协议配置
        /// </summary>
        /// <param name="input">删除参数</param>
        /// <returns>无</returns>
        /// <exception cref="BizException">当协议配置不存在时抛出异常</exception>
        /// <remarks>
        /// 执行软删除：
        /// 1. 更新IsDelete状态
        /// 2. 记录操作者信息
        /// 3. 清除相关缓存
        /// </remarks>
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
