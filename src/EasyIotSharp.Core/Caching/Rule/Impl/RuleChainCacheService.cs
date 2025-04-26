using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Dto.Rule.Params;
using EasyIotSharp.Core.Dto.Rule;
using System.Threading.Tasks;
using UPrime.Runtime.Caching;
using UPrime.Services.Dto;

namespace EasyIotSharp.Core.Caching.Rule.Impl
{
    public class RuleChainCacheService : CachingServiceBase, IRuleChainCacheService
    {
        public ICache Cache => CacheManager.GetCache($"{CachingConsts.Keys.Rule}RuleChainCacheService");
        public const string KEY_RULE_RULECHAIN_QUERY = "Rule:RuleChain-{0}-{1}";
        public const string KEY_RULE_RULECHAINBASE_QUERY = "Rule:RuleChainBase";
        public void Clear()
        {
            Cache.Clear();
        }
        public async Task<PagedResultDto<RuleChainDto>> QueryRuleChain(SceneManagementInput input, Func<Task<PagedResultDto<RuleChainDto>>> action)
        {
            var key = KEY_RULE_RULECHAIN_QUERY.FormatWith(input.PageIndex,
                                          input.PageSize);
            return await Cache.GetAsyncExt(key, action, TimeSpan.FromHours(CachingConsts.TENANT_EXPIRES_MINUTES));
        }

        public async Task<List<RuleChainDto>> QueryRuleChainBase(Func<Task<List<RuleChainDto>>> action)
        {
            var key = KEY_RULE_RULECHAINBASE_QUERY.FormatWith();
            return await Cache.GetAsyncExt(key, action, TimeSpan.FromHours(CachingConsts.TENANT_EXPIRES_MINUTES));
        }
    }
}
