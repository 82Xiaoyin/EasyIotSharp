using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Dto.Rule.Params;
using EasyIotSharp.Core.Dto.Rule;
using System.Threading.Tasks;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Caching.Rule
{
    public interface IRuleChainCacheService : ICacheService
    {
        /// <summary>
        /// 场景联动
        /// </summary>
        /// <param name="input"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        Task<PagedResultDto<RuleChainDto>> QueryRuleChain(SceneManagementInput input, Func<Task<PagedResultDto<RuleChainDto>>> action);

        /// <summary>
        /// 获取所有场景联动列表
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        Task<List<RuleChainDto>> QueryRuleChainBase(Func<Task<List<RuleChainDto>>> action);
    }
}
