using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EasyIotSharp.Core.Domain.Rule;
using EasyIotSharp.Core.Dto;
using EasyIotSharp.Core.Dto.Rule;
using EasyIotSharp.Core.Dto.Rule.Params;
using EasyIotSharp.Core.Repositories.Rule;
using Mysqlx.Expr;
using UPrime.AutoMapper;
using UPrime.Services.Dto;
using static Nest.JoinField;

namespace EasyIotSharp.Core.Services.Rule.Impl
{
    /// <summary>
    /// 规则条件服务
    /// </summary>
    public class RuleConditionService : ServiceBase, IRuleConditionService
    {
        private readonly IRuleConditionRepository _ruleConditionRepository;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ruleConditionRepository">规则条件仓储</param>
        public RuleConditionService(IRuleConditionRepository ruleConditionRepository)
        {
            _ruleConditionRepository = ruleConditionRepository;
        }

        /// <summary>
        /// 查询规则条件列表
        /// </summary>
        /// <param name="input">查询参数</param>
        /// <returns>分页结果</returns>
        public async Task<PagedResultDto<RuleConditionDto>> QueryRuleCondition(RuleConditionInput input)
        {
            var query = await _ruleConditionRepository.Query(
                input.RuleCode,
                input.KeyWord,
                input.IsEnable,
                input.PageIndex,
                input.PageSize);

            return new PagedResultDto<RuleConditionDto>()
            {
                TotalCount = query.totalCount,
                Items = query.items
            };
        }

        /// <summary>
        /// 新增规则条件
        /// </summary>
        /// <param name="input">新增参数</param>
        /// <returns>执行结果</returns>
        /// <exception cref="BizException">规则条件标签重复时抛出异常</exception>
        public async Task InsertRuleCondition(InsertRuleCondition input)
        {
            var isExist = await _ruleConditionRepository.FirstOrDefaultAsync(x =>
                x.RuleConditionCode == input.RuleConditionCode &&
                x.Label == input.Label &&
                x.IsDelete == false);

            if (isExist.IsNotNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "规则条件标签重复");
            }

            var model = new RuleCondition
            {
                Id = Guid.NewGuid().ToString().Replace("-", ""),
                RuleConditionCode = input.RuleConditionCode,
                Label = input.Label,
                Tag = input.Tag,
                Remark = input.Remark,
                Identifier = input.Identifier,
                ParentId = input.ParentId,
                Sort = input.Sort,
                Placeholder = input.Placeholder,
                TagType = input.TagType,
                IsEnable = input.IsEnable,
                CreationTime = DateTime.Now,
                UpdatedAt = DateTime.Now,
                OperatorId = ContextUser.UserId,
                OperatorName = ContextUser.UserName
            };

            await _ruleConditionRepository.InsertAsync(model);
        }

        /// <summary>
        /// 修改规则条件
        /// </summary>
        /// <param name="input">修改参数</param>
        /// <returns>执行结果</returns>
        /// <exception cref="BizException">未找到指定数据或规则条件标签重复时抛出异常</exception>
        public async Task UpdateRuleCondition(InsertRuleCondition input)
        {
            var info = await _ruleConditionRepository.GetByIdAsync(input.Id);
            if (info.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "未找到指定数据");
            }

            var isExist = await _ruleConditionRepository.FirstOrDefaultAsync(x =>
                x.Id != input.Id &&
                x.RuleConditionCode == input.RuleConditionCode &&
                x.Label == input.Label &&
                x.IsDelete == false);

            if (isExist.IsNotNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "规则条件标签重复");
            }

            info.RuleConditionCode = input.RuleConditionCode;
            info.Label = input.Label;
            info.Tag = input.Tag;
            info.IsEnable = input.IsEnable;
            info.TagType = input.TagType;
            info.Remark = input.Remark;
            info.Identifier = input.Identifier;
            info.ParentId = input.ParentId;
            info.Sort = input.Sort;
            info.Placeholder = input.Placeholder;
            info.UpdatedAt = DateTime.Now;
            info.OperatorId = ContextUser.UserId;
            info.OperatorName = ContextUser.UserName;

            await _ruleConditionRepository.UpdateAsync(info);
        }

        /// <summary>
        /// 删除规则条件
        /// </summary>
        /// <param name="input">删除参数</param>
        /// <returns>执行结果</returns>
        /// <exception cref="BizException">未找到指定数据时抛出异常</exception>
        public async Task DeleteRuleCondition(DeleteInput input)
        {
            var info = await _ruleConditionRepository.GetByIdAsync(input.Id);
            if (info.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "未找到指定数据");
            }

            info.IsDelete = true;
            info.UpdatedAt = DateTime.Now;
            info.OperatorId = ContextUser.UserId;
            info.OperatorName = ContextUser.UserName;

            await _ruleConditionRepository.UpdateAsync(info);
        }

        /// <summary>
        /// 修改规则条件状态
        /// </summary>
        /// <param name="input">状态参数</param>
        /// <returns>执行结果</returns>
        /// <exception cref="BizException">未找到指定数据时抛出异常</exception>
        public async Task UpdateRuleConditionState(InsertRuleCondition input)
        {
            var info = await _ruleConditionRepository.GetByIdAsync(input.Id);
            if (info.IsNull())
            {
                throw new BizException(BizError.BIND_EXCEPTION_ERROR, "未找到指定数据");
            }

            info.IsEnable = input.IsEnable;
            info.UpdatedAt = DateTime.Now;
            info.OperatorId = ContextUser.UserId;
            info.OperatorName = ContextUser.UserName;

            await _ruleConditionRepository.UpdateAsync(info);
        }
    }
}