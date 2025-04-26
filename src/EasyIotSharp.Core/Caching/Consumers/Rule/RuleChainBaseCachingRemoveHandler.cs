using System;
using System.Collections.Generic;
using System.Text;
using EasyIotSharp.Core.Caching.Rule.Impl;
using EasyIotSharp.Core.Caching.Rule;
using EasyIotSharp.Core.Events.Rule;
using UPrime.Dependency;
using UPrime.Events.Bus.Handlers;

namespace EasyIotSharp.Core.Caching.Consumers.Rule
{
    public class RuleChainBaseCachingRemoveHandler : IEventHandler<RuleChainBaseEventData>, ITransientDependency
    {
        private readonly IRuleChainCacheService _ruleChainCacheService;
        public RuleChainBaseCachingRemoveHandler(IRuleChainCacheService ruleChainCacheService)
        {
            _ruleChainCacheService = ruleChainCacheService;
        }
        public void HandleEvent(RuleChainBaseEventData eventData)
        {
            _ruleChainCacheService.Cache.RemoveAsync(RuleChainCacheService.KEY_RULE_RULECHAIN_QUERY.FormatWith());
        }
    }
}
