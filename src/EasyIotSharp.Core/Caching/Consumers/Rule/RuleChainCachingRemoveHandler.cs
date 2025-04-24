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
    public class RuleChainCachingRemoveHandler : IEventHandler<RuleChainEventData>, ITransientDependency
    {
        private readonly IRuleChainCacheService _ruleChainCacheService;
        public RuleChainCachingRemoveHandler(IRuleChainCacheService ruleChainCacheService)
        {
            _ruleChainCacheService = ruleChainCacheService;
        }
        public void HandleEvent(RuleChainEventData eventData)
        {
            for (int i = 1; i <= 5; i++)
            {
                _ruleChainCacheService.Cache.RemoveAsync(RuleChainCacheService.KEY_RULE_RULECHAIN_QUERY.FormatWith(i, 10));
            }
        }
    }
}
