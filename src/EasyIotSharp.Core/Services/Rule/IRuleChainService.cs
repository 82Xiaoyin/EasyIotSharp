using EasyIotSharp.Core.Dto;
using EasyIotSharp.Core.Dto.Rule;
using EasyIotSharp.Core.Dto.Rule.Params;
using System.Threading.Tasks;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Services.Rule
{
    /// <summary>
    /// 规则链服务接口
    /// </summary>
    public interface IRuleChainService
    {
        /// <summary>
        /// 查询规则链列表
        /// </summary>
        Task<PagedResultDto<RuleChainDto>> QueryRuleChain(SceneManagementInput input);

        /// <summary>
        /// 新增规则链
        /// </summary>
        Task InsertRuleChain(InsertRuleChain input);

        /// <summary>
        /// 修改规则链
        /// </summary>
        Task UpdateRuleChain(InsertRuleChain input);

        /// <summary>
        /// 删除规则链
        /// </summary>
        Task DeleteRuleChain(DeleteInput input);

        /// <summary>
        /// 修改规则链状态
        /// </summary>
        Task UpdateRuleChainState(InsertRuleChain input);
    }
}