using System;
using System.Threading.Tasks;
using EasyIotSharp.Core.Caching.Rule;
using EasyIotSharp.Core.Caching.Rule.Impl;
using EasyIotSharp.Core.Domain.Rule;
using EasyIotSharp.Core.Dto;
using EasyIotSharp.Core.Dto.Rule;
using EasyIotSharp.Core.Dto.Rule.Params;
using EasyIotSharp.Core.Events.Rule;
using EasyIotSharp.Core.Repositories.Rule;
using UPrime.AutoMapper;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Services.Rule.Impl
{
    /// <summary>
    /// 规则链服务
    /// </summary>
    public class RuleChainService : ServiceBase, IRuleChainService
    {
        private readonly IRuleChainRepository _ruleChainRepository;
        private readonly IRuleChainCacheService _ruleChainCacheService;

        /// <summary>
        /// 构造函数
        /// </summary>
        public RuleChainService(IRuleChainRepository ruleChainRepository,
                                IRuleChainCacheService ruleChainCacheService)
        {
            _ruleChainRepository = ruleChainRepository;
            _ruleChainCacheService = ruleChainCacheService;
        }

        /// <summary>
        /// 查询规则链列表
        /// </summary>
        public async Task<PagedResultDto<RuleChainDto>> QueryRuleChain(SceneManagementInput input)
        {
            if (string.IsNullOrEmpty(input.Keyword)
               && input.IsPage.Equals(true)
               && input.PageIndex <= 5 && input.PageSize == 10)
            {
                return await _ruleChainCacheService.QueryRuleChain(input, async () =>
                {
                    var query = await _ruleChainRepository.Query(
                    input.Keyword,
                    input.ProjectId,
                    input.PageIndex,
                    input.PageSize);

                    return new PagedResultDto<RuleChainDto>()
                    {
                        TotalCount = query.totalCount,
                        Items = query.items
                    };
                });
            }
            else
            {
                var query = await _ruleChainRepository.Query(
                input.Keyword,
                input.ProjectId,
                input.PageIndex,
                input.PageSize);

                return new PagedResultDto<RuleChainDto>()
                {
                    TotalCount = query.totalCount,
                    Items = query.items
                };
            }
        }

        /// <summary>
        /// 新增规则链
        /// </summary>
        public async Task InsertRuleChain(InsertRuleChain input)
        {
            var isExist = await _ruleChainRepository.FirstOrDefaultAsync(x =>
                x.Name == input.Name &&
                x.IsDelete == false);

            if (isExist.IsNotNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "规则链名称重复");
            }

            var model = new RuleChain
            {
                Id = Guid.NewGuid().ToString().Replace("-", ""),
                Name = input.Name,
                AlarmsJSON = input.AlarmsJSON,
                ProjectId = input.ProjectId,
                Remark = input.Remark,
                RuleContentJson = input.RuleContentJson,
                State = input.State,
                CreationTime = DateTime.Now,
                UpdatedAt = DateTime.Now,
                OperatorId = ContextUser.UserId,
                OperatorName = ContextUser.UserName
            };

            await _ruleChainRepository.InsertAsync(model);

            //清除缓存
            await EventBus.TriggerAsync(new RuleChainEventData() { });
        }

        /// <summary>
        /// 修改规则链
        /// </summary>
        public async Task UpdateRuleChain(InsertRuleChain input)
        {
            var info = await _ruleChainRepository.GetByIdAsync(input.Id);
            if (info.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "未找到指定数据");
            }

            var isExist = await _ruleChainRepository.FirstOrDefaultAsync(x =>
                x.Id != input.Id &&
                x.Name == input.Name &&
                x.IsDelete == false);

            if (isExist.IsNotNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "规则链名称重复");
            }

            input.Name = info.Name;
            input.AlarmsJSON = info.AlarmsJSON;
            input.ProjectId = info.ProjectId;
            input.Remark = info.Remark;
            input.RuleContentJson = info.RuleContentJson;
            input.State = info.State;
            info.UpdatedAt = DateTime.Now;
            info.OperatorId = ContextUser.UserId;
            info.OperatorName = ContextUser.UserName;

            await _ruleChainRepository.UpdateAsync(info);

            //清除缓存
            await EventBus.TriggerAsync(new RuleChainEventData() { });
        }

        /// <summary>
        /// 删除规则链
        /// </summary>
        public async Task DeleteRuleChain(DeleteInput input)
        {
            var info = await _ruleChainRepository.GetByIdAsync(input.Id);
            if (info.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "未找到指定数据");
            }

            info.IsDelete = true;
            info.UpdatedAt = DateTime.Now;
            info.OperatorId = ContextUser.UserId;
            info.OperatorName = ContextUser.UserName;

            await _ruleChainRepository.UpdateAsync(info);

            //清除缓存
            await EventBus.TriggerAsync(new RuleChainEventData() { });
        }

        /// <summary>
        /// 修改规则链状态
        /// </summary>
        public async Task UpdateRuleChainState(InsertRuleChain input)
        {
            var info = await _ruleChainRepository.GetByIdAsync(input.Id);
            if (info.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "未找到指定数据");
            }

            info.State = input.State;
            info.UpdatedAt = DateTime.Now;
            info.OperatorId = ContextUser.UserId;
            info.OperatorName = ContextUser.UserName;

            await _ruleChainRepository.UpdateAsync(info);

            //清除缓存
            await EventBus.TriggerAsync(new RuleChainEventData() { });
        }
    }
}