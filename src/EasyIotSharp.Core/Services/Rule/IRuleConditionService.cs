using EasyIotSharp.Core.Dto;
using EasyIotSharp.Core.Dto.Rule;
using EasyIotSharp.Core.Dto.Rule.Params;
using System.Threading.Tasks;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Services.Rule
{
    public interface IRuleConditionService
    {
        /// <summary>
        /// 查询规则条件列表
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<PagedResultDto<RuleConditionDto>> QueryRuleCondition(RuleConditionInput input);

        /// <summary>
        /// 新增规则条件
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task InsertRuleCondition(InsertRuleCondition input);

        /// <summary>
        /// 修改规则条件
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task UpdateRuleCondition(InsertRuleCondition input);

        /// <summary>
        /// 删除规则条件
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task DeleteRuleCondition(DeleteInput input);

        /// <summary>
        /// 修改规则条件状态
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task UpdateRuleConditionState(InsertRuleCondition input);
    }
}